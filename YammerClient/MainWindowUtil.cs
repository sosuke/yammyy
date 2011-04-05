using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Net;
using System.Windows.Threading;
using System.IO;
using Yammer;
using OAuth;
using System.Xml;
using System.Security.Cryptography;

namespace Yammer.Client
{
    public partial class MainWindow : Window
    {
        private const string CONSUMER_KEY = "nIpQqqWQzQuxEIh9q9omow";
        private const string CONSUMER_SECRET = "NfTjcmGAnSHI3bQkKAbxDauKvczMQHQ92vSEb6zLNrQ";
        private const string SETTING_FILE_NAME = "settings.xml";


        private void SaveConfiguration()
        {
            File.Delete(SETTING_FILE_NAME);
            XmlTextWriter writer = new XmlTextWriter(SETTING_FILE_NAME, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument();
            writer.WriteStartElement("settings");

            writer.WriteStartElement("proxy");
            if (AuthPanel.Proxy == null)
            {
                writer.WriteAttributeString("enable", "false");
                writer.WriteElementString("address", "");
                writer.WriteElementString("id", "");
                writer.WriteElementString("password", "");
            }
            else
            {
                writer.WriteAttributeString("enable", "true");
                writer.WriteElementString("address", AuthPanel.Proxy.Address.ToString());
                NetworkCredential cr = (NetworkCredential)AuthPanel.Proxy.Credentials;
                writer.WriteElementString("id", cr.UserName);
                writer.WriteElementString("password", EncryptString(cr.Password, "yammyy"));
            }
            writer.WriteEndElement(); // End of proxy

            writer.WriteStartElement("oauth");
            writer.WriteElementString("tokenkey", AuthPanel.OAuthKey.TokenKey);
            writer.WriteElementString("tokensecret", EncryptString(AuthPanel.OAuthKey.TokenSecret, "yammyy"));
            writer.WriteEndElement(); // End of oauth

            writer.WriteEndElement(); //End of settings
            writer.WriteEndDocument();
            writer.Close();

            FileAttributes fas = File.GetAttributes(SETTING_FILE_NAME);
            fas |= FileAttributes.Hidden;
            File.SetAttributes(SETTING_FILE_NAME, fas);
        }
        private WebProxy GetProxy()
        {
            WebProxy proxy = new WebProxy();

            if (!File.Exists(SETTING_FILE_NAME))
            {
                return null;
            }
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(SETTING_FILE_NAME);
                if (xdoc.SelectSingleNode("/settings/proxy/@enable").InnerText != "true")
                {
                    return null;
                }
                proxy.Address = new Uri(xdoc.SelectSingleNode("/settings/proxy/address").InnerText);
                proxy.Credentials =
                    new NetworkCredential
                    {
                        UserName = xdoc.SelectSingleNode("/settings/proxy/id").InnerText,
                        Password = DecryptString(xdoc.SelectSingleNode("/settings/proxy/password").InnerText, "yammyy")
                    };
            }
            catch (Exception)
            {
                return null;
            }
            return proxy;
        }

        private OAuthKey GetOAuthKey()
        {
            OAuthKey key = new OAuthKey();

            if (!File.Exists(SETTING_FILE_NAME))
            {
                return null;
            }
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(SETTING_FILE_NAME);
                key.TokenKey = xdoc.SelectSingleNode("/settings/oauth/tokenkey").InnerText;
                key.TokenSecret = DecryptString(xdoc.SelectSingleNode("/settings/oauth/tokensecret").InnerText, "yammyy");
            }
            catch (Exception)
            {
                return null;
            }
            key.ConsumerKey = CONSUMER_KEY;
            key.ConsumerSecret = CONSUMER_SECRET;
            return key;
        }

        /// <summary>
        /// refer http://dobon.net/vb/dotnet/string/encryptstring.html
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string EncryptString(string str, string key)
        {
            byte[] bytesIn = Encoding.UTF8.GetBytes(str);

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] bytesKey = Encoding.UTF8.GetBytes(key);
            des.Key = ResizeBytesArray(bytesKey, des.Key.Length);
            des.IV = ResizeBytesArray(bytesKey, des.IV.Length);

            MemoryStream msOut = new MemoryStream();
            ICryptoTransform desdecrypt = des.CreateEncryptor();
            CryptoStream cryptStreem = new CryptoStream(msOut, desdecrypt, CryptoStreamMode.Write);
            cryptStreem.Write(bytesIn, 0, bytesIn.Length);
            cryptStreem.FlushFinalBlock();
            byte[] bytesOut = msOut.ToArray();

            cryptStreem.Close();
            msOut.Close();

            return Convert.ToBase64String(bytesOut);
        }

        /// <summary>
        /// refer http://dobon.net/vb/dotnet/string/encryptstring.html
        /// </summary>
        /// <param name="str"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string DecryptString(string str, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] bytesKey = Encoding.UTF8.GetBytes(key);
            des.Key = ResizeBytesArray(bytesKey, des.Key.Length);
            des.IV = ResizeBytesArray(bytesKey, des.IV.Length);

            byte[] bytesIn = Convert.FromBase64String(str);
            MemoryStream msIn = new MemoryStream(bytesIn);
            ICryptoTransform desdecrypt = des.CreateDecryptor();

            CryptoStream cryptStreem = new CryptoStream(msIn, desdecrypt, CryptoStreamMode.Read);

            StreamReader srOut = new StreamReader(cryptStreem, Encoding.UTF8);
            string result = srOut.ReadToEnd();

            srOut.Close();
            cryptStreem.Close();
            msIn.Close();

            return result;
        }

        /// <summary>
        /// refer http://dobon.net/vb/dotnet/string/encryptstring.html
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="newSize"></param>
        /// <returns></returns>
        private static byte[] ResizeBytesArray(byte[] bytes, int newSize)
        {
            byte[] newBytes = new byte[newSize];
            if (bytes.Length <= newSize)
            {
                for (int i = 0; i < bytes.Length; i++)
                    newBytes[i] = bytes[i];
            }
            else
            {
                int pos = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    newBytes[pos++] ^= bytes[i];
                    if (pos >= newBytes.Length)
                        pos = 0;
                }
            }
            return newBytes;
        }
    }
}
