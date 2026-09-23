namespace Mahjong.Core
{
    /// <summary>Time-driven state. It keeps running through undo and redo.</summary>
    public sealed class GameClock
    {
        /// <summary>Length of the bonus clock: finishing inside it earns the end-of-game bonuses.</summary>
        public const int BonusClockLength = 300;

        /// <summary>Length of each speed-bonus window.</summary>
        public const int SpeedWindowLength = 30;

        public const int QuickFinishBonusStart = 5000;

        public int Seconds { get; private set; }

        /// <summary>Counts down from 5 minutes. Pace bonuses and the time bonus pay out what's left.</summary>
        public int BonusClockSeconds { get; private set; } = BonusClockLength;

        /// <summary>Seconds left in the current speed-bonus window.</summary>
        public int SpeedWindowSeconds { get; private set; } = SpeedWindowLength;

        /// <summary>Paid for finishing fast; shrinks in steps once the game passes 2:40.</summary>
        public int QuickFinishBonus { get; private set; } = QuickFinishBonusStart;

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

            if (BonusClockSeconds > 0)
            {
                BonusClockSeconds--;
            }

            QuickFinishBonus -= QuickFinishPenalty(Seconds);

            if (--SpeedWindowSeconds == 0)
            {
                ResetSpeedWindow();
                return true;
            }

            return false;
        }

        internal void ResetSpeedWindow() => SpeedWindowSeconds = SpeedWindowLength;

        private static int QuickFinishPenalty(int seconds)
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

    /// <summary>
    /// The running score and every part it's made of. This is what undo and redo restore.
    /// <see cref="Score"/> always equals the sum of the parts.
    /// </summary>
    public sealed class ScoreCard
    {
        public const int NoShuffleBonusStart = 3000;

        public int Score { get; internal set; }

        /// <summary>Points from the tiles themselves.</summary>
        public int TilePoints { get; internal set; }

        /// <summary>Tiles removed since the last pace bonus (one is paid every 20 tiles).</summary>
        public int TilesTowardPaceBonus { get; internal set; }

        /// <summary>Total of all pace bonuses: each pays the seconds left on the bonus clock.</summary>
        public int PaceBonusTotal { get; internal set; }

        /// <summary>Tiles removed in the current speed-bonus window.</summary>
        public int SpeedWindowTiles { get; internal set; }

        /// <summary>How many speed bonuses were earned (20 tiles inside one window).</summary>
        public int SpeedBonusCount { get; internal set; }

        public int SpeedBonusTotal { get; internal set; }

        /// <summary>Paid at the end for not shuffling; each shuffle costs 1000.</summary>
        public int NoShuffleBonus { get; internal set; } = NoShuffleBonusStart;

        /// <summary>Set once the game is finished: the end-of-game bonuses that were paid.</summary>
        public FinalBonuses Final { get; internal set; }

        public ScoreBreakdown Breakdown => new ScoreBreakdown(
            TilePoints,
            SpeedBonusCount,
            SpeedBonusTotal,
            PaceBonusTotal,
            Final?.TimeBonus ?? 0,
            Final?.QuickFinishBonus ?? 0,
            Final?.NoShuffleBonus ?? 0,
            Score);

        internal ScoreCard Clone() => (ScoreCard)MemberwiseClone();
    }

    /// <summary>The end-of-game bonuses, all zero if the game took longer than the bonus clock.</summary>
    public sealed class FinalBonuses
    {
        public FinalBonuses(int timeBonus, int quickFinishBonus, int noShuffleBonus)
        {
            TimeBonus = timeBonus;
            QuickFinishBonus = quickFinishBonus;
            NoShuffleBonus = noShuffleBonus;
        }

        public int TimeBonus { get; }
        public int QuickFinishBonus { get; }
        public int NoShuffleBonus { get; }
        public int Total => TimeBonus + QuickFinishBonus + NoShuffleBonus;
    }

    /// <summary>A score split into the parts a player can understand.</summary>
    public sealed class ScoreBreakdown
    {
        public ScoreBreakdown(int tilePoints, int speedBonusCount, int speedBonusTotal, int paceBonusTotal,
            int timeBonus, int quickFinishBonus, int noShuffleBonus, int total)
        {
            TilePoints = tilePoints;
            SpeedBonusCount = speedBonusCount;
            SpeedBonusTotal = speedBonusTotal;
            PaceBonusTotal = paceBonusTotal;
            TimeBonus = timeBonus;
            QuickFinishBonus = quickFinishBonus;
            NoShuffleBonus = noShuffleBonus;
            Total = total;
        }

        public int TilePoints { get; }
        public int SpeedBonusCount { get; }
        public int SpeedBonusTotal { get; }
        public int PaceBonusTotal { get; }
        public int TimeBonus { get; }
        public int QuickFinishBonus { get; }
        public int NoShuffleBonus { get; }
        public int Total { get; }
    }

    /// <summary>
    /// The scoring rules:
    /// <list type="bullet">
    /// <item>Each tile is worth its face value.</item>
    /// <item>Pace bonus: every 20 tiles adds the seconds left on the 5-minute bonus clock.</item>
    /// <item>Speed bonus: 20 tiles inside one 30-second window adds 100 x the seconds left in it.</item>
    /// <item>At the end, if the bonus clock hasn't run out: time bonus (seconds left x 30),
    /// quick-finish bonus, and no-shuffle bonus.</item>
    /// </list>
    /// </summary>
    public sealed class Scoring
    {
        public const int TilesPerBonus = 20;
        public const int SpeedBonusPerSecond = 100;
        public const int TimeBonusPerSecond = 30;
        public const int ShufflePenalty = 1000;

        public GameClock Clock { get; } = new GameClock();

        public ScoreCard Card { get; private set; } = new ScoreCard();

        /// <summary>The end-of-game bonuses are only paid if the game finishes before the bonus clock runs out.</summary>
        public bool EarnsFinalBonus => Clock.BonusClockSeconds > 0;

        /// <summary>What the time bonus would be if the game finished now.</summary>
        public int TimeBonusNow => Clock.BonusClockSeconds * TimeBonusPerSecond;

        internal void Tick()
        {
            if (Clock.Tick())
            {
                Card.SpeedWindowTiles = 0;
            }
        }

        internal void TileRemoved(TileFace face)
        {
            Card.TilePoints += face.Value;
            Card.Score += face.Value;

            if (++Card.TilesTowardPaceBonus == TilesPerBonus)
            {
                Card.PaceBonusTotal += Clock.BonusClockSeconds;
                Card.Score += Clock.BonusClockSeconds;
                Card.TilesTowardPaceBonus = 0;
            }

            if (Clock.SpeedWindowSeconds > 0 && ++Card.SpeedWindowTiles == TilesPerBonus)
            {
                int bonus = SpeedBonusPerSecond * Clock.SpeedWindowSeconds;
                Card.SpeedBonusTotal += bonus;
                Card.Score += bonus;
                Card.SpeedBonusCount++;

                Clock.ResetSpeedWindow();
                Card.SpeedWindowTiles = 0;
            }
        }

        internal void Shuffled()
        {
            if (Card.NoShuffleBonus > 0)
            {
                Card.NoShuffleBonus -= ShufflePenalty;
            }
        }

        internal void ApplyFinalBonus()
        {
            if (Card.Final != null)
            {
                return;
            }

            Card.Final = EarnsFinalBonus
                ? new FinalBonuses(TimeBonusNow, Clock.QuickFinishBonus, Card.NoShuffleBonus)
                : new FinalBonuses(0, 0, 0);
            Card.Score += Card.Final.Total;
        }

        internal ScoreCard Snapshot() => Card.Clone();

        internal void Restore(ScoreCard card) => Card = card.Clone();
    }
}
