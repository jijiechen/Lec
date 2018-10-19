using System.Threading.Tasks;
using Lec.Acme.Models;

namespace Lec.Web.Services
{
    internal interface ICertificateStore
    {
        Task<IssuedCertificate> RetrieveAsync(string hostname);
        Task SaveAsync(string hostname, IssuedCertificate certificate);
    }
}