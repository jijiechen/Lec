using System.IO;
using Lec.Web.Models;
using Microsoft.Extensions.Options;

namespace Lec.Web.Services.Impl
{
    class DiskApplicantStore: DiskJsonStoreBase<CertificateApplicant>, ICertificateApplicantStore
    {
        private readonly LecStorageConfiguration _storeConfig;
        public DiskApplicantStore(IOptions<LecStorageConfiguration> storeConfig)
        {
            _storeConfig = storeConfig.Value;
        }
        
        protected override string ComposeStorageFilePath(string domain)
        {
            return Path.Combine(_storeConfig.StorageBaseDirectory, domain + ".json");
        }
    }
}