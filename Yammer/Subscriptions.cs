using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Yammer
{
    public class Subscriptions
    {
        public Session Session { get; set; }

        public Subscriptions(Session session)
        {
            this.Session = session;
        }

        public bool IsSubscribeUser(int id)
        {
            id = 99999;
            string szReq = Resources.SUBSCRIPTIONS_TOUSER + id.ToString() + ".xml";
            string data = Method.Get(szReq, this.Session);
            // XML Parse
            return true;
        }

        public bool IsSubscribeTag(int id)
        {
            string szReq = Resources.SUBSCRIPTIONS_TOTAG + id.ToString() + ".xml";
            string data = Method.Get(szReq, this.Session);
            // XML Parse
            return true;
        }

        public void SubscribeToUser(int id)
        {
            string szReq = Resources.SUBSCRIPTIONS_POST;
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("target_type", "user");
            parameters.Add("target_id", id.ToString());

            string data = Method.Post(szReq, parameters, this.Session);
        }

        public void SubscribeToTag(int id)
        {
            string szReq = Resources.SUBSCRIPTIONS_POST;
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("target_type", "tag");
            parameters.Add("target_id", id.ToString());
            string data = Method.Post(szReq, parameters, this.Session);
        }

        public void DeleteSubscriptionToUser(int id)
        {
            string szReq = Resources.SUBSCRIPTIONS_DELETE +
                "?target_type=user" +
                "&target_id=" + id.ToString();
            string data = Method.Delete(szReq, this.Session);
        }

        public void DeleteSubscriptionToTag(int id)
        {
            string szReq = Resources.SUBSCRIPTIONS_DELETE +
                "?target_type=user" +
                "&target_id=" + id.ToString();
            string data = Method.Delete(szReq, this.Session);
        }
    }
}
