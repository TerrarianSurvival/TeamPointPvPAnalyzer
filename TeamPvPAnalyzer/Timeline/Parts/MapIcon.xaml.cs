namespace TeamPvPAnalyzer.Timeline.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// MapIcon.xaml の相互作用ロジック
    /// </summary>
    public partial class MapIcon : UserControl
    {
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
             "BackgroundColor", typeof(Brush), typeof(MapIcon), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty PlayerIdentityColorProperty = DependencyProperty.Register(
             "PlayerIdentityColor", typeof(Brush), typeof(MapIcon), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty PlayerNameProperty = DependencyProperty.Register(
             "PlayerName", typeof(string), typeof(MapIcon), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ClassImageSourceProperty = DependencyProperty.Register(
             "ClassImageSource", typeof(ImageSource), typeof(MapIcon), new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty EventContentProperty = DependencyProperty.Register(
             "EventContent", typeof(object), typeof(MapIcon), new PropertyMetadata(null));

        public MapIcon()
        {
            InitializeComponent();
        }

        public Brush BackgroundColor
        {
            get
            {
                return (Brush)GetValue(BackgroundColorProperty);
            }

            set
            {
                SetValue(BackgroundColorProperty, value);
            }
        }

        public Brush PlayerIdentityColor
        {
            get
            {
                return (Brush)GetValue(PlayerIdentityColorProperty);
            }

            set
            {
                SetValue(PlayerIdentityColorProperty, value);
            }
        }

        public string PlayerName
        {
            get
            {
                return (string)GetValue(PlayerNameProperty);
            }

            set
            {
                SetValue(PlayerNameProperty, value);
            }
        }

        public ImageSource ClassImageSource
        {
            get
            {
                return (ImageSource)GetValue(ClassImageSourceProperty);
            }

            set
            {
                SetValue(ClassImageSourceProperty, value);
            }
        }

        public object EventContent
        {
            get
            {
                return GetValue(EventContentProperty);
            }

            set
            {
                SetValue(EventContentProperty, value);
            }
        }
    }
}
