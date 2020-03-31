namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using TeamPvPAnalyzer.Enums;

    /// <summary>
    /// ツール提供クラス
    /// </summary>
    public static class Utils
    {
        public static ReadOnlyDictionary<StageData.StageName, StageData> StageDatas { get; } = new ReadOnlyDictionary<StageData.StageName, StageData>(new Dictionary<StageData.StageName, StageData>()
        {
            [StageData.StageName.Espeon] = new StageData(StageData.StageName.Espeon, 3720, 381, 4042, 490),
            [StageData.StageName.Hell] = new StageData(StageData.StageName.Hell, 3681, 1036, 4054, 1133),
            [StageData.StageName.Ice] = new StageData(StageData.StageName.Ice, 3606, 539, 4062, 650),
            [StageData.StageName.Forest] = new StageData(StageData.StageName.Forest, 3697, 708, 3955, 843),
        });

        /// <summary>
        /// ログ1行からDateTimeとPvPログテキストを取得する
        /// </summary>
        /// <param name="logOneLine">ログ1行</param>
        /// <param name="pvpLogText">PvPログテキスト</param>
        /// <returns>ログが書かれた時間</returns>
        public static DateTime GetLogTimeAndPvPText(string logOneLine, out string pvpLogText)
        {
            if (string.IsNullOrEmpty(logOneLine))
            {
                throw new ArgumentException("logOneLine must not null or empty.");
            }

            string timeText = logOneLine.Substring(0, 19);

            int index = logOneLine.IndexOf("INFO: ", StringComparison.InvariantCulture);

            if (index == -1)
            {
                pvpLogText = null;
                return default;
            }

            pvpLogText = logOneLine.Substring(index + 6);

            return DateTime.Parse(timeText, CultureInfo.InvariantCulture);
        }
    }
}
