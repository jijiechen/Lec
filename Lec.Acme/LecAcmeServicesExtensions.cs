using Lec.Acme.Services;
using Lec.Acme.Services.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lec.Acme
{
    public static class LecAcmeServicesExtensions
    {
        public static IServiceCollection AddLecAcme(this IServiceCollection services)
        {
            services.TryAddSingleton<IDnsAuthorizer, DnsAuthorizer>();
            services.TryAddSingleton<ICsrGenerator, CsrGenerator>();
            services.TryAddSingleton<ICertificateCollector, CertificateCollector>();
            services.TryAddSingleton<IAcmeClientFactory, AcmeClientFactory>();
            services.Configure<LecAcmeConfiguration>(config => { });
            services.AddOptions();
            
            services.TryAddTransient<ILecManager, LecManager>();

            return services;
        }
    }
}