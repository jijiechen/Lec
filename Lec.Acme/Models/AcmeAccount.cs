using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;

namespace Lec.Acme.Models
{
    public class AcmeAccount
    {
        public AccountDetails Account { get; set; }
        public IJwsTool Signer { get; set; }
    }
}