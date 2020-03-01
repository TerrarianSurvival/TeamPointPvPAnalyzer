namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// 戦闘イベント
    /// </summary>
    public class DamageEvent : PlayerEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        /// <param name="type">イベントの種類</param>s
        public DamageEvent(DateTime time, EventType type)
            : base(time, type)
        {
        }

        /// <summary>
        /// ダメージ
        /// </summary>
        public int Damage { get; protected set; }
    }
}
