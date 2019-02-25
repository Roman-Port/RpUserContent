using System;
using System.Collections.Generic;
using System.Text;

namespace RpUserContent
{
    public class StandardError : Exception
    {
        public string screen_message;
        public StandardErrorType screen_error;
        public string screen_error_string;

        public StandardError(string screen_message, StandardErrorType type)
        {
            this.screen_message = screen_message;
            this.screen_error = type;
            this.screen_error_string = this.screen_error.ToString();
        }
    }

    public enum StandardErrorType
    {
        NotFound,
        UnknownError,
        BadAuth,
        MissingArgs,
        NoFile,
        FileTooBig,

        ImageOpenFailed
    }
}
