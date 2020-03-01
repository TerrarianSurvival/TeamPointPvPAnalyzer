﻿namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// タイルやデバフなどからダメージを受けたイベント
    /// </summary>
    public class RecieveDamageByOtherEvent : DamageEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        public RecieveDamageByOtherEvent(DateTime time)
             : base(time, EventType.DamagedByOther)
        {
        }

        public RecieveDamageByOtherEvent(DateTime time, string[] args)
             : base(time, EventType.DamagedByOther)
        {
            ParseFromArguments(args);
        }

        /// <summary>
        /// ダメージソース名
        /// </summary>
        public string KillerOther { get; protected set; }

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

            LogInfoText = Player.Name + " Damage: " + Damage + " KillerOther: " + KillerOther;

            return this;
        }

        public override void CreateIcons()
        {

            var player_source = Utils.GetImageSource(Player.Class.ToLowerInvariant());
            var death_source = Utils.GetImageSource("icon_shield");

            var color = Utils.GetTeamColor(Player.Team);

            MapIcon = Utils.CreateMapIcon(color, death_source);

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

            var grid = Utils.CreateBaseLogIcon(color);
            grid.Children.Add(player_image);
            grid.Children.Add(death_image);

            LogIcon = grid;
        }
    }
}
