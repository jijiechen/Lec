using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;

namespace Lec.Acme.Services
{
    public interface IAcmeClientFactory
    {
        Task<AcmeProtocolClient> CreateAcmeClientAsync(LecAcmeConfiguration configuration, AccountDetails account, IJwsTool signer);
    }
}