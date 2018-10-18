using Lec.CertManager;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Protocol;
using Lec.Acme.DnsProviders;
using Lec.Acme.Models;
using Lec.Acme.Services;
using Lec.Acme.Utilities;
using static Lec.ConsoleUtils;
using static Lec.PathUtils;

namespace Lec.Commands
{
    class RequestCertificateCommand
    {
        private readonly ILecManager _lecManager;
        public RequestCertificateCommand(ILecManager lecManager)
        {
            _lecManager = lecManager;
        }
        
        static Dictionary<string, Type> _allSupportedDnsProviderTypes = null;

        public void Setup(CommandLineApplication command)
        {
            command.Description = "Request a new certificate from the open Let's Encrypt CA.";

            var argCN = command.Argument("cn", "Common name of the certificate");

            var optionOutFile = command.Option("-o|--out <OUT_FILE>", "The output file path to which the issued certificate file generate.", CommandOptionType.SingleValue);
            var optionOutType = command.Option("-t|--out-type <OUT_TYPE>", "The file type to export from the issued certificate.", CommandOptionType.SingleValue);

            var optionReg = command.Option("--reg <REG_FILE>", "The file that contains the registeration that will be used to request the certificate.", CommandOptionType.SingleValue);
            var optionSigner = command.Option("--signer <SIGNER_FILE>", "The signer correspondes to the registeration file.", CommandOptionType.SingleValue);

            var optionDnsName = command.Option("--dns <DNS_PROVIDER_NAME>", "The provider program name of your dynamic dns service provider.", CommandOptionType.SingleValue);
            var optionDnsConf = command.Option("--dns-conf <DNS_PROVIDER_CONFIGURATION>", "Configuration string to initialize the DNS provider program.", CommandOptionType.SingleValue);


            command.HelpOption("-?|-h|--help");
            command.OnExecute(() =>
            {
                var opt = new RequestNewCertificateOptions
                {
                    CommonName = argCN.Value?.Trim(),
                    OutputFile = optionOutFile.Value()?.Trim(),

                    RegisterationFile = optionReg.Value()?.Trim(),
                    SignerFile = optionSigner.Value()?.Trim(),

                    DnsProviderName = optionDnsName.Value()?.Trim(),
                    DnsProviderConfiguration = optionDnsConf.Value()?.Trim()
                };

                CertOutputType outType;
                if(Enum.TryParse(optionOutType.Value(), out outType))
                {
                    opt.OutputType = outType;
                }

                if (_allSupportedDnsProviderTypes == null)
                {
                    _allSupportedDnsProviderTypes = DnsProviderTypeDiscoverer.Discover();
                }
                return Execute(opt);
            });
        }

        async Task<int> Execute(RequestNewCertificateOptions options)
        {
            int errorCode;
            if (OptionsInBadFormat(options, out errorCode))
            {
                return errorCode;
            }

            var requestContext = InitializeRequestContext(options);
            if (requestContext == null)
            {
                return 210;
            }

            
            Console.Write("Initializing...");
            await _lecManager.InitializeAsync(new AcmeAccount
            {
                Account = requestContext.Account,
                Signer = requestContext.Signer
            });
            Console.WriteLine("Done.");

            try
            {
                Console.Write("Requesting a new certificate for common name {0}...", options.CommonName);
                var issuedCertificate = await _lecManager.RequestCertificateAsync(requestContext.DnsProvider, options.CommonName, Enumerable.Empty<string>() /* no other hostnames */);
                Console.WriteLine("Done.");
                
                Console.WriteLine("Exporting certificate to file...");
                var outTypeString = options.OutputType.ToString().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(options.OutputFile))
                {
                    options.OutputFile = Path.Combine(AppliationPath, string.Concat(options.CommonName, '-', DateTime.Now.ToString("yyyyMMddHHmm"), '.', outTypeString));
                }

                options.OutputFile = PrepareOutputFilePath(options.OutputFile, out _);
                using (var file = File.Create(options.OutputFile))
                {
                    CertExporter.Export(issuedCertificate, options.OutputType, file);
                }
                Console.WriteLine("Certificate has been exported as {0} format at {1}.", outTypeString, options.OutputFile); 
            }
            finally
            {
                requestContext.Signer.Dispose();
                requestContext.DnsProvider.Dispose();
            }

            return 0;
        }

        CertRequestContext InitializeRequestContext(RequestNewCertificateOptions options)
        {
            var context = new CertRequestContext();

            try
            {
                context.Account = AccountPersistence.LoadFromFile(options.RegisterationFile);
            }catch(Exception ex)
            {
                ConsoleErrorOutput($"Could not load registration file: {ex.Message}");
                goto errorHandling;
            }

            try
            {
                context.Signer = SignerHelper.LoadFromFile(options.SignerFile);
            }
            catch (Exception ex)
            {
                ConsoleErrorOutput($"Could not load signer file: {ex.Message}");
                goto errorHandling;
            }
            

            try
            {
                var dnsProviderType = _allSupportedDnsProviderTypes[options.DnsProviderName];
                context.DnsProvider = Activator.CreateInstance(dnsProviderType) as IDnsProvider;
                context.DnsProvider?.Initialize(options.DnsProviderConfiguration ?? string.Empty);
            }
            catch(Exception ex)
            {
                ConsoleErrorOutput($"Could not initialize dns provider: {ex.Message}");
                goto errorHandling;
            }
            return context;

            errorHandling:
            return null;
        }

        static bool OptionsInBadFormat(RequestNewCertificateOptions options, out int exitCode)
        {
            if (string.IsNullOrEmpty(options.CommonName))
            {
                ConsoleErrorOutput("Could not request a certificate without a common name.");
                exitCode = 21;
                return true;
            }

            if (!File.Exists(options.RegisterationFile))
            {
                ConsoleErrorOutput($"Registeration file does not exist at {options.RegisterationFile}.");
                exitCode = 22;
                return true;
            }


            if (!File.Exists(options.SignerFile))
            {
                ConsoleErrorOutput($"Signer file does not exist at {options.SignerFile}.");
                exitCode = 23;
                return true;
            }

            if (!_allSupportedDnsProviderTypes.ContainsKey(options.DnsProviderName))
            {
                var allKeys = string.Join(",", _allSupportedDnsProviderTypes.Keys);
                ConsoleErrorOutput($"Unknown DNS provider '{options.DnsProviderName}'. The supported providers are: {allKeys}");
                exitCode = 24;
                return true;
            }
            
            exitCode = 0;
            return false;
        }      

        static bool IsSubDomainName(string domainName, out string toplevel)
        {
            var parts = domainName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length > 2)
            {
                toplevel = string.Concat(parts[parts.Length - 2], '.', parts[parts.Length - 1]);
                return true;
            }

            toplevel = domainName;
            return false;
        }

        class CertRequestContext
        {
            public IJwsTool Signer { get; set; }
            public AccountDetails Account { get; set; }
            public IDnsProvider DnsProvider { get; set; }
        }

        class RequestNewCertificateOptions
        {
            public string CommonName { get; set; }

            public CertOutputType OutputType { get; set; } = CertOutputType.Pem;
            public string OutputFile { get; set; }
            
            public string SignerFile { get; set; }
            public string RegisterationFile { get; set; }

            public string DnsProviderName { get; set; }
            public string DnsProviderConfiguration { get; set; }
        }

    }

}
