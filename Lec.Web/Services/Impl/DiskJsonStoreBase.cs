using System.Text;
using Newtonsoft.Json;

namespace Lec.Web.Services.Impl
{
    abstract class DiskJsonStoreBase<T> : DiskStoreBase<T> where T: class 
    {
        protected override byte[] EntityToBytes(T entity)
        {
            var content = JsonConvert.SerializeObject(entity);
            return Encoding.UTF8.GetBytes(content);
        }

        protected override T BytesToEntity(byte[] bytes)
        {
            var content = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}