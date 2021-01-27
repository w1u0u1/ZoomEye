using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace ZoomEye
{
    class libZoomEye
    {
        public static string GetJsonIP(JToken jtoken)
        {
            return jtoken["ip"].ToString();
        }

        public static string GetJsonCountry(JToken jtoken)
        {
            return jtoken["geoinfo"]["country"]["names"]["en"].ToString();
        }

        public static string GetJsonOS(JToken jtoken)
        {
            return jtoken["portinfo"]["os"].ToString();
        }

        public static string GetJsonSite(JToken jtoken)
        {
            return "";
        }

        public static string GetJsonTitle(JToken jtoken)
        {
            string ret = "";
            foreach (var token in jtoken["portinfo"]["title"])
                ret += token + ",";
            if (ret.Length > 0)
                ret = ret.Remove(ret.Length - 1);
            return ret;
        }

        public static string GetJsonServer(JToken jtoken)
        {
            return "";
        }

        public static string GetAccessToken(string user, string pass)
        {
            string url = "https://api.zoomeye.org/user/login";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Proxy = CONFIG.Proxy;

            JObject data = new JObject();
            data.Add("username", user);
            data.Add("password", pass);

            byte[] buf = Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = buf.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(buf, 0, buf.Length);
            writer.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string html = sr.ReadToEnd();
            sr.Close();

            JObject jObject = (JObject)JsonConvert.DeserializeObject(html);
            return jObject["access_token"].ToString();
        }

        public static string GetResourcesInfo(string token)
        {
            string url = "https://api.zoomeye.org/resources-info";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Proxy = CONFIG.Proxy;
            request.Headers.Add("Authorization", "JWT " + token);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string html = sr.ReadToEnd();
            sr.Close();

            return html;
        }     
    }
}