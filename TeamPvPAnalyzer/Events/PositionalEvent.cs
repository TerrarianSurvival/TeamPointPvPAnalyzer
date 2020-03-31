namespace TeamPvPAnalyzer.Events
{
    using System;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// マップに出るイベントの基底クラス
    /// </summary>
    public class PositionalEvent : ILogEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        /// <param name="type">イベントの種類</param>
        public PositionalEvent(DateTime time, EventType type)
        {
            Time = time;
            Type = type;
        }

        /// <summary>
        /// イベントが起こった時間
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// イベントの種類
        /// </summary>
        public EventType Type { get; }

        /// <summary>
        /// イベントが起こった位置X
        /// </summary>
        public float EventPosX { get; protected set; }

        /// <summary>
        /// イベントが起こった位置Y
        /// </summary>
        public float EventPosY { get; protected set; }

        /// <summary>
        /// ログ一覧に表示するテキスト
        /// </summary>
        public string LogInfoText { get; protected set; }

        public System.Windows.FrameworkElement LogIcon { get; protected set; }

        public System.Windows.FrameworkElement MapIcon { get; protected set; }

        /// <summary>
        /// ログに記録されたデータから情報を復元する
        /// </summary>
        /// <param name="args">情報</param>
        /// <returns>イベント(this)</returns>
        public virtual ILogEvent ParseFromArguments(string[] args)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateIcons()
        {
            throw new NotImplementedException();
        }
    }
}