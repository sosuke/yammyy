using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using OAuth;
using System.Web;

namespace Yammer
{
    public class Session
    {
        public WebProxy Proxy { get; set; }
        public User CurrentUser { get; set; }
        public OAuthKey Auth { get; set; }

        public Session(OAuthKey oauth)
        {
            this.CurrentUser = GetCurrentUser(oauth, null);
            this.Auth = oauth;
            this.Proxy = null;
        }

        public Session(OAuthKey oauth, WebProxy proxy)
        {
            this.CurrentUser = GetCurrentUser(oauth, proxy);
            this.Auth = oauth;
            this.Proxy = proxy;
        }

        private User GetCurrentUser(OAuthKey auth, WebProxy proxy)
        {
            #region OAuth
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string nurl, nrp;
            string query = Resources.USERS_CURRENT + "?";

            Uri uri = new Uri(query);
            string sig = oAuth.GenerateSignature(
                uri,
                auth.ConsumerKey,
                auth.ConsumerSecret,
                auth.TokenKey,
                auth.TokenSecret,
                "GET",
                timeStamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out nurl, out nrp);
            sig = HttpUtility.UrlEncode(sig);
            StringBuilder sb = new StringBuilder(uri.ToString());
            sb.AppendFormat("oauth_consumer_key={0}&", auth.ConsumerKey);
            sb.AppendFormat("oauth_token={0}&", auth.TokenKey);
            sb.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            sb.AppendFormat("oauth_timestamp={0}&", timeStamp);
            sb.AppendFormat("oauth_nonce={0}&", nonce);
            sb.AppendFormat("oauth_version={0}&", "1.0");
            sb.AppendFormat("oauth_signature={0}", sig);
            query = sb.ToString();
            #endregion

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "GET";
            req.PreAuthenticate = true;
            req.Accept = "text/xml, application/xml";
            req.Proxy = proxy;

            WebResponse res = req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            string data = reader.ReadToEnd();
            reader.Close();
            res.Close();
            
            #region XmlDocument
            User u = new User();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(data);

            u.JobTitle = xdoc.SelectSingleNode("/response/job-title").InnerText;
            u.FullName = xdoc.SelectSingleNode("/response/full-name").InnerText;
            u.MugshotUrl = new Uri(xdoc.SelectSingleNode("/response/mugshot-url").InnerText);
            u.Name = xdoc.SelectSingleNode("/response/name").InnerText;
            u.Url = new Uri(xdoc.SelectSingleNode("/response/url").InnerText);
            u.WebUrl = new Uri(xdoc.SelectSingleNode("/response/web-url").InnerText);
            u.Id = int.Parse(xdoc.SelectSingleNode("/response/id").InnerText);

            #region extend
            UserExtention ux = new UserExtention();
            ux.NetworkName = xdoc.SelectSingleNode("/response/network-name").InnerText;
            ux.NetworkId = int.Parse(xdoc.SelectSingleNode("/response/network-id").InnerText);
            ux.BirthDate = xdoc.SelectSingleNode("/response/birth-date").InnerText;
            ux.HireDate = DateTime.ParseExact(xdoc.SelectSingleNode("/response/hire-date").InnerText, "yyyy-MM-dd", null);
            Status s = new Status();
            s.Followers = int.Parse(xdoc.SelectSingleNode("/response/stats/followers").InnerText);
            s.Following = int.Parse(xdoc.SelectSingleNode("/response/stats/following").InnerText);
            s.Updates = int.Parse(xdoc.SelectSingleNode("/response/stats/updates").InnerText);
            ux.Status = s;

            Location l = new Location();
            //l.Id = int.Parse(xdoc.SelectSingleNode("/response/location/id").InnerText);
            //l.UserId = int.Parse(xdoc.SelectSingleNode("/response/location/user-id").InnerText);
            //l.Name = xdoc.SelectSingleNode("/response/location/name").InnerText;
            //l.Value = xdoc.SelectSingleNode("/response/location/value").InnerText;
            //l.CreatedAt = DateTime.ParseExact(
            //    xdoc.SelectSingleNode("/response/location/created-at").InnerText,
            //    "yyyy-MM-ddTHH:mm:ssZ", null);
            //l.UpdatedAt = DateTime.ParseExact(
            //    xdoc.SelectSingleNode("/response/location/updated-at").InnerText,
            //    "yyyy-MM-ddTHH:mm:ssZ", null);
            ux.Location = l;

            Contact c = new Contact();
            List<EmailAddress> emails = new List<EmailAddress>();
            foreach (XmlNode aNode in xdoc.SelectNodes("/response/contact/email-address"))
            {
                EmailAddress email = new EmailAddress();
                email.Type = aNode.SelectSingleNode("type").InnerText;
                email.Address = aNode.SelectSingleNode("address").InnerText;
                emails.Add(email);
            }
            c.EmailAddresses = emails;
            List<PhoneNumber> phones = new List<PhoneNumber>();
            foreach (XmlNode pNode in xdoc.SelectNodes("/response/contact/phone-number"))
            {
                PhoneNumber phone = new PhoneNumber();
                phone.Type = pNode.SelectSingleNode("type").InnerText;
                phone.Number = pNode.SelectSingleNode("number").InnerText;
                phones.Add(phone);                
            }
            c.PhoneNumbers = phones;
            Im im = new Im();
            im.UserName = xdoc.SelectSingleNode("/response/contact/im/username").InnerText;
            im.Provider = xdoc.SelectSingleNode("/response/contact/im/provider").InnerText;
            c.Im = im;            
            ux.Contact = c;

            u.Extention = ux;
            #endregion

            #endregion

            #region case LINQ
            //var xml = XElement.Parse(Encoding.UTF8.GetString(data));
            //var result = from response in xml.Root.Elements()
            //             select new User
            //             {
            //                 Id = int.Parse(response.Element("id").Value),
            //                 NetworkName = response.Element("network-name").Value,
            //                 Name = response.Element("name").Value,
            //                 FullName = response.Element("full-name").Value,
            //                 JobTitle = response.Element("job-title").Value,
            //                 NetworkId = int.Parse(response.Element("network-id").Value),
            //                 BirthDate = response.Element("birth-date").Value,
            //                 Url = new Uri(response.Element("url").Value),
            //                 MugshotUrl = new Uri(response.Element("mugshot-url").Value),
            //                 HireDate = DateTime.ParseExact(response.Element("hire-date").Value, "yyyy-MM-dd", null),
            //                 WebUrl = new Uri(response.Element("web-url").Value),
            //             };
            #endregion

            return u;
        }

    }
}
