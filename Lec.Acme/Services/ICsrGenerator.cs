using System.Collections.Generic;
using System.Threading.Tasks;
using Lec.Acme.Models;

namespace Lec.Acme.Services
{
    public interface ICsrGenerator
    {
        Task<CertificateRequest> GenerateCsrAsync(short privateKeyBitLength, string hostName, IEnumerable<string> alternativeNames);
    }
}