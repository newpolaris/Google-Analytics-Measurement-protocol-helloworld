// jared-mess/unity3d-google-analytics : GoogleAnalytics.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace GoogleAnalytics
{
    public class GAEvent
    {
        public Hashtable data = new Hashtable();

        public GAEvent(string category, string action, string label=null, string value=null)
        {
            data["ec"] = category;
            data["ec"] = category;
            if (label != null) data["el"] = label;
            if (value != null) data["ev"] = value;
        }
    }

    public class GAPage
    {
        public Hashtable data = new Hashtable();

        public GAPage(string page, string title = null, string host = null)
        {
            if (host != null) data["eh"] = host;
            if (title != null) data["dt"] = title;

            data["ep"] = page;
        }
    }

    public static class GoogleAnalytics
    {
        private static string userID;
        private static bool dispatchInProgress = false;

        // google analytics's tracking ID - made by New Property - website
        private const string trackingID = "UA-50475045-1";

        private static Hashtable sessionRequestParams
        {
            get
            {
                var request = new Hashtable();
                request["v"] = 1;
                request["tid"] = trackingID;
                request["cid"] = AnonymouseUserID;

                return request;
            }
        }
        private static Queue<Hashtable> requestQueue = new Queue<Hashtable>();
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

        public static void Add(GAPage gaPage) 
        {
            var eventSpecificParams = new Hashtable(sessionRequestParams);

            eventSpecificParams["t"] = "pageview";

            foreach (var key in gaPage.data.Keys)
                eventSpecificParams[key] = gaPage.data[key];

            requestQueue.Enqueue(eventSpecificParams);
        }

        public static void Add(GAEvent gaEvent)
        {
            var eventSpecificParams = new Hashtable(sessionRequestParams);

            eventSpecificParams["t"] = "event";

            foreach (var key in gaEvent.data.Keys)
                eventSpecificParams[key] = gaEvent.data[key];

            requestQueue.Enqueue(eventSpecificParams);
        }

        public static async void Dispatch()
        {
            if ((requestQueue.Count > 0) && !dispatchInProgress)
                await DelayedDispatch();
        }

        private static async Task DelayedDispatch()
        {
            dispatchInProgress = true;

            while (requestQueue.Count > 0)
            {
                var Result = await Report();
                if (Result == true)
                    requestQueue.Dequeue();
                else
                    await Task.Delay(5000*60); // 5 min.
            }

            dispatchInProgress = false;
        }

        private static string BuildRequestString(Hashtable urlParms)
        {
            return string.Join("&", (from object key in urlParms.Keys select key + "=" + urlParms[key]).ToArray());
        }

        [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
        private static async Task<bool> Report()
        {
            var eventParms = requestQueue.Peek();
            var dataParams = BuildRequestString(eventParms);
            var byteDataparams = Encoding.UTF8.GetBytes(dataParams);

            const string gaURL = "http://www.google-analytics.com/collect";
            var request = (HttpWebRequest)WebRequest.Create(gaURL);

            request.Method = "POST";
            request.UserAgent = "Platform";
            request.ContentLength = byteDataparams.Length;
            request.Timeout = 3000; // 3 sec.
            var stDataParams = request.GetRequestStream();

            stDataParams.Write(byteDataparams, 0, byteDataparams.Length);
            stDataParams.Close();

            try
            {
                var response = await request.GetResponseAsync();
                var webResponse = (HttpWebResponse) response;

                // The Measurement Protocol will return a 2xx status code 
                // if the HTTP request was received. The Measurement Protocol 
                // does not return an error code if the payload data was malformed,
                // or if the data in the payload was incorrect or was not processed
                // by Google Analytics.
                var status = webResponse.StatusCode;
                if (status == HttpStatusCode.Accepted ||
                    status == HttpStatusCode.Created ||
                    status == HttpStatusCode.OK)
                    return true;
            }
            // Discard.
            catch (WebException) {}

            return false;
        }
    }
}
