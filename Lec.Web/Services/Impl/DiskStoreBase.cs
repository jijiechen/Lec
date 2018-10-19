using System.IO;
using System.Threading.Tasks;

namespace Lec.Web.Services.Impl
{
    abstract class DiskStoreBase<T> where T : class
    {
        public async Task SaveAsync(string id, T entity)
        {
            var filePath = ComposeStorageFileName(id);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);   
            }

            await File.WriteAllBytesAsync(filePath, EntityToBytes(entity));
        }

        public async Task<T> RetrieveAsync(string id)
        {
            var filePath = ComposeStorageFileName(id);
            if (!File.Exists(filePath))
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(filePath);
            return BytesToEntity(bytes);
        }

        protected abstract string ComposeStorageFileName(string id);
        protected abstract byte[] EntityToBytes(T entity);
        protected abstract T BytesToEntity(byte[] bytes);
    }
}