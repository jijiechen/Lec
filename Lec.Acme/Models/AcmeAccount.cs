using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
using Newtonsoft.Json;

namespace Lec.Acme.Models
{
    public class AcmeAccount
    {
        public AccountDetails Account { get; set; }
        public string SignerString { get; set; }
        public string JwsAlg { get; set; }
        
        [JsonIgnore]
        public IJwsTool Signer { get; set; }
    }
}