using RpUserContent.PersistEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RpUserContent.Services
{
    public static class ImageRequestService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, string id)
        {
            //Find this in the uploaded collection.
            var collec = Program.GetFileEntryCollection();
            var f = collec.FindOne(x => x._id == id);
            if (f == null)
                throw new StandardError("Image not found.", StandardErrorType.NotFound);

            //Get the request method
            var method = Program.FindRequestMethod(e);

            //If this is a get request, just serve the image.
            if (method == RequestHttpMethod.get)
                return OnGetRequest(e, f);

            //Unknown method
            throw new StandardError("Unknown image request method.", StandardErrorType.NotFound);
        }

        static Task OnGetRequest(Microsoft.AspNetCore.Http.HttpContext e, UserContentFileEntry f)
        {
            //Open stream on file
            using(FileStream fs = new FileStream(Program.config.uploaded_content_path+f.filename, FileMode.Open))
            {
                e.Response.ContentLength = fs.Length;
                e.Response.ContentType = f.mimeType;
                return fs.CopyToAsync(e.Response.Body);
            }
        }
    }
}
