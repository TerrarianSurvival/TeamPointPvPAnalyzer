namespace TeamPvPAnalyzer.Events
{
    using System;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// プレイヤーがかかわるイベントの基底クラス
    /// </summary>
    public class PlayerEvent : PositionalEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        /// <param name="type">イベントの種類</param>
        public PlayerEvent(DateTime time, EventType type)
            : base(time, type)
        {
        }

        /// <summary>
        /// イベントが起こったプレイヤー
        /// </summary>
        public PvPPlayer Player { get; protected set; }

        public virtual void DoEffect()
        {
            // Do something
        }

        public virtual void UndoEffect()
        {
            // Do something
        }
    }
}
