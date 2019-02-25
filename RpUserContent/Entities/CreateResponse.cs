using System;
using System.Collections.Generic;
using System.Text;

namespace RpUserContent.Entities
{
    public class CreateResponse
    {
        public bool ok; //Should always be true
        public string token; //Token to be sent to the server so the server can validate response
        public string url; //URL that should ONLY ever be used BY THE CLIENT and NEVER trusted by the server
    }
}
