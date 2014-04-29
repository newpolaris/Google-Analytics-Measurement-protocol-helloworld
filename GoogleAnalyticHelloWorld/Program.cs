using System;
using System.IO;
using System.Net;
using System.Text;

namespace GoogleAnalyticHelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            // google analytics's tracking ID - made by New Property - website
            var trackingID = "UA-50475045-1";

            // Request 구성.
            var dataParams = new StringBuilder();
            dataParams.Append("v=1");
            dataParams.Append("&tid=" + trackingID);
            dataParams.Append("&cid=5555");
            dataParams.Append("&t=pageview");
            dataParams.Append("&dp=%2FpageA"); // /pageA

            var byteDataparams = UTF8Encoding.UTF8.GetBytes(dataParams.ToString());

            var request = WebRequest.Create("http://www.google-analytics.com/collect") as HttpWebRequest;
            request.Method = "POST";
            request.UserAgent = "Platform";
            request.ContentLength = byteDataparams.Length;
            var stDataParams = request.GetRequestStream();

            stDataParams.Write(byteDataparams, 0, byteDataparams.Length);
            stDataParams.Close();

            var response = request.GetResponse();
            if (request.HaveResponse && response != null)
            {
                Stream stReadData = response.GetResponseStream();
                StreamReader srReadData = new StreamReader(stReadData, Encoding.Default);

                string strResult = srReadData.ReadToEnd();

                Console.WriteLine(strResult);
            }
        }
    }
}
