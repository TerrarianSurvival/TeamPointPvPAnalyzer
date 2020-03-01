namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// ツール提供クラス
    /// </summary>
    public static class Utils
    {
        public static ReadOnlyDictionary<StageData.StageName, StageData> StageDatas { get; } = new ReadOnlyDictionary<StageData.StageName, StageData>(new Dictionary<StageData.StageName, StageData>()
        {
            [StageData.StageName.Espeon] = new StageData(StageData.StageName.Espeon, 3720, 381, 4042, 490),
            [StageData.StageName.Hell] = new StageData(StageData.StageName.Hell, 3681, 1036, 4054, 1133),
            [StageData.StageName.Ice] = new StageData(StageData.StageName.Ice, 3606, 539, 4062, 650),
            [StageData.StageName.Forest] = new StageData(StageData.StageName.Forest, 3697, 708, 3955, 843),
        });

        private static readonly Geometry MapIcon = Geometry.Parse("M 0,0.5 A 0.5,0.5 0 0 1 1,0.5 A 0.5,0.5 0 0 1 0.85355339059,0.85355339059 C 0.7,1 0.55,1 0.5,1.2 C 0.45,1 0.3,1 0.14644660941,0.85355339059 A 0.5,0.5 0 0 1 0,0.5");

        private static readonly Geometry TeamIcon = Geometry.Parse("M 0.1,0 A 0.1,0.1, 0 0 0 0,0.1 V 0.7 L 0.5,1.0 L 1,0.7 V 0.1 A 0.1,0.1 0 0 0 0.9,0 Z");

        private static Dictionary<string, ImageSource> imageSourceDict = new Dictionary<string, ImageSource>();

        /// <summary>
        /// ログ1行からDateTimeとPvPログテキストを取得する
        /// </summary>
        /// <param name="logOneLine">ログ1行</param>
        /// <param name="pvpLogText">PvPログテキスト</param>
        /// <returns>ログが書かれた時間</returns>
        public static DateTime GetLogTimeAndPvPText(string logOneLine, out string pvpLogText)
        {
            if (string.IsNullOrEmpty(logOneLine))
            {
                throw new ArgumentException("logOneLine must not null or empty.");
            }

            string timeText = logOneLine.Substring(0, 19);

            int index = logOneLine.IndexOf("INFO: ", StringComparison.InvariantCulture);

            if (index == -1)
            {
                pvpLogText = null;
                return default;
            }

            pvpLogText = logOneLine.Substring(index + 6);

            return DateTime.Parse(timeText, CultureInfo.InvariantCulture);
        }

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

        public static Grid CreateMapIcon(Brush backgroundColor, ImageSource source)
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

        public static Grid CreateMapIcon(Brush backgroundColor, UIElement element)
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

            grid.Children.Add(element);

            return grid;
        }

        public static Grid CreateBaseLogIcon(SolidColorBrush backgroundColor)
        {
            var grid = new Grid
            {
                Width = 100,
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

        public static Path CreateTeamIcon(TeamID teamID)
        {
            var backgroundColor = GetTeamColor(teamID);
            return new Path() { Fill = backgroundColor, Data = TeamIcon, Stretch = Stretch.Uniform, Stroke = new SolidColorBrush(Color.FromRgb((byte)(backgroundColor.Color.R / 2), (byte)(backgroundColor.Color.G / 2), (byte)(backgroundColor.Color.B / 2))) };
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
