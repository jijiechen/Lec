using System.Threading.Tasks;
using ACMESharp.Protocol;
using Lec.Acme.Models;

namespace Lec.Acme.Services
{
    public interface ICertificateCollector
    {
        Task<IssuedCertificate> CollectCertificateAsync(AcmeProtocolClient client, OrderDetails order, CertificateRequest csr);
    }
}