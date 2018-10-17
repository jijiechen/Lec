using System;
using System.Collections.Generic;
using System.Linq;

namespace LeDNSCertStore
{
    public class KVConfigurationParser
    {
        public static Dictionary<string, string> Parse(string configuration, string[] requiredConfKeys = null)
        {
            var encoderReplacement = string.Format("${0}$", Guid.NewGuid().ToString("N").Substring(28));

            var conf = configuration
                        .Replace(";;", encoderReplacement)
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Replace(encoderReplacement, ";").Split('='))
                        .Select(parts => new KeyValuePair<string, string>(parts[0], string.Join("=", parts.Skip(1)) /* Values can contain '=' */ ))
                        .Aggregate(new Dictionary<string, string>(), (dic, item) => { dic[item.Key] = item.Value; return dic; });

            if (requiredConfKeys != null)
            {
                var missingConf = requiredConfKeys.FirstOrDefault(k => !conf.ContainsKey(k));
                if (missingConf != null)
                {
                    throw new DnsProviderMissingConfigurationException(missingConf);
                }
            }

            return conf;
        }
    }
}
