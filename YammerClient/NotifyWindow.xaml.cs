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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Yammer.Client
{
    public partial class NotifyWindow : Window
    {
        DispatcherTimer FrameTimer = new DispatcherTimer();
        private string _userId;
        private string _bodyText;
        private ImageSource _userImage;
        public string UserId 
        { 
            get { return _userId; }
            set
            {
                _userId = value;
                TbUserId.Text = _userId;
            }
        }
        public string BodyText 
        {
            get { return _bodyText; }
            set
            {
                _bodyText = value;
                TbMessage.Text = _bodyText;
            }
        }
        public ImageSource UserImage 
        {
            get { return _userImage; }
            set
            {
                _userImage = value;
                ImgUser.Source = _userImage;
            }
        }

        public NotifyWindow()
        {
            InitializeComponent();

            int ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            int Screenheigth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            int AppWidth = (int)this.Width;
            int AppHeight = (int)this.Height;

            this.Left = ScreenWidth - AppWidth;
            this.Top = Screenheigth - AppHeight;

            FrameTimer.Tick += new EventHandler(CanvasPanelHide);
            FrameTimer.Interval = TimeSpan.FromMilliseconds(10000.0);
            FrameTimer.Start();
        }

        private void CanvasPanelHide(Object sender, EventArgs e)
        {
            FrameTimer.Stop();
            this.Close();
        }
    }
}
