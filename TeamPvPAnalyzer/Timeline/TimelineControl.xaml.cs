// <copyright file="TimelineControl.xaml.cs" company="TerrarianSurvival">
// Copyright (c) 2020 TerrarianSurvival. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace TeamPvPAnalyzer.Timeline
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TeamPvPAnalyzer.Timeline.Parts;

    /// <summary>
    /// TimelineControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineControl : UserControl, IDisposable
    {
        public const int LogElementPassSecond = 5;

        private const int LogElementWidth = 100;
        private const int HeaderHeight = 25;
        private const int WidthPerSecond = LogElementWidth / LogElementPassSecond;

        private const int CancelWaitMS = 50;
        private const int WaitMS = 5;

        private readonly List<TimelineElement> elements = new List<TimelineElement>();
        private readonly List<TimelineElement> visibleElements = new List<TimelineElement>();

        private readonly List<Layer> layers = new List<Layer>();

        private DateTime currentTime;
        private DateTime startTime;
        private DateTime endTime;

        private CancellationTokenSource cancellationTokenSource = null;
        private double canvasHeight;
        private double canvasWidth;

        private bool disposed = false;

        private bool playingBarMoving = false;
        private bool continueWithPlay = false;

        public TimelineControl()
        {
            InitializeComponent();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IsPlaying)
            {
                continueWithPlay = true;
                Pause();
            }
            else
            {
                Trace.WriteLine("Not IsPlaying");
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                if (!IsPlaying)
                {
                    return currentTime;
                }

                return startTime;
            }

            set
            {
                if (!IsPlaying)
                {
                    currentTime = value;
                }
            }
        }

        public Canvas MapCanvas { get; set; }

        public bool IsPlaying { get; private set; }

        public async Task SetDatas(IEnumerable<TimelineElement> elements)
        {
            bool playRequested = false;
            if (IsPlaying)
            {
                cancellationTokenSource.Cancel();

                // キャンセルされるかCancelWaitMSまで待機
                int waited = 0;
                while (IsPlaying)
                {
                    Task.Delay(WaitMS).Wait();
                    waited += WaitMS;
                    if (waited >= CancelWaitMS)
                    {
                        break;
                    }
                }

                playRequested = true;
            }

            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            this.elements.Clear();
            this.elements.AddRange(elements);
            visibleElements.Clear();

            if (this.elements.Count == 0)
            {
                throw new ArgumentException("logEvents.Count is zero.");
            }

            layers.Clear();
            startTime = this.elements[0].Time;
            endTime = this.elements[^1].Time;
            var length = endTime - startTime;

            if (currentTime < startTime || currentTime > endTime)
            {
                currentTime = startTime;
            }

            await this.MapCanvas.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.MapCanvas.Children.Clear();
            }));

            // 待つ
            _ = LogGrid.Dispatcher.BeginInvoke((Action)(() =>
            {
                LogGrid.Children.Clear();
                LogGrid.Children.Add(PlayingBar);
                LogGrid.Children.Add(TimeLineBorder);
                LogGrid.Width = (length.TotalMilliseconds / 1000d * WidthPerSecond) + TimeLineView.ViewportWidth;

                var marginSpan = new TimeSpan((long)Math.Ceiling(TimeLineView.ViewportWidth / WidthPerSecond) * 10000000);
                LogGrid.Children.Add(CreateScale(length + marginSpan, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 10)));
            })).Result;

            int layerHeight = 45;
            int margin = 5;
            foreach (var element in elements)
            {
                bool allocated = false;

                int layerIndex = 0;
                foreach (var layer in layers)
                {
                    if (layer.Allocate(element))
                    {
                        allocated = true;
                        break;
                    }

                    layerIndex++;
                }

                if (!allocated)
                {
                    var layer = new Layer();
                    layers.Add(layer);
                    layer.Allocate(element);
                }

                if (element.TimelineIcon != null)
                {
                    // elementの中身がセットされてから追加
                    _ = element.TimelineIcon.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        element.TimelineIcon.HorizontalAlignment = HorizontalAlignment.Left;
                        element.TimelineIcon.VerticalAlignment = VerticalAlignment.Top;

                        element.TimelineIcon.Margin = new Thickness((element.Time - startTime).TotalMilliseconds * WidthPerSecond / 1000, HeaderHeight + margin + (layerIndex * layerHeight), 0, 0);
                    })).Result;

                    await LogGrid.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        LogGrid.Children.Add(element.TimelineIcon);
                    }));
                }
            }

            await LogGrid.Dispatcher.BeginInvoke((Action)(() =>
            {
                LogGrid.Height = Math.Max((layerHeight * layers.Count) + HeaderHeight + margin, TimeLineView.ViewportHeight);
            }));

            if (playRequested)
            {
                await Task.Run(() => Play()).ConfigureAwait(false);
            }
        }

        public void ClearEvents()
        {
            if (IsPlaying)
            {
                cancellationTokenSource.Cancel();

                // キャンセルされるかCancelWaitMSまで待機
                int waited = 0;
                while (IsPlaying)
                {
                    Task.Delay(WaitMS).Wait();
                    waited += WaitMS;
                    if (waited >= CancelWaitMS)
                    {
                        break;
                    }
                }
            }

            elements.Clear();
            visibleElements.Clear();
            layers.Clear();

            MapCanvas?.Children.Clear();

            LogGrid.Children.Clear();
            LogGrid.Children.Add(PlayingBar);
            LogGrid.Children.Add(TimeLineBorder);
        }

        public virtual async Task Play()
        {
            if (IsPlaying)
            {
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            await PlayAsync(cancellationTokenSource.Token).ConfigureAwait(false);
        }

        public virtual void Pause()
        {
            if (!IsPlaying)
            {
                return;
            }

            cancellationTokenSource.Cancel();
        }

        public virtual void Stop()
        {
            if (IsPlaying)
            {
                cancellationTokenSource.Cancel();

                // キャンセルされるかCancelWaitMSまで待機
                int waited = 0;
                while (IsPlaying)
                {
                    Task.Delay(WaitMS).Wait();
                    waited += WaitMS;
                    if (waited >= CancelWaitMS)
                    {
                        break;
                    }
                }
            }

            MapCanvas.Children.Clear();
            visibleElements.Clear();
            currentTime = startTime;
            PlayingBar.Margin = new Thickness(0);
        }

        public virtual void Reset()
        {
            ClearEvents();
            PlayingBar.Margin = new Thickness(0);
        }

        protected virtual async Task PlayAsync(CancellationToken cancellationToken, double speed = 1d)
        {
            if (IsPlaying)
            {
                return;
            }

            IsPlaying = true;

            DateTime last = DateTime.UtcNow;
            while (currentTime <= endTime)
            {
                var now = DateTime.UtcNow;
                var diff = new TimeSpan((long)((now - last).Ticks * speed));

                currentTime += diff;

                await PlayingBar.Dispatcher.BeginInvoke((Action)(() => { PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0); }));
                await UpdateElements().ConfigureAwait(false);

                last = now;
                Thread.Sleep(5);

                if (cancellationToken.IsCancellationRequested)
                {
                    IsPlaying = false;
                    return;
                }
            }

            currentTime = endTime;
            IsPlaying = false;
        }

        private static Grid CreateScale(TimeSpan length, TimeSpan interval, TimeSpan bigInterval)
        {
            var grid = new Grid();

            for (TimeSpan time = new TimeSpan(0, 0, 0); time < length; time += interval)
            {
                // 大きい目盛り(時刻付き)
                if (time.Ticks % bigInterval.Ticks == 0)
                {
                    var rect = new Rectangle()
                    {
                        Height = HeaderHeight * 0.60,
                        Width = 2,
                        Fill = Brushes.DarkGray,
                        Margin = new Thickness((time.TotalMilliseconds / 1000d * WidthPerSecond) - 1, HeaderHeight * 0.40, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    var timeText = new Label()
                    {
                        Content = time.Hours > 0 ? time.ToString("s", Thread.CurrentThread.CurrentCulture) : time.ToString(@"mm\:ss", Thread.CurrentThread.CurrentCulture),
                        FontSize = 10,
                        Width = 50,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Background = Brushes.Transparent,
                    };
                    timeText.Margin = new Thickness((time.TotalMilliseconds / 1000d * WidthPerSecond) - (50 / 2d), (HeaderHeight * 0.25) - 15, 0, 0);

                    grid.Children.Add(rect);
                    grid.Children.Add(timeText);
                }

                // 小さい目盛り
                else
                {
                    var rect = new Rectangle()
                    {
                        Height = HeaderHeight * 0.5,
                        Width = 1,
                        Fill = Brushes.DarkGray,
                        Margin = new Thickness(time.TotalMilliseconds / 1000d * WidthPerSecond, HeaderHeight * 0.5, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    grid.Children.Add(rect);
                }
            }

            return grid;
        }

        private async Task UpdateElements()
        {
            // Update elements
            foreach (var element in elements)
            {
                // 実行順を守るため、同期的に実行する
                // リセットをした時に確実に止める意味もある
                element.UpdateAsync(currentTime).ConfigureAwait(false);
                if (element.RequireApperenceUpdate)
                {
                    if (element.ShowMapIcon
                        && !visibleElements.Contains(element)
                        && element.FixedEventPosition.X >= 0
                        && element.FixedEventPosition.Y >= 0)
                    {
                        visibleElements.Add(element);
                        await MapCanvas.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Canvas.SetLeft(element.MapIcon, (element.FixedEventPosition.X * canvasWidth) - (element.MapIcon.Width / 2));
                            Canvas.SetTop(element.MapIcon, (element.FixedEventPosition.Y * canvasHeight) - element.MapIcon.Height);
                            MapCanvas.Children.Add(element.MapIcon);
                        }));
                    }
                }
                else
                {
                    if (visibleElements.Contains(element))
                    {
                        visibleElements.Remove(element);
                        await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { MapCanvas.Children.Remove(element.MapIcon); }));
                    }
                }
            }

            // Update elements appearance
            foreach (var visible in visibleElements)
            {
                visible.UpdateAppearanceAsync(currentTime).ConfigureAwait(false);
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                return;
            }

            await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasWidth = MapCanvas.Width; }));
            await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasHeight = MapCanvas.Height; }));

            await Task.Run(() => Play()).ConfigureAwait(false);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void LogGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsPlaying)
            {
                e.Handled = true;

                cancellationTokenSource.Cancel();

                // キャンセルされるかCancelWaitMSまで待機
                int waited = 0;
                while (IsPlaying)
                {
                    Task.Delay(WaitMS).Wait();
                    waited += WaitMS;
                    if (waited >= CancelWaitMS)
                    {
                        break;
                    }
                }

                continueWithPlay = true;
            }

            if (playingBarMoving)
            {
                return;
            }

            e.Handled = true;

            playingBarMoving = true;
            currentTime = startTime + new TimeSpan((long)Math.Ceiling(e.GetPosition(LogGrid).X / WidthPerSecond * 10000000));
            PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0);

            canvasHeight = MapCanvas.Height;
            canvasWidth = MapCanvas.Width;
            UpdateElements();
        }

        private void LogGrid_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (!playingBarMoving)
                {
                    return;
                }

                if (IsPlaying)
                {
                    return;
                }

                e.Handled = true;

                currentTime = startTime + new TimeSpan((long)Math.Ceiling(e.GetPosition(LogGrid).X / WidthPerSecond * 10000000));
                PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0);

                canvasHeight = MapCanvas.Height;
                canvasWidth = MapCanvas.Width;
                UpdateElements();
            }
        }

        private async void LogGrid_PreviewMouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!playingBarMoving)
            {
                return;
            }

            if (IsPlaying)
            {
                return;
            }

            e.Handled = true;

            currentTime = startTime + new TimeSpan((long)Math.Ceiling(e.GetPosition(LogGrid).X / WidthPerSecond * 10000000));
            PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0);

            playingBarMoving = false;

            if (continueWithPlay)
            {
                foreach (var visible in visibleElements)
                {
                    await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { MapCanvas.Children.Remove(visible.MapIcon); }));
                }

                visibleElements.Clear();

                await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasWidth = MapCanvas.Width; }));
                await MapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasHeight = MapCanvas.Height; }));

                await Task.Run(() => Play()).ConfigureAwait(false);
                continueWithPlay = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                cancellationTokenSource?.Dispose();
            }

            disposed = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Deactivated += Window_Deactivated;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Deactivated -= Window_Deactivated;
        }
    }
}
