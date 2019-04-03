using System;
using System.Collections.Generic;
using Lec.Acme.DnsProviders;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Rest.Azure.Authentication;
using TxtRecord = Microsoft.Azure.Management.Dns.Models.TxtRecord;

namespace Lec.DnsProviders.BuiltinProviders
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
        DnsManagementClient _dnsClient;
        string _resourceGroupName;
        string _dnsZoneName;

        public void Initialize(string configuration)
        {
            string tenantId, clientId, clientKey, subscriptionId;
            ParseConfiguration(configuration, out tenantId, out clientId, out clientKey, out subscriptionId);

            try
            {
                var serviceCreds = ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientKey).Result;
                _dnsClient = new DnsManagementClient(serviceCreds) { SubscriptionId = subscriptionId };
                var zone = _dnsClient.Zones.GetWithHttpMessagesAsync(_resourceGroupName, _dnsZoneName).Result.Body;
                if (zone == null)
                {
                    throw new Exception($"DNS Zone not found: '{_dnsZoneName}'");
                }
            }
            catch (Exception authExeception)
            {
                throw new Exception($"Failed to connect to Microsoft Azure DNS with resource group '{_resourceGroupName}' and zone name '{_dnsZoneName}'", authExeception);
            }
        }

        public string AddTxtRecord(string name, string value)
        {
            var recordSetParams = new RecordSet
            {
                TTL = 10,
                TxtRecords = new List<TxtRecord>
                {
                    new TxtRecord(new List<string> { value })
                },
                Metadata = new Dictionary<string, string>
                {
                    {"created_by", "Lec"},
                    {"created_at_utc", DateTime.UtcNow.ToString("o")}
                }
            };

            if (name.EndsWith(_dnsZoneName, StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - _dnsZoneName.Length - 1);
            }
            
            var recordSet = _dnsClient.RecordSets.CreateOrUpdateAsync(
                _resourceGroupName,
                _dnsZoneName,
                name,
                RecordType.TXT,
                recordSetParams).Result;

            return recordSet.Name;
        }

        public void RemoveTxtRecord(string recordRef)
        {
            _dnsClient.RecordSets.Delete(
                _resourceGroupName,
                _dnsZoneName,
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

            _resourceGroupName = conf[confkey_resource_group];
            _dnsZoneName = conf[confkey_zone_name];
        }

        const string confkey_tenant_id = "tenant_id";
        const string confkey_client_id = "client_id";
        const string confkey_client_key = "client_key";

        const string confkey_subscription_id = "subscription_id";

        const string confkey_resource_group = "resource_group";
        const string confkey_zone_name = "zone_name";

        void IDisposable.Dispose()
        {
            _dnsClient.Dispose();
        }
    }
}
