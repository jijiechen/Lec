using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Rest.Azure.Authentication;

namespace LetsEncryptCentral.DnsProviders.BuiltinProviders
{
    /// <summary>
    /// Manage DNS records using the Azure DNS
    /// </summary>
    /// <remarks>
    /// User should provide information to authenticate.
    /// Please see documentation at https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications 
    /// Or https://go.microsoft.com/fwlink/?LinkID=623000&clcid=0x409
    /// </remarks>
    [DnsProvider("Azure")]
    public class AzureProvider : IDnsProvider
    {
        DnsManagementClient dnsClient;
        string resourceGroupName;
        string dnszoneName;

        public void Initialize(string configuration)
        {
            string tenantId, clientId, clientKey, subscriptionId;
            ParseConfiguration(configuration, out tenantId, out clientId, out clientKey, out subscriptionId);

            try
            {
                var serviceCreds = ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientKey).Result;
                dnsClient = new DnsManagementClient(serviceCreds) { SubscriptionId = subscriptionId };
                var zone = dnsClient.Zones.GetWithHttpMessagesAsync(resourceGroupName, dnszoneName).Result.Body;
                if (zone == null)
                {
                    throw new Exception($"DNS Zone not found: '{dnszoneName}'");
                }
            }
            catch (Exception authExeception)
            {
                throw new Exception($"Failed to connect to Microsoft Azure DNS with resource group '{resourceGroupName}' and zone name '{dnszoneName}'", authExeception);
            }
        }

        public string AddTxtRecord(string name, string value)
        {
            var recordSetParams = new RecordSet
            {
                TTL = 10,
                TxtRecords = new List<TxtRecord>()
                {
                    new TxtRecord(new List<string> { value })
                },
                Metadata = new Dictionary<string, string>
                {
                    {"created_by", LetsEncryptCentral.Program.ApplicationName},
                    {"created_at_utc", DateTime.UtcNow.ToString("o")}
                }
            };

            var recordSet = dnsClient.RecordSets.CreateOrUpdateAsync(
                resourceGroupName,
                dnszoneName,
                name,
                RecordType.TXT,
                recordSetParams).Result;

            return recordSet.Name;
        }

        public void RemoveTxtRecord(string recordRef)
        {
            dnsClient.RecordSets.Delete(
                resourceGroupName,
                dnszoneName,
                recordRef,
                RecordType.TXT);
        }

        void ParseConfiguration(string configuration, out string tenantId, out string clientId, out string clientKey, out string subscriptionId)
        {
            var conf = KVConfigurationParser.Parse(
                configuration,
                new[]
                {
                    confkey_tenant_id,
                    confkey_client_id,
                    confkey_client_key,
                    confkey_subscription_id,
                    confkey_resource_group,
                    confkey_zone_name
                });

            tenantId = conf[confkey_tenant_id];
            clientId = conf[confkey_client_id];
            clientKey = conf[confkey_client_key];
            subscriptionId = conf[confkey_subscription_id];

            resourceGroupName = conf[confkey_resource_group];
            dnszoneName = conf[confkey_zone_name];
        }

        const string confkey_resource_group = "resource_group";
        const string confkey_zone_name = "zone_name";

        const string confkey_tenant_id = "tenant_id";
        const string confkey_client_id = "client_id";
        const string confkey_client_key = "client_key";

        const string confkey_subscription_id = "subscription_id";

        void IDisposable.Dispose()
        {
            dnsClient.Dispose();
        }
    }
}
