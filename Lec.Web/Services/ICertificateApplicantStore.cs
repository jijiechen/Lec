using System.Threading.Tasks;
using Lec.Web.Models;

namespace Lec.Web.Services
{
    interface ICertificateApplicantStore
    {
        Task SaveAsync(string domain, CertificateApplicant applicant);
        Task<CertificateApplicant> RetrieveAsync(string domain);
    }
}