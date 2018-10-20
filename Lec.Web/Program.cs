using System.IO;
using Lec.Acme;
using Lec.Web.Services;
using Lec.Web.Services.Impl;
using Lec.Web.WebMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lec.Web
{
    class Program
    {
        static void Main()
        {
            new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLecAcme();
                    services.AddScoped<IAccountStore, DiskAccountStore>();
                    services.AddScoped<ICertificateApplicantStore, DiskApplicantStore>();
                    services.AddScoped<ICertificateStore, DiskCertificateStore>();
                    services.Configure<LecStorageConfiguration>(option =>
                    {
                        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "lec-data");
                        if (!Directory.Exists(dataDir))
                        {
                            Directory.CreateDirectory(dataDir);
                        }
                        
                        option.StorageBaseDirectory = dataDir;
                    });
                    
                    services.AddScoped<ErrorHandlingMiddleware>();
                    services.AddScoped<StaticFileMiddleware>();
                    services.AddScoped<DomainManagementMiddleware>();
                    services.AddScoped<CertificateMiddleware>();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<ErrorHandlingMiddleware>();
                    app.UseMiddleware<StaticFileMiddleware>();
                    app.UseMiddleware<DomainManagementMiddleware>();
                    app.UseMiddleware<CertificateMiddleware>();
                })
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}