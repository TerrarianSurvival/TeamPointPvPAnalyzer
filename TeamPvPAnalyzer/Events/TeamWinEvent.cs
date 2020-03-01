namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    public class TeamWinEvent : GameEvent
    {
        public TeamWinEvent(DateTime time)
            : base(time, EventType.TeamWin)
        {
        }

        public TeamWinEvent(DateTime time, string[] args)
            : base(time, EventType.TeamWin)
        {
            ParseFromArguments(args);
        }

        public TeamID WinnerTeam { get; protected set; }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            if (args[3].Contains("BLUE"))
            {
                WinnerTeam = TeamID.Blue;
            }

            if (args[3].Contains("YELLOW"))
            {
                WinnerTeam = TeamID.Yellow;
            }

            return this;
        }

        public override void CreateIcons()
        {
            var color = Utils.GetTeamColor(WinnerTeam);

            var grid = Utils.CreateBaseLogIcon(color);

            SolidColorBrush textColor = Brushes.Black;

            switch (WinnerTeam)
            {
                case TeamID.Blue:
                case TeamID.Red:
                case TeamID.Pink:
                    textColor = Brushes.White;
                    break;
            }

            var label = new Label
            {
                Content = string.Format(CultureInfo.InvariantCulture, "{0} WIN", WinnerTeam.ToString()),
                Foreground = textColor,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
            };
            grid.Children.Add(label);

            LogIcon = grid;
        }
    }
}
