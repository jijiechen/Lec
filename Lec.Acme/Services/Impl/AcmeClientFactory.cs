using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;

namespace Lec.Acme.Services.Impl
{
    class AcmeClientFactory: IAcmeClientFactory
    {
        public async Task<AcmeProtocolClient> CreateAcmeClientAsync(LecAcmeConfiguration configuration, AccountDetails account = null, IJwsTool signer = null)
        {
            var http = CreateHttpClient(configuration);
            // 根据 Acme 要求，使用 Post-As-Get 功能
            // https://community.letsencrypt.org/t/acme-v2-scheduled-deprecation-of-unauthenticated-resource-gets/74380
            var client = new AcmeProtocolClient(http, null, account, signer, 
                false, null, true);
            
            client.Directory = await client.GetDirectoryAsync();
            await client.GetNonceAsync();
            
            return client;
        }

        private static HttpClient CreateHttpClient(LecAcmeConfiguration configuration)
        {
            var serverUri = new Uri(configuration.AcmeServerBaseUri);
            var proxyUri = configuration.ProxyUri;
            
            if (string.IsNullOrEmpty(proxyUri))
                return new HttpClient {BaseAddress = serverUri};
            
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new WebProxy(proxyUri,
                    false /* byPassOnLocal */,
                    new string[0] /* byPassList */,
                    new NetworkCredential(configuration.ProxyUserName,
                        configuration.ProxyPassword)),
                PreAuthenticate = true,
                UseDefaultCredentials = false,
            };

            return new HttpClient(httpClientHandler) {BaseAddress = serverUri};

        }
    }
}
