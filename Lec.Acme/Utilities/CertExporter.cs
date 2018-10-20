using System;
using System.IO;
using System.Text;
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
            var lineBreakBytes = Encoding.ASCII.GetBytes("\n");
            var totalLength = certificate.PemPublicKey.Length + lineBreakBytes.Length + certificate.PemPrivateKey.Length;
            var combinedBytes = new byte[totalLength];
            
            Buffer.BlockCopy(certificate.PemPublicKey, 0, combinedBytes, 0, certificate.PemPublicKey.Length);
            Buffer.BlockCopy(lineBreakBytes, 0, combinedBytes, certificate.PemPublicKey.Length, lineBreakBytes.Length);
            Buffer.BlockCopy(certificate.PemPrivateKey, 0, combinedBytes, certificate.PemPublicKey.Length + lineBreakBytes.Length, certificate.PemPrivateKey.Length);

            outputStream.Write(combinedBytes, 0, totalLength);
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
