namespace TeamPvPAnalyzer.Timeline.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Layer
    {
        private List<Tuple<DateTime, TimeSpan>> allocatedTimes = new List<Tuple<DateTime, TimeSpan>>();
        private List<TimelineElement> elements = new List<TimelineElement>();

        public Layer()
        {
        }

        public bool Allocate(TimelineElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var duration = new TimeSpan(0, 0, TimelineControl.LogElementPassTime);
            var end = element.Time + duration;
            foreach (var tuple in allocatedTimes)
            {
                var allocatedEnd = tuple.Item1 + tuple.Item2;

                // すでに占有済み
                if ((tuple.Item1 <= end && end <= allocatedEnd)
                    || (element.Time <= allocatedEnd && allocatedEnd <= end))
                {
                    return false;
                }
            }

            allocatedTimes.Add(new Tuple<DateTime, TimeSpan>(element.Time, duration));
            elements.Add(element);
            return true;
        }

        public void Release(TimelineElement element)
        {
            if (allocatedTimes.Count > 0)
            {
                allocatedTimes.Remove(allocatedTimes.First(x => x.Item1 == element.Time));
                elements.Remove(element);
            }
        }
    }
}
