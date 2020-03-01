namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// プレイヤーがスポーンしたイベント
    /// </summary>
    public class PlayerSpawnEvent : PlayerEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        public PlayerSpawnEvent(DateTime time)
            : base(time, EventType.PlayerSpawn)
        {
        }

        public PlayerSpawnEvent(DateTime time, string[] args)
            : base(time, EventType.PlayerSpawn)
        {
            ParseFromArguments(args);
        }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentException("args length must be 3.");
            }

            EventPosX = int.Parse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture) * 16;
            EventPosY = (int.Parse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture) - 1) * 16;
            Player = LogParser.GetPlayer(args[0]);

            return this;
        }

        public override void CreateIcons()
        {
            var player_source = Utils.GetImageSource(Player.Class.ToLowerInvariant());
            var spawn_source = Utils.GetImageSource("icon_respawn");

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

            var spawn_image = new Image
            {
                Source = spawn_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var grid = Utils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(spawn_image);

            LogIcon = grid;
        }
    }
}
