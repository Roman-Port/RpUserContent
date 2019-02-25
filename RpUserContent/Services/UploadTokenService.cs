using RpUserContent.PersistEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RpUserContent.Services
{
    public static class UploadTokenService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Get the upload token from the URL
            if (!e.Request.Query.ContainsKey("token"))
                throw new StandardError("Missing required argument 'token' in URL.", StandardErrorType.MissingArgs);
            string token = e.Request.Query["token"];
            if (!Program.fileCreationTokens.ContainsKey(token))
                throw new StandardError("This token is not valid.", StandardErrorType.BadAuth);
            UserContentFileEntry entry = Program.fileCreationTokens[token];

            //Respond with this token.
            Task t = Program.QuickWriteJsonToDoc(e, entry);

            //Remove token
            Program.fileCreationTokens.Remove(token);

            //Await
            return t;
        }
    }
}
