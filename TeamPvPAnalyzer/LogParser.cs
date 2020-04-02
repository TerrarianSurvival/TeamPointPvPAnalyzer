namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using TeamPvPAnalyzer.Events;

    /// <summary>
    /// ログパース用クラス
    /// </summary>
    internal class LogParser
    {
        public static Dictionary<string, PvPPlayer> PlayerDict { get; } = new Dictionary<string, PvPPlayer>();

        /// <summary>
        /// 名前からプレイヤーを取得する。なければ新規生成する。
        /// </summary>
        /// <param name="name">プレイヤー名</param>
        /// <returns>プレイヤー</returns>
        public static PvPPlayer GetPlayer(string name)
        {
            lock (PlayerDict)
            {
                if (PlayerDict.TryGetValue(name, out PvPPlayer player))
                {
                    return player;
                }
                else
                {
                    // UIスレッドで作成しないとDependecyObjectとして利用したときにエラーが出る
                    PvPPlayer newPlayer = null;
                    Application.Current.Dispatcher.BeginInvoke((Action)(() => { newPlayer = new PvPPlayer(name); })).Wait();
                    PlayerDict.Add(name, newPlayer);
                    newPlayer.PropertyChanged += Player_PropertyChanged;

                    return newPlayer;
                }
            }
        }

        private static void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var player = (PvPPlayer)sender;
            switch (e.PropertyName)
            {
                case "Name":
                    PlayerDict.Remove(PlayerDict.FirstOrDefault(x => ReferenceEquals(x.Value, player)).Key);
                    PlayerDict.Add(player.Name, player);
                    break;
            }
        }

        /// <summary>
        /// ログのパースを実行
        /// </summary>
        /// <param name="fileName">ファイルパス</param>
        /// <returns>イベントリスト</returns>
        public static async Task<OrderedParallelQuery<ILogEvent>> Parse(string fileName)
        {
            var eventList = new ConcurrentBag<ILogEvent>();
            using (var sr = new StreamReader(fileName, System.Text.Encoding.UTF8))
            {
                string text = await sr.ReadToEndAsync().ConfigureAwait(false);
                string[] lines = text.Split(new char[] { '\n' });
                int length = lines.Length;

                var result = Parallel.ForEach(lines, line =>
                {
                    var @event = ParseOneLine(line.Trim(new char[] { '\r' }));
                    if (@event is VictimEvent victimEvent)
                    {
                        var killerEvent = victimEvent.PairEvent;
                        if (killerEvent != null)
                        {
                            eventList.Add(killerEvent);
                        }
                    }

                    if (@event != null)
                    {
                        eventList.Add(@event);
                    }
                });
            }

            return eventList.AsParallel().OrderBy(x => x.Time);

            ILogEvent ParseOneLine(string line)
            {
                if (string.IsNullOrEmpty(line))
                {
                    return null;
                }

                DateTime time = Utils.GetLogTimeAndPvPText(line, out string pvpLogText);

                if (pvpLogText == null)
                {
                    return null;
                }

                string[] split = pvpLogText.Split(new char[] { ':' });
                string typeText = split[0];

                if (split.Length < 2)
                {
                    return null;
                }

                string argText = pvpLogText.Substring(typeText.Length + 1, pvpLogText.Length - typeText.Length - 1);
                string[] args = argText.Split(new char[] { ',' });

                switch (typeText)
                {
                    case "SPAWN":
                        return new PlayerSpawnEvent(time, args);

                    case "DEATH":
                        if (!string.IsNullOrEmpty(args[10]))
                        {
                            return new DeathByOtherEvent(time, args);
                        }
                        else if (!string.IsNullOrEmpty(args[7]))
                        {
                            return new DeathByPlayerEvent(time, args);
                        }
                        else
                        {
                            return null;
                        }

                    case "DAMAGED":
                        if (!string.IsNullOrEmpty(args[10]))
                        {
                            return new RecieveDamageByOtherEvent(time, args);
                        }
                        else if (!string.IsNullOrEmpty(args[7]))
                        {
                            return new RecieveDamageByPlayerEvent(time, args);
                        }
                        else
                        {
                            return null;
                        }

                    case "CHANGECLASS":
                        return new ChangeClassEvent(time, args);

                    case "CHANGETEAM":
                        return new ChangeTeamEvent(time, args);

                    case "ANNOUNCEMENTBOX":
                        {
                            if (args[3].Contains("Game Start"))
                            {
                                return new GameEvent(time, Enums.EventType.GameStart);
                            }
                            else if (args[3].Contains("Game Over"))
                            {
                                return new GameEvent(time, Enums.EventType.GameEnd);
                            }
                            else if (args[3].Contains("point"))
                            {
                                return new GetPointEvent(time, args);
                            }
                            else if (args[3].Contains("WIN"))
                            {
                                return new TeamWinEvent(time, args);
                            }
                            else if (args[3].Contains("Stage"))
                            {
                                return new ChangeStageEvent(time, args);
                            }

                            return null;
                        }

                    case "TELEPORT":
                        return new PlayerTeleportEvent(time, args);

                    default:
                        return null;
                }
            }
        }
    }
}
