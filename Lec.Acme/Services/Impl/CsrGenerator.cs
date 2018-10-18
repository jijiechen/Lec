using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lec.Acme.Models;
using PKISharp.SimplePKI;

namespace Lec.Acme.Services.Impl
{
    class CsrGenerator: ICsrGenerator
    {
        public async Task<CertificateRequest> GenerateCsrAsync(short privateKeyBitLength, string hostName, IEnumerable<string> alternativeNames)
        {
            var createCsrTask = Task.Factory.StartNew(() => PkiKeyPair.GenerateRsaKeyPair(privateKeyBitLength));
            var keys = await createCsrTask.ConfigureAwait(false);
            
            var csr = new PkiCertificateSigningRequest($"cn={hostName}", keys, PkiHashAlgorithm.Sha256);
            var atns = alternativeNames == null ? new List<string>() : alternativeNames.ToList(); 
            if (atns.Count > 0)
            {
                csr.CertificateExtensions.Add(PkiCertificateExtension.CreateDnsSubjectAlternativeNames(atns));
            }

            return new CertificateRequest
            {
                DerCsr = csr.ExportSigningRequest(PkiEncodingFormat.Der),
                PemPrivateKey = keys.PrivateKey.Export(PkiEncodingFormat.Pem)
            };
            
        }
    }
}