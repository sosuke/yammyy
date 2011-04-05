using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yammer
{
    /// <summary>
    /// Yammer Client Profile Information
    /// </summary>
    public class ClientProfile
    {
        public string Name { get; set; }
        public string Nonce { get; set; }
        public string Hexdigest { get; set; }
    }

    /// <summary>
    /// Yammer EmailAddress
    /// </summary>
    public class EmailAddress
    {
        public string Type { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// Yammer PhoneNumber
    /// </summary>
    public class PhoneNumber
    {
        public string Type { get; set; }
        public string Number { get; set; }
    }

    /// <summary>
    /// Yammer Im
    /// </summary>
    public class Im
    {
        public string UserName { get; set; }
        public string Provider { get; set; }
    }

    /// <summary>
    /// Yammer User Contact
    /// </summary>
    public class Contact
    {
        public List<EmailAddress> EmailAddresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
        public Im Im { get; set; }
    }

    /// <summary>
    /// Yammer User Status
    /// </summary>
    public class Status
    {
        public int Updates { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
    }

    /// <summary>
    /// Yammer User Location
    /// </summary>
    public class Location
    {
        public DateTime CreatedAt { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Yammer User Information : Extention
    /// </summary>
    public class UserExtention
    {
        public DateTime HireDate { get; set; }
        public int NetworkId { get; set; }
        public string NetworkName { get; set; }
        public string BirthDate { get; set; }
        public Status Status { get; set; }
        public Location Location { get; set; }
        public Contact Contact { get; set; }
        //public List<string> KidsNames { get; set; }
        //public string SignificantOther { get; set; }
        //public string WebPreferences { get; set; }      
    }

    /// <summary>
    /// Yammer User Information : Base
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public Uri Url { get; set; }
        public Uri MugshotUrl { get; set; }
        public Uri WebUrl { get; set; }
        public UserExtention Extention { get; set; }
    }

    /// <summary>
    /// Yammer Authentication
    /// </summary>
    public class Authentication
    {
        public string UserID { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Yammer Tag
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Uri Url { get; set; }
        public Uri WebUrl { get; set; }
    }
    public class Stats
    {
        public int Updates { get; set; }
        public DateTime LatestReplyAt { get; set; }
    }

    public class Thread
    {
        public int Id { get; set; }
        public Uri WebUrl { get; set; }
        public Uri Url { get; set; }
        public Stats Stats { get; set; }
    }

    public enum SenderType
    {
        USER,
        SYSTEM
    }
    public enum MessageType
    {
        SYSTEM,
        UPDATE
    }
    public enum ObjectType
    {
        MESSAGE,
        USER,
        TAG,
        THREAD,
        GROUP
    }

    public class Reference
    {
        public int Id { get; set; }
        public ObjectType ObjectType { get; set; }
        public Object Object { get; set; }

    }

    public class MessageBody
    {
        public string Parsed { get; set; }
        public string Plain { get; set; }
        public List<Uri> Urls { get; set; }
    }

    public class Message
    {
        public Uri WebUrl { get; set; }
        public string ClientType { get; set; }
        public MessageBody Body { get; set; }
        public SenderType SenderType { get; set; }
        public Uri ClientUrl { get; set; }
        public int Id { get; set; }
        public int MyProperty { get; set; }
        public bool IsSystemMessage { get; set; }
        public Uri Url { get; set; }
        public int ThreadId { get; set; }
        public int RepliedToId { get; set; }
        public int SenderId { get; set; }
        public MessageType MessageType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MessageObjects
    {
        public List<Reference> References { get; set; }
        public List<Message> Messages { get; set; }
    }
}
