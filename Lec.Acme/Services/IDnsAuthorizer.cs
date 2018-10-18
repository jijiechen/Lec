using System.Threading.Tasks;
using ACMESharp.Protocol;
using Lec.Acme.DnsProviders;

namespace Lec.Acme.Services
{
    public interface IDnsAuthorizer
    {
        Task AuthorizeAsync(AcmeProtocolClient client, OrderDetails order, IDnsProvider dnsProvider);
    }
}