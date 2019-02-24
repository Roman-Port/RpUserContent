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

            return Program.QuickWriteToDoc(e, "ok");
        }
    }
}
