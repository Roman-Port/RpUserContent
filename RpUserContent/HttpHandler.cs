using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RpUserContent
{
    public static class HttpHandler
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            Program.Log($"[Incoming request] {e.Request.Path}{e.Request.QueryString}", ConsoleColor.Blue);

            //Add content headers
            e.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            try
            {
                if (e.Request.Path == "/upload")
                {
                    return Services.UploadService.OnHttpRequest(e);
                }
                if (e.Request.Path == "/upload_token")
                {
                    return Services.UploadTokenService.OnHttpRequest(e);
                }
                if (e.Request.Path.ToString().StartsWith("/u/"))
                {
                    return Services.ImageRequestService.OnHttpRequest(e, e.Request.Path.ToString().Substring(3));
                }
                throw new StandardError("Not Found", StandardErrorType.NotFound);
            } catch (StandardError str)
            {
                Program.Log("[Request Failed] Error: " + str.screen_error, ConsoleColor.Red);
                return Program.QuickWriteJsonToDoc(e, str, 500);
            } catch (Exception ex)
            {
                Program.Log("[Request Failed] Error: " + ex.Message, ConsoleColor.Red);
                return Program.QuickWriteJsonToDoc(e, ex, 500);
            }
        }
    }
}
