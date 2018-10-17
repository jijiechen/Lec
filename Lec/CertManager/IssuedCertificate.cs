using System.IO;
using PKISharp.SimplePKI;

namespace Lec.CertManager
{
   
    
    class IssuedCertificate
    {
        
        public byte[] CertificateBytes { get; set; }
        
        public PkiKey CAPublicKey { get; set; }
        
        
        
        public CertPrivateKey GetAsCertPrivateKey()
        {
            using (var ms = new MemoryStream(CertificateBytes))
            {
                ms.Seek(0, SeekOrigin.Begin);
                return CertHelper.ImportPemPrivateKey(ms);
            }
        }
    }

}