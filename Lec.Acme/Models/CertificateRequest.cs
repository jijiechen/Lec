namespace Lec.Acme.Models
{
    public class CertificateRequest
    {
        public byte[] DerCsr { get; set; }
        public byte[] PemPrivateKey { get; set; }
    }
}