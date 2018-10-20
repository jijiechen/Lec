using System;
using System.IO;
using ACMESharp.Crypto.JOSE;
using Lec.Acme.Models;
using Microsoft.Extensions.Options;

namespace Lec.Web.Services.Impl
{
    class DiskAccountStore: DiskJsonStoreBase<AcmeAccount>, IAccountStore
    {
        
        private readonly LecStorageConfiguration _storeConfig;
        public DiskAccountStore(IOptions<LecStorageConfiguration> storeConfig)
        {
            _storeConfig = storeConfig.Value;
        }
        
        
        protected override byte[] EntityToBytes(AcmeAccount entity)
        {
            entity.JwsAlg = entity.Signer.JwsAlg;
            entity.SignerString = entity.Signer.Export();
            return base.EntityToBytes(entity);
        }

        protected override AcmeAccount BytesToEntity(byte[] bytes)
        {
            var entity = base.BytesToEntity(bytes);
            entity.Signer = GenerateTool(entity.JwsAlg, entity.SignerString);
            entity.SignerString = null;

            return entity;
        }

        protected override string ComposeStorageFilePath(string contact)
        {
            var fileName = contact.Replace("@", "_");
            var filePath = Path.Combine(_storeConfig.StorageBaseDirectory, fileName + ".json");
            return filePath;
        }
        
        
        private static IJwsTool GenerateTool(string keyType, string json)
        {
            if (keyType.StartsWith("ES"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
                tool.HashSize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(json);
                return tool;
            }

            if (keyType.StartsWith("RS"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.RSJwsTool();
                tool.KeySize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(json);
                return tool;
            }

            throw new Exception($"Unknown or unsupported KeyType [{keyType}]");
        }
    }
}