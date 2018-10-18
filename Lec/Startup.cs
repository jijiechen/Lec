using System.Reflection;
using Lec.Acme;
using Lec.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Lec
{
    static class Startup
    {
        static IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLecAcme();
            serviceCollection.AddSingleton<RegisterAccountCommand>();
            serviceCollection.AddSingleton<RequestCertificateCommand>();

            return serviceCollection;
        }
        
        internal static void RunApp(string appName, string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = appName,
                FullName = "A central Let's Encrypt client that applies certificates using the DNS-01 challenge"
            };

            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            var services = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
            using (services)
            {
                // Show help information if no subcommand/option was specified
                app.OnExecute(() =>
                {
                    app.ShowHelp();
                    return 9;
                });
                
                app.Command("reg", services.GetService<RegisterAccountCommand>().Setup);
                app.Command("apply", services.GetService<RequestCertificateCommand>().Setup);

                app.Execute(args);
            }
        }
    }
}