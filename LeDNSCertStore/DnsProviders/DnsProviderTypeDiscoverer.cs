using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace LeDNSCertStore.DnsProviders
{
    class DnsProviderTypeDiscoverer
    {
        static readonly Regex ProviderFileNameRegex = new Regex(@"LeDNSCertStore\.DnsProviders\.(?<name>[^.]+)\.dll$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static Dictionary<string, Type> Discover()
        {
            var baseLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var providerFiles = Directory.GetFiles(baseLocation)
                .Select(filename => new {Filename = filename, Match = ProviderFileNameRegex.Match(filename)})
                .Where(file => file.Match.Success)
                .Select(file => new { FileName = file.Filename, DefaultProviderName = file.Match.Groups["name"].Value })
                .ToArray();

            var allProviderTypes = providerFiles
                .Select(file => new
                {
                    ProviderAssembly = LoadAssembly(file.FileName),
                    ProviderFile = file
                })
                .Where(item => item.ProviderAssembly != null)
                .Select(item => GetProviderTypes(item.ProviderAssembly, item.ProviderFile.DefaultProviderName))
                .ToList();

            var defaultDic = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            return allProviderTypes.Aggregate(defaultDic,
                (all, types) =>
                {
                    types.Keys
                        .ToList()
                        .ForEach(k =>
                        {
                            all[k] = types[k];
                        });

                    return all;
                });
        }

        static Dictionary<string, Type> GetProviderTypes(Assembly providerAssembly, string defaultProviderName)
        {
            var providerTypes = providerAssembly.GetExportedTypes()
                                .Where(type => type.IsPublic && !type.IsAbstract && !type.IsInterface)
                                .Where(type => typeof(IDnsProvider).IsAssignableFrom(type))
                                .ToList();
            var isTheOnlyProviderType = providerTypes.Count == 1;

            return providerTypes.Aggregate(new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase),
                (dic, type) =>
                {
                    var providerName = GetProviderName(type, isTheOnlyProviderType, defaultProviderName);
                    if (providerName != null)
                    {
                        dic[providerName] = type;
                    }

                    return dic;
                });
        }

        static string GetProviderName(Type providerType, bool isTheOnlyProviderType, string defaultProviderName)
        {
            var attr = providerType.GetCustomAttributes<DnsProviderAttribute>().FirstOrDefault();

            if (attr != null) return attr.Name;
            if (isTheOnlyProviderType) return defaultProviderName;

            Console.WriteLine($"There is no name defined for provider type '{providerType.FullName}'");
            return null;
        }

        static Assembly LoadAssembly(string filename)
        {
            try
            {
                return Assembly.LoadFile(filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can not load provider assembly '{filename}'.\r\nBecause of exception: {ex.Message}");
            }

            return null;
        }
    }
}