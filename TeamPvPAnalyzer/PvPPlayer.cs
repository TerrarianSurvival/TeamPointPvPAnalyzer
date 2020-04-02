namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;
    using TeamPvPAnalyzer.Events;

    /// <summary>
    /// プレイヤークラス
    /// </summary>
    public class PvPPlayer : INotifyPropertyChanged
    {
        private static readonly Random Random = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private string name;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">プレイヤー名</param>
        public PvPPlayer(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        /// <summary>
        /// クラスの名前
        /// </summary>
        public string Class { get; set; } = "none";

        /// <summary>
        /// プレイヤー固有の色
        /// </summary>
        public Brush IdentityColor { get; } = new SolidColorBrush(Color.FromRgb((byte)Random.Next(), (byte)Random.Next(), (byte)Random.Next()));

        /// <summary>
        /// チームID
        /// </summary>
        public TeamID Team { get; set; }

        /// <summary>
        /// 位置X(タイル*16)
        /// </summary>
        public float PositionX { get; set; }

        /// <summary>
        /// 位置Y(タイル*16)
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        /// プレイヤーに起きたイベントリスト
        /// </summary>
        public List<PlayerEvent> Events { get; private set; } = new List<PlayerEvent>();

        public void SetPropertysAt(DateTime time)
        {
            Events = Events.OrderBy(x => x.Time).ToList();

            int index = Events.FindLastIndex(x => x.Time <= time);

            Class = "none";
            Team = default;
            PositionX = default;
            PositionY = default;

            bool classNameSet = false;
            bool teamSet = false;
            bool positionSet = false;

            for (int i = index; i >= 0; i--)
            {
                var @event = Events[i];

                if (!classNameSet && @event is ChangeClassEvent changeClassEvent)
                {
                    Class = changeClassEvent.CurrentClass;
                    classNameSet = true;
                }

                if (!teamSet && @event is ChangeTeamEvent changeTeamEvent)
                {
                    Team = changeTeamEvent.NewTeam;
                    teamSet = true;
                }

                if (!positionSet && @event is PositionalEvent positionalEvent)
                {
                    PositionX = positionalEvent.EventPosX;
                    PositionY = positionalEvent.EventPosY;
                    positionSet = true;
                }

                if (classNameSet && teamSet && positionSet)
                {
                    break;
                }
            }
        }
    }
}
