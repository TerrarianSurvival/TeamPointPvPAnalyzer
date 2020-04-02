namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TeamPvPAnalyzer.Enums;
    using TeamPvPAnalyzer.Timeline.Parts;

    public class IconUtils
    {
        public const double LogBaseIconWidth = 100;

        private static readonly Geometry TeamIcon = Geometry.Parse("M 0.1,0 A 0.1,0.1, 0 0 0 0,0.1 V 0.7 L 0.5,1.0 L 1,0.7 V 0.1 A 0.1,0.1 0 0 0 0.9,0 Z");

        private static readonly Dictionary<string, ImageSource> ImageSourceDict = new Dictionary<string, ImageSource>();

        public static ImageSource GetImageSource(string name)
        {
            if (ImageSourceDict.TryGetValue(name, out ImageSource value))
            {
                return value;
            }
            else
            {
                string path = string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/Resources/{0}.png", name);
                var source = (ImageSource)new ImageSourceConverter().ConvertFromString(path);
                ImageSourceDict.Add(name, source);
                return source;
            }
        }

        public static MapIcon CreateMapIcon(PvPPlayer player, ImageSource eventImageSource)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var eventImage = new Image
            {
                Source = eventImageSource,
                Stretch = Stretch.Uniform,
            };

            return CreateMapIcon(player, eventImage);
        }

        public static MapIcon CreateMapIcon(PvPPlayer player, FrameworkElement uiElement)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (uiElement == null)
            {
                throw new ArgumentNullException(nameof(uiElement));
            }

            var playerIcon = new MapIcon
            {
                BackgroundColor = GetTeamColor(player.Team),
                PlayerIdentityColor = player.IdentityColor,
                EventContent = uiElement,
            };
            var playerNameBinding = new Binding("Name")
            {
                Source = player,
            };
            playerIcon.SetBinding(Timeline.Parts.MapIcon.PlayerNameProperty, playerNameBinding);

            return playerIcon;
        }

        public static Path CreateTeamIcon(TeamID teamID)
        {
            var backgroundColor = GetTeamColor(teamID);
            return new Path()
            {
                Data = TeamIcon,
                Stretch = Stretch.Uniform,
                Fill = backgroundColor,
                Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))),
            };
        }

        public static Grid CreateBaseLogIcon(SolidColorBrush backgroundColor)
        {
            var grid = new Grid
            {
                Width = LogBaseIconWidth,
                Height = 30,
            };

            var rect = new Rectangle
            {
                Fill = backgroundColor ?? throw new ArgumentNullException(nameof(backgroundColor)),
                // Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))),
                RadiusX = 5,
                RadiusY = 5,
            };

            grid.Children.Add(rect);

            return grid;
        }

        public static Grid GetPlayerIcon(PvPPlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var nameBinding = new Binding("Name")
            {
                Source = player,
            };

            var heightBinding = new Binding("ActualHeight")
            {
                RelativeSource = RelativeSource.Self,
            };

            var grid = new Grid();
            grid.SetBinding(Grid.WidthProperty, heightBinding);

            var ellipse = new Ellipse()
            {
                Fill = player.IdentityColor,
                Stretch = Stretch.Uniform,
            };
            ellipse.SetBinding(Ellipse.HeightProperty, heightBinding);

            grid.Children.Add(ellipse);

            var nameView = new Viewbox()
            {
                Margin = new Thickness(2),
            };
            var nameTextBlock = new TextBlock()
            {
                Foreground = Brushes.White,
            };
            nameTextBlock.SetBinding(TextBlock.TextProperty, nameBinding);
            nameView.Child = nameTextBlock;
            grid.Children.Add(nameView);

            return grid;
        }

        public static SolidColorBrush GetTeamColor(TeamID teamID)
        {
            switch (teamID)
            {
                case TeamID.None:
                    return Brushes.White;

                case TeamID.Red:
                    return new SolidColorBrush(Color.FromRgb(218, 59, 59));

                case TeamID.Green:
                    return new SolidColorBrush(Color.FromRgb(59, 218, 85));

                case TeamID.Blue:
                    return new SolidColorBrush(Color.FromRgb(59, 149, 218));

                case TeamID.Yellow:
                    return new SolidColorBrush(Color.FromRgb(242, 221, 100));

                case TeamID.Pink:
                    return new SolidColorBrush(Color.FromRgb(224, 100, 242));

                default:
                    return Brushes.White;
            }
        }
    }
}
