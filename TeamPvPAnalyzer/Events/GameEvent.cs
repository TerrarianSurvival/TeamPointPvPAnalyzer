namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// ゲーム全般のイベント
    /// </summary>
    public class GameEvent : ILogEvent
    {
        private FrameworkElement logIcon;

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

        public FrameworkElement LogIcon
        {
            get
            {
                if (logIcon == null)
                {
                    logIcon = CreateLogIcon();
                }

                return logIcon;
            }
        }

        public FrameworkElement MapIcon { get; }

        /// <summary>
        /// ログに記録されたデータから情報を復元する
        /// </summary>
        /// <param name="args">情報</param>
        /// <returns>イベント</returns>
        public virtual ILogEvent ParseFromArguments(string[] args)
        {
            throw new NotImplementedException();
        }

        public virtual FrameworkElement CreateLogIcon()
        {
            var grid = IconUtils.CreateBaseLogIcon(Brushes.Black);

            var label = new Label
            {
                Content = Type.ToString(),
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            grid.Children.Add(label);

            return grid;
        }

        public virtual FrameworkElement CreateMapIcon()
        {
            throw new NotImplementedException();
        }
    }
}
