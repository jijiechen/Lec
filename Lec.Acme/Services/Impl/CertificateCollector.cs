using System;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Protocol;
using Lec.Acme.Models;

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
            int maxTry = 20;
            int trySleep = 3 * 1000;
            var valid = false;

            OrderDetails updatedOrder = null;

            for (var tryCount = 0; tryCount < maxTry; ++tryCount)
            {
                if (tryCount > 0)
                {
                    // Wait just a bit for
                    // subsequent queries
                    Thread.Sleep(trySleep);
                }
                
                updatedOrder = await client.GetOrderDetailsAsync(orderUrl);

                if (!valid)
                {
                    // The Order is either Valid, still Pending or some other UNEXPECTED state

                    if ("valid" == updatedOrder.Payload.Status)
                    {
                        valid = true;
                    }
                    else if ("pending" != updatedOrder.Payload.Status)
                    {
                        throw new InvalidOperationException("Unexpected status for Order: " + updatedOrder.Payload.Status);
                    }
                }

                if (valid)
                {
                    // Once it's valid, then we need to wait for the Cert

                    if (!string.IsNullOrEmpty(updatedOrder.Payload.Certificate))
                    {
                        break;
                    }
                }
            }
            

            var certBytes = await client.GetOrderCertificateAsync(updatedOrder);
            return new IssuedCertificate
            {
                PemPrivateKey = csr.PemPrivateKey,
                PemPublicKey = certBytes
            };
        }

        
    }
}
