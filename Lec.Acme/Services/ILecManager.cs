using System.Collections.Generic;
using System.Threading.Tasks;
using Lec.Acme.DnsProviders;
using Lec.Acme.Models;

namespace Lec.Acme.Services
{
    public interface ILecManager
    {
        Task InitializeAsync(AcmeAccount account);
        
        Task<AcmeAccount> CreateAccountAsync(IEnumerable<string> contacts, bool acceptTos);

        Task<IssuedCertificate> RequestCertificateAsync(IDnsProvider dnsProvider, string hostname, IEnumerable<string> alternativeHostnames);
    }
}