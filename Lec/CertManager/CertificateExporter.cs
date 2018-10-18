using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;

namespace Lec.CertManager
{
    class CertificateExporter
    {
        public static void Export(IssuedCertificate certificate, CertOutputType outType, string filePath)
        {
            switch (outType)
            {
                case CertOutputType.Pfx:
                    ExportPfx(certificate, filePath);
                    break;
                case CertOutputType.Pem:
                    ExportPem(certificate, filePath);
                    break;
            }
        }
  
        static void ExportPfx(IssuedCertificate certificate, string filePath)
        {
            using (var crtStream = new MemoryStream(certificate.PublicKey))
            using (var pfxStream = File.Create(filePath))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = ToKey(certificate.PrivateKey);
                
                CertHelper.ExportArchive(key, new[] { cert }, ArchiveFormat.PKCS12, pfxStream);
            }
        }

        static void ExportPem(IssuedCertificate certificate, string filePath)
        {
            using (var crtStream = new MemoryStream(certificate.PublicKey))
            using (var pemStream = File.Create(filePath))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = ToKey(certificate.PrivateKey);
                
                CertHelper.ExportCertificate(cert, EncodingFormat.PEM, pemStream);
                CertHelper.ExportPrivateKey(key, EncodingFormat.PEM, pemStream);
            }
        }
              
        static CertPrivateKey ToKey(byte[] privateKey)
        {
            using (var ms = new MemoryStream(privateKey))
            {
                return CertHelper.ImportPemPrivateKey(ms);
            }
        }

        

    }


    enum CertOutputType
    {
        Pfx,
        Pem
    }
}
