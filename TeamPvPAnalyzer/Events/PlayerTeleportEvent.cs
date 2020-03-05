namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using TeamPvPAnalyzer.Enums;

    public class PlayerTeleportEvent : PlayerEvent
    {
        public PlayerTeleportEvent(DateTime time)
            : base(time, EventType.TeleportPlayer)
        {
        }

        public PlayerTeleportEvent(DateTime time, string[] args)
            : base(time, EventType.TeleportPlayer)
        {
            ParseFromArguments(args);
        }

        public string LogInfoText { get { return Player.Name; } }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            EventPosX = float.Parse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture);
            EventPosY = float.Parse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture);

            Player = LogParser.GetPlayer(args[0]);

            return this;
        }

        public override void CreateIcons()
        {
            var player_source = Utils.GetImageSource(Player.Class);
            var teleport_source = Utils.GetImageSource("icon_teleport");

            var color = Utils.GetTeamColor(Player.Team);

            MapIcon = Utils.CreateMapIcon(color, player_source);

            var player_image = new Image
            {
                Source = player_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var teleport_image = new Image
            {
                Source = teleport_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var grid = Utils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(teleport_image);

            LogIcon = grid;
        }
    }
}
