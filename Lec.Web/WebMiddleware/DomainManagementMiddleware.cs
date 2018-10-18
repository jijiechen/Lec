using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lec.Web.WebMiddleware
{
    internal class DomainManagementMiddleware: IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
}