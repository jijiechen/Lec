using System.Collections.Generic;

namespace Lec.Web.Models
{
    class CertificateApplicant
    {
        public string ContactEmail { get; set; }
        public bool AcceptTos { get; set; }
            
        public string Domain { get; set; }
        public string DnsProvider { get; set; }
        public string DnsProviderConf { get; set; }
        public Dictionary<string, string> DnsProviderConfiguration { get; set; }
    }
}