using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace WebDeploy.Utils
{
    public static class HttpWebRequestHelper
    {
        private const string DEFAULT_USER_AGENT = "aoh server";
        private const int RequestTimeout = 60 * 1000;
        public static string Get(string uri)
        {
            return Get(uri, Encoding.UTF8);
        }
        public static string Get(string uri, Encoding encoding)
        {

            var req = WebRequest.Create(uri) as HttpWebRequest;
            req.Timeout = RequestTimeout;
            req.Method = "GET";
            req.UserAgent = DEFAULT_USER_AGENT;
            req.Accept = "application/json";

            return GetResponseString(encoding, req);

        }

        private static string GetResponseString(Encoding encoding, HttpWebRequest req)
        {
            using (var rep = req.GetResponse() as HttpWebResponse)
            {
                if (rep.ContentEncoding.ToLower().Contains("gzip"))
                {
                    using (GZipStream stream = new GZipStream(rep.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {

                            return reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    using (var stream = rep.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream, encoding))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string BinaryPost(string uri, string fileKeyName, string fileName, string fileContentType, Stream fileStream, IDictionary<string, object> parameters)
        {
            return BinaryPost(uri, fileKeyName, fileName, fileContentType, fileStream, parameters, Encoding.UTF8);
        }
        public static string BinaryPost(string uri, string fileKeyName, string fileName, string fileContentType, Stream fileStream, IDictionary<string, object> parameters, Encoding encoding)
        {

            if (string.IsNullOrWhiteSpace(fileKeyName))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            if (encoding == null)
                encoding = Encoding.UTF8;


            var req = WebRequest.Create(uri) as HttpWebRequest;
            req.Timeout = RequestTimeout;
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.Accept = "application/json";
            req.AllowWriteStreamBuffering = false;
            //req.UserAgent = DEFAULT_USER_AGENT;

            var fbody = new StringBuilder();
            var dbody = new StringBuilder();


            fbody.Append("--").Append(boundary).Append("\r\n");
            fbody.Append("Content-Disposition: form-data; name=\"").Append(fileKeyName).Append("\"; filename=\"");
            fbody.Append(fileName).Append("\"\r\nContent-Type: application/octet-stream\r\n\r\n");


            if (parameters != null)
            {
                foreach (var i in parameters)
                {
                    dbody.Append("\r\n--").Append(boundary).Append("\r\n");
                    dbody.Append("Content-Disposition: form-data; name=\"").Append(i.Key).Append("\"\r\n\r\n");
                    dbody.Append(i.Value);
                }

                dbody.Append("\r\n--").Append(boundary).Append("--");
            }

            var fdata = encoding.GetBytes(fbody.ToString());
            var ddata = encoding.GetBytes(dbody.ToString());

            req.ContentLength = fileStream.Length + fdata.Length + ddata.Length;

            using (var ustream = req.GetRequestStream())
            {
                ustream.Write(fdata, 0, fdata.Length);

                var buffer = new byte[4096];

                fileStream.Position = 0;
                var size = fileStream.Read(buffer, 0, buffer.Length);

                while (size > 0)
                {
                    ustream.Write(buffer, 0, size);

                    size = fileStream.Read(buffer, 0, buffer.Length);
                }

                ustream.Write(ddata, 0, ddata.Length);
                ustream.Flush();
            }

            return GetResponseString(encoding, req);

        }
        public static string Post(string uri, IDictionary<string, object> parameters)
        {
            return Post(uri, parameters, Encoding.UTF8);
        }
        public static string Post(string uri, IDictionary<string, object> parameters, Encoding encoding)
        {

            var req = WebRequest.Create(uri) as HttpWebRequest;
            req.Timeout = RequestTimeout;

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "application/json";
            req.UserAgent = DEFAULT_USER_AGENT;

            var buffer = string.Empty;

            if (parameters != null)
            {
                foreach (var i in parameters.Keys)
                {
                    if (string.Empty != buffer)
                    {
                        buffer += "&";
                    }

                    buffer += i + "=" + System.Web.HttpUtility.UrlEncode(Convert.ToString(parameters[i]));
                }
            }

            var data = encoding.GetBytes(buffer);

            using (var ustream = req.GetRequestStream())
            {
                ustream.Write(data, 0, data.Length);
            }

            return GetResponseString(encoding, req);

        }

        public static string PostRaw(string uri, string content, string mimeType)
        {
            return PostRaw(uri, content, mimeType, Encoding.UTF8);
        }
        public static string PostRaw(string uri, string content, string mimeType, Encoding encoding
             )
        {

            var req = WebRequest.Create(uri) as HttpWebRequest;
            req.Timeout = RequestTimeout;
            req.Method = "POST";
            req.ContentType = mimeType;
            req.Accept = "application/json";
            req.UserAgent = DEFAULT_USER_AGENT;

            var data = encoding.GetBytes(content);

            using (var ustream = req.GetRequestStream())
            {
                ustream.Write(data, 0, data.Length);
            }

            return GetResponseString(encoding, req);

        }


        /// <summary>  
        /// 获取真ip(忽略代理)  
        /// </summary>  
        /// <returns></returns>  
        public static string GetRealIP()
        {
            string result = String.Empty;
            result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //可能有代理   
            if (!string.IsNullOrWhiteSpace(result))
            {
                //没有"." 肯定是非IP格式  
                if (result.IndexOf(".") == -1)
                {
                    result = null;
                }
                else
                {
                    //有","，估计多个代理。取第一个不是内网的IP。  
                    if (result.IndexOf(",") != -1)
                    {
                        result = result.Replace(" ", string.Empty).Replace("\"", string.Empty);

                        string[] temparyip = result.Split(",;".ToCharArray());

                        if (temparyip != null && temparyip.Length > 0)
                        {
                            for (int i = 0; i < temparyip.Length; i++)
                            {
                                //找到不是内网的地址  
                                if (IsIPAddress(temparyip[i]) && temparyip[i].Substring(0, 3) != "10." && temparyip[i].Substring(0, 7) != "192.168" && temparyip[i].Substring(0, 7) != "172.16.")
                                {
                                    return temparyip[i];
                                }
                            }
                        }
                    }
                    //代理即是IP格式  
                    else if (IsIPAddress(result))
                    {
                        return result;
                    }
                    //代理中的内容非IP  
                    else
                    {
                        result = null;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            return result;
        }

        /// <summary>
        /// 判断是否为IP地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsIPAddress(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length < 7 || str.Length > 15)
                return false;

            string regformat = @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(str);
        }
    }
}
