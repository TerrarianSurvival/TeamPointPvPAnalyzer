namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// ゲーム全般のイベント
    /// </summary>
    public class GameEvent : ILogEvent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="time">ログが記録された時間</param>
        /// <param name="type">イベントの種類</param>
        public GameEvent(DateTime time, EventType type)
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
        /// ログ一覧に表示するテキスト
        /// </summary>
        public string LogInfoText { get; protected set; }

        public System.Windows.UIElement LogIcon { get; protected set; }

        public System.Windows.FrameworkElement MapIcon { get; protected set; }

        /// <summary>
        /// ログに記録されたデータから情報を復元する
        /// </summary>
        /// <param name="args">情報</param>
        /// <returns>イベント</returns>
        public virtual ILogEvent ParseFromArguments(string[] args)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateIcons()
        {
            var grid = Utils.CreateBaseLogIcon(Brushes.Black);

            var label = new Label
            {
                Content = Type.ToString(),
                Foreground = Brushes.White,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
            };
            grid.Children.Add(label);

            LogIcon = grid;
        }
    }
}
