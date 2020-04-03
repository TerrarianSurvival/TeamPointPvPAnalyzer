namespace TeamPvPAnalyzer.Timeline
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using TeamPvPAnalyzer.Events;

    public class TimelineElement : FrameworkElement
    {
        private Point fixedEventPos = new Point(-1, -1);

        public TimelineElement(ILogEvent logEvent, Point? fixedPoint = null, bool showMapIcon = true)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (fixedPoint.HasValue)
            {
                fixedEventPos = fixedPoint.Value;
            }

            Time = logEvent.Time;
            Event = logEvent;

            TimelineIcon = logEvent.LogIcon;
            try
            {
                MapIcon = logEvent.MapIcon;
            }
            catch (Exception e)
            {

            }
            if (MapIcon == null)
            {
                ShowMapIcon = false;
            }
            else
            {
                ShowMapIcon = showMapIcon;
                MapIcon.Width = 40;
                MapIcon.Height = 40;
            }
        }

        public DateTime Time { get; }

        public ILogEvent Event { get; }

        public bool RequireApperenceUpdate { get; protected set; }

        public FrameworkElement TimelineIcon { get; }

        public FrameworkElement MapIcon { get; }

        public bool ShowMapIcon { get; }

        public Point FixedEventPosition
        {
            get
            {
                return fixedEventPos;
            }
        }

        public virtual async Task UpdateAsync(DateTime currentTime, TimeSpan elapsed)
        {
            if (currentTime >= Time && currentTime <= Time + new TimeSpan(0, 0, Timeline.LogElementPassTime))
            {
                RequireApperenceUpdate = true;
            }
            else
            {
                RequireApperenceUpdate = false;
            }
        }

        public virtual async Task UpdateAppearanceAsync(DateTime currentTime, TimeSpan elapsed)
        {
            await MapIcon.Dispatcher.BeginInvoke(
                (Action)(() =>
                {
                    MapIcon.Opacity -= elapsed.TotalMilliseconds / 5000d;
                }));
        }

        public virtual void Reset()
        {
        }
    }
}
