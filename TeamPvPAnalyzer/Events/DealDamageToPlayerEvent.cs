namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// 他のプレイヤーにダメージを与えたイベント
    /// </summary>
    public class DealDamageToPlayerEvent : KillerEvent
    {
        public DealDamageToPlayerEvent(DateTime time, RecieveDamageByPlayerEvent pairEvent)
            : base(time, Enums.EventType.DealDamageToPlayer, pairEvent)
        {
        }

        public override System.Windows.FrameworkElement CreateLogIcon()
        {
            var player_source = IconUtils.GetImageSource(Player.Class.ToLowerInvariant());
            var victim_source = IconUtils.GetImageSource(VictimPlayer.Class.ToLowerInvariant());
            var sword_source = IconUtils.GetImageSource("icon_sword");

            var color = IconUtils.GetTeamColor(Player.Team);

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

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(sword_image);
            grid.Children.Add(victim_image);

            return grid;
        }

        public override System.Windows.FrameworkElement CreateMapIcon()
        {
            var sword_source = IconUtils.GetImageSource("icon_sword");
            return IconUtils.CreateMapIcon(Player, sword_source);
        }
    }
}
