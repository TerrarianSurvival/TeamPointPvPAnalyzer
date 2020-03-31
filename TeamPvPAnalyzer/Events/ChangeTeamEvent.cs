namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    public class ChangeTeamEvent : PlayerEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        public ChangeTeamEvent(DateTime time)
            : base(time, EventType.TeamChanged)
        {
        }

        public ChangeTeamEvent(DateTime time, string[] args)
            : base(time, EventType.TeamChanged)
        {
            ParseFromArguments(args);
        }

        public TeamID OldTeam { get; protected set; }

        public TeamID NewTeam { get; protected set; }

        public new string LogInfoText { get { return Player.Name; } }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            EventPosX = float.Parse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            EventPosY = float.Parse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture);

            OldTeam = (TeamID)int.Parse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            NewTeam = (TeamID)int.Parse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture);

            Player = LogParser.GetPlayer(args[0]);

            return this;
        }

        public override void DoEffect()
        {
            Player.Team = NewTeam;
        }

        public override void UndoEffect()
        {
            Player.Team = OldTeam;
        }

        public override void CreateIcons()
        {
            var arrow_source = IconUtils.GetImageSource("icon_arrow");

            var color = IconUtils.GetTeamColor(NewTeam);

            MapIcon = IconUtils.CreateMapTeamIcon(NewTeam, Player);

            var new_team_icon = IconUtils.CreateTeamIcon(NewTeam);
            new_team_icon.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            new_team_icon.Margin = new System.Windows.Thickness(2);
            new_team_icon.Width = 26;

            var old_team_icon = IconUtils.CreateTeamIcon(OldTeam);
            old_team_icon.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            old_team_icon.Margin = new System.Windows.Thickness(2);
            old_team_icon.Width = 26;

            var arrow_image = new Image
            {
                Source = arrow_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(old_team_icon);
            grid.Children.Add(arrow_image);
            grid.Children.Add(new_team_icon);

            LogIcon = grid;
        }
    }
}
