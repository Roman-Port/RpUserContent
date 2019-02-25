using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using RpUserContent.Entities;
using RpUserContent.PersistEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RpUserContent
{
    class Program
    {
        public static Random rand = new Random();
        public static RpConfig config;
        public static LiteDatabase db;
        public static Dictionary<string, UserContentFileEntry> fileCreationTokens = new Dictionary<string, UserContentFileEntry>(); //Tokens that are one-time use and can be used to securely send the server data that the client has

        static void Main(string[] args)
        {
            Log("Loading config...", ConsoleColor.White);
            config = JsonConvert.DeserializeObject<RpConfig>(File.ReadAllText("config.json"));

            Log("Loading database...", ConsoleColor.White);
            db = new LiteDatabase(config.database_path);

            Log("Starting Kestrel...", ConsoleColor.White);
            MainAsync().GetAwaiter().GetResult();
        }

        public static LiteCollection<UserContentFileEntry> GetFileEntryCollection()
        {
            return db.GetCollection<UserContentFileEntry>("uploads");
        }

        public static void Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, config.port);

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(HttpHandler.OnHttpRequest);

        }

        public static Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            //Read post body
            byte[] buffer = new byte[(int)context.Request.ContentLength];
            context.Request.Body.Read(buffer, 0, buffer.Length);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(buffer));
        }

        public static Task QuickWriteStatusToDoc(Microsoft.AspNetCore.Http.HttpContext e, bool ok, int code = 200)
        {
            return QuickWriteJsonToDoc(e, new TrueFalseReply
            {
                ok = ok
            }, code);
        }

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomString(int length)
        {
            return GenerateRandomStringCustom(length, "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
        }

        public static string GenerateRandomStringCustom(int length, char[] chars)
        {
            string output = "";
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        public static RequestHttpMethod FindRequestMethod(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return Enum.Parse<RequestHttpMethod>(context.Request.Method.ToLower());
        }
    }

    public enum RequestHttpMethod
    {
        get,
        post,
        put,
        delete,
        options
    }
}
