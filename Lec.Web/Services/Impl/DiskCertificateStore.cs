using System.IO;
using System.Linq;
using Lec.Acme.Models;
using Lec.Acme.Utilities;

namespace Lec.Web.Services.Impl
{
    class DiskCertificateStore: DiskStoreBase<IssuedCertificate>, ICertificateStore
    {
        protected override byte[] EntityToBytes(IssuedCertificate entity)
        {
            return entity.PemPublicKey
                .Concat(entity.PemPrivateKey)
                .ToArray();
        }

        protected override IssuedCertificate BytesToEntity(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                CertHelper.ImportCertificate(EncodingFormat.PEM, ms);
                // todo: buggy
                var pos = ms.Position;
               
                return new IssuedCertificate
                {
                    PemPublicKey = bytes.Take((int)pos).ToArray(),
                    PemPrivateKey = bytes.Skip((int)pos).ToArray()
                };
            }
        }

        protected override string ComposeStorageFileName(string hostname)
        {
            return hostname + ".pem";
        }
    }
}