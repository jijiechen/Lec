using System.IO;

namespace LeDNSCertStore.CertManager
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
            using (var crtStream = new MemoryStream(certificate.CertificateBytes))
            using (var pfxStream = File.Create(filePath))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = certificate.GetAsCertPrivateKey();
                
                CertHelper.ExportArchive(key, new[] { cert }, ArchiveFormat.PKCS12, pfxStream);
            }
        }

        static void ExportPem(IssuedCertificate certificate, string filePath)
        {
            using (var crtStream = new MemoryStream(certificate.CertificateBytes))
            using (var pemStream = File.Create(filePath))
            {
                var cert = CertHelper.ImportCertificate(EncodingFormat.PEM, crtStream);
                var key = certificate.GetAsCertPrivateKey();

                CertHelper.ExportCertificate(cert, EncodingFormat.PEM, pemStream);
                CertHelper.ExportPrivateKey(key, EncodingFormat.PEM, pemStream);
            }
        }

    }


    enum CertOutputType
    {
        Pfx,
        Pem
    }
}
