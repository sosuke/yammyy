using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yammer
{
    /// <summary>
    /// Yammer REST ENTRY
    /// </summary>
    static class Resources
    {
        // OAUTH
        public static string OAUTH_REQUEST_TOKEN = "https://www.yammer.com/oauth/request_token";
        public static string OAUTH_AUTHORIZE     = "https://www.yammer.com/oauth/authorize";
        public static string OAUTH_ACCESSTOKEN = "https://www.yammer.com/oauth/access_token";        

        // USER
        public static string USERS_CURRENT = "https://www.yammer.com/api/v1/users/current.xml";
        public static string USERS_ALL     = "https://www.yammer.com/api/v1/users.xml";
        public static string USERS_TARGET  = "https://www.yammer.com/api/v1/users/";

        // MESSAGES
        public static string MESSAGES_POST       = "https://www.yammer.com/api/v1/messages/";
        public static string MESSAGES_DELETE     = "https://www.yammer.com/api/v1/messages/";
        public static string MESSAGES_ALL        = "https://www.yammer.com/api/v1/messages.xml";
        public static string MESSAGES_SENT       = "https://www.yammer.com/api/v1/messages/sent.xml";
        public static string MESSAGES_RECEIVED   = "https://www.yammer.com/api/v1/messages/received.xml";
        public static string MESSAGES_FOLLOWING  = "https://www.yammer.com/api/v1/messages/following.xml";
        public static string MESSAGES_FROMUSER   = "https://www.yammer.com/api/v1/messages/from_user/";
        public static string MESSAGES_TAGGEDWITH = "https://www.yammer.com/api/v1/messages/tagged_with/";
        public static string MESSAGES_INTHREAD   = "https://www.yammer.com/api/v1/messages/in_thread/";

        // TAGS
        public static string TAGS_ALL    = "https://www.yammer.com/api/v1/tags.xml";
        public static string TAGS_TARGET = "https://www.yammer.com/api/v1/tags/";

        // SUBSCRIPTIONS
        public static string SUBSCRIPTIONS_TOUSER = "https://www.yammer.com/api/v1/subscriptions/to_user/";
        public static string SUBSCRIPTIONS_TOTAG  = "https://www.yammer.com/api/v1/subscriptions/to_tag/";
        public static string SUBSCRIPTIONS_POST   = "https://www.yammer.com/api/v1/subscriptions/";
        public static string SUBSCRIPTIONS_DELETE = "https://www.yammer.com/api/v1/subscriptions/";


    }
}
