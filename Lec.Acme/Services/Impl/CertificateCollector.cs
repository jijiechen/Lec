using System.Threading.Tasks;
using ACMESharp.Protocol;
using Lec.Acme.Models;
using Lec.Acme.Utilities;

namespace Lec.Acme.Services.Impl
{
    class CertificateCollector: ICertificateCollector
    {
        public async Task<IssuedCertificate> CollectCertificateAsync(AcmeProtocolClient client, OrderDetails order, CertificateRequest csr)
        {
            var updatedOrder = await client.FinalizeOrderAsync(order.Payload.Finalize, csr.DerCsr);
            return await TryRequestCertificate(csr, client, updatedOrder.OrderUrl);
        }

        private static async Task<IssuedCertificate> TryRequestCertificate(CertificateRequest csr, AcmeProtocolClient client, string orderUrl)
        {
            var updatedOrder = await AutoRetry.Start(
                async () => await client.GetOrderDetailsAsync(orderUrl),
                order => "valid" == order.Payload.Status &&  !string.IsNullOrEmpty(order.Payload.Certificate),
                3 * 1000,
                30);

            var certBytes = await client.GetOrderCertificateAsync(updatedOrder);
            return new IssuedCertificate
            {
                PemPrivateKey = csr.PemPrivateKey,
                PemPublicKey = certBytes
            };
        }

        
    }
}
