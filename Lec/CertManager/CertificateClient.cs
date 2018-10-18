using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Protocol;
using PKISharp.SimplePKI;

namespace Lec.CertManager
{
    class CertificateClient
    {
        public static async Task<IssuedCertificate> RequestCertificate(AcmeProtocolClient client, OrderDetails order, string hostName, List<string> alternativeNames = null)
        {
            if (alternativeNames == null)
            {
                alternativeNames = new List<string>();
            }

            var csr = GenerateCSR(hostName, alternativeNames, out PkiKey privateKey);
            var updatedOrder = await client.FinalizeOrderAsync(order.Payload.Finalize, csr);
            

            return await TryRequestCertificate(privateKey, client, updatedOrder.OrderUrl);
        }

        private static async Task<IssuedCertificate> TryRequestCertificate(PkiKey privateKey, AcmeProtocolClient client, string orderUrl)
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
                PrivateKey = privateKey.Export(PkiEncodingFormat.Pem),
                PublicKey = certBytes
            };
        }

        static byte[] GenerateCSR(string hostName, List<string> alternativeNames, out PkiKey privateKey)
        { 
            var keys = PkiKeyPair.GenerateRsaKeyPair(Program.GlobalConfiguration.RSAKeyBits);
            privateKey = keys.PrivateKey;
            
            var csr = new PkiCertificateSigningRequest($"cn={hostName}", keys, PkiHashAlgorithm.Sha256);
            if (alternativeNames.Count > 0)
            {
                csr.CertificateExtensions.Add(PkiCertificateExtension.CreateDnsSubjectAlternativeNames(alternativeNames));
            }
            return csr.ExportSigningRequest(PkiEncodingFormat.Der);
        }
    }
}
