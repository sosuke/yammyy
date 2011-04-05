using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;

namespace Yammer
{
    public class Users
    {
        private Session Session { get; set; }
        

        public Users(Session session)        
        {
            this.Session = session;
        }

        public User GetCurrentUser()
        {
            return this.Session.CurrentUser;
        }

        public User GetUserByName(string name)
        {
            User u = new User();
            return u;
        }

        public User GetUserById(string id)
        {
            int me = this.Session.CurrentUser.Id;
            string szReq = Resources.USERS_TARGET + me.ToString() + ".xml";
            string data = Method.Get(szReq, this.Session);

            User u = new User();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(data);
            
            return u;
        }

        public List<User> GetAllUsers()
        {
            string data = Method.Get(Resources.USERS_ALL, this.Session);
            
            List<User> users = new List<User>();

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(data);
            foreach (XmlNode node in xdoc.SelectNodes("/response/response"))
            {
                User u = new User();

                u.JobTitle = node.SelectSingleNode("job-title").InnerText;
                u.FullName = node.SelectSingleNode("full-name").InnerText;
                u.MugshotUrl = new Uri(node.SelectSingleNode("mugshot-url").InnerText);
                u.Name = node.SelectSingleNode("name").InnerText;
                u.Url = new Uri(node.SelectSingleNode("url").InnerText);
                u.WebUrl = new Uri(node.SelectSingleNode("web-url").InnerText);
                u.Id = int.Parse(node.SelectSingleNode("id").InnerText);
                u.Extention = null;

                users.Add(u);
            }

            return users;
        }

        public List<User> GetAllUsers(int page)
        {
            string szReq = Resources.USERS_ALL + "?page=" + page.ToString();
            string data = Method.Get(szReq, this.Session);

            List<User> users = new List<User>();

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(data);
            foreach (XmlNode node in xdoc.SelectNodes("/response/response"))
            {
                User u = new User();

                u.JobTitle = node.SelectSingleNode("job-title").InnerText;
                u.FullName = node.SelectSingleNode("full-name").InnerText;
                u.MugshotUrl = new Uri(node.SelectSingleNode("mugshot-url").InnerText);
                u.Name = node.SelectSingleNode("name").InnerText;
                u.Url = new Uri(node.SelectSingleNode("url").InnerText);
                u.WebUrl = new Uri(node.SelectSingleNode("web-url").InnerText);
                u.Id = int.Parse(node.SelectSingleNode("id").InnerText);
                u.Extention = null;

                users.Add(u);
            }

            return users;
        }

    }
}
