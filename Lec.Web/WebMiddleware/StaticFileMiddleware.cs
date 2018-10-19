using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lec.Web.WebMiddleware
{
    class StaticFileMiddleware: IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            const string staticPrefix = "/static/";
            var requestPath = context.Request.Path.ToString().ToLower();
            if (!requestPath.StartsWith(staticPrefix))
            {
                await next(context);
                return;
            }

            
            var subPath = requestPath.Substring(staticPrefix.Length).Replace("/", ".");
            var namespaceBase = typeof(Program).Namespace;
            var resName = $"{namespaceBase}.static.{subPath}";
            try
            {               
                using (var resStream = typeof(Program).Assembly.GetManifestResourceStream(resName))
                {
                    var mime = MapMimeType(subPath);
                    if (mime != null)
                    {
                        context.Response.ContentType = mime;
                    }
                    
                    context.Response.ContentLength = resStream.Length;
                    await resStream.CopyToAsync(context.Response.Body);
                }
            }
            catch (FileNotFoundException)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
            }
            catch (Exception)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
            }
        }

        static string MapMimeType(string path)
        {
            var dic = new Dictionary<string, string>()
            {
                {"html", "text/html"},
                {"txt", "plain/text"},
                {"css", "text/css"},
                {"js", "application/javascript"},
                {"json", "application/json"},
                {"png", "application/json"},
                {"jpg", "application/json"},
            };

            var ext = path.Substring(path.LastIndexOf('.') + 1);
            dic.TryGetValue(ext, out var mime);
            return mime;
        }
    }
}