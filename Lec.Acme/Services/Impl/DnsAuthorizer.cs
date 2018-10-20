using System.Linq;
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
            var dnsRecord = await ApplyDnsRecordAsync(challenge, auth, client, dnsProvider);
            await client.AnswerChallengeAsync(challenge.Url);

            await AutoRetry.Start(async () => await client.GetChallengeDetailsAsync(challenge.Url),
                latest => "valid" == latest.Status,
                millisecondsInterval: 4 * 1000,
                maxTry: 30);

            try
            {
                await RemoveRecordFromDnsAsync(dnsProvider, dnsRecord);
            }
            catch
            {
#if DEBUG
                throw;
#endif
                /* ignore this error in release mode */
            }
        }

        private static async Task<string> ApplyDnsRecordAsync(Challenge challenge, Authorization auth, AcmeProtocolClient client, IDnsProvider dnsProvider)
        {
            var dnsChallenge = AuthorizationDecoder.ResolveChallengeForDns01(auth, challenge, client.Signer);
            var txtRecord = await AddRecordToDnsAsync(dnsProvider, dnsChallenge);

            await AutoRetry.Start(
                async () => await DnsUtil.LookupRecordAsync("TXT", dnsChallenge.DnsRecordName),
                records => records != null && records.Any());

            return txtRecord;
        }

        static async Task<string> AddRecordToDnsAsync(IDnsProvider dnsProvider, Dns01ChallengeValidationDetails dnsChallenge)
        {
            return await Task.Factory.StartNew(() => 
                dnsProvider.AddTxtRecord(dnsChallenge.DnsRecordName, 
                                        dnsChallenge.DnsRecordValue));
        }
        
        static async Task RemoveRecordFromDnsAsync(IDnsProvider dnsProvider, string dnsRecordRef)
        {
            await Task.Factory.StartNew(() => dnsProvider.RemoveTxtRecord(dnsRecordRef));
        }

    }
}
