namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using TeamPvPAnalyzer.Events;
    using TeamPvPAnalyzer.Timeline;

    /// <summary>
    /// 統計情報の集計結果を取得する関数
    /// </summary>
    /// <param name="stat">解析済み情報</param>
    /// <returns>集計結果</returns>
    public delegate string GetStatValue(PvPPlayerStat stat);

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Game> games = new Dictionary<string, Game>();

        private PlayerWindow playerWindow;

        public MainWindow()
        {
            InitializeComponent();

            TimeLine.MapCanvas = MapCanvas;
        }

        private void Log_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string filePath = fileList[0];
            if (filePath.EndsWith(".log", StringComparison.InvariantCultureIgnoreCase))
            {
                if (File.Exists(filePath))
                {
                    FileDropOverlay.Visibility = Visibility.Hidden;

                    if (playerWindow == null)
                    {
                        playerWindow = new PlayerWindow();
                        playerWindow.Show();
                    }
                    else
                    {
                        playerWindow.Activate();
                    }

                    // UIスレッドでContinueWith
                    LogParser.Parse(filePath).ContinueWith(task => CreateGames(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private void Log_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            FileDropOverlay.Visibility = Visibility.Visible;
        }

        private void CreateGames(OrderedParallelQuery<ILogEvent> rawEvents)
        {
            games.Clear();

            DateTime start = default;
            DateTime end = DateTime.MaxValue;

            var gameEvents = new List<ILogEvent>();
            var preGameStartEvents = new List<ILogEvent>();

            bool isGame = false;
            int gameCount = 0;
            Enums.TeamID teamID = Enums.TeamID.None;

            Enums.StageData.StageName currentStage = Enums.StageData.StageName.Espeon;

            int currentPointBlue = 0;
            int currentPointYellow = 0;

            foreach (var @event in rawEvents)
            {
                // プレイヤーに起きたイベントとして追加
                if (@event is PlayerEvent playerEvent)
                {
                    playerEvent.Player.Events.Add(playerEvent);
                    playerEvent.DoEffect();
                }
                else if (@event is TeamWinEvent teamWinEvent)
                {
                    teamID = teamWinEvent.WinnerTeam;
                }
                else if (@event is GetPointEvent pointEvent)
                {
                    if (pointEvent.Team == Enums.TeamID.Blue)
                    {
                        pointEvent.OldPoint = currentPointBlue;
                        currentPointBlue = pointEvent.CurrentPoint;
                    }
                    else if (pointEvent.Team == Enums.TeamID.Yellow)
                    {
                        pointEvent.OldPoint = currentPointYellow;
                        currentPointYellow = pointEvent.CurrentPoint;
                    }
                }
                else if (@event is ChangeStageEvent changeStage)
                {
                    changeStage.OldStage = currentStage;
                    currentStage = changeStage.CurrentStage;
                }
                else if (@event.Type == Enums.EventType.GameStart)
                {
                    start = @event.Time;
                    isGame = true;

                    currentPointBlue = 0;
                    currentPointYellow = 0;
                }
                else if (@event.Type == Enums.EventType.GameEnd)
                {
                    end = @event.Time;
                }

                if (end < @event.Time)
                {
                    // ゲームエンドの時間を過ぎたのでイベントを追加してリセット(次のゲームへ)
                    var game = new Game(currentStage, start, end, teamID, currentPointBlue, currentPointYellow);
                    game.AddRangePreGameStartEvents(preGameStartEvents);
                    game.AddRangeInGameEvent(gameEvents);

                    preGameStartEvents.Clear();
                    gameEvents.Clear();

                    games.Add(string.Format(CultureInfo.InvariantCulture, "#{0:D2} {1}", gameCount, game.Stage), game);
                    gameCount++;

                    end = DateTime.MaxValue;
                    start = default;

                    teamID = Enums.TeamID.None;

                    isGame = false;
                }

                if (isGame)
                {
                    gameEvents.Add(@event);
                }
                else
                {
                    preGameStartEvents.Add(@event);
                }
            }

            GameSelectBox.ItemsSource = games;
            if (games.Count > 0)
            {
                GameSelectBox.SelectedIndex = 0;
            }
        }

        private void TSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var playerWins = new Dictionary<string, int>();
            var playerLoses = new Dictionary<string, int>();

            if (!Directory.Exists("Output"))
            {
                Directory.CreateDirectory("Output");
            }

            if (games.Count == 0)
            {
                return;
            }

            var startDate = games.First(x => x.Key.StartsWith("#00", StringComparison.Ordinal)).Value.Start.ToString("T", CultureInfo.InvariantCulture).Replace(":", "-");
            foreach (var pair in games)
            {
                string gameLength = (pair.Value.End - pair.Value.Start).ToString("g", CultureInfo.InvariantCulture);
                string winner = pair.Value.WinnerTeam.ToString();
                string loser = pair.Value.WinnerTeam == Enums.TeamID.Blue ? Enums.TeamID.Yellow.ToString() : Enums.TeamID.Blue.ToString();

                string bluePoint = pair.Value.BluePoint.ToString();
                string yellowPoint = pair.Value.YellowPoint.ToString();

                string stageName = pair.Value.Stage.ToString();

                var blueTeam = new List<PvPPlayer>();
                var yellowTeam = new List<PvPPlayer>();

                var lastClassChangeTime = new Dictionary<PvPPlayer, DateTime>();

                foreach (var playerDictPair in LogParser.PlayerDict)
                {
                    playerDictPair.Value.SetPropertysAt(pair.Value.Start);

                    if (playerDictPair.Value.Team == Enums.TeamID.Blue)
                    {
                        blueTeam.Add(playerDictPair.Value);

                        if (pair.Value.WinnerTeam == Enums.TeamID.Blue)
                        {
                            if (playerWins.ContainsKey(playerDictPair.Key))
                            {
                                playerWins[playerDictPair.Key] += 1;
                            }
                            else
                            {
                                playerWins.Add(playerDictPair.Key, 1);
                            }
                        }
                        else
                        {
                            if (playerLoses.ContainsKey(playerDictPair.Key))
                            {
                                playerLoses[playerDictPair.Key] += 1;
                            }
                            else
                            {
                                playerLoses.Add(playerDictPair.Key, 1);
                            }
                        }
                    }
                    else if (playerDictPair.Value.Team == Enums.TeamID.Yellow)
                    {
                        yellowTeam.Add(playerDictPair.Value);

                        if (pair.Value.WinnerTeam == Enums.TeamID.Yellow)
                        {
                            if (playerWins.ContainsKey(playerDictPair.Key))
                            {
                                playerWins[playerDictPair.Key] += 1;
                            }
                            else
                            {
                                playerWins.Add(playerDictPair.Key, 1);
                            }
                        }
                        else
                        {
                            if (playerLoses.ContainsKey(playerDictPair.Key))
                            {
                                playerLoses[playerDictPair.Key] += 1;
                            }
                            else
                            {
                                playerLoses.Add(playerDictPair.Key, 1);
                            }
                        }
                    }

                    lastClassChangeTime.Add(playerDictPair.Value, pair.Value.Start);
                }

                var blueStat = new List<PvPPlayerStat>();
                var yellowStat = new List<PvPPlayerStat>();

                var blueClassTime = new Dictionary<string, TimeSpan>();
                var yellowClassTime = new Dictionary<string, TimeSpan>();

                var blueClassChangeTime = new Dictionary<string, int>();
                var yellowClassChangeTime = new Dictionary<string, int>();

                foreach (var player in blueTeam)
                {
                    var stat = new PvPPlayerStat(player, pair.Value.Start, pair.Value.End);
                    blueStat.Add(stat);

                    foreach (var timePair in stat.ClassTime)
                    {
                        if (!blueClassTime.ContainsKey(timePair.Key))
                        {
                            blueClassTime.Add(timePair.Key, timePair.Value);
                            blueClassChangeTime.Add(timePair.Key, 1);
                        }
                        else
                        {
                            blueClassTime[timePair.Key] += timePair.Value;
                            blueClassChangeTime[timePair.Key] += 1;
                        }
                    }
                }

                foreach (var player in yellowTeam)
                {
                    var stat = new PvPPlayerStat(player, pair.Value.Start, pair.Value.End);
                    yellowStat.Add(stat);

                    foreach (var timePair in stat.ClassTime)
                    {
                        if (!yellowClassTime.ContainsKey(timePair.Key))
                        {
                            yellowClassTime.Add(timePair.Key, timePair.Value);
                            yellowClassChangeTime.Add(timePair.Key, 1);
                        }
                        else
                        {
                            yellowClassTime[timePair.Key] += timePair.Value;
                            yellowClassChangeTime[timePair.Key] += 1;
                        }
                    }
                }

                using (var sw = new StreamWriter(Path.Combine("Output", "stat_" + startDate + ".tsv"), true))
                {
                    sw.WriteLine(
                        string.Join(
                            "\t",
                            new object[]
                            {
                               stageName,
                               winner,
                               loser,
                               gameLength,
                               bluePoint,
                               yellowPoint,
                               GetClassStatString(blueClassTime, yellowClassTime, pair.Value.WinnerTeam == Enums.TeamID.Blue),
                            }));
                }
            }
        }

        private static List<KeyValuePair<string, TimeSpan>> SortAndPaddingClassDictionary(IDictionary<string, TimeSpan> dict)
        {
            var list = new List<KeyValuePair<string, TimeSpan>>();
            var sortList = new List<string>
            {
                "STANDARD",
                "KILLER",
                "GALE",
                "HOPLITE",
                "CREEPER",
                "NINJA",
                "DESERT",
                "WIZARD",
                "FROST",
                "SUMMON",
                "FROG",
                "RIDER",
                "MARTIAN",
                "MARIO",
            };
            foreach (var className in sortList)
            {
                if (dict.TryGetValue(className, out TimeSpan time))
                {
                    list.Add(new KeyValuePair<string, TimeSpan>(className, time));
                }
                else
                {
                    list.Add(new KeyValuePair<string, TimeSpan>(className, TimeSpan.Zero));
                }
            }

            return list;
        }

        private static string GetClassStatString(Dictionary<string, TimeSpan> blueClassTime, Dictionary<string, TimeSpan> yellowClassTime, bool blueWin)
        {
            var blueList = SortAndPaddingClassDictionary(blueClassTime);
            var yellowList = SortAndPaddingClassDictionary(yellowClassTime);

            var builder = new StringBuilder();

            string delimiter = "\t";
            for (int i = 0; i < blueList.Count; i++)
            {
                if (blueWin)
                {
                    builder.Append(blueList[i].Value.ToString("g", CultureInfo.InvariantCulture));
                    builder.Append(delimiter);
                    builder.Append(yellowList[i].Value.ToString("g", CultureInfo.InvariantCulture));
                    builder.Append(delimiter);
                }
                else
                {
                    builder.Append(yellowList[i].Value.ToString("g", CultureInfo.InvariantCulture));
                    builder.Append(delimiter);
                    builder.Append(blueList[i].Value.ToString("g", CultureInfo.InvariantCulture));
                    builder.Append(delimiter);
                }
            }

            return builder.ToString().Trim();
        }

        private void JsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PlayerWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (playerWindow == null)
            {
                playerWindow = new PlayerWindow();
                playerWindow.Show();
            }
            else
            {
                playerWindow.Activate();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            playerWindow?.Close();
        }

        private void Grid_PreviewDragLeave(object sender, DragEventArgs e)
        {
            // 外に出たら0,0になる
            var position = e.GetPosition(FileDropOverlay);
            if (ReferenceEquals(e.OriginalSource, FileDropOverlay)
                && (position.X <= 0
                || position.Y <= 0
                || position.X > FileDropOverlay.ActualWidth
                || position.Y > FileDropOverlay.ActualHeight))
            {
                e.Handled = true;
                Trace.WriteLine(position);
                Trace.WriteLine(FileDropOverlay.ActualWidth);
                Trace.WriteLine(FileDropOverlay.ActualHeight);
                Trace.WriteLine("PreviewDragLeave" + e.Source.ToString());
                FileDropOverlay.Visibility = Visibility.Hidden;
            }
        }
    }
}
