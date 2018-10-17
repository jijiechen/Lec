using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Authorizations;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;
using Lec.DnsProviders;

namespace Lec.CertManager
{
    class DnsAuthorizer
    {
        public static async Task Authorize(AcmeProtocolClient client, OrderDetails order, IDnsProvider dnsProvider)
        {
            var getAuthorizations = order.Payload.Authorizations.Select(x => client.GetAuthorizationDetailsAsync(x));
            var allAuthorizations = await Task.WhenAll(getAuthorizations);
            
            foreach (var auth in allAuthorizations)
            {
                var challengeTasks = auth.Challenges
                    .Where(x => x.Type == Dns01ChallengeValidationDetails.Dns01ChallengeType)
                    .Select(ch => AcceptDnsChallengeAsync(ch, auth, client, dnsProvider));

                await Task.WhenAll(challengeTasks);
            }
            
            var refreshAuthorizations = order.Payload.Authorizations.Select(x => client.GetAuthorizationDetailsAsync(x));
            var authResults = await Task.WhenAll(refreshAuthorizations);

            if (authResults.Any(result => result.Status != "valid"))
            {
                throw new AuthorizationFailedException(authResults);
            }
        }

        static async Task AcceptDnsChallengeAsync(Challenge challenge, Authorization auth, AcmeProtocolClient client, IDnsProvider dnsProvider)
        {
            await ApplyDnsRecordAsync(challenge, auth, client, dnsProvider);


            var maxTry = 30;
            var trySleep = 3 * 1000;
            
            for (var tryCount = 0; tryCount < maxTry; ++tryCount)
            {
                if (tryCount > 0)
                {
                    Thread.Sleep(trySleep);
                }
               
                var latestStage = await client.GetChallengeDetailsAsync(challenge.Url);
                if ("valid" == latestStage.Status)
                {
                    break;
                }

                if ("pending" != latestStage.Status)
                {
                    throw new InvalidOperationException("Unexpected status for answered Challenge: " + latestStage.Status);
                }
            }
        }

        private static async Task ApplyDnsRecordAsync(Challenge challenge, Authorization auth, AcmeProtocolClient client, IDnsProvider dnsProvider)
        {
            var dnsChallenge = AuthorizationDecoder.ResolveChallengeForDns01(auth, challenge, client.Signer);
            var txtRecord = await AddRecordToDnsAsync(dnsProvider, dnsChallenge);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        static async Task<string> AddRecordToDnsAsync(IDnsProvider dnsProvider, Dns01ChallengeValidationDetails dnsChallenge)
        {
           
            var dnsName = dnsChallenge.DnsRecordName;
            var dnsValue = Regex.Replace(dnsChallenge.DnsRecordValue, "\\s", "");
            
            return await Task.Factory.StartNew(() => dnsProvider.AddTxtRecord(dnsName, dnsValue));
        }
    }
}
