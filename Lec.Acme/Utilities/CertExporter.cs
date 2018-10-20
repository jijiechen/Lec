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
                var key = ToKey(certificate.PemPrivateKey);

                crtStream.CopyTo(outputStream);
                CertHelper.ExportPrivateKey(key, EncodingFormat.PEM, outputStream);
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


    public enum CertOutputType
    {
        Pfx,
        Pem
    }
}
