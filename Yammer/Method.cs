using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;
using System.Web;
using OAuth;


namespace Yammer
{
    static class Method
    {
        public static string Get(string query, Session session)
        {
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string nurl, nrp;            

            Uri uri = new Uri(query);
            string sig = oAuth.GenerateSignature(
                uri,
                session.Auth.ConsumerKey,
                session.Auth.ConsumerSecret,
                session.Auth.TokenKey,
                session.Auth.TokenSecret,
                "GET",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "GET";
            req.Proxy = session.Proxy;
            string authHeader = string.Empty;
            authHeader = "OAuth " +
                "oauth_consumer_key=\"" + session.Auth.ConsumerKey + "\"," +
                "oauth_token=\"" + session.Auth.TokenKey + "\"," +
                "oauth_nonce=\"" + nonce + "\"," +
                "oauth_timestamp=\"" + timeStamp + "\"," +
                "oauth_signature_method=\"" + "HMAC-SHA1" + "\"," +
                "oauth_version=\"" + "1.0" + "\"," +
                "oauth_signature=\"" + sig + "\"";
            req.ContentType = Constants.HttpPostUrlEncodedContentType;
            req.Headers.Add(Constants.AuthorizationHeaderParameter, authHeader);

            WebResponse res = req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            string data = reader.ReadToEnd();

            res.Close();
            reader.Close();     

            return data;
        }

        public static string Post(string query, NameValueCollection parameters, Session session)
        {
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string nurl, nrp;
            string q = string.Empty;
            int count = 0;

            foreach (string key in parameters.Keys)
            {                
                if (count == 0)
                {
                    q = query + "?" + key + "=" + Rfc3986.Encode(parameters[key]);
                }
                else
                {
                    q += "&" + key + "=" + Rfc3986.Encode(parameters[key]);
                }
                count++;
            }
            
            Uri uri = new Uri(q);
            string sig = oAuth.GenerateSignature(
                uri,
                session.Auth.ConsumerKey,
                session.Auth.ConsumerSecret,
                session.Auth.TokenKey,
                session.Auth.TokenSecret,
                "POST",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);

            req.Method = "POST";
            req.Proxy = session.Proxy;
            string authHeader = string.Empty;
            authHeader = "OAuth " +
                "realm=\"" + Resources.MESSAGES_POST + "\"," +
                "oauth_consumer_key=\"" + session.Auth.ConsumerKey + "\"," +
                "oauth_token=\"" + session.Auth.TokenKey + "\"," +
                "oauth_nonce=\"" + nonce + "\"," +
                "oauth_timestamp=\"" + timeStamp + "\"," +
                "oauth_signature_method=\"" + "HMAC-SHA1" + "\"," +
                "oauth_version=\"" + "1.0" + "\"," +
                "oauth_signature=\"" + sig + "\"";
            req.ContentType = Constants.HttpPostUrlEncodedContentType;
            req.Headers.Add(Constants.AuthorizationHeaderParameter, authHeader);

            count = 0;
            string wdata = string.Empty;
            foreach (string key in parameters.Keys)
            {
                if (count == 0)
                {
                    wdata = key + "=" + Rfc3986.Encode(parameters[key]);
                }
                else
                {
                    wdata += "&" + key + "=" + Rfc3986.Encode(parameters[key]);
                }
                count++;
            }
            
            byte[] postDataBytes = Encoding.ASCII.GetBytes(wdata);
            req.ContentLength = postDataBytes.Length;
            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            reqStream.Close();

            WebResponse res = req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            string data = reader.ReadToEnd();
            reader.Close();
            res.Close();

            return data;
        }

        public static string Delete(string query, Session session)
        {
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string nurl, nrp;

            Uri uri = new Uri(query);
            string sig = oAuth.GenerateSignature(
                uri,
                session.Auth.ConsumerKey,
                session.Auth.ConsumerSecret,
                session.Auth.TokenKey,
                session.Auth.TokenSecret,
                "DELETE",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "DELETE";
            req.Proxy = session.Proxy;
            string authHeader = string.Empty;
            authHeader = "OAuth " +
                "oauth_consumer_key=\"" + session.Auth.ConsumerKey + "\"," +
                "oauth_token=\"" + session.Auth.TokenKey + "\"," +
                "oauth_nonce=\"" + nonce + "\"," +
                "oauth_timestamp=\"" + timeStamp + "\"," +
                "oauth_signature_method=\"" + "HMAC-SHA1" + "\"," +
                "oauth_version=\"" + "1.0" + "\"," +
                "oauth_signature=\"" + sig + "\"";
            req.ContentType = Constants.HttpPostUrlEncodedContentType;
            req.Headers.Add(Constants.AuthorizationHeaderParameter, authHeader);

            WebResponse res = req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            string data = reader.ReadToEnd();
            reader.Close();
            res.Close();

            return data;
        }
    }


}
