using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lec.Acme.Models;
using Lec.Acme.Services;
using Lec.Acme.Utilities;
using Lec.Web.Models;
using Lec.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Lec.Web.WebMiddleware
{
    internal class CertificateMiddleware: IMiddleware
    {
        private readonly ICertificateApplicantStore _applicantStore;
        private readonly ICertificateStore _certificateStore;
        private readonly IAccountStore _accountStore;
        private readonly ILecManager _lecManager;

        public CertificateMiddleware(ICertificateApplicantStore applicantStore, IAccountStore accountStore, ILecManager lecManager, ICertificateStore certificateStore)
        {
            _applicantStore = applicantStore;
            _accountStore = accountStore;
            _lecManager = lecManager;
            _certificateStore = certificateStore;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // /api/certificate/abc.com?refresh_days=30
            var urlPattern = new Regex(@"^/api/certificate/(?<domain>[a-z0-9\-_\.]+)", RegexOptions.IgnoreCase);
            var match = urlPattern.Match(context.Request.Path.ToString().ToLower());
            if (!match.Success)
            {
                await next(context);
                return;
            }

            var domain = match.Groups["domain"].Value;
            var refresh = context.Request.Query["refresh_days"];
            if (string.IsNullOrWhiteSpace(refresh) || !int.TryParse(refresh, out var refreshDays))
            {
                refreshDays = 60;
            }

            var applicant = await _applicantStore.RetrieveAsync(domain);
            if (applicant == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }

            context.Response.ContentType = "application/octet-stream";
            var certificate = await _certificateStore.RetrieveAsync(applicant.Domain);
            if (certificate == null || !IsValid(certificate, refreshDays))
            {
                certificate = await RequestNewCertificateAsync(applicant);
                await _certificateStore.SaveAsync(applicant.Domain, certificate);
            }

            CertExporter.Export(certificate, CertOutputType.Pem, context.Response.Body);
        }

        private async Task<IssuedCertificate> RequestNewCertificateAsync(CertificateApplicant applicant)
        {
            await InitLecManagerAccountAsync(applicant);

            var certificate = await _lecManager.RequestCertificateAsync(null, applicant.Domain, alternativeHostnames: null);
            await _certificateStore.SaveAsync(applicant.Domain, certificate);
            return certificate;
        }

        private async Task InitLecManagerAccountAsync(CertificateApplicant applicant)
        {
            var account = await _accountStore.RetrieveAsync(applicant.ContactEmail);
            if (account == null)
            {
                await _lecManager.InitializeAsync(account: null);
                account = await _lecManager.CreateAccountAsync(new[] {applicant.ContactEmail}, applicant.AcceptTos);
                await _accountStore.SaveAsync(applicant.ContactEmail, account);
            }
            
            // BUG: can _lecManager be initialized twice when an account is just created above?
            await _lecManager.InitializeAsync(account);
        }

        private bool IsValid(IssuedCertificate certificate, int refreshDays)
        {
            using (var pemStream = new MemoryStream(certificate.PemPublicKey))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, pemStream);
                return cert.IsValidNow && cert.NotAfter > DateTime.UtcNow.AddDays(refreshDays + 1);
            }
        }
    }
}