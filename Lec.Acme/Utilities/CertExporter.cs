using System.IO;
using Lec.Acme.Models;

namespace Lec.Acme.Utilities
{
    public class CertExporter
    {
        public static void Export(IssuedCertificate certificate, CertOutputType outType, Stream outputStream)
        {
            switch (outType)
            {
                case CertOutputType.Pfx:
                    ExportPfx(certificate, outputStream);
                    break;
                case CertOutputType.Pem:
                    ExportPem(certificate, outputStream);
                    break;
            }
        }

        static void ExportPfx(IssuedCertificate certificate, Stream outputStream)
        {
            using (var crtStream = new MemoryStream(certificate.PemPublicKey))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = ToKey(certificate.PemPrivateKey);

                CertHelper.ExportArchive(key, new[] {cert}, ArchiveFormat.PKCS12, outputStream);
            }
        }

        static void ExportPem(IssuedCertificate certificate, Stream outputStream)
        {
            using (var crtStream = new MemoryStream(certificate.PemPublicKey))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = ToKey(certificate.PemPrivateKey);

                CertHelper.ExportCertificate(cert, EncodingFormat.PEM, outputStream);
                CertHelper.ExportPrivateKey(key, EncodingFormat.PEM, outputStream);
            }
            
            // todo: Copy received certificate directly as public key
        }
              
        static CertPrivateKey ToKey(byte[] privateKey)
        {
            using (var ms = new MemoryStream(privateKey))
            {
                return CertHelper.ImportPemPrivateKey(ms);
            }
        }

        

    }


    public enum CertOutputType
    {
        Pfx,
        Pem
    }
}
