using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Lec.Web
{
    class Program
    {
        static void Main()
        {
            new WebHostBuilder()
                .Configure(app => app.Run(async context => { await context.Response.WriteAsync("Lec is started!"); }))
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}