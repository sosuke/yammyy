using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yammer
{
    public class Tags
    {
        public Session Session { get; set; }

        public Tags(Session session)
        {
            this.Session = session;
        }

        // NOP
        public List<Tag> GetTags()
        {
            List<Tag> tags = new List<Tag>();
            string data = Method.Get(Resources.TAGS_ALL, this.Session);
            // XML Parse
            return tags;
        }

        // NOP
        public Tag GetTag(int id)
        {
            Tag tag = new Tag();
            string szReq = Resources.TAGS_TARGET + id.ToString() + ".xml";
            string data = Method.Get(szReq, this.Session);
            // XML Parse
            return tag;
        }
    }
}
