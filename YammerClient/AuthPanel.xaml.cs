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
using System.Xml;
using System.IO;
using System.Net;
using OAuth;

namespace Yammer.Client
{
    public partial class AuthPanel : UserControl
    {
        public int PanelStatus { get; set; } // 1: Intro, 2: Proxy, 3: OAuth, 4: Fin
        public WebProxy Proxy { get; set; }
        public OAuthKey OAuthKey { get; set; }

        public AuthPanel()
        {
            InitializeComponent();
            PanelStatus = 1;
        }

    }
}
