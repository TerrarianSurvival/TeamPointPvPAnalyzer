namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// プレイヤーをキルしたイベント
    /// </summary>
    public class KillPlayerEvent : KillerEvent
    {
        public KillPlayerEvent(DateTime time, DeathByPlayerEvent pairEvent)
            : base(time, Enums.EventType.KillPlayer, pairEvent)
        {
        }

        public override void CreateIcons()
        {
            var player_source = IconUtils.GetImageSource(Player.Class.ToLowerInvariant());
            var victim_source = IconUtils.GetImageSource(VictimPlayer.Class.ToLowerInvariant());
            var kill_source = IconUtils.GetImageSource("icon_kill");

            var color = IconUtils.GetTeamColor(Player.Team);

            MapIcon = IconUtils.CreateMapSingleImageIcon(color, player_source);

            var player_image = new Image
            {
                Source = player_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var kill_image = new Image
            {
                Source = kill_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var victim_image = new Image
            {
                Source = victim_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(kill_image);
            grid.Children.Add(victim_image);

            LogIcon = grid;
        }
    }
}
