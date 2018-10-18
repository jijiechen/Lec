using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;
using ACMESharp.Crypto.JOSE.Impl;
using Lec.CertManager;
using static Lec.ConsoleUtils;
using static Lec.PathUtils;

namespace Lec.Commands
{
    class RegisterAccountCommand
    {
        public void Setup(CommandLineApplication command)
        {
            command.Description = "Create a new Let's Encrypt registration.";

            var optionTos = command.Option("--accept-tos", "Accept to the terms of services", CommandOptionType.NoValue);
            var optionContact = command.Option("-c|--contact <CONTACT_EMAIL>", "Email address to contact.", CommandOptionType.SingleValue);
            var optionOutReg = command.Option("-r|--out-reg <REGISTERTION_OUTPUT_FILE>", "A file path to output registeration information.", CommandOptionType.SingleValue);
            var optionOutSigner = command.Option("-s|--out-signer <REGISTERTION_OUTPUT_SIGNER>", "A file path to output signer information corresponds to the registeration.", CommandOptionType.SingleValue);
            
            command.HelpOption("-?|-h|--help");
            command.OnExecute(() =>
            {
                var opt = new RegisterCommandOptions
                {
                    AcceptTos = optionTos.HasValue(),
                    ContactEmailAddress = optionContact.Value(),
                    OutputPathRegisteration = optionOutReg.Value(),
                    OutputPathSigner = optionOutSigner.Value()
                };
                return Execute(opt);
            });
        }

        async Task<int> Execute(RegisterCommandOptions options)
        {
            if (!options.AcceptTos)
            {
                ConsoleErrorOutput("Could not create a registration before you accept the terms of services at https://letsencrypt.org/repository/.\r\nPlease specify --accept-tos option to accept the terms of services.");
                return 11;
            }
            UseDefaultOptionsIfNeed(ref options);
            Console.Write("Initializing...");


            var client = await ClientHelper.CreateAcmeClient();
            Console.WriteLine("Done.");
            Console.WriteLine("Requesting new registration for {0}...", options.ContactEmailAddress);
            
            
            var contacts = new[] { "mailto:" + options.ContactEmailAddress };
            var account = await client.CreateAccountAsync(contacts, true);
            
            var signer = client.Signer;
            AccountHelper.SaveToFile(account, options.OutputPathRegisteration);
            SignerHelper.SaveToFile((ESJwsTool)signer, options.OutputPathSigner);

            Console.WriteLine("Registration created for {0}.", options.ContactEmailAddress);
            Console.WriteLine("Registration profile saved at {0}.", options.OutputPathRegisteration);
            Console.WriteLine("Registration signer saved at {0}.", options.OutputPathSigner);

            return 0;
        }

        static void UseDefaultOptionsIfNeed(ref RegisterCommandOptions options)
        {
            var contactGuid = string.Concat(Program.ApplicationName, "user.");

            if (string.IsNullOrWhiteSpace(options.ContactEmailAddress))
            {
                contactGuid += Guid.NewGuid().ToString("n").Substring(15);
                options.ContactEmailAddress = string.Concat(contactGuid, "@example.com");
            }
            else
            {
                contactGuid += options.ContactEmailAddress.GetHashCode().ToString();
            }

            if (string.IsNullOrWhiteSpace(options.OutputPathRegisteration))
            {
                options.OutputPathRegisteration = Path.Combine(AppliationPath, contactGuid + ".reg.json");
            }
            else
            {
                string dir;
                options.OutputPathRegisteration = PrepareOutputFilePath(options.OutputPathRegisteration, out dir);
                if (string.IsNullOrWhiteSpace(options.OutputPathSigner))
                    options.OutputPathSigner = Path.Combine(dir, contactGuid + ".signer.key");
            }

            if (string.IsNullOrWhiteSpace(options.OutputPathSigner))
            {
                options.OutputPathSigner = Path.Combine(AppliationPath, contactGuid + ".signer.key");
            }
            else
            {
                string dir;
                options.OutputPathSigner = PrepareOutputFilePath(options.OutputPathSigner, out dir);
                if (string.IsNullOrWhiteSpace(options.OutputPathRegisteration))
                    options.OutputPathRegisteration = Path.Combine(dir, contactGuid + ".reg.json");
            }
        }

        class RegisterCommandOptions
        {
            public string ContactEmailAddress { get; set; }
            public string OutputPathRegisteration { get; set; }
            public string OutputPathSigner { get; set; }
            public bool AcceptTos { get; set; }
        }
    }
    
}
