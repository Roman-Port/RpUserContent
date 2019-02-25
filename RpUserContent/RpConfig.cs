using System;
using System.Collections.Generic;
using System.Text;

namespace RpUserContent
{
    public class RpConfig
    {
        public int port; //Port this will operate on
        public long maximum_upload_size; //Maximum size, in bytes, of an uploaded file.
        public string database_path; //Path to the LiteDB file
        public string uploaded_content_path; //Path to the uploaded content. Will be appended by the ID in the database.
        public UserContentApp[] applications; //Registered applications
    }

    public class UserContentApp
    {
        public string id;
        public string name;
        public UserContentAppType upload_type;
        public string mandatedMimeType; //Can be null
    }

    public enum UserContentAppType
    {
        binary = 0, //Do not touch the uploaded contents
        image_png = 1, //Process this as an image and stop this if it is not an image.
        image_png_square512 = 2 //Process this as an image, rescale it to a square 512x512, and stop this if it is not an image.
    }
}
