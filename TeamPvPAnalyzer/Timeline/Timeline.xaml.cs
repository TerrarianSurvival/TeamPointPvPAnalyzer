namespace TeamPvPAnalyzer.Timeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TeamPvPAnalyzer.Enums;
    using TeamPvPAnalyzer.Timeline.Parts;

    /// <summary>
    /// Timeline.xaml の相互作用ロジック
    /// </summary>
    public partial class Timeline : UserControl
    {
        public const int LogElementPassTime = 5;

        private const int LogElementWidth = 100;
        private const int HeaderHeight = 25;
        private const int WidthPerSecond = LogElementWidth / LogElementPassTime;

        private readonly List<TimelineElement> elements = new List<TimelineElement>();
        private readonly List<TimelineElement> visibleElements = new List<TimelineElement>();

        private readonly List<Layer> layers = new List<Layer>();

        private DateTime currentTime;
        private DateTime startTime;
        private DateTime endTime;

        private CancellationTokenSource cancellationTokenSource = null;

        private Canvas mapCanvas;

        private double canvasHeight;
        private double canvasWidth;

        public Timeline()
        {
            InitializeComponent();
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

        public bool IsPlaying { get; private set; }

        public void SetDatas(IEnumerable<TimelineElement> elements, Canvas mapCanvas)
        {
            if (IsPlaying)
            {
                return;
            }

            this.mapCanvas = mapCanvas ?? throw new ArgumentNullException(nameof(mapCanvas));

            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            this.elements.Clear();
            this.elements.AddRange(elements);

            layers.Clear();

            LogGrid.Children.Clear();
            LogGrid.Children.Add(PlayingBar);
            LogGrid.Children.Add(TimeLineBorder);

            if (this.elements.Count == 0)
            {
                throw new ArgumentException("logEvents.Count is zero.");
            }

            currentTime = this.elements[0].Time;
            startTime = currentTime;
            endTime = this.elements[this.elements.Count - 1].Time;

            var length = endTime - startTime;
            LogGrid.Width = (length.TotalMilliseconds / 1000d * WidthPerSecond) + TimeLineView.ViewportWidth;

            var marginSpan = new TimeSpan((long)Math.Ceiling(TimeLineView.ViewportWidth / WidthPerSecond) * 10000000);
            LogGrid.Children.Add(CreateScale(length + marginSpan, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 10)));

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
                    element.TimelineIcon.HorizontalAlignment = HorizontalAlignment.Left;
                    element.TimelineIcon.VerticalAlignment = VerticalAlignment.Top;

                    element.TimelineIcon.Margin = new Thickness((element.Time - startTime).TotalMilliseconds * WidthPerSecond / 1000, HeaderHeight + margin + (layerIndex * layerHeight), 0, 0);

                    LogGrid.Children.Add(element.TimelineIcon);
                }
            }

            LogGrid.Height = layerHeight * layers.Count;
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
            if (!IsPlaying)
            {
                return;
            }

            cancellationTokenSource.Cancel();

            mapCanvas.Children.Clear();

            visibleElements.Clear();

            currentTime = startTime;
        }

        protected virtual async Task PlayAsync(CancellationToken cancellationToken, double speed = 1d)
        {
            IsPlaying = true;

            DateTime last = DateTime.UtcNow;

            while (currentTime <= endTime)
            {
                var now = DateTime.UtcNow;
                var diff = new TimeSpan((long)((now - last).Ticks * speed));

                currentTime += diff;

                await PlayingBar.Dispatcher.BeginInvoke((Action)(() => { PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0); }));

                await UpdateElements(diff).ConfigureAwait(false);

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

        private async Task UpdateElements(TimeSpan diff)
        {
            // Update elements
            foreach (var element in elements)
            {
                // 実行順を守るため、同期的に実行する
                // リセットをした時に確実に止める意味もある
                element.UpdateAsync(currentTime, diff).ConfigureAwait(false);
                if (element.RequireApperenceUpdate)
                {
                    if (element.ShowMapIcon
                        && !visibleElements.Contains(element)
                        && element.FixedEventPosition.X >= 0
                        && element.FixedEventPosition.Y >= 0)
                    {
                        visibleElements.Add(element);
                        await mapCanvas.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Canvas.SetLeft(element.MapIcon, (element.FixedEventPosition.X * canvasWidth) - (element.MapIcon.Width / 2));
                            Canvas.SetTop(element.MapIcon, (element.FixedEventPosition.Y * canvasHeight) - element.MapIcon.Height);
                            mapCanvas.Children.Add(element.MapIcon);
                        }));
                    }
                }
                else
                {
                    if (visibleElements.Contains(element))
                    {
                        visibleElements.Remove(element);
                        await mapCanvas.Dispatcher.BeginInvoke((Action)(() => { mapCanvas.Children.Remove(element.MapIcon); }));
                    }
                }
            }

            // Update elements appearance
            foreach (var visible in visibleElements)
            {
                visible.UpdateAppearanceAsync(currentTime, diff).ConfigureAwait(false);
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await mapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasWidth = mapCanvas.Width; }));
            await mapCanvas.Dispatcher.BeginInvoke((Action)(() => { canvasHeight = mapCanvas.Height; }));

            var task = Task.Run(() => Play()).ConfigureAwait(false);
            await task;
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
                return;
            }

            currentTime = startTime + new TimeSpan((long)Math.Ceiling(e.GetPosition(LogGrid).X / WidthPerSecond * 10000000));
            PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0);
        }

        private void LogGrid_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsPlaying)
            {
                return;
            }

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                currentTime = startTime + new TimeSpan((long)Math.Ceiling(e.GetPosition(LogGrid).X / WidthPerSecond * 10000000));
                PlayingBar.Margin = new Thickness((currentTime - startTime).TotalMilliseconds * WidthPerSecond / 1000d, 0, 0, 0);
            }
        }
    }
}
