using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace GoogleAnalyticHelloWorld
{
    class Program
    {
        private static string userID = null;

        // google analytics's tracking ID - made by New Property - website
        const string trackingID = "UA-50475045-1";

        // TODO: Exception handling for Envrionment
        private static string AnonymouseUserID
        {
            get
            {
                if (userID != null) return userID;
                var computer = Environment.MachineName;
                var login = Environment.UserName;

                using (MD5 md5 = new MD5CryptoServiceProvider())
                {
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(computer + login));
                    userID = BitConverter.ToString(hash).Replace("-", "");
                }
                return userID;
            }
        }

        static void Main(string[] args)
        {
            // Request 구성.
            var dataParams = new StringBuilder();
            dataParams.Append("v=1");
            dataParams.Append("&tid=" + trackingID);
            dataParams.Append("&cid=" + userID);
            dataParams.Append("&t=pageview");
            dataParams.Append("&dp=%2FpageA"); // /pageA

            var byteDataparams = Encoding.UTF8.GetBytes(dataParams.ToString());

            var request = WebRequest.Create("http://www.google-analytics.com/collect") as HttpWebRequest;
            if (request == null) return;

            request.Method = "POST";
            request.UserAgent = "Platform";
            request.ContentLength = byteDataparams.Length;
            var stDataParams = request.GetRequestStream();

            stDataParams.Write(byteDataparams, 0, byteDataparams.Length);
            stDataParams.Close();

            var response = request.GetResponse();
            if (!request.HaveResponse) return;
            var stReadData = response.GetResponseStream();
            if (stReadData == null) return;
            var srReadData = new StreamReader(stReadData, Encoding.Default);

            var strResult = srReadData.ReadToEnd();

            Console.WriteLine(strResult);
        }
    }
}
