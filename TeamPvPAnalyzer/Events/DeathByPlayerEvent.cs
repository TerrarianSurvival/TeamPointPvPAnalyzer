namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class DeathByPlayerEvent : VictimEvent
    {
        public DeathByPlayerEvent(DateTime time)
            : base (time, Enums.EventType.DeathByPlayer)
        {
        }

        public DeathByPlayerEvent(DateTime time, string[] args)
            : base(time, Enums.EventType.DeathByPlayer)
        {
            ParseFromArguments(args);
        }

        public new string LogInfoText
        {
            get
            {
                var text = Player.Name + " Damage: " + Damage + " Item: " + KillerItem;
                if (HasKillerProjectile)
                {
                    text += " Projectile: " + KillerProjectile;
                }

                return text;
            }
        }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            // Format: DeadPlayer, KillerPlayer, Damage, DeadPlayerX, DeadPlayerY, KillerPlayerX, KillerPlayerY, KillerItem, KillerProj, KillerNPC, KillerOther
            if (args == null || args.Length != 11)
            {
                throw new ArgumentException("args length must be 11.");
            }

            Player = LogParser.GetPlayer(args[0]);
            EventPosX = float.Parse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            EventPosY = float.Parse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture);

            Damage = int.Parse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture);

            KillerPlayer = LogParser.GetPlayer(args[1]);
            KillerPositionX = float.Parse(args[5], NumberStyles.Float, CultureInfo.InvariantCulture);
            KillerPositionY = float.Parse(args[6], NumberStyles.Float, CultureInfo.InvariantCulture);

            KillerItem = args[7];
            KillerProjectile = args[8] ?? string.Empty;
            HasKillerProjectile = !string.IsNullOrEmpty(KillerProjectile);

            PairEvent = new KillPlayerEvent(Time, this);

            return this;
        }

        public override FrameworkElement CreateLogIcon()
        {
            var player_source = IconUtils.GetImageSource(Player.Class.ToLowerInvariant());
            var killer_source = IconUtils.GetImageSource(KillerPlayer.Class.ToLowerInvariant());
            var death_source = IconUtils.GetImageSource("icon_death");

            var color = IconUtils.GetTeamColor(Player.Team);

            var player_image = new Image
            {
                Source = player_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var death_image = new Image
            {
                Source = death_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var killer_image = new Image
            {
                Source = killer_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new System.Windows.Thickness(2),
                Width = 26,
            };

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(death_image);
            grid.Children.Add(killer_image);

            return grid;
        }

        public override FrameworkElement CreateMapIcon()
        {
            var death_source = IconUtils.GetImageSource("icon_death");
            return IconUtils.CreateMapIcon(Player, death_source);
        }
    }
}
