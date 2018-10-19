using System.Threading.Tasks;
using Lec.Acme.Models;

namespace Lec.Web.Services
{
    interface IAccountStore
    {
        Task<AcmeAccount> RetrieveAsync(string contactEmail);
        Task SaveAsync(string contactEmail, AcmeAccount account);
    }
}