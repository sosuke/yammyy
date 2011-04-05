using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using System.Web;
using System.IO;
using System.Xml;
using OAuth;
using System.Collections.Specialized;

namespace Yammer
{
    public class Messages
    {
        public Session Session { get; set; }

        public Messages(Session session)
        {
            this.Session = session;
        }

        public MessageObjects GetAllMessage()
        {                      
            return GetMessageObjects(Method.Get(Resources.MESSAGES_ALL, this.Session));
        }
        public MessageObjects GetAllMessage(int newer_than)
        {
            return GetMessageObjects(Method.Get(Resources.MESSAGES_ALL + "?newer_than=" + newer_than.ToString(), this.Session));
        }
        public MessageObjects GetSentMessage()
        {
            return GetMessageObjects(Method.Get(Resources.MESSAGES_SENT, this.Session));
        }

        public MessageObjects GetReceivedMessage()
        {
            return GetMessageObjects(Method.Get(Resources.MESSAGES_RECEIVED, this.Session));
        }

        public MessageObjects GetFollowingMessage()
        {
            return GetMessageObjects(Method.Get(Resources.MESSAGES_FOLLOWING, this.Session));;
        }

        public string GetMessageByUser(int id)
        {
            string szReq = Resources.MESSAGES_FROMUSER + id.ToString() + ".xml";
            return szReq;
        }

        public string GetMessageByTag(int id)
        {
            string szReq = Resources.MESSAGES_TAGGEDWITH + id.ToString() + ".xml";
            return szReq;
        }

        public string getMessageByThread(int id)
        {
            string szReq = Resources.MESSAGES_INTHREAD + id.ToString() + ".xml";
            return szReq;
        }

        public void PostMessage(string body)
        {
            string szReq = Resources.MESSAGES_POST;
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("body", body);
            Method.Post(szReq, parameters, this.Session);
        }

        public void PostMessage(string body, int reply)
        {
            string szReq = Resources.MESSAGES_POST;
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("body", body);
            parameters.Add("replied_to_id", reply.ToString());
            Method.Post(szReq, parameters, this.Session);
        }

        public void DeleteMessage(int id)
        {
            string szReq = Resources.MESSAGES_DELETE + id.ToString();
            Method.Delete(szReq, this.Session);
        }

        private MessageObjects GetMessageObjects(string data)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(data);

            MessageObjects objects = new MessageObjects();
            List<Message> messages = new List<Message>();
            foreach (XmlNode mnode in xdoc.SelectNodes("/response/messages/message"))
            {
                Message message = new Message();
                message.WebUrl = new Uri(mnode.SelectSingleNode("web-url").InnerText);
                message.ClientType = mnode.SelectSingleNode("client-type").InnerText;
                MessageBody mb = new MessageBody();
                mb.Parsed = mnode.SelectSingleNode("body/parsed").InnerText;
                mb.Plain = mnode.SelectSingleNode("body/plain").InnerText;
                List<Uri> urls = new List<Uri>();
                foreach (XmlNode unode in mnode.SelectNodes("urls/url"))
                {
                    Uri u = new Uri(unode.InnerText);
                    urls.Add(u);
                }
                mb.Urls = urls;
                message.Body = mb;
                message.SenderType = (SenderType)Enum.Parse(Type.GetType("Yammer.SenderType"), mnode.SelectSingleNode("sender-type").InnerText.ToUpper());
                if (mnode.SelectSingleNode("client-url").InnerText != string.Empty)
                {
                    message.ClientUrl = new Uri(mnode.SelectSingleNode("client-url").InnerText);
                }
                message.Id = int.Parse(mnode.SelectSingleNode("id").InnerText);
                message.IsSystemMessage = (mnode.SelectSingleNode("system-message").InnerText == "true");
                message.Url = new Uri(mnode.SelectSingleNode("url").InnerText);
                message.ThreadId = int.Parse(mnode.SelectSingleNode("thread-id").InnerText);
                if (mnode.SelectSingleNode("replied-to-id").InnerText != string.Empty)
                {
                    message.RepliedToId = int.Parse(mnode.SelectSingleNode("replied-to-id").InnerText);
                }
                message.SenderId = int.Parse(mnode.SelectSingleNode("sender-id").InnerText);
                message.MessageType = (MessageType)Enum.Parse(Type.GetType("Yammer.MessageType"), mnode.SelectSingleNode("message-type").InnerText.ToUpper());
                message.CreatedAt = DateTime.Parse(mnode.SelectSingleNode("created-at").InnerText);

                messages.Add(message);
            }
            
            List<Reference> references = new List<Reference>();
            foreach (XmlNode rnode in xdoc.SelectNodes("/response/references/reference"))
            {
                Reference reference = new Reference();
                reference.Id = int.Parse(rnode.SelectSingleNode("id").InnerText);
                reference.ObjectType = (ObjectType)Enum.Parse(Type.GetType("Yammer.ObjectType"), rnode.SelectSingleNode("type").InnerText.ToUpper());
                switch (reference.ObjectType)
                {
                    case ObjectType.MESSAGE:
                        Message msg = new Message();
                        msg.Id = reference.Id;
                        msg.WebUrl = new Uri(rnode.SelectSingleNode("web-url").InnerText);
                        msg.SenderType = (SenderType)Enum.Parse(Type.GetType("Yammer.SenderType"), rnode.SelectSingleNode("sender-type").InnerText.ToUpper());
                        msg.Url = new Uri(rnode.SelectSingleNode("url").InnerText);
                        msg.ThreadId = int.Parse(rnode.SelectSingleNode("thread-id").InnerText);
                        if (rnode.SelectSingleNode("replied-to-id").InnerText != string.Empty)
                        {
                            msg.RepliedToId = int.Parse(rnode.SelectSingleNode("replied-to-id").InnerText);
                        }                        
                        msg.SenderId = int.Parse(rnode.SelectSingleNode("sender-id").InnerText);
                        msg.CreatedAt = DateTime.Parse(rnode.SelectSingleNode("created-at").InnerText);
                        msg.Body = new MessageBody { Plain = rnode.SelectSingleNode("body/plain").InnerText };
                        reference.Object = msg;
                        break;
                    case ObjectType.USER:
                        User usr = new User();
                        usr.Id = reference.Id;
                        usr.FullName = rnode.SelectSingleNode("full-name").InnerText;
                        usr.JobTitle = rnode.SelectSingleNode("job-title").InnerText;
                        usr.MugshotUrl = new Uri(rnode.SelectSingleNode("mugshot-url").InnerText);
                        usr.Name = rnode.SelectSingleNode("name").InnerText;
                        usr.Url = new Uri(rnode.SelectSingleNode("url").InnerText);
                        reference.Object = usr;
                        break;
                    case ObjectType.TAG:
                        Tag tag = new Tag();
                        tag.Id = reference.Id;
                        tag.WebUrl = new Uri(rnode.SelectSingleNode("web-url").InnerText);
                        tag.Name = rnode.SelectSingleNode("name").InnerText;
                        tag.Url = new Uri(rnode.SelectSingleNode("url").InnerText);
                        reference.Object = tag;
                        break;
                    case ObjectType.THREAD:
                        Thread thread = new Thread();
                        thread.Id = reference.Id;
                        thread.Url = new Uri(rnode.SelectSingleNode("url").InnerText);
                        thread.WebUrl = new Uri(rnode.SelectSingleNode("web-url").InnerText);
                        Stats stats = new Stats();
                        stats.Updates = int.Parse(rnode.SelectSingleNode("stats/updates").InnerText);
                        if (rnode.SelectSingleNode("stats/latest-reply-at").InnerText != string.Empty)
                        {
                            stats.LatestReplyAt = DateTime.Parse(rnode.SelectSingleNode("stats/latest-reply-at").InnerText);
                        }
                        thread.Stats = stats;
                        reference.Object = thread;
                        break;
                    default:
                        // NOP
                        break;
                }
                references.Add(reference);
                
            }

            objects.Messages = messages;
            objects.References = references;

            return objects;
        }
    }


}
