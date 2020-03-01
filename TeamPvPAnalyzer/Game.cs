namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using TeamPvPAnalyzer.Enums;
    using TeamPvPAnalyzer.Events;

    public class Game
    {
        private List<ILogEvent> inGameEvents = new List<ILogEvent>();

        private List<ILogEvent> preGameStartEvents = new List<ILogEvent>();

        public Game(StageData.StageName stageName, DateTime start, DateTime end, TeamID winnerTeam, int bluePoint, int yellowPoint)
        {
            Stage = stageName;
            Start = start;
            End = end;
            WinnerTeam = winnerTeam;
            BluePoint = bluePoint;
            YellowPoint = yellowPoint;
        }

        public StageData.StageName Stage { get; }

        public DateTime Start { get; }

        public DateTime End { get; }

        public TeamID WinnerTeam { get; }

        public int BluePoint { get; }

        public int YellowPoint { get; }

        public ReadOnlyCollection<ILogEvent> InGameEvents
        {
            get { return inGameEvents.AsReadOnly(); }
        }

        public ReadOnlyCollection<ILogEvent> PreGameStartEvents
        {
            get { return preGameStartEvents.AsReadOnly(); }
        }

        public ReadOnlyCollection<ILogEvent> AllEvents
        {
            get
            {
                var list = new List<ILogEvent>(preGameStartEvents);
                list.AddRange(inGameEvents);
                return list.AsReadOnly();
            }
        }

        public void AddInGameEvent(ILogEvent @event)
        {
            inGameEvents.Add(@event);
        }

        public void AddPreGameStartEvents(ILogEvent @event)
        {
            preGameStartEvents.Add(@event);
        }

        public void AddRangeInGameEvent(IEnumerable<ILogEvent> events)
        {
            inGameEvents.AddRange(events);
        }

        public void AddRangePreGameStartEvents(IEnumerable<ILogEvent> events)
        {
            preGameStartEvents.AddRange(events);
        }
    }
}
