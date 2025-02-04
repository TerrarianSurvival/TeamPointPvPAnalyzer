﻿namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// クラス変更イベント
    /// </summary>
    public class ChangeClassEvent : PlayerEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        public ChangeClassEvent(DateTime time)
            : base(time, EventType.ClassChanged)
        {
        }

        public ChangeClassEvent(DateTime time, string[] args)
             : base(time, EventType.ClassChanged)
        {
            ParseFromArguments(args);
        }

        public string OldClass { get; protected set; }

        public string CurrentClass { get; protected set; }

        public new string LogInfoText { get { return Player.Name; } }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            EventPosX = float.Parse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            EventPosY = float.Parse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture);
            Player = LogParser.GetPlayer(args[0]);
            OldClass = args[1];
            CurrentClass = args[2];

            return this;
        }

        public override void DoEffect()
        {
            Player.Class = CurrentClass;
        }

        public override void UndoEffect()
        {
            Player.Class = OldClass;
        }

        public override FrameworkElement CreateLogIcon()
        {
            var current_source = IconUtils.GetImageSource(CurrentClass.ToLowerInvariant());
            var old_source = IconUtils.GetImageSource(OldClass.ToLowerInvariant());
            var arrow_source = IconUtils.GetImageSource("icon_arrow");

            var color = IconUtils.GetTeamColor(Player.Team);

            var old_image = new Image
            {
                Source = old_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2),
                Width = 26,
            };

            var arrow_image = new Image
            {
                Source = arrow_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(2),
                Width = 26,
            };

            var current_image = new Image
            {
                Source = current_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(2),
                Width = 26,
            };

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(current_image);
            grid.Children.Add(arrow_image);
            grid.Children.Add(old_image);

            return grid;
        }

        public override FrameworkElement CreateMapIcon()
        {
            var current_source = IconUtils.GetImageSource(CurrentClass.ToLowerInvariant());
            return IconUtils.CreateMapIcon(Player, current_source);
        }
    }
}
