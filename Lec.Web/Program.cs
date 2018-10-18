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