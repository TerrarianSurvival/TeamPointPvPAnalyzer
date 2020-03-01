namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using TeamPvPAnalyzer.Events;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Game> games = new Dictionary<string, Game>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogTextBox_Drop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            string filePath = fileList[0];
            if (filePath.EndsWith(".log", StringComparison.InvariantCultureIgnoreCase))
            {
                LogTextBox.Text = filePath;
                if (File.Exists(filePath))
                {
                    LogParser.Parse(filePath).ContinueWith(task => CreateGames(task.Result), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private void LogTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateGames(OrderedParallelQuery<ILogEvent> rawEvents)
        {
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

                @event.CreateIcons();
            }

            GameSelectBox.ItemsSource = games;
        }

        private void LogListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                MapCanvas.Children.Remove(((ILogEvent)item).MapIcon);
            }

            var stageData = Utils.StageDatas[((KeyValuePair<string, Game>)GameSelectBox.SelectedItem).Value.Stage];

            double canvasHeight = MapCanvas.Height;
            double canvasWidth = MapCanvas.Width;

            foreach (var item in e.AddedItems)
            {
                if (item is PositionalEvent positionalEvent)
                {
                    float x = positionalEvent.EventPosX + 10;
                    float y = positionalEvent.EventPosY + 21;
                    if (positionalEvent.MapIcon != null && stageData.IsInRange(x, y))
                    {
                        float fixedX = (x - (stageData.X * 16)) / (stageData.Width * 16);
                        float fixedY = (y - (stageData.Y * 16)) / (stageData.Height * 16);

                        positionalEvent.MapIcon.Height = 40;
                        positionalEvent.MapIcon.Width = 40;

                        Canvas.SetLeft(positionalEvent.MapIcon, (fixedX * canvasWidth) - 20);
                        Canvas.SetTop(positionalEvent.MapIcon, (fixedY * canvasHeight) - 40);

                        MapCanvas.Children.Add(positionalEvent.MapIcon);
                    }
                }
            }
        }

        private void GameSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is KeyValuePair<string, Game> pair)
            {
                LogListBox.ItemsSource = pair.Value.AllEvents;

                GameLengthText.Text = "Game Length: " + (pair.Value.End - pair.Value.Start).ToString("c", CultureInfo.InvariantCulture);
                WinnerTeam.Text = "Winner: " + pair.Value.WinnerTeam.ToString();

                MapBackground.Source = Utils.GetImageSource(pair.Value.Stage.ToString());

                var blueTeam = new List<PvPPlayer>();
                var yellowTeam = new List<PvPPlayer>();

                var lastClassChangeTime = new Dictionary<PvPPlayer, DateTime>();

                foreach (var playerDictPair in LogParser.PlayerDict)
                {
                    playerDictPair.Value.SetPropertysAt(pair.Value.Start);

                    if (playerDictPair.Value.Team == Enums.TeamID.Blue)
                    {
                        blueTeam.Add(playerDictPair.Value);
                    }
                    else if (playerDictPair.Value.Team == Enums.TeamID.Yellow)
                    {
                        yellowTeam.Add(playerDictPair.Value);
                    }

                    lastClassChangeTime.Add(playerDictPair.Value, pair.Value.Start);
                }

                BlueTeam.ItemsSource = blueTeam;
                YellowTeam.ItemsSource = yellowTeam;

                BluePoints.Text = pair.Value.BluePoint + " points";
                YellowPoints.Text = pair.Value.YellowPoint + " points";

                var blueStat = new List<PvPPlayerStat>();
                var yellowStat = new List<PvPPlayerStat>();

                var blueClassTime = new Dictionary<string, TimeSpan>();
                var yellowClassTime = new Dictionary<string, TimeSpan>();

                foreach (var player in blueTeam)
                {
                    var stat = new PvPPlayerStat(player, pair.Value.Start, pair.Value.End);
                    blueStat.Add(stat);

                    foreach (var timePair in stat.ClassTime)
                    {
                        if (!blueClassTime.ContainsKey(timePair.Key))
                        {
                            blueClassTime.Add(timePair.Key, timePair.Value);
                        }
                        else
                        {
                            blueClassTime[timePair.Key] += timePair.Value;
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
                        }
                        else
                        {
                            yellowClassTime[timePair.Key] += timePair.Value;
                        }
                    }
                }

                BlueTeamClass.ItemsSource = blueClassTime.OrderByDescending(x => x.Value);
                YellowTeamClass.ItemsSource = yellowClassTime.OrderByDescending(x => x.Value);

                BlueTeamStat.ItemsSource = blueStat;
                YellowTeamStat.ItemsSource = yellowStat;
            }
        }
    }
}
