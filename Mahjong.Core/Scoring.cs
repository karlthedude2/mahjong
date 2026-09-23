using System.Collections.Generic;

namespace Mahjong.Core
{
    /// <summary>Time-driven state. It keeps running through undo and redo.</summary>
    public sealed class GameClock
    {
        public const int SuperBonusWindow = 30;

        public int Seconds { get; private set; }

        /// <summary>Counts down from 5 minutes. What's left pays out along the way and at the end.</summary>
        public int BonusSeconds { get; private set; } = 300;

        /// <summary>Counts down the current speed-bonus window.</summary>
        public int SuperBonusSeconds { get; private set; } = SuperBonusWindow;

        /// <summary>Paid at the end of a fast game; shrinks once the game passes about 2.5 minutes.</summary>
        public int MinuteBonus { get; private set; } = 5000;

        public string ClockText
        {
            get
            {
                int hours = Seconds / 3600;
                int minutes = Seconds / 60 % 60;
                int secs = Seconds % 60;
                return hours > 0 ? $"{hours}:{minutes:00}:{secs:00}" : $"{minutes:00}:{secs:00}";
            }
        }

        /// <summary>Advances one second. Returns true when the speed-bonus window ran out.</summary>
        internal bool Tick()
        {
            Seconds++;

            if (BonusSeconds > 0)
            {
                BonusSeconds--;
            }

            MinuteBonus -= MinuteBonusPenalty(Seconds);

            if (--SuperBonusSeconds == 0)
            {
                ResetSuperBonus();
                return true;
            }

            return false;
        }

        internal void ResetSuperBonus() => SuperBonusSeconds = SuperBonusWindow;

        private static int MinuteBonusPenalty(int seconds)
        {
            switch (seconds)
            {
                case 160:
                case 170:
                    return 1000;
                case 180:
                case 195:
                case 210:
                case 225:
                case 240:
                    return 500;
                case 255:
                case 270:
                    return 250;
                default:
                    return 0;
            }
        }
    }

    /// <summary>Everything that undo and redo restore.</summary>
    public sealed class ScoreCard
    {
        public int Score { get; internal set; }

        /// <summary>Tiles removed since the last every-20-tiles time bonus.</summary>
        public int TilesTowardTimeBonus { get; internal set; }

        /// <summary>Tiles removed in the current speed-bonus window.</summary>
        public int SuperBonusTilesRemoved { get; internal set; }

        public int SuperBonusScore { get; internal set; }
        public int SuperBonusQuantity { get; internal set; }
        public List<string> SuperBonusHistory { get; private set; } = new List<string>();
        public int ShuffleBonus { get; internal set; } = 3000;

        internal ScoreCard Clone()
        {
            var copy = (ScoreCard)MemberwiseClone();
            copy.SuperBonusHistory = new List<string>(SuperBonusHistory);
            return copy;
        }
    }

    public sealed class Scoring
    {
        public const int TilesPerBonus = 20;
        public const int FinalBonusPerSecond = 30;

        public GameClock Clock { get; } = new GameClock();

        public ScoreCard Card { get; private set; } = new ScoreCard();

        /// <summary>The end-of-game bonus is only paid if the game finishes before the bonus clock runs out.</summary>
        public bool EarnsFinalBonus => Clock.BonusSeconds > 0;

        public int FinalTimeBonus => Clock.BonusSeconds * FinalBonusPerSecond;

        internal void Tick()
        {
            if (Clock.Tick())
            {
                Card.SuperBonusTilesRemoved = 0;
            }
        }

        internal void TileRemoved(TileFace face)
        {
            Card.Score += face.Value;

            if (++Card.TilesTowardTimeBonus == TilesPerBonus)
            {
                Card.Score += Clock.BonusSeconds;
                Card.TilesTowardTimeBonus = 0;
            }

            if (Clock.SuperBonusSeconds > 0 && ++Card.SuperBonusTilesRemoved == TilesPerBonus)
            {
                int bonus = 100 * Clock.SuperBonusSeconds;
                Card.Score += bonus;
                Card.SuperBonusScore += bonus;
                Card.SuperBonusQuantity++;
                Card.SuperBonusHistory.Add($"{Card.SuperBonusHistory.Count + 1}  {bonus}");

                Clock.ResetSuperBonus();
                Card.SuperBonusTilesRemoved = 0;
            }
        }

        internal void Shuffled()
        {
            if (Card.ShuffleBonus > 0)
            {
                Card.ShuffleBonus -= 1000;
            }
        }

        internal void ApplyFinalBonus()
        {
            if (EarnsFinalBonus)
            {
                Card.Score += FinalTimeBonus + Card.ShuffleBonus + Clock.MinuteBonus;
            }
        }

        internal ScoreCard Snapshot() => Card.Clone();

        internal void Restore(ScoreCard card) => Card = card.Clone();
    }
}
