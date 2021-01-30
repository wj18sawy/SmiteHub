using Newtonsoft.Json;
using SmiteHub.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Configuration;

namespace SmiteHub
{
    public class Program
    {
        private string devKey = ConfigurationManager.AppSettings["devKey"];
        private string authKey = ConfigurationManager.AppSettings["authKey"]; 
        private string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        private string urlPrefix = "http://api.smitegame.com/smiteapi.svc/";


        private string signature = "";
        private string session = "";

        public static void Main(string[] args)
        {           
            Program run = new Program();
            run.CreateSession();
            run.GetGods();
        }

        private static string GetMD5Hash(string input)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            bytes = md5.ComputeHash(bytes);
            var sb = new System.Text.StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        private void CreateSession()
        {
            // Get Signature that is specific to "createsession"
            //
            signature = GetMD5Hash(devKey + "createsession" + authKey + timestamp);

            // Call the "createsession" API method & wait for synchronous response
            //
            WebRequest request = WebRequest.Create(urlPrefix + "createsessionjson/" + devKey + "/" + signature + "/" + timestamp);
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();

            // Parse returned JSON into "session" data
            //
            using (var web = new WebClient())
            {
                web.Encoding = System.Text.Encoding.UTF8;
                var jsonString = responseFromServer;
                var sessionInfo = JsonConvert.DeserializeObject<SessionInfo>(jsonString);

                session = sessionInfo.session_id;
            }
        }

        private void GetGods()
        {
            // Get Signature that is specific to "getgods"
            //
            signature = GetMD5Hash(devKey + "getgods" + authKey + timestamp);


            // Call the "getgods" API method & wait for synchronous response
            //
            string languageCode = "1";

            WebRequest request = WebRequest.Create(urlPrefix + "getgodsjson/" + devKey + "/" + signature + "/" + session + "/" + timestamp + "/" + languageCode);
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();

            // Parse returned JSON into "gods" data
            //
            using (var web = new WebClient())
            {
                web.Encoding = System.Text.Encoding.UTF8;
                var jsonString = responseFromServer;
                var GodsList = JsonConvert.DeserializeObject<List<Gods>>(jsonString);
                string GodsListStr = "";

                foreach (Gods x in GodsList)
                    GodsListStr = GodsListStr + ", " + x.Name;
            }

        }

    }
}
