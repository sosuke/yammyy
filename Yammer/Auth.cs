using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OAuth;
using System.Web;

namespace Yammer
{
    public class Auth
    {
        public Auth()
        {
        }

        public string GetAccessTokenQuery(string consumerKey, string consumerSecret, string tokenKey, string tokenSecret)
        {
            Uri uri = new Uri(Resources.OAUTH_ACCESSTOKEN);
            string nurl;
            string nrp;

            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(
                uri,
                consumerKey,
                consumerSecret,
                tokenKey,
                tokenSecret,
                "POST",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);

            StringBuilder sb = new StringBuilder(uri.ToString());
            sb.AppendFormat("?oauth_consumer_key={0}&", consumerKey);
            sb.AppendFormat("oauth_token={0}&", tokenKey);
            sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
            sb.AppendFormat("oauth_nonce={0}&", nonce);
            sb.AppendFormat("oauth_version={0}&", "1.0");
            sb.AppendFormat("oauth_signature={0}", sig);

            string query = sb.ToString();

            return query;
        }

        public string GetAuthorizeQuery(string tokenKey, string callback)
        {
            return Resources.OAUTH_AUTHORIZE + "?oauth_token=" + tokenKey;
        }

        public string GetRequestTokenQuery(string consumerKey, string consumerSecret)
        {
            Uri uri = new Uri(Resources.OAUTH_REQUEST_TOKEN);
            string nurl;
            string nrp;

            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string sig = oAuth.GenerateSignature(
                uri,
                consumerKey,
                consumerSecret,
                string.Empty,
                string.Empty,
                "GET",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);

            StringBuilder sb = new StringBuilder(uri.ToString());
            sb.AppendFormat("?oauth_consumer_key={0}&", consumerKey);
            sb.AppendFormat("oauth_nonce={0}&", nonce);
            sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
            sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            sb.AppendFormat("oauth_version={0}&", "1.0");
            sb.AppendFormat("oauth_signature={0}", sig);
            string query = sb.ToString();

            return query;
        }
    }
}
