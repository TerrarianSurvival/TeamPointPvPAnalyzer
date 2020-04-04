using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TeamPvPAnalyzer.Events;
using TeamPvPAnalyzer.Timeline;

namespace TeamPvPAnalyzer
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        private static readonly Dictionary<string, GetStatValue> StatFunctions = new Dictionary<string, GetStatValue>()
        {
            ["Damage"] = x => { return x.TotalDamage.ToString(CultureInfo.InvariantCulture); },
            ["Damage Times"] = x => { return x.DamageCount.ToString(CultureInfo.InvariantCulture); },
            ["Recieve Damage"] = x => { return x.TotalRecieveDamage.ToString(CultureInfo.InvariantCulture); },
            ["Recieve Damage Times"] = x => { return x.RecieveDamageCount.ToString(CultureInfo.InvariantCulture); },
            ["Death"] = x => { return x.DeathCount.ToString(CultureInfo.InvariantCulture); },
            ["Kill"] = x => { return x.KillCount.ToString(CultureInfo.InvariantCulture); },
            ["Teleport"] = x => { return x.TeleportCount.ToString(CultureInfo.InvariantCulture); },
            ["Spawn"] = x => { return x.SpawnCount.ToString(CultureInfo.InvariantCulture); },
        };

        private List<ILogEvent> logAllEvents;
        private List<ILogEvent> filteredEvents = new List<ILogEvent>();
        private Dictionary<PvPPlayer, bool> playerFilters = new Dictionary<PvPPlayer, bool>();

        private bool aggregateApply = false;

        private readonly TimelineControl timelineControl;

        public PlayerWindow()
        {
            InitializeComponent();

            MainWindow.GameSelectBox.SelectionChanged += GameSelectBox_SelectionChanged;
            timelineControl = MainWindow.TimeLine;
        }

        private MainWindow MainWindow { get { return (MainWindow)Application.Current.MainWindow; } }

        private void GameSelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            timelineControl.Reset();

            if (e.AddedItems[0] is KeyValuePair<string, Game> pair)
            {
                logAllEvents = new List<ILogEvent>(pair.Value.AllEvents);

                playerFilters.Clear();

                GameLengthText.Text = "Game Length: " + (pair.Value.End - pair.Value.Start).ToString("c", CultureInfo.InvariantCulture);
                WinnerTeam.Text = "Winner: " + pair.Value.WinnerTeam.ToString();

                MainWindow.MapBackground.Source = IconUtils.GetImageSource(pair.Value.Stage.ToString());

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

                    playerFilters.Add(playerDictPair.Value, false);
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

                ApplyFilters();
                SetEventsToTimeline();
            }
        }

        private void ApplyFilters()
        {
            if (aggregateApply)
            {
                return;
            }

            filteredEvents.Clear();

            foreach (var @event in logAllEvents)
            {
                if (@event is PlayerEvent playerEvent)
                {
                    if (playerFilters.TryGetValue(playerEvent.Player, out bool flag) && flag)
                    {
                        filteredEvents.Add(@event);
                    }
                }
                else
                {
                    filteredEvents.Add(@event);
                }
            }
        }

        private async void BlueTeamCheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            aggregateApply = true;

            if (((CheckBox)sender).IsChecked == true)
            {
                foreach (var item in BlueTeamStat.Items)
                {
                    if (BlueTeamStat.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem element)
                    {
                        element.IsSelected = true;
                    }
                }
            }
            else
            {
                foreach (var item in BlueTeamStat.Items)
                {
                    if (BlueTeamStat.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem element)
                    {
                        element.IsSelected = false;
                    }
                }
            }

            aggregateApply = false;
            ApplyFilters();
            try
            {
                await SetEventsToTimeline().ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }

        private async void YellowTeamCheckAll_Clicked(object sender, RoutedEventArgs e)
        {
            aggregateApply = true;

            if (((CheckBox)sender).IsChecked == true)
            {
                foreach (var item in YellowTeamStat.Items)
                {
                    if (YellowTeamStat.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem element)
                    {
                        element.IsSelected = true;
                    }
                }
            }
            else
            {
                foreach (var item in YellowTeamStat.Items)
                {
                    if (YellowTeamStat.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem element)
                    {
                        element.IsSelected = false;
                    }
                }
            }

            aggregateApply = false;
            ApplyFilters();
            await SetEventsToTimeline().ConfigureAwait(false);
        }

        private async void Player_Checked(object sender, RoutedEventArgs e)
        {
            var playerStat = ((StackPanel)((CheckBox)sender).Parent).DataContext as PvPPlayerStat;
            if (playerStat != null)
            {
                playerFilters[playerStat.Player] = true;
            }

            ApplyFilters();
            await SetEventsToTimeline().ConfigureAwait(false);
        }

        private async void Player_Unchecked(object sender, RoutedEventArgs e)
        {
            var playerStat = ((StackPanel)((CheckBox)sender).Parent).DataContext as PvPPlayerStat;
            if (playerStat != null)
            {
                playerFilters[playerStat.Player] = false;
            }

            ApplyFilters();
            await SetEventsToTimeline().ConfigureAwait(false);
        }

        private async Task SetEventsToTimeline()
        {
            if (aggregateApply)
            {
                return;
            }

            var stageData = Utils.StageDatas[((KeyValuePair<string, Game>)MainWindow.GameSelectBox.SelectedItem).Value.Stage];
            var elements = new List<TimelineElement>();

            // もし他の場所でも使うならLockする
            foreach (var logEvent in filteredEvents)
            {
                var point = new Point(-1, -1);
                if (logEvent is PositionalEvent positionalEvent)
                {
                    float x = positionalEvent.EventPosX + 10;
                    float y = positionalEvent.EventPosY + 21;
                    if (stageData.IsInRange(x, y))
                    {
                        float fixedX = (x - (stageData.X * 16)) / (stageData.Width * 16);
                        float fixedY = (y - (stageData.Y * 16)) / (stageData.Height * 16);

                        point = new Point(fixedX, fixedY);
                    }
                }

                elements.Add(new TimelineElement(logEvent, point));
            }

            if (elements.Count > 0)
            {
                try
                {
                    // UIスレッド以外で実行する必要がある
                    await Task.Run(() => timelineControl.SetDatas(elements)).ConfigureAwait(false);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                timelineControl.ClearEvents();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var items = StatFunctions.Keys;

            BlueTeamStatComboBox.ItemsSource = items;
            YellowTeamStatComboBox.ItemsSource = items;
        }

        private void BlueTeamStatCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox box && box.Parent is DockPanel panel && BlueTeamStat.ItemsSource != null)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock text)
                    {
                        string name = text.Text;
                        foreach (var source in BlueTeamStat.ItemsSource)
                        {
                            if (source is PvPPlayerStat stat)
                            {
                                if (box.IsChecked == true)
                                {
                                    if (!stat.StatList.Where(x => x.Item1 == name + ": ").Any())
                                    {
                                        stat.StatList.Add(new Tuple<string, string>(name + ": ", StatFunctions[name](stat)));
                                    }
                                }
                                else
                                {
                                    stat.StatList.Remove(stat.StatList.Where(x => x.Item1 == name + ": ").FirstOrDefault());
                                }
                            }
                        }
                    }
                }
            }
        }

        private void YellowTeamStatCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox box && box.Parent is DockPanel panel && YellowTeamStat.ItemsSource != null)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock text)
                    {
                        string name = text.Text;
                        foreach (var source in YellowTeamStat.ItemsSource)
                        {
                            if (source is PvPPlayerStat stat)
                            {
                                if (box.IsChecked == true)
                                {
                                    if (!stat.StatList.Where(x => x.Item1 == name + ": ").Any())
                                    {
                                        stat.StatList.Add(new Tuple<string, string>(name + ": ", StatFunctions[name](stat)));
                                    }
                                }
                                else
                                {
                                    stat.StatList.Remove(stat.StatList.Where(x => x.Item1 == name + ": ").FirstOrDefault());
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.GameSelectBox.SelectionChanged -= GameSelectBox_SelectionChanged;
        }
    }
}
