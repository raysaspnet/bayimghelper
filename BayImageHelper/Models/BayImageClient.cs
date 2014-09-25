using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace BayImage.Models
{
    public class BayimgClient : WebClient
    {
        Uri _responseUri;

        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                WebResponse response = base.GetWebResponse(request);

                _responseUri = response.ResponseUri;
                return response;
            }
            catch (Exception e)
            {
                System.Threading.Thread.Sleep(2000);
                return null;
            }
        }

        public CookieContainer Cookies { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address); //cast to HttpWebRequest and check if it worked

            if (request is HttpWebRequest)
                (request as HttpWebRequest).CookieContainer = this.Cookies;

            return request;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                this.Cookies.Add(cookies);
            }
        }

        private void AddCookies(CookieCollection cookies)
        {
            foreach (Cookie one in cookies)
            {
                this.Cookies.Add(new Cookie()
                {
                    Name = one.Name,
                    Expires = one.Expires,
                    Domain = one.Domain,
                    Value = one.Value,
                    Path = one.Path,
                    Discard = one.Discard,
                    Expired = one.Expired,
                    HttpOnly = one.HttpOnly

                });

            }
        }

        public BayimgClient()
        {
            this.Cookies = new CookieContainer(CookieContainer.DefaultCookieLimit, CookieContainer.DefaultPerDomainCookieLimit * 2, CookieContainer.DefaultCookieLengthLimit);
            this.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3" +
            ".0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .N" +
            "ET CLR 1.1.4322)");
            this.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            this.Headers.Add(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            this.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
        }


        /// <summary>
        /// Makes a request on the supplied Url
        /// </summary>
        /// <param name="URL">the url you wish to request on</param>
        /// <returns>a string with the url return contents (think html)</returns>
        public virtual string Request(string URL)
        {
            try
            {
                Stream data = OpenRead(new Uri(URL));
                StreamReader reader = new StreamReader(data);
                string formString = reader.ReadToEnd();
                data.Close();
                reader.Close();

                return formString;
            }
            catch (Exception e)
            {
                System.Threading.Thread.Sleep(2000);
                var xx = e.Message;
                return "";
            }
        }

        public string POST(string URL, string PostData)
        {
            string result = "";
            var webrequest = (HttpWebRequest)GetWebRequest(new Uri(URL));
            //webrequest.Headers["x-requested-with"] = "XMLHttpRequest";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            webrequest.Method = "POST";
            webrequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webrequest.AllowAutoRedirect = false;

            var encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(PostData);
            webrequest.ContentLength = bytes.Length;
            Stream newStream = webrequest.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            using (HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse())
            {
                AddCookies(webresponse.Cookies);
                if (((int)webresponse.StatusCode) < 400)
                {
                    using (var str = new StreamReader(webresponse.GetResponseStream()))
                    {
                        result = str.ReadToEnd();
                    }
                }
                else
                {
                    throw new Exception("Unable to POST to " + URL);
                }
                webresponse.Close();
            }
            return result;
        }


        public string UploadFileEx(string uploadfile, string url, string fileFormName, string contenttype, CookieContainer cookies)
        {
            if ((fileFormName == null) ||
                (fileFormName.Length == 0))
            {
                fileFormName = "file";
            }

            if ((contenttype == null) ||
                (contenttype.Length == 0))
            {
                contenttype = "application/octet-stream";
            }


            string postdata;
            postdata = "?code=test";
            Uri uri = new Uri(url + postdata);

            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.CookieContainer = this.Cookies;
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";


            // Build up the post message header
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(uploadfile));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("\r\n");
            sb.Append("\r\n");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array
            // ensuring the boundary appears on a line by itself
            byte[] boundaryBytes =
                   Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            FileStream fileStream = new FileStream(uploadfile,
                                        FileMode.Open, FileAccess.Read);
            long length = postHeaderBytes.Length + fileStream.Length +
                                                   boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents
            byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                                     (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();
            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
        }

        public string POSTImage()
        {
            var webrequest = (HttpWebRequest)GetWebRequest(new Uri("http://bayimg.com"));
            string URL = "http://bayimg.com/upload";
            string picturePath = @"C:\Users\Public\Pictures\Sample Pictures\Hydrangeas.jpg";
            var result = this.UploadFileEx(picturePath, URL, "", "", this.Cookies);
            string a = GetImageURL(result);
            return a;
        }
        public string POSTBayImage(HttpPostedFileBase file, string OriginaluploadfileName = "", string removeCode = "BayImageHelper")
        {
            string url = "http://bayimg.com/upload";

            HttpWebRequest webrequest = (HttpWebRequest)GetWebRequest(new Uri("http://bayimg.com"));

            string fileFormName = "file";
            string contenttype = "application/octet-stream";
            string postdata = "?code=" + removeCode;
            Uri uri = new Uri(url + postdata);

            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

            webrequest = (HttpWebRequest)WebRequest.Create(uri);
            webrequest.CookieContainer = this.Cookies;
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";


            // Build up the post message header
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(boundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"");
            sb.Append(fileFormName);
            sb.Append("\"; filename=\"");
            sb.Append(Path.GetFileName(OriginaluploadfileName));
            sb.Append("\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append(contenttype);
            sb.Append("\r\n");
            sb.Append("\r\n");

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            // Build the trailing boundary string as a byte array
            // ensuring the boundary appears on a line by itself
            byte[] boundaryBytes =
                   Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var fileStream = file.InputStream;

            long length = postHeaderBytes.Length + fileStream.Length +
                                                   boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // Write out our post header
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // Write out the file contents
            byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                                     (int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                requestStream.Write(buffer, 0, bytesRead);

            // Write out the trailing boundary
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            WebResponse responce = webrequest.GetResponse();
            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return GetImageURL(sr.ReadToEnd());

        }

        private string GetImageURL(string htmlBody)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlBody);

            var urldiv = htmlDoc.DocumentNode.Descendants("div").Where(d => d.Id == "extra2");
            string urlFirst = urldiv.FirstOrDefault().Descendants("a").Where(d => d.Attributes["href"].Value.Contains("bayimg")).FirstOrDefault().Attributes["href"].Value;

            string htmlBody2 = new WebClient().DownloadString("http:"+urlFirst);

            htmlDoc.LoadHtml(htmlBody2);

            /*
             <img src="//image.bayimg.com/03d256c52d629033eef5e7456518184235dc1094.jpg" id="mainImage" alt="Use 'view image' in your browser to see full size image" class="image-setting-big" border="0"/><
             */
            string urlSecond = htmlDoc.DocumentNode.Descendants("img").Where(d => d.Id == "mainImage" && d.Attributes["src"].Value.Contains("bayimg")).FirstOrDefault().Attributes["src"].Value;;
            return string.Format("http:{1}<br/>You can remove your image from: http:{0} . The remove code is: BayImageHelper", urlFirst, urlSecond);
        }
    }
}