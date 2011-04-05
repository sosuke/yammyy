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
using System.Diagnostics;

namespace Yammer.Client
{
    /// <summary>
    /// MainWindow Class
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.NotifyIcon taskIcon;
        private bool IsPostPanelExpand { get; set; }
        private bool IsFunctionPanelExpand { get; set; }
        private bool IsMessagePostPanelExpand { get; set; }
        private Session Session { get; set; }

        private int MostLastestMessageId { get; set; }
        DispatcherTimer FrameTimer = new DispatcherTimer();


        public MainWindow()
        {
            InitializeComponent();

            this.IsPostPanelExpand = true;
            this.IsFunctionPanelExpand = true;
            this.IsMessagePostPanelExpand = true;

            this.AuthPanel.OkButton.Click += new RoutedEventHandler(OkButton_Click);
            this.AuthPanel.PrevButton.Click += new RoutedEventHandler(PrevButton_Click);
            this.AuthPanel.NextButton.Click += new RoutedEventHandler(NextButton_Click);

            this.ReplyPanel.ReplyButton.Click += new RoutedEventHandler(ReplyPanelReplyButton_Click);
            this.ReplyPanel.CancelButton.Click += new RoutedEventHandler(ReplyPanelCancelButton_Click);
            

            #region Task Tray 
            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            int Screenheigth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            int AppWidth = (int)this.Width;
            int AppHeight = (int)this.Height;
            Canvas.SetLeft(this, ScreenWidth - AppWidth);  //ScreenWidth / 2 - AppWidth / 2;
            Canvas.SetTop(this, Screenheigth - AppHeight); //Screenheigth / 2 - AppHeight / 2;
            this.StateChanged += new EventHandler(MainWindow_StateChanged);
            this.taskIcon = new System.Windows.Forms.NotifyIcon();
            this.taskIcon.Visible = true;
            this.taskIcon.Icon = Properties.Resources.YammyyIcon;                     
            this.taskIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(taskIcon_MouseClick);
            this.taskIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem item = new System.Windows.Forms.MenuItem("&Exit", taskIconItemExit_Click);
            this.taskIcon.ContextMenu.MenuItems.Add(item);
            #endregion


            
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(InitialCheck));
            th.Start();
        }

        void AuthorizeLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Auth auth = new Auth();
            string authurl = auth.GetAuthorizeQuery(AuthPanel.OAuthKey.TokenKey, null);
            Process.Start(authurl);
        }


        private void InitialCheck()
        {            
            OAuthKey oauth = GetOAuthKey();
            WebProxy proxy = GetProxy();

            //oauth = null; // tmp
            if (oauth == null)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new AuthPanelShowDelegate(AuthPanelShow));
            }
            try
            {
                Session session = new Session(oauth, proxy);
                this.Session = session;
                Messages messages = new Messages(this.Session);
                MessageObjects allobjects = messages.GetAllMessage();
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new MainPanelShowDelegate(MainPanelShow), allobjects);
            }
            catch (Exception)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new AuthPanelShowDelegate(AuthPanelShow));
            }           
        }

        delegate void AuthPanelShowDelegate();
        private void AuthPanelShow()
        {
            this.AuthPanel.Visibility = Visibility.Visible;
            this.ProgressPanel.Visibility = Visibility.Collapsed;
        }

        delegate void MainPanelShowDelegate(MessageObjects messages);
        private void MainPanelShow(MessageObjects messages)
        {
            string szNetwork = this.Session.CurrentUser.Extention.NetworkName;
            CompanyName.Text = szNetwork;
            LogoImage.Visibility = Visibility.Visible;
            RefreshAllMessagePanel(messages);
            this.ProgressPanel.Visibility = Visibility.Collapsed;
            this.AuthPanel.Visibility = Visibility.Collapsed;
            this.PanelContainer.Visibility = Visibility.Visible;

            #region check cycle test
            FrameTimer.Tick += new EventHandler(FrameTimer_Tick);
            FrameTimer.Interval = TimeSpan.FromMinutes(1.00);
            FrameTimer.Start();
            #endregion
        }

        void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            switch (AuthPanel.PanelStatus)
            {
                case 1:    // Intro
                    break;
                case 2:    // Proxy
                    AuthPanel.ProxySetting.Visibility = Visibility.Collapsed;
                    AuthPanel.Intro.Visibility = Visibility.Visible;
                    AuthPanel.PanelStatus--;
                    break;
                case 3:    // OAuth
                    AuthPanel.OAuthSetting.Visibility = Visibility.Collapsed;
                    AuthPanel.ProxySetting.Visibility = Visibility.Visible;
                    AuthPanel.PanelStatus--;
                    break;
                case 4:    // Fin
                    break;
            }
        }

        delegate void ThreadMethodDelegate(WebProxy proxy); 

        void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (AuthPanel.PanelStatus)
            {
                case 1:    // Intro
                    AuthPanel.Intro.Visibility = Visibility.Collapsed;
                    AuthPanel.ProxySetting.Visibility = Visibility.Visible;
                    AuthPanel.PanelStatus++;
                    break;
                case 2:    // Proxy
                    StatusMessage.Text = "Connection Check...";
                    ProgressPanel.Visibility = Visibility.Visible;
                    AuthPanel.ProxySetting.Opacity = 0.5;
                    WebProxy proxy = null;
                    if ((bool)AuthPanel.ProxyUse.IsChecked)
                    {
                        proxy = new WebProxy();
                        proxy.Address = new Uri(AuthPanel.ProxyAddress.Text);
                        proxy.Credentials = new NetworkCredential { UserName = AuthPanel.ProxyId.Text, Password = AuthPanel.ProxyPassword.Password };
                    }
                    ThreadMethodDelegate threadMethodDelegate = new ThreadMethodDelegate(ConnectionCheck);
                    threadMethodDelegate.BeginInvoke(proxy, null, null);
                    //System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(ConnectionCheck));
                    //th.Start();
                    break;
                case 3:    // OAuth
                    StatusMessage.Text = "Authorize Check...";
                    ProgressPanel.Visibility = Visibility.Visible;
                    AuthPanel.OAuthSetting.Opacity = 0.5;
                    System.Threading.Thread th2 = new System.Threading.Thread(new System.Threading.ThreadStart(ConfigurationCheck));
                    th2.Start();                    
                    break;
                case 4:    // Fin
                    break;
            }
        }
        private void ConnectionCheck(WebProxy proxy)
        {
            bool bIsSuccess = true;
            OAuthKey key = null;
            Auth auth = new Auth();
            string query = auth.GetRequestTokenQuery(CONSUMER_KEY, CONSUMER_SECRET);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "GET";
            req.PreAuthenticate = true;
            //req.Accept = "text/xml, application/xml";
            req.Proxy = proxy;

            try 
            {	        
                WebResponse res = req.GetResponse();
                StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string data = reader.ReadToEnd();
                reader.Close();
                res.Close();

                string szTokeyKey = string.Empty;
                string szTokenSecret = string.Empty;
                #region url parse 
                string[] queryset = data.Split('&');
                szTokeyKey = queryset[0].Split('=')[1];
                szTokenSecret = queryset[1].Split('=')[1];  
                #endregion

                key = new OAuthKey();
                key.ConsumerKey = CONSUMER_KEY;
                key.ConsumerSecret = CONSUMER_SECRET;
                key.TokenKey = szTokeyKey;
                key.TokenSecret = szTokenSecret;

                bIsSuccess = true;
            }            
            catch (Exception)
            {
                bIsSuccess = false;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new ConnectionCheckCompletedDelegate(ConnectionCheckCompleted), bIsSuccess,proxy, key);
        }

        delegate void ConnectionCheckCompletedDelegate(bool IsSuccess, WebProxy proxy, OAuthKey key);
        private void ConnectionCheckCompleted(bool IsSuccess, WebProxy proxy, OAuthKey key)
        {
            AuthPanel.ProxySetting.Opacity = 1.0;
            ProgressPanel.Visibility = Visibility.Collapsed;
            if (IsSuccess)
            {
                AuthPanel.Proxy = proxy;
                AuthPanel.OAuthKey = key;
                AuthPanel.ProxySetting.Visibility = Visibility.Collapsed;
                AuthPanel.OAuthSetting.Visibility = Visibility.Visible;
                AuthPanel.PanelStatus++;

                this.AuthPanel.AuthorizeLink.MouseDown += new MouseButtonEventHandler(AuthorizeLink_MouseDown);
                //this.AuthPanel.AuthorizeLink.MouseLeftButtonDown += new MouseButtonEventHandler(AuthorizeLink_MouseDown);
            }
            else
            {
                MessageBox.Show("Connection Failed.");
            }
        }
        private void ConfigurationCheck()
        {
            bool bIsSuccess = true;
            
            Auth auth = new Auth();
            string query = auth.GetAccessTokenQuery(CONSUMER_KEY, CONSUMER_SECRET, AuthPanel.OAuthKey.TokenKey, AuthPanel.OAuthKey.TokenSecret);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(query);
            req.Method = "POST";
            req.PreAuthenticate = true;
            req.Proxy = AuthPanel.Proxy;

            try
            {
                WebResponse res = req.GetResponse();
                StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string data = reader.ReadToEnd();
                reader.Close();
                res.Close();

                string szTokenKey = string.Empty;
                string szTokenSecret = string.Empty;
                #region url parse
                string[] queryset = data.Split('&');
                szTokenKey = queryset[0].Split('=')[1];
                szTokenSecret = queryset[1].Split('=')[1];
                #endregion

                AuthPanel.OAuthKey.TokenKey = szTokenKey;
                AuthPanel.OAuthKey.TokenSecret = szTokenSecret;
                bIsSuccess = true;
            }
            catch (Exception)
            {
                bIsSuccess = false;
            }

            SaveConfiguration();

            this.Dispatcher.Invoke(DispatcherPriority.Normal, new ConfigurationCompletedDelegate(ConfigurationCompleted), bIsSuccess);                
        }
        delegate void ConfigurationCompletedDelegate(bool IsSuccess);
        private void ConfigurationCompleted(bool IsSuccess)
        {
            ProgressPanel.Visibility = Visibility.Collapsed;
            AuthPanel.OAuthSetting.Opacity = 1.0;
            if (IsSuccess)
            {
                AuthPanel.OAuthSetting.Visibility = Visibility.Collapsed;
                AuthPanel.Final.Visibility = Visibility.Visible;            
                AuthPanel.PanelStatus++;
                AuthPanel.OkButton.Visibility = Visibility.Visible;
                AuthPanel.PrevButton.Visibility = Visibility.Collapsed;
                AuthPanel.NextButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Authentication Failed.");
            }
        }

        void OkButton_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(InitialCheck));
            th.Start();
        }

        void FrameTimer_Tick(object sender, EventArgs e)
        {
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(FeedCycleCheck));
            th.Start();
        }

        private void FeedCycleCheck()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects objects = messages.GetAllMessage(this.MostLastestMessageId);
            if (objects.Messages.Count > 0)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, new FeedCheckerDelegate(FeedCheckerShow), objects);                
            }
        }
        delegate void FeedCheckerDelegate(MessageObjects objects);
        private void FeedCheckerShow(MessageObjects objects)
        {
            NotifyWindow notify = new NotifyWindow();
            Message message = objects.Messages[0];
            this.MostLastestMessageId = message.Id;
            notify.BodyText = message.Body.Plain;
            Reference refer = objects.References.Find(o => o.Id == message.SenderId && o.ObjectType == ObjectType.USER);
            User user = (User)refer.Object;
            notify.UserId = user.FullName;
            if (this.Session.Proxy != null)
            {
                notify.UserImage = GetMugshotImage(user.MugshotUrl);
            }
            else
            {
                notify.UserImage = new BitmapImage(user.MugshotUrl);
            }
            notify.Show();

            RefreshMessagePanels();
        }

 

        void TbReplyTo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            int repliedto = (int)tb.DataContext;
            int msgnum = MessageAllContainer.Children.Count;
            int replyfrom = 0;
            int matchindex = int.MinValue;
            double height = 0;
            foreach (var item in MessageAllContainer.Children)
            {
                MessagePanel mp = item as MessagePanel;                
                if(mp == null)
                {
                    continue;
                }
                height = mp.Height;
                replyfrom = (int)mp.DataContext;
                if (replyfrom == repliedto)
                {
                    matchindex = MessageAllContainer.Children.IndexOf(mp);
                    break;
                }
            }
            if (matchindex != int.MinValue)
            {

                //Storyboard sb = (Storyboard)this.FindResource("MessagePanelShow");
                //DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                MessageAllScrollViewer.ScrollToVerticalOffset(height * matchindex);
            }
        }

        void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Canvas canvas = button.Parent as Canvas;
            MessagePanel mp = canvas.Parent as MessagePanel;
            this.ReplyPanel.ReplyID = (int)mp.DataContext;
            this.ReplyPanel.Visibility = Visibility.Visible;
            TabControlMessages.IsEnabled = false;
            TabControlMessages.Opacity = 0.6;
        }
        void ReplyPanelReplyButton_Click(object sender, RoutedEventArgs e)
        {
            Messages messages = new Messages(this.Session);
            messages.PostMessage(this.ReplyPanel.ReplyText.Text, this.ReplyPanel.ReplyID);
            this.ReplyPanel.ReplyText.Text = string.Empty;

            this.ReplyPanel.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }
        void ReplyPanelCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReplyPanel.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }

        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Canvas canvas = button.Parent as Canvas;
            MessagePanel mp = canvas.Parent as MessagePanel;
            Messages messages = new Messages(this.Session);
            messages.DeleteMessage((int)mp.DataContext);

            MessageBox.Show("Message Deleted."); // tmp
        }

        private void RefreshAllMessagePanel(MessageObjects allobjects)
        {
            MessageAllContainer.Children.Clear();

            //MessageObjects allobjects = messages.GetAllMessage();
            int i = 0;
            foreach (Message item in allobjects.Messages)
            {
                MessagePanel mp = new MessagePanel();
                mp.TbMessage.Text = item.Body.Plain;
                if (item.RepliedToId != 0)
                {
                    mp.TbReplyTo.Visibility = Visibility.Visible;
                    mp.TbReplyTo.MouseDown += new MouseButtonEventHandler(TbReplyTo_MouseDown);
                    mp.TbReplyTo.DataContext = item.RepliedToId;
                }
                Reference refer = allobjects.References.Find(o => o.Id == item.SenderId && o.ObjectType == ObjectType.USER);
                User user = (User)refer.Object;
                mp.TbUserId.Text = user.FullName;
                if (this.Session.Proxy != null)
                {
                    mp.ImgUser.Source = GetMugshotImage(user.MugshotUrl);
                }
                else
                {
                    mp.ImgUser.Source = new BitmapImage(user.MugshotUrl);
                }
                TimeSpan tp = DateTime.Now - item.CreatedAt;
                if (tp.Days != 0)
                {
                    mp.TbUpdateTime.Text = tp.Days + " days ago";
                }
                else if (tp.Hours != 0)
                {
                    mp.TbUpdateTime.Text = tp.Hours + " hours ago";
                }
                else if (tp.Minutes != 0)
                {
                    mp.TbUpdateTime.Text = tp.Minutes + " minutes ago";
                }
                else
                {
                    mp.TbUpdateTime.Text = tp.Seconds + " seconds ago";
                }

                if (i == 0)
                {
                    this.MostLastestMessageId = item.Id;
                }
                mp.DataContext = item.Id;
                mp.DeleteButton.Click += new RoutedEventHandler(DeleteButton_Click);
                mp.ReplyButton.Click += new RoutedEventHandler(ReplyButton_Click);
                MessageAllContainer.Children.Add(mp);
                i++;
            }
            MessageAllContainer.Children.Add(new MoreInformationPanel());
        }

        private void RefreshSentMessagePanel(MessageObjects sentobjects)
        {
            MessageSentContainer.Children.Clear();

            
            foreach (Message item in sentobjects.Messages)
            {
                MessagePanel mp = new MessagePanel();
                mp.TbMessage.Text = item.Body.Plain;
                Reference refer = sentobjects.References.Find(o => o.Id == item.SenderId && o.ObjectType == ObjectType.USER);
                User user = (User)refer.Object;
                mp.TbUserId.Text = user.FullName;
                if (this.Session.Proxy != null)
                {
                    mp.ImgUser.Source = GetMugshotImage(user.MugshotUrl);
                }
                else
                {
                    mp.ImgUser.Source = new BitmapImage(user.MugshotUrl);
                }
                MessageSentContainer.Children.Add(mp);
            }
            MessageSentContainer.Children.Add(new MoreInformationPanel());
        }

        private void RefreshFollowingMessagePanel(MessageObjects followingobjects)
        {
            MessageFollowingContainer.Children.Clear();

            foreach (Message item in followingobjects.Messages)
            {
                MessagePanel mp = new MessagePanel();
                mp.TbMessage.Text = item.Body.Plain;
                Reference refer = followingobjects.References.Find(o => o.Id == item.SenderId && o.ObjectType == ObjectType.USER);
                User user = (User)refer.Object;
                mp.TbUserId.Text = user.FullName;
                if (this.Session.Proxy != null)
                {
                    mp.ImgUser.Source = GetMugshotImage(user.MugshotUrl);
                }
                else
                {
                    mp.ImgUser.Source = new BitmapImage(user.MugshotUrl);
                }

                MessageFollowingContainer.Children.Add(mp);
            }
            MessageFollowingContainer.Children.Add(new MoreInformationPanel());
        }

        private void RefreshReceivedMessagePanel(MessageObjects receivedobjects)
        {
            MessageReceivedContainer.Children.Clear();

            foreach (Message item in receivedobjects.Messages)
            {
                MessagePanel mp = new MessagePanel();
                mp.TbMessage.Text = item.Body.Plain;
                Reference refer = receivedobjects.References.Find(o => o.Id == item.SenderId && o.ObjectType == ObjectType.USER);
                User user = (User)refer.Object;
                mp.TbUserId.Text = user.FullName;
                if (this.Session.Proxy != null)
                {
                    mp.ImgUser.Source = GetMugshotImage(user.MugshotUrl);
                }
                else
                {
                    mp.ImgUser.Source = new BitmapImage(user.MugshotUrl);
                }

                MessageReceivedContainer.Children.Add(mp);
            }
            MessageReceivedContainer.Children.Add(new MoreInformationPanel());
        }

        private void RefreshMessagePanels()
        {
            TabControlMessages.IsEnabled = false;
            MessagePanelRefresh.Visibility = Visibility.Visible;
            TabControlMessages.Opacity = 0.6;
            System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshMessageProcess));
            th.Start();
        }
        private void RefreshMessageProcess()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects allobjects = messages.GetAllMessage();
            MessageObjects sentobjects = messages.GetSentMessage();
            MessageObjects followingobjects = messages.GetFollowingMessage();
            MessageObjects receivedobjects = messages.GetReceivedMessage();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new RefreshMessagePanelsCallbackDelegate(RefreshMessagePanelsCallback), allobjects, sentobjects, followingobjects, receivedobjects);
        }

        delegate void RefreshMessagePanelsCallbackDelegate(MessageObjects allobjects, MessageObjects sentobjects, MessageObjects followingobjects, MessageObjects receivedobjects);
        private void RefreshMessagePanelsCallback(MessageObjects allobjects, MessageObjects sentobjects, MessageObjects followingobjects, MessageObjects receivedobjects)
        {
            RefreshAllMessagePanel(allobjects);
            RefreshSentMessagePanel(sentobjects);
            RefreshFollowingMessagePanel(followingobjects);
            RefreshReceivedMessagePanel(receivedobjects);

            MessagePanelRefresh.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;            
        }

        private BitmapImage GetMugshotImage(Uri uri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);

            req.Method = "GET";
            req.PreAuthenticate = true;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Proxy = this.Session.Proxy;

            BitmapImage img = new BitmapImage();

            WebResponse res = req.GetResponse();
            Stream s = res.GetResponseStream();

            FileStream fs = new FileStream(System.IO.Path.GetFullPath("./cache"), FileMode.Create, FileAccess.Write);
            int bytes;
            while ((bytes = s.ReadByte()) != -1)
            {
                fs.WriteByte(Convert.ToByte(bytes));
            }
            fs.Close();
            s.Close();
            res.Close();

            byte[] p_bytes = File.ReadAllBytes(System.IO.Path.GetFullPath("./cache"));
            MemoryStream p_memory = new MemoryStream(p_bytes);
            img.BeginInit();
            img.StreamSource = p_memory;
            img.EndInit();
            //img = new BitmapImage(new Uri(System.IO.Path.GetFullPath("./cache")in));
            //File.Delete(System.IO.Path.GetFullPath("./cache"));

            return img;
        }

        void taskIconItemExit_Click(object sender, EventArgs e)
        {
            this.taskIcon.Visible = false;
            App.Current.Shutdown();
        }        


        void taskIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (this.Visibility == Visibility.Hidden)
                {
                    this.Visibility = Visibility.Visible;
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    this.Focus();
                }
                else
                {
                    this.WindowState = WindowState.Minimized;
                    this.Visibility = Visibility.Hidden;
                    this.Hide();
                }
            }            
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }                 
        }


        void RootCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();        
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshMessagePanels();
        }

        private void Expand1Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.IsMessagePostPanelExpand = !this.IsMessagePostPanelExpand;
            if (this.IsMessagePostPanelExpand)
            {
                button.Style = (Style)this.FindResource("ExpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("MessagePanelShow");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height + 354;
                

                sb.Begin(this);
            }
            else
            {
                button.Style = (Style)this.FindResource("UnexpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("MessagePanelHide");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height - 354;

                sb.Begin(this);
            }
        }

        private void Expand2Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.IsFunctionPanelExpand = !this.IsFunctionPanelExpand;
            if (this.IsFunctionPanelExpand)
            {
                button.Style = (Style)this.FindResource("ExpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("FunctionPanelShow");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height + 40;

                sb.Begin(this);
            }
            else
            {
                button.Style = (Style)this.FindResource("UnexpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("FunctionPanelHide");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height - 40;

                sb.Begin(this);
            }
        }

        private void Expand3Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            this.IsPostPanelExpand = !this.IsPostPanelExpand;
            if (this.IsPostPanelExpand)
            {
                button.Style = (Style)this.FindResource("ExpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("PostPanelShow");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height + 58;

                sb.Begin(this);
            }
            else
            {
                button.Style = (Style)this.FindResource("UnexpandButtonStyle");

                Storyboard sb = (Storyboard)this.FindResource("PostPanelHide");
                DoubleAnimationUsingKeyFrames BaseRectAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[0];
                DoubleAnimationUsingKeyFrames ReflectPathAnimation = (DoubleAnimationUsingKeyFrames)sb.Children[1];
                BaseRectAnimation.KeyFrames[0].Value = ReflectPathAnimation.KeyFrames[0].Value = BaseRectangle.Height - 58;

                sb.Begin(this);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Messages m = new Messages(this.Session);
            m.PostMessage(UpdateText.Text);
            // m.CompletedPost += ;
            // Storyboard Start
            UpdateText.Text = string.Empty;
        }


        private void UpdateText_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Opacity != 1.0)
            {
                tb.Text = string.Empty;
                tb.Opacity = 1.0;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Visibility = Visibility.Hidden;
            this.Hide();
        }

        private void Config4_Click(object sender, RoutedEventArgs e)
        {
            AutoView av = new AutoView(this.Session);
            av.Show();
        }


        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            TabItem ti = sender as TabItem;
            switch (ti.Name)
            {
                case "TabItemAll":
                    if (MessageAllContainer.Children.Count == 0)
                    {
                        TabControlMessages.IsEnabled = false;
                        MessagePanelRefresh.Visibility = Visibility.Visible;
                        TabControlMessages.Opacity = 0.6;
                        System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshAllMessagePanelProcess));
                        th.Start();
                    }                                       
                    break;
                case "TabItemFollowing":
                    if (MessageFollowingContainer.Children.Count == 0)
                    {
                        TabControlMessages.IsEnabled = false;
                        MessagePanelRefresh.Visibility = Visibility.Visible;
                        TabControlMessages.Opacity = 0.6;
                        System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshFollowingMessagePanelProcess));
                        th.Start();
                    }                    
                    break;
                case "TabItemReceived":
                    if (MessageReceivedContainer.Children.Count == 0)
                    {
                        TabControlMessages.IsEnabled = false;
                        MessagePanelRefresh.Visibility = Visibility.Visible;
                        TabControlMessages.Opacity = 0.6;
                        System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshReceivedMessagePanelProcess));
                        th.Start();
                    }                    
                    break;
                case "TabItemSent":
                    if (MessageSentContainer.Children.Count == 0)
                    {
                        TabControlMessages.IsEnabled = false;
                        MessagePanelRefresh.Visibility = Visibility.Visible;
                        TabControlMessages.Opacity = 0.6;
                        System.Threading.Thread th = new System.Threading.Thread(new System.Threading.ThreadStart(RefreshSentMessagePanelProcess));
                        th.Start();
                    }                    
                    break;
            }
        }
        #region All Panel Background Refresh
        private void RefreshAllMessagePanelProcess()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects allobjects = messages.GetAllMessage();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new RefreshAllMessagePanelCallbackDelegate(RefreshAllMessagePanelCallback), allobjects);
        }
        delegate void RefreshAllMessagePanelCallbackDelegate(MessageObjects allobjects);
        private void RefreshAllMessagePanelCallback(MessageObjects allobjects)
        {
            RefreshAllMessagePanel(allobjects);
            MessagePanelRefresh.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }
        #endregion


        #region Sent Panel Background Refresh
        private void RefreshSentMessagePanelProcess()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects sentobjects = messages.GetSentMessage();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new RefreshSentMessagePanelCallbackDelegate(RefreshSentMessagePanelCallback), sentobjects);
        }
        delegate void RefreshSentMessagePanelCallbackDelegate(MessageObjects sentobjects);
        private void RefreshSentMessagePanelCallback(MessageObjects sentobjects)
        {
            RefreshSentMessagePanel(sentobjects);
            MessagePanelRefresh.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }
        #endregion

        #region Following Panel Background Refresh
        private void RefreshFollowingMessagePanelProcess()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects followingobjects = messages.GetFollowingMessage();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new RefreshFollowingMessagePanelCallbackDelegate(RefreshFollowingMessagePanelCallback), followingobjects);
        }
        delegate void RefreshFollowingMessagePanelCallbackDelegate(MessageObjects followingobjects);
        private void RefreshFollowingMessagePanelCallback(MessageObjects followingobjects)
        {
            RefreshFollowingMessagePanel(followingobjects);
            MessagePanelRefresh.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }
        #endregion

        #region Received Panel Background Refresh
        private void RefreshReceivedMessagePanelProcess()
        {
            Messages messages = new Messages(this.Session);
            MessageObjects receivedobjects = messages.GetReceivedMessage();
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new RefreshReceivedMessagePanelCallbackDelegate(RefreshReceivedMessagePanelCallback), receivedobjects);
        }
        delegate void RefreshReceivedMessagePanelCallbackDelegate(MessageObjects receivedobjects);
        private void RefreshReceivedMessagePanelCallback(MessageObjects receivedobjects)
        {
            RefreshReceivedMessagePanel(receivedobjects);
            MessagePanelRefresh.Visibility = Visibility.Collapsed;
            TabControlMessages.Opacity = 1.0;
            TabControlMessages.IsEnabled = true;
        }
        #endregion
    }
}
