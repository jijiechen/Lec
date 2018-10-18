using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lec.Web.WebMiddleware
{
    internal class CertificateMiddleware: IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
}