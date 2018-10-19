using System.IO;
using System.Net;
using System.Threading.Tasks;
using Lec.Web.Models;
using Lec.Web.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lec.Web.WebMiddleware
{
    internal class DomainManagementMiddleware: IMiddleware
    {
        private readonly ICertificateApplicantStore _applicantStore;
        public DomainManagementMiddleware(ICertificateApplicantStore applicantStore)
        {
            _applicantStore = applicantStore;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            const string domainApiPrefix = "/api/domains/";
            var matches = context.Request.Path.ToString().ToLower().StartsWith(domainApiPrefix) && context.Request.Method.Equals("POST");
            if (!matches)
            {
                await next(context);
                return;
            }

            string requestBody;
            using (var sr = new StreamReader(context.Request.Body))
            {
                requestBody = await sr.ReadToEndAsync();
            }

            var applicant = JsonConvert.DeserializeObject<CertificateApplicant>(requestBody);
            await _applicantStore.SaveAsync(applicant.Domain, applicant);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}