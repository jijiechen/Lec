
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
namespace LeDNSCertStore.CertManager
{
    class ClientHelper
    {
        public static async Task<AcmeProtocolClient> CreateAcmeClient(AccountDetails account = null, IJwsTool signer = null)
        {
            var http = CreateHttpClient();
            var client = new AcmeProtocolClient(http, null, account, signer);
            
            await client.GetDirectoryAsync();
            await client.GetNonceAsync();
            
            return client;
        }

        private static HttpClient CreateHttpClient()
        {
            var serverUri = new Uri(Program.GlobalConfiguration.AcmeServerBaseUri);
            var proxyUri = Program.GlobalConfiguration.ProxyUri;
            
            if (string.IsNullOrEmpty(proxyUri))
                return new HttpClient {BaseAddress = serverUri};
            
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new WebProxy(proxyUri,
                    false /* byPassOnLocal */,
                    new string[0] /* byPassList */,
                    new NetworkCredential(Program.GlobalConfiguration.ProxyUserName,
                        Program.GlobalConfiguration.ProxyPassword)),
                PreAuthenticate = true,
                UseDefaultCredentials = false,
            };

            return new HttpClient(httpClientHandler) {BaseAddress = serverUri};

        }
    }
}
