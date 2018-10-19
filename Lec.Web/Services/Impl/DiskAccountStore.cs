using System.IO;
using Lec.Acme.Models;
using Microsoft.Extensions.Options;

namespace Lec.Web.Services.Impl
{
    class DiskAccountStore: DiskJsonStoreBase<AcmeAccount>, IAccountStore
    {
        // todo: buggy
        
        private readonly LecStorageConfiguration _storeConfig;
        public DiskAccountStore(IOptions<LecStorageConfiguration> storeConfig)
        {
            _storeConfig = storeConfig.Value;
        }
        

        protected override string ComposeStorageFileName(string contact)
        {
            var fileName = contact.Replace("@", "_");
            var filePath = Path.Combine(_storeConfig.StorageBaseDirectory, fileName + ".json");
            return filePath;
        }
    }
}