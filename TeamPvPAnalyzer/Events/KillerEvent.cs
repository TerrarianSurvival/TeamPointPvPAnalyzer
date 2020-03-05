namespace TeamPvPAnalyzer.Events
{
    using System;

    public class KillerEvent : DamageEvent
    {
        public KillerEvent(DateTime time, Enums.EventType type, VictimEvent pairEvent)
            : base(time, type)
        {
            this.PairEvent = pairEvent ?? throw new ArgumentNullException(nameof(pairEvent));

            Damage = pairEvent.Damage;

            KillerItem = pairEvent.KillerItem;
            KillerProjectile = pairEvent.KillerProjectile;
            HasKillerProjectile = pairEvent.HasKillerProjectile;

            VictimPlayer = pairEvent.Player;
            VictimPositionX = pairEvent.EventPosX;
            VictimPositionY = pairEvent.EventPosY;

            Player = pairEvent.KillerPlayer;
            EventPosX = pairEvent.KillerPositionX;
            EventPosY = pairEvent.KillerPositionY;
        }

        public VictimEvent PairEvent { get; protected set; }

        public PvPPlayer VictimPlayer { get; protected set; }

        public float VictimPositionX { get; protected set; }

        public float VictimPositionY { get; protected set; }

        public bool HasKillerProjectile { get; protected set; }

        public string KillerProjectile { get; protected set; }

        public string KillerItem { get; protected set; }

        public string LogInfoText
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
    }
}
