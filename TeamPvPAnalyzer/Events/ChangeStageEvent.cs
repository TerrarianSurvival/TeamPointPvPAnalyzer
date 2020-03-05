namespace TeamPvPAnalyzer.Events
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Enums;

    public class ChangeStageEvent : GameEvent
    {
        public ChangeStageEvent(DateTime time)
            : base(time, EventType.ChangeStage)
        {
        }

        public ChangeStageEvent(DateTime time, string[] args)
            : base(time, EventType.ChangeStage)
        {
            ParseFromArguments(args);
        }

        public StageData.StageName CurrentStage { get; protected set; }

        public StageData.StageName OldStage { get; protected internal set; }

        public override ILogEvent ParseFromArguments(string[] args)
        {
            switch (args[3].Substring(7))
            {
                case "Espeon":
                    CurrentStage = StageData.StageName.Espeon;
                    break;

                case "Hell":
                    CurrentStage = StageData.StageName.Hell;
                    break;

                case "Ice":
                    CurrentStage = StageData.StageName.Ice;
                    break;

                case "Forest":
                    CurrentStage = StageData.StageName.Forest;
                    break;
            }

            return this;
        }

        public override void CreateIcons()
        {
            var grid = Utils.CreateBaseLogIcon(Brushes.Black);

            var label = new Label
            {
                Content = string.Format(CultureInfo.InvariantCulture, "{0}", CurrentStage.ToString()),
                Foreground = Brushes.White,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center,
            };
            grid.Children.Add(label);

            LogIcon = grid;
        }
    }
}
