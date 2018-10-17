using System;
using System.Net;
using System.Reflection;
using ACMESharp.Protocol;
using LeDNSCertStore.CertManager;
using LeDNSCertStore.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace LeDNSCertStore
{
    class Program
    {
        public const string ApplicationName = "lec";
        public static CertManagerConfiguration GlobalConfiguration = new CertManagerConfiguration();

        static void Main(string[] args)
        {
            Console.Title = "lec";
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif

            var app = new CommandLineApplication
            {
                Name = ApplicationName,
                FullName = "A central Let's Encrypt client that applies certificates using the DNS-01 challenge"
            };

            app.HelpOption("-?|-h|--help");
            app.VersionOption("--version", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            // Show help information if no subcommand/option was specified
            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 9;
            });

            app.Command("reg", new RegisterAccountCommand().Setup);
            app.Command("apply", new RequestCertificateCommand().Setup);

            app.Execute(args);
        }
        
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine();
            ConsoleUtils.ConsoleErrorOutput("!!! Unhandled exception has occured,  application is now exiting. !!!");
            var exception = e.ExceptionObject as Exception;
            if (exception == null)
            {
                Environment.Exit(999);
                return;
            }
            
            var exceptionPrinted = false;
            var acmeException = exception as AcmeProtocolException;
            var webException = exception as WebException;
            if (acmeException != null)
            {
                exceptionPrinted = true;
                ConsoleUtils.ConsoleErrorOutput($"type: {acmeException.ProblemTypeRaw}, status: {acmeException.ProblemStatus}");
                ConsoleUtils.ConsoleErrorOutput(acmeException.ProblemDetail);
                ConsoleUtils.ConsoleErrorOutput(acmeException.Message);
            }
            else if (webException != null)
            {
                exceptionPrinted = true;
                ConsoleUtils.ConsoleErrorOutput($"{webException.Status}: {webException.Message}");
            }

            if (!exceptionPrinted)
            {
                ConsoleUtils.ConsoleErrorOutput(exception.Message);
                ConsoleUtils.ConsoleErrorOutput(exception.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
