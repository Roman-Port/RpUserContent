using RpUserContent.Entities;
using RpUserContent.PersistEntities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpUserContent.Services
{
    public static class UploadService
    {
        public static Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //If this isn't a post request, stop
            if (Program.FindRequestMethod(e) != RequestHttpMethod.post)
                throw new StandardError("Invalid method for this service.", StandardErrorType.NotFound);
            
            //Extract the application ID from the query.
            UserContentApp application = GetApplication(e);

            //Grab the stream of the uploaded file
            var file = e.Request.Form.Files["f"];
            if (file == null)
                throw new StandardError("No file found. Make sure this is submitted as a form with the file under the 'f' name.", StandardErrorType.NoFile);
            Stream s;
            try
            {
                s = file.OpenReadStream();
            } catch
            {
                throw new StandardError("Could not open stream on file.", StandardErrorType.NoFile);
            }

            //Check if the length is longer than the maximum allowed.
            if (file.Length > Program.config.maximum_upload_size)
                throw new StandardError($"The file you uploaded was too large. Please keep the length under {Program.config.maximum_upload_size} bytes (this file is {file.Length}).", StandardErrorType.FileTooBig);

            //Do processing if needed.
            Stream outStream = DoProcess(e, s, application);

            //It is now time to output this to the disk. Generate an ID
            var collec = Program.GetFileEntryCollection();
            string id = Program.GenerateRandomString(24);
            while (collec.Find(x => x._id == id).Count() != 0)
                id = Program.GenerateRandomString(24);

            //Generate a one-time token. This can be used to securely send the server the details, even if this is sent to a web client
            string token = Program.GenerateRandomString(32);
            while (Program.fileCreationTokens.ContainsKey(token))
                token = Program.GenerateRandomString(32);

            //Create entry.
            UserContentFileEntry entry = new UserContentFileEntry
            {
                _id = id,
                uploadTime = DateTime.UtcNow.Ticks,
                filename = id,
                applicationId = application.id,
                mimeType = GetMimeType(e, application),
                deletionToken = Program.GenerateRandomString(64),
                url = "https://user-content.romanport.com/u/"+id
            };

            //Write file
            try
            {
                using (FileStream fs = new FileStream(Program.config.uploaded_content_path+entry.filename, FileMode.Create))
                {
                    outStream.CopyTo(fs);
                }
            } catch
            {
                throw new StandardError("Failed to place file on disk. Try again later.", StandardErrorType.UnknownError);
            }

            //Insert entry
            collec.Insert(entry);

            //Insert entry into the temporary tokens directory
            Program.fileCreationTokens.Add(token, entry);

            //Close the stream
            try
            {
                outStream.Close();
            }
            catch { }

            //Respond
            return Program.QuickWriteJsonToDoc(e, new CreateResponse
            {
                ok = true,
                token = token,
                url = entry.url
            });
        }

        static Stream DoProcess(Microsoft.AspNetCore.Http.HttpContext e, Stream input, UserContentApp app) 
        {
            //If this is a binary upload, just return the same stream
            if (app.upload_type == UserContentAppType.binary)
                return input;

            //If this is a type of image, open it and throw an error if it is invalid.
            if (app.upload_type == UserContentAppType.image_png || app.upload_type == UserContentAppType.image_png_square512)
                return DoProcessImage(e, input, app);

            //Unknown
            throw new StandardError("Unknown application type.", StandardErrorType.UnknownError);
        }

        static Stream DoProcessImage(Microsoft.AspNetCore.Http.HttpContext e, Stream input, UserContentApp app)
        {
            //Try and open this as an image
            Image<Rgba32> img;
            try
            {
                img = Image.Load<Rgba32>(input);
            } catch
            {
                throw new StandardError("Failed to open image for processing.", StandardErrorType.ImageOpenFailed);
            }

            //If we were instructed to do something special with the image, do so
            if(app.upload_type == UserContentAppType.image_png_square512)
            {
                img.Mutate(x => x.Resize(512, 512));
            }

            //Save the image as a PNG file, then return the stream
            MemoryStream ms = new MemoryStream();
            img.SaveAsPng(ms);
            ms.Position = 0;

            //Close stream
            img.Dispose();
            input.Close();
            return ms;
        }

        static UserContentApp GetApplication(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //If this is missing, stop
            if (!e.Request.Query.ContainsKey("application_id"))
                throw new StandardError("Missing query arg 'application_id'.", StandardErrorType.MissingArgs);

            //Get the ID and look it up in the config file.
            string id = e.Request.Query["application_id"];
            var results = Program.config.applications.Where(x => x.id == id).ToArray();
            if (results.Length != 1)
                throw new StandardError("Incorrect application_id.", StandardErrorType.BadAuth);

            return results[0];
        }

        static string GetMimeType(Microsoft.AspNetCore.Http.HttpContext e, UserContentApp app)
        {
            //Some apps have mandated mime types. Check if this one has that.
            if (app.mandatedMimeType != null)
                return app.mandatedMimeType;

            //Get it from the URL params if we can
            //If this is missing, return a generic one
            if (!e.Request.Query.ContainsKey("mime_type"))
                return "application/octet-stream";

            //Get the ID and look it up in the config file.
            return e.Request.Query["mime_type"];
        }
    }
}
