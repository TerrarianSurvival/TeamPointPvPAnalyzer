﻿namespace TeamPvPAnalyzer.Events
{
    using System;

    public class VictimEvent : DamageEvent
    {
        public VictimEvent(DateTime time, Enums.EventType type)
            : base(time, type)
        {
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

        public VictimEvent(DateTime time, Enums.EventType type, KillerEvent pairEvent)
            : base(time, type)
        {
            PairEvent = pairEvent ?? throw new ArgumentNullException(nameof(pairEvent));

            Damage = pairEvent.Damage;

            KillerItem = pairEvent.KillerItem;
            KillerProjectile = pairEvent.KillerProjectile;
            HasKillerProjectile = pairEvent.HasKillerProjectile;

            KillerPlayer = pairEvent.Player;
            KillerPositionX = pairEvent.EventPosX;
            KillerPositionY = pairEvent.EventPosY;

            Player = pairEvent.VictimPlayer;
            EventPosX = pairEvent.VictimPositionX;
            EventPosY = pairEvent.VictimPositionY;
        }

        public KillerEvent PairEvent { get; protected set; }

        public PvPPlayer KillerPlayer { get; protected set; }

        public float KillerPositionX { get; protected set; }

        public float KillerPositionY { get; protected set; }

        public bool HasKillerProjectile { get; protected set; }

        public string KillerProjectile { get; protected set; }

        public string KillerItem { get; protected set; }
    }
}
