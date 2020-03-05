namespace TeamPvPAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media;
    using TeamPvPAnalyzer.Events;

    public class PvPPlayerStat
    {
        public ReadOnlyCollection<KeyValuePair<string, TimeSpan>> ClassHistory { get; }

        public ReadOnlyDictionary<string, TimeSpan> ClassTime { get; }

        public string MostUsedClass { get; }

        public ImageSource IconSource { get; }

        public int TotalDamage { get; } = 0;

        public int DamageCount { get; } = 0;

        public int TotalRecieveDamage { get; } = 0;

        public int RecieveDamageCount { get; } = 0;

        public int DeathCount { get; } = 0;

        public int KillCount { get; } = 0;

        public int TeleportCount { get; } = 0;

        public int SpawnCount { get; } = 0;

        public ObservableCollection<Tuple<string, string>> StatList { get; } = new ObservableCollection<Tuple<string, string>>();

        public PvPPlayerStat(PvPPlayer player, DateTime startTime, DateTime endTime)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));

            Player.SetPropertysAt(startTime);

            var classHistory = new List<KeyValuePair<string, TimeSpan>>();

            var classTime = new Dictionary<string, TimeSpan>();

            var lastTime = startTime;

            foreach (var @event in player.Events)
            {
                if (@event.Time < startTime)
                {
                    continue;
                }
                else if (@event.Time > endTime)
                {
                    break;
                }

                if (@event is KillerEvent killerEvent)
                {
                    TotalDamage += killerEvent.Damage;
                    DamageCount++;

                    if (killerEvent.Type == Enums.EventType.KillPlayer)
                    {
                        KillCount++;
                    }
                    else if (killerEvent.Type == Enums.EventType.DealDamageToPlayer)
                    {
                        // Do nothing for now
                    }
                }
                else if (@event is VictimEvent victimEvent)
                {
                    TotalRecieveDamage += victimEvent.Damage;
                    RecieveDamageCount++;

                    if (victimEvent.Type == Enums.EventType.DeathByPlayer)
                    {
                        DeathCount++;
                    }
                    else if (victimEvent.Type == Enums.EventType.DamagedByPlayer)
                    {
                        // Do nothing for now
                    }
                }
                else if (@event is DeathByOtherEvent deathByOther)
                {
                    TotalRecieveDamage += deathByOther.Damage;
                    RecieveDamageCount++;
                    DeathCount++;
                }
                else if (@event is RecieveDamageByOtherEvent recieveDamage)
                {
                    TotalRecieveDamage += recieveDamage.Damage;
                    RecieveDamageCount++;
                }
                else if (@event.Type == Enums.EventType.TeleportPlayer)
                {
                    TeleportCount++;
                }
                else if (@event.Type == Enums.EventType.PlayerSpawn)
                {
                    SpawnCount++;
                }
                else if (@event is ChangeClassEvent changeClass)
                {
                    classHistory.Add(new KeyValuePair<string, TimeSpan>(changeClass.OldClass, changeClass.Time - lastTime));

                    if (!classTime.ContainsKey(changeClass.OldClass))
                    {
                        classTime.Add(changeClass.OldClass, changeClass.Time - lastTime);
                    }
                    else
                    {
                        classTime[changeClass.OldClass] += changeClass.Time - lastTime;
                    }

                    lastTime = changeClass.Time;

                    @event.DoEffect();
                }
            }

            classHistory.Add(new KeyValuePair<string, TimeSpan>(Player.Class, endTime - lastTime));

            if (!classTime.ContainsKey(Player.Class))
            {
                classTime.Add(Player.Class, endTime - lastTime);
            }
            else
            {
                classTime[Player.Class] += endTime - lastTime;
            }

            ClassTime = new ReadOnlyDictionary<string, TimeSpan>(classTime);

            ClassHistory = classHistory.AsReadOnly();

            MostUsedClass = classTime.OrderByDescending(x => x.Value).First().Key;

            IconSource = Utils.GetImageSource(MostUsedClass.ToLowerInvariant());
        }

        public PvPPlayer Player { get; }
    }
}
