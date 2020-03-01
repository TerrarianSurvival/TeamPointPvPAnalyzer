using System.Windows;

namespace TeamPvPAnalyzer.Events
{
    /// <summary>
    /// ログから変換可能なイベントのインターフェース
    /// </summary>
    public interface ILogEvent
    {
        /// <summary>
        /// イベントが記録された時間
        /// </summary>
        System.DateTime Time { get; }

        /// <summary>
        /// イベントの種類
        /// </summary>
        Enums.EventType Type { get; }

        /// <summary>
        /// ログ一覧に出すテキスト
        /// </summary>
        string LogInfoText { get; }

        UIElement LogIcon { get; }

        FrameworkElement MapIcon { get; }

        /// <summary>
        /// ログに記録されたデータから情報を復元する
        /// </summary>
        /// <param name="args">情報</param>
        /// <returns>イベント(this)</returns>
        ILogEvent ParseFromArguments(string[] args);

        void CreateIcons();
    }
}
