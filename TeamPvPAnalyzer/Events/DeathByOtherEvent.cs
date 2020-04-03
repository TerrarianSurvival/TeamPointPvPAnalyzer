namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class DeathByOtherEvent : DamageEvent
    {
        public DeathByOtherEvent(DateTime time)
            : base(time, Enums.EventType.DeathByOther)
        {
        }

        public DeathByOtherEvent(DateTime time, string[] args)
            : base(time, Enums.EventType.DeathByOther)
        {
            ParseFromArguments(args);
        }

        public string KillerOther { get; protected set; }

        public new string LogInfoText { get { return Player.Name + " Damage: " + Damage + " KillerOther: " + KillerOther; } }

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

            KillerOther = args[10];

            return this;
        }

        public override FrameworkElement CreateLogIcon()
        {
            var player_source = IconUtils.GetImageSource(Player.Class.ToLowerInvariant());
            var death_source = IconUtils.GetImageSource("icon_death");

            var color = IconUtils.GetTeamColor(Player.Team);

            var player_image = new Image
            {
                Source = player_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2),
                Width = 26,
            };

            var death_image = new Image
            {
                Source = death_source,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(2),
                Width = 26,
            };

            var grid = IconUtils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(death_image);

            return grid;
        }

        public override FrameworkElement CreateMapIcon()
        {
            var death_source = IconUtils.GetImageSource("icon_death");
            return IconUtils.CreateMapIcon(Player, death_source);
        }
    }
}
