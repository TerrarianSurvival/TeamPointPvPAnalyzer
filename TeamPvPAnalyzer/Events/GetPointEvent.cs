namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// ポイント取得イベント
    /// </summary>
    public class GetPointEvent : GameEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        public GetPointEvent(DateTime time)
            : base(time, EventType.GetPoint)
        {
        }

        public GetPointEvent(DateTime time, string[] args)
            : base(time, EventType.GetPoint)
        {
            ParseFromArguments(args);
        }

        /// <summary>
        /// 得点を得たチーム
        /// </summary>
        public TeamID Team { get; protected set; }

        public int OldPoint { get; protected internal set; }

        /// <summary>
        /// 獲得した後の得点
        /// </summary>
        public int CurrentPoint { get; protected set; }

        public new string LogInfoText { get { return CurrentPoint.ToString(CultureInfo.InvariantCulture); } }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            // int who = int.Parse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
            // EventPosX = int.Parse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            // EventPosY = int.Parse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            string text = args[3];
            string colorCode = text.Substring(text.IndexOf('/') + 1, 6);

            var colonInd = text.IndexOf(':');
            var spaceIndex = text.IndexOf(' ', colonInd + 1);
            string p = text.Substring(colonInd + 1, spaceIndex - colonInd - 1);

            CurrentPoint = int.Parse(p, NumberStyles.Integer, CultureInfo.InvariantCulture);

            if (colorCode == "FFFF00")
            {
                Team = TeamID.Yellow;
            }
            else if (colorCode == "3333FF")
            {
                Team = TeamID.Blue;
            }

            return this;
        }

        public override System.Windows.FrameworkElement CreateLogIcon()
        {
            var color = IconUtils.GetTeamColor(Team);

            var grid = IconUtils.CreateBaseLogIcon(color);

            SolidColorBrush textColor = Brushes.Black;

            switch (Team)
            {
                case TeamID.Blue:
                case TeamID.Red:
                case TeamID.Pink:
                    textColor = Brushes.White;
                    break;
            }

            var label = new Label
            {
                Content = string.Format(CultureInfo.InvariantCulture, "{0:+##;-##;+0}", CurrentPoint - OldPoint),
                Foreground = textColor,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
            };
            grid.Children.Add(label);

            return grid;
        }
    }
}
