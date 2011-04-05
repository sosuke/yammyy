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
using System.Windows.Media.Animation;
using Yammer;

namespace Yammer.Client
{
    /// <summary>
    /// AutoView.xaml の相互作用ロジック
    /// </summary>
    public partial class AutoView : Window
    {
        System.Windows.Threading.DispatcherTimer frameTimer;
        System.Windows.Threading.DispatcherTimer frameTimer2;
        private int lastTick;
        private Random rand;
        private Session session = null;

        public AutoView(Session session)
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.session = session;

            frameTimer = new System.Windows.Threading.DispatcherTimer();
            frameTimer.Tick += OnFrame;
            frameTimer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
            frameTimer.Start();

            frameTimer2 = new System.Windows.Threading.DispatcherTimer();
            frameTimer2.Tick += OnFrame2;
            frameTimer2.Interval = TimeSpan.FromSeconds(4.0);
            frameTimer2.Start();

            this.lastTick = Environment.TickCount;

            rand = new Random(this.GetHashCode());

            this.Show();

            this.KeyDown += new System.Windows.Input.KeyEventHandler(AutoView_KeyDown);

            CreateCircles();
            CreateMessagePanels();
        }

        void AutoView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void OnFrame(object sender, EventArgs e)
        {
        }

        private void OnFrame2(object sender, EventArgs e)
        {
            this.MainCanvas.Children.RemoveAt(24);
            CreateMessagePanels();
        }

        private void CreateMessagePanels()
        {
            Messages messages = new Messages(this.session);
                        
            MessageObjects allobjects = messages.GetAllMessage();
            List<Message> msgs  = allobjects.Messages;
            int index = rand.Next(msgs.Count);
            Message msg = msgs[index];

            TextBlock tb = new TextBlock();
            tb.Text = msg.Body.Plain;
            tb.FontSize = 14;

            this.MainCanvas.Children.Add(tb);

            double centerX = this.MainCanvas.ActualWidth / 2.0;
            double centerY = this.MainCanvas.ActualHeight / 2.0;

            double offsetX = 16 - rand.Next(32);
            double offsetY = 16 - rand.Next(32);

            //tb.SetValue(Canvas.LeftProperty, centerX + offsetX);
            //tb.SetValue(Canvas.TopProperty, centerY + offsetY);
            tb.SetValue(Canvas.LeftProperty, centerX );
            tb.SetValue(Canvas.TopProperty, centerY );

            double duration = 6.0 + 10.0 * rand.NextDouble();
            double delay = 16.0 * rand.NextDouble();
            //TranslateTransform offsetTransform = new TranslateTransform();
            //DoubleAnimation offsetXAnimation = new DoubleAnimation(0.0, -256.0, new Duration(TimeSpan.FromSeconds(duration)));
            //offsetXAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //offsetXAnimation.BeginTime = TimeSpan.FromSeconds(delay);
            //offsetTransform.BeginAnimation(TranslateTransform.XProperty, offsetXAnimation);
            //offsetTransform.BeginAnimation(TranslateTransform.YProperty, offsetXAnimation);
            //tb.RenderTransform = offsetTransform;
            DoubleAnimation opacityAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(4.0)));
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            tb.BeginAnimation(TextBlock.OpacityProperty, opacityAnimation);
        }

        private void CreateCircles()
        {
            double centerX = this.MainCanvas.ActualWidth / 2.0;
            double centerY = this.MainCanvas.ActualHeight / 2.0;

            Color[] colors = new Color[] { Colors.White, Colors.Green, Colors.Green, Colors.Lime };

            for (int i = 0; i < 24; ++i)
            {
                Ellipse e = new Ellipse();
                byte alpha = (byte)rand.Next(96, 192);
                int colorIndex = rand.Next(4);
                e.Stroke = new SolidColorBrush(Color.FromArgb(alpha, colors[colorIndex].R, colors[colorIndex].G, colors[colorIndex].B));
                e.StrokeThickness = rand.Next(1, 4);
                e.Width = 0.0;
                e.Height = 0.0;
                double offsetX = 16 - rand.Next(32);
                double offsetY = 16 - rand.Next(32);

                this.MainCanvas.Children.Add(e);

                e.SetValue(Canvas.LeftProperty, centerX + offsetX);
                e.SetValue(Canvas.TopProperty, centerY + offsetY);

                double duration = 6.0 + 10.0 * rand.NextDouble();
                double delay = 16.0 * rand.NextDouble();

                TranslateTransform offsetTransform = new TranslateTransform();

                DoubleAnimation offsetXAnimation = new DoubleAnimation(0.0, -256.0, new Duration(TimeSpan.FromSeconds(duration)));
                offsetXAnimation.RepeatBehavior = RepeatBehavior.Forever;
                offsetXAnimation.BeginTime = TimeSpan.FromSeconds(delay);
                offsetTransform.BeginAnimation(TranslateTransform.XProperty, offsetXAnimation);
                offsetTransform.BeginAnimation(TranslateTransform.YProperty, offsetXAnimation);

                e.RenderTransform = offsetTransform;


                DoubleAnimation sizeAnimation = new DoubleAnimation(0.0, 512.0, new Duration(TimeSpan.FromSeconds(duration)));
                sizeAnimation.RepeatBehavior = RepeatBehavior.Forever;
                sizeAnimation.BeginTime = TimeSpan.FromSeconds(delay);
                e.BeginAnimation(Ellipse.WidthProperty, sizeAnimation);
                e.BeginAnimation(Ellipse.HeightProperty, sizeAnimation);

                DoubleAnimation opacityAnimation = new DoubleAnimation(duration - 1.0, 0.0, new Duration(TimeSpan.FromSeconds(duration)));
                opacityAnimation.BeginTime = TimeSpan.FromSeconds(delay);
                opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                e.BeginAnimation(Ellipse.OpacityProperty, opacityAnimation);

            }
        }
    }
}
