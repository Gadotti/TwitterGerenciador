﻿using System;
using System.Configuration;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace Autenticador
{
    public class oAuthTwitter : OAuthBase
    {
        public enum Method { GET, POST };
        public const string REQUEST_TOKEN = "http://twitter.com/oauth/request_token";
        public const string AUTHORIZE = "http://twitter.com/oauth/authorize";
        public const string ACCESS_TOKEN = "http://twitter.com/oauth/access_token";

        private string _consumerKey = "zCAWzBY7NZroHtjW9O1Q";
        private string _consumerSecret = "idHgDe8ClVZuihHYP5pFGEYp8rogB7rWTk4ph8iv3Q";
        private string _token = "";
        private string _tokenSecret = "";
        private string _verifier = "";

        #region Properties
        public string ConsumerKey
        {
            get
            {
                if (_consumerKey.Length == 0)
                {
                    _consumerKey = ConfigurationManager.AppSettings["consumerKey"];
                }
                return _consumerKey;
            }
            set { _consumerKey = value; }
        }

        public string ConsumerSecret
        {
            get
            {
                if (_consumerSecret.Length == 0)
                {
                    _consumerSecret = ConfigurationManager.AppSettings["consumerSecret"];
                }
                return _consumerSecret;
            }
            set { _consumerSecret = value; }
        }

        public string Token { get { return _token; } set { _token = value; } }
        public string TokenSecret { get { return _tokenSecret; } set { _tokenSecret = value; } }
        public string Verifier { get { return _verifier; } set { _verifier = value; } }

        #endregion

        /// <summary>
        /// Get the link to Twitter's authorization page for this application.
        /// </summary>
        /// <returns>The url with a valid request token, or a null string.</returns>
        public string AuthorizationLinkGet()
        {
            string ret = null;

            string response = oAuthWebRequest(Method.GET, REQUEST_TOKEN, String.Empty);
            if (response.Length > 0)
            {
                //response contains token and token secret.  We only need the token.
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    ret = AUTHORIZE + "?oauth_token=" + qs["oauth_token"];
                }
            }
            return ret;
        }

        /// <summary>
        /// Exchange the request token for an access token.
        /// </summary>
        /// <param name="authToken">The oauth_token is supplied by Twitter's authorization page following the callback.</param>
        public void AccessTokenGet(string authToken, string verifier)
        {
            Token = authToken;
            Verifier = verifier;

            var response = oAuthWebRequest(Method.GET, ACCESS_TOKEN, String.Empty);

            if (response.Length > 0)
            {
                //Store the Token and Token Secret
                var qs = HttpUtility.ParseQueryString(response);
                if (qs["oauth_token"] != null)
                {
                    Token = qs["oauth_token"];
                }
                if (qs["oauth_token_secret"] != null)
                {
                    TokenSecret = qs["oauth_token_secret"];
                }
            }
        }

        /// <summary>
        /// Submit a web request using oAuth.
        /// </summary>
        /// <param name="method">GET or POST</param>
        /// <param name="url">The full url, including the querystring.</param>
        /// <param name="postData">Data to post (querystring format)</param>
        /// <returns>The web server response.</returns>
        public string oAuthWebRequest(Method method, string url, string postData)
        {
            string outUrl;
            string querystring;


            //Setup postData for signing.
            //Add the postData to the querystring.
            if (method == Method.POST)
            {
                if (postData.Length > 0)
                {
                    //Decode the parameters and re-encode using the oAuth UrlEncode method.
                    var qs = HttpUtility.ParseQueryString(postData);
                    postData = "";
                    foreach (string key in qs.AllKeys)
                    {
                        if (postData.Length > 0)
                        {
                            postData += "&";
                        }
                        qs[key] = HttpUtility.UrlDecode(qs[key]);
                        qs[key] = UrlEncode(qs[key]);
                        postData += key + "=" + qs[key];

                    }
                    if (url.IndexOf("?") > 0)
                    {
                        url += "&";
                    }
                    else
                    {
                        url += "?";
                    }
                    url += postData;
                }
            }

            var uri = new Uri(url);

            var nonce = GenerateNonce();
            var timeStamp = GenerateTimeStamp();



            //Generate Signature
            var sig = GenerateSignature(uri,
                ConsumerKey,
                ConsumerSecret,
                Token,
                TokenSecret,
                Verifier,
                method.ToString(),
                timeStamp,
                nonce,
                out outUrl,
                out querystring);

            querystring += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

            //Convert the querystring to postData
            if (method == Method.POST)
            {
                postData = querystring;
                querystring = "";
            }

            if (querystring.Length > 0)
            {
                outUrl += "?";
            }

            var ret = WebRequest(method, outUrl + querystring, postData);

            return ret;
        }

        /// <summary>
        /// Web Request Wrapper
        /// </summary>
        /// <param name="method">Http Method</param>
        /// <param name="url">Full url to the web resource</param>
        /// <param name="postData">Data to post in querystring format</param>
        /// <returns>The web server response.</returns>
        public string WebRequest(Method method, string url, string postData)
        {
            StreamWriter requestWriter ;

            var webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            //webRequest.UserAgent  = "Identify your application please.";
            //webRequest.Timeout = 20000;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    requestWriter.Write(postData);
                }
                //catch
                //{
                //    throw;
                //}
                finally
                {
                    requestWriter.Close();
                    //requestWriter = null;
                }
            }

            var responseData = WebResponseGet(webRequest);

            //webRequest = null;

            return responseData;

        }

        /// <summary>
        /// Process the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <returns>The response data.</returns>
        public string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData;

            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            //catch
            //{
            //    throw;
            //}
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                if (responseReader != null)
                    responseReader.Close();
                //responseReader = null;
            }

            return responseData;
        }
    }
}
