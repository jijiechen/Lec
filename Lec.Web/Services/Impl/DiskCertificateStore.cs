using System.IO;
using System.Linq;
using System.Text;
using Lec.Acme.Models;
using Lec.Acme.Utilities;
using Microsoft.Extensions.Options;

namespace Lec.Web.Services.Impl
{
    class DiskCertificateStore: DiskStoreBase<IssuedCertificate>, ICertificateStore
    {
        private readonly LecStorageConfiguration _storeConfig;

        public DiskCertificateStore(IOptions<LecStorageConfiguration> storeConfig)
        {
            _storeConfig = storeConfig.Value;
        }

        protected override byte[] EntityToBytes(IssuedCertificate entity)
        {
            using (var ms = new MemoryStream())
            {
                CertExporter.Export(entity, CertOutputType.Pem, ms);
                return ms.GetBuffer();
            }
        }

        protected override IssuedCertificate BytesToEntity(byte[] bytes)
        {
            var pkHeader = "-----BEGIN RSA PRIVATE KEY-----";
            var pkHeaderBytes = Encoding.ASCII.GetBytes(pkHeader);
            var pkPos = lastIndexOfBytes(pkHeaderBytes, bytes);
           
            return new IssuedCertificate
            {
                PemPublicKey = bytes.Take((int)pkPos).ToArray(),
                PemPrivateKey = bytes.Skip((int)pkPos).ToArray()
            };
        }

        private long lastIndexOfBytes(byte[] pkHeaderBytes, byte[] bytes)
        {
            if (pkHeaderBytes == null
                || bytes == null
                || pkHeaderBytes.Length == 0 
                || bytes.Length < pkHeaderBytes.Length)
            {
                return -1;
            }

            var curIndex = bytes.Length - pkHeaderBytes.Length;
            do
            {
                var notMatching = pkHeaderBytes.Where((curByte, i) => bytes[curIndex + i] != curByte).Any();
                if (!notMatching)
                {
                    return curIndex;
                }
            } while (--curIndex >= 0);

            return -1;
        }

        protected override string ComposeStorageFilePath(string hostname)
        {
            var filename = hostname + ".pem";
            return Path.Combine(_storeConfig.StorageBaseDirectory, filename);
        }
    }
}