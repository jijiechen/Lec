using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACMESharp.Authorizations;
using ACMESharp.Protocol;
using ACMESharp.Protocol.Resources;
using Lec.Acme.DnsProviders;
using Lec.Acme.Utilities;

namespace Lec.Acme.Services.Impl
{
    class DnsAuthorizer: IDnsAuthorizer
    {
        public async Task AuthorizeAsync(AcmeProtocolClient client, OrderDetails order, IDnsProvider dnsProvider)
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
            await client.AnswerChallengeAsync(challenge.Url);

            var maxTry = 30;
            var trySleep = 3 * 1000;
            
            for (var tryCount = 0; tryCount < maxTry; ++tryCount)
            {
                if (tryCount > 0)
                {
                    Thread.Sleep(trySleep);
                }
               
                var latest = await client.GetChallengeDetailsAsync(challenge.Url);
                if ("valid" == latest.Status)
                {
                    break;
                }

                if ("pending" != latest.Status)
                {
                    throw new InvalidOperationException("Unexpected status for answered Challenge: " + latest.Status);
                }
            }
            
            // todo: Extract a common auto retry
        }

        private static async Task ApplyDnsRecordAsync(Challenge challenge, Authorization auth, AcmeProtocolClient client, IDnsProvider dnsProvider)
        {
            var dnsChallenge = AuthorizationDecoder.ResolveChallengeForDns01(auth, challenge, client.Signer);
            var txtRecord = await AddRecordToDnsAsync(dnsProvider, dnsChallenge);

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                var record = await DnsUtil.LookupRecordAsync("TXT", dnsChallenge.DnsRecordName);
                if (record != null)
                {
                    break;
                }
            }
            
            // todo: 1. Remove record after auth
            // todo: 2. Break the above while(true) to prevent infinite loops
        }

        static async Task<string> AddRecordToDnsAsync(IDnsProvider dnsProvider, Dns01ChallengeValidationDetails dnsChallenge)
        {
            return await Task.Factory.StartNew(() => 
                dnsProvider.AddTxtRecord(dnsChallenge.DnsRecordName, 
                                        dnsChallenge.DnsRecordValue));
        }

    }
}
