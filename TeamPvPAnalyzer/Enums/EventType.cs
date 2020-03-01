namespace TeamPvPAnalyzer.Enums
{
    /// <summary>
    /// イベントの種類
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// なし。デフォルト値
        /// </summary>
        None,

        /// <summary>
        /// プレイヤーがスポーンしたイベント
        /// </summary>
        PlayerSpawn,

        /// <summary>
        /// プレイヤーが他のプレイヤーによって死んだイベント
        /// </summary>
        DeathByPlayer,

        /// <summary>
        /// プレイヤーがタイル、デバフなどで死んだイベント
        /// </summary>
        DeathByOther,

        /// <summary>
        /// プレイヤーをキルしたイベント
        /// </summary>
        KillPlayer,

        /// <summary>
        /// プレイヤーがダメージをプレイヤーから受けたイベント
        /// </summary>
        DamagedByPlayer,

        /// <summary>
        /// プレイヤーがダメージをタイル、デバフなどから受けたイベント
        /// </summary>
        DamagedByOther,

        /// <summary>
        /// プレイヤーにダメージを与えたイベント
        /// </summary>
        DealDamageToPlayer,

        /// <summary>
        /// プレイヤーがクラスを変更したイベント
        /// </summary>
        ClassChanged,

        /// <summary>
        /// プレイヤーがチームを変更したイベント
        /// </summary>
        TeamChanged,

        /// <summary>
        /// ステージ変更イベント
        /// </summary>
        ChangeStage,

        /// <summary>
        /// ゲームが開始したイベント
        /// </summary>
        GameStart,

        /// <summary>
        /// ゲームが終了したイベント
        /// </summary>
        GameEnd,

        /// <summary>
        /// 得点イベント
        /// </summary>
        GetPoint,

        /// <summary>
        /// チーム勝利イベント
        /// </summary>
        TeamWin,

        /// <summary>
        /// プレイヤーテレポートイベント
        /// </summary>
        TeleportPlayer,
    }
}
