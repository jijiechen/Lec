using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;

namespace Lec.Acme.Utilities
{
    public static class DnsUtil
    {
        private static LookupClient _dnsClient;

        public static string[] DnsServers { get; set; }

        public static LookupClient DnsClient
        {
            get
            {
                if (_dnsClient == null)
                {
                    lock (typeof(DnsUtil))
                    {
                        if (_dnsClient == null)
                        {
                            if (DnsServers?.Length > 0)
                            {
                                var nameServers = DnsServers.SelectMany(x => Dns.GetHostAddresses(x)).ToArray();
                                _dnsClient = new DnsClient.LookupClient(nameServers);
                            }
                            else
                            {
                                _dnsClient = new DnsClient.LookupClient();
                            }
                        }
                    }
                }
                return _dnsClient;
            }
        }
        
        public static async Task<IEnumerable<string>> LookupRecordAsync(string type, string name)
        {
            IDnsQueryResponse dnsResp = null;
            try
            {
                var dnsType = (DnsClient.QueryType) Enum.Parse(typeof(DnsClient.QueryType), type);
                dnsResp = await DnsClient.QueryAsync(name, dnsType);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("DNS lookup unexpected error:  " + ex.ToString());
                return null;
            }

            if (dnsResp != null && dnsResp.HasError)
            {
                if ("Non-Existent Domain".Equals(dnsResp.ErrorMessage,StringComparison.OrdinalIgnoreCase))
                    return null;
                
                throw new Exception("DNS lookup failed:  " + dnsResp.ErrorMessage);
            }

            return dnsResp == null ? new string[0] : dnsResp.AllRecords.SelectMany(x => x.ValueAsStrings());
        }

        public static IEnumerable<string> ValueAsStrings(this DnsResourceRecord drr)
        {
            switch (drr)
            {
                case TxtRecord txt:
                    return txt.Text;
                default:
                    return new[] { drr.ToString() };
            }
        }
    }
}
