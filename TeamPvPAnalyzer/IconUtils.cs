namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TeamPvPAnalyzer.Enums;

    public class IconUtils
    {
        public const double LogBaseIconWidth = 100;

        private static readonly Geometry MapIcon = Geometry.Parse("M 0,0.5 A 0.5,0.5 0 0 1 1,0.5 A 0.5,0.5 0 0 1 0.85355339059,0.85355339059 C 0.7,1 0.55,1 0.5,1.2 C 0.45,1 0.3,1 0.14644660941,0.85355339059 A 0.5,0.5 0 0 1 0,0.5");

        private static readonly Geometry TeamIcon = Geometry.Parse("M 0.1,0 A 0.1,0.1, 0 0 0 0,0.1 V 0.7 L 0.5,1.0 L 1,0.7 V 0.1 A 0.1,0.1 0 0 0 0.9,0 Z");

        private static readonly Dictionary<string, ImageSource> imageSourceDict = new Dictionary<string, ImageSource>();

        public static ImageSource GetImageSource(string name)
        {
            if (imageSourceDict.TryGetValue(name, out ImageSource value))
            {
                return value;
            }
            else
            {
                string path = string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/Resources/{0}.png", name);
                var source = (ImageSource)new ImageSourceConverter().ConvertFromString(path);
                imageSourceDict.Add(name, source);
                return source;
            }
        }

        public static Grid CreateMapSingleImageIcon(Brush backgroundColor, ImageSource source)
        {
            var grid = new Grid();

            var shape = new Path
            {
                Data = MapIcon,
                Stretch = Stretch.Uniform,
                Fill = backgroundColor,
                // Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))),
            };
            grid.Children.Add(shape);

            var icon = new Image
            {
                Source = source,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
            };
            grid.Children.Add(icon);

            return grid;
        }

        public static Grid CreateMapTwoImageIcon(Brush backgroundColor, PvPPlayer player, ImageSource classIconSource)
        {
            var grid = new Grid();
            var row = new RowDefinition
            {
                Height = new GridLength(0.4, GridUnitType.Star),
            };

            var row2 = new RowDefinition
            {
                Height = new GridLength(0.6, GridUnitType.Star),
            };

            grid.RowDefinitions.Add(row);
            grid.RowDefinitions.Add(row2);

            var shape = new Path
            {
                Data = MapIcon,
                Stretch = Stretch.Uniform,
                Fill = backgroundColor,
                // Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))),
            };
            grid.Children.Add(shape);

            var classIcon = new Image
            {
                Source = classIconSource,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
            };

            grid.Children.Add(classIcon);
            Grid.SetRow(classIcon, 0);

            var playerIcon = GetPlayerIcon(player);
            grid.Children.Add(playerIcon);
            Grid.SetRow(playerIcon, 1);

            return grid;
        }

        public static Grid CreateMapTwoImageIcon(Brush backgroundColor, PvPPlayer player, UIElement actionIcon)
        {
            var grid = new Grid();
            var row = new RowDefinition
            {
                Height = new GridLength(0.4, GridUnitType.Star),
            };

            var row2 = new RowDefinition
            {
                Height = new GridLength(0.6, GridUnitType.Star),
            };

            grid.RowDefinitions.Add(row);
            grid.RowDefinitions.Add(row2);

            var shape = new Path
            {
                Data = MapIcon,
                Stretch = Stretch.Uniform,
                Fill = backgroundColor,
                // Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))),
            };
            grid.Children.Add(shape);

            grid.Children.Add(actionIcon);
            Grid.SetRow(actionIcon, 0);

            var playerIcon = GetPlayerIcon(player);
            grid.Children.Add(playerIcon);
            Grid.SetRow(playerIcon, 1);

            return grid;
        }

        public static Grid CreateMapThreeImageIcon(Brush backgroundColor, PvPPlayer player, UIElement actionIcon, ImageSource damageIconSource)
        {
            var grid = CreateMapTwoImageIcon(backgroundColor, player, actionIcon);

            var damageIcon = new Image
            {
                Source = damageIconSource,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            grid.Children.Add(damageIcon);
            Grid.SetRow(damageIcon, 1);

            return grid;
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

        public static Grid CreateMapTeamIcon(TeamID teamID, PvPPlayer player)
        {
            if (player == null)
            {
                return null;
            }

            var backgroundColor = GetTeamColor(teamID);
            var teamShape = CreateTeamIcon(teamID);
            return CreateMapTwoImageIcon(backgroundColor, player, teamShape);
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

            var rand = new Random();
            var nameBinding = new Binding("Name")
            {
                Source = player,
            };

            var grid = new Grid();
            var ellipse = new Ellipse()
            {
                Fill = new SolidColorBrush(Color.FromRgb((byte)rand.Next(), (byte)rand.Next(), (byte)rand.Next())),
            };
            grid.Children.Add(ellipse);

            var nameView = new Viewbox();
            var nameTextBlock = new TextBlock();
            nameTextBlock.SetBinding(TextBlock.TextProperty, nameBinding);
            nameView.DataContext = nameTextBlock;
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
