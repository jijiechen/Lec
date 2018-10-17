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

            var csr = GenerateCSR(hostName, alternativeNames);
            var updatedOrder = await client.FinalizeOrderAsync(order.Payload.Finalize, csr);
            

            return await TryRequestCertificate(client, updatedOrder.OrderUrl);
        }

        private static async Task<IssuedCertificate> TryRequestCertificate(AcmeProtocolClient client, string orderUrl)
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


                    updatedOrder = await client.GetOrderDetailsAsync(orderUrl);
                }

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
                CertificateBytes = certBytes
            };
        }

        static byte[] GenerateCSR(string hostName, List<string> alternativeNames)
        {   var keys = PkiKeyPair.GenerateRsaKeyPair(Program.GlobalConfiguration.RSAKeyBits);
//            privateKey = keys.PrivateKey;
            
            var csr = new PkiCertificateSigningRequest($"cn={hostName}", keys, PkiHashAlgorithm.Sha256);
            if (alternativeNames.Count > 0)
            {
                csr.CertificateExtensions.Add(PkiCertificateExtension.CreateDnsSubjectAlternativeNames(alternativeNames));
            }
            return csr.ExportSigningRequest(PkiEncodingFormat.Der);
        }

//        static Crt GetCACertificate(CertificateProvider cp, CertificateRequest certRequest)
//        {
//            var links = new LinkCollection(certRequest.Links);
//            var upLink = links.GetFirstOrDefault("up");
//
//            var temporaryFileName = Path.GetTempFileName();
//            try
//            {
//                var uri = new Uri(new Uri(Program.GlobalConfiguration.AcmeServerBaseUri), upLink.Uri);
//                TheWebClient.DownloadFile(uri, temporaryFileName);
//
//                var cacert = new X509Certificate2(temporaryFileName);
//                var serverSN = cacert.GetSerialNumberString();
//
//
//                using (Stream source = new FileStream(temporaryFileName, FileMode.Open))
//                {
//                    return cp.ImportCertificate(EncodingFormat.DER, source);
//                }
//            }
//            finally
//            {
//                if (File.Exists(temporaryFileName))
//                    File.Delete(temporaryFileName);
//            }
//        }

    }
}
