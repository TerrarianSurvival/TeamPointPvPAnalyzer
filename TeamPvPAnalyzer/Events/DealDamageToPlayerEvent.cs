namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class DealDamageToPlayerEvent : KillerEvent
    {
        public DealDamageToPlayerEvent(DateTime time, RecieveDamageByPlayerEvent pairEvent)
            : base(time, Enums.EventType.DealDamageToPlayer, pairEvent)
        {
        }

        public override void CreateIcons()
        {
            var player_source = Utils.GetImageSource(Player.Class.ToLowerInvariant());
            var victim_source = Utils.GetImageSource(VictimPlayer.Class.ToLowerInvariant());
            var sword_source = Utils.GetImageSource("icon_sword");

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

            var sword_image = new Image
            {
                Source = sword_source,
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

            var grid = Utils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(sword_image);
            grid.Children.Add(victim_image);

            LogIcon = grid;
        }
    }
}
