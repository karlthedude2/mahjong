namespace Mahjong.Web.Client.Game;

public enum BonusKind
{
    Speed,
    Pace,
}

/// <summary>A bonus just earned, for its badge.</summary>
public sealed record EarnedBonus(BonusKind Kind, int Points);

/// <summary>
/// Watches the score card's speed and pace bonus totals and reports a bonus when one goes up. The
/// first look only notes the totals, and a total going down (an undo) reports nothing.
/// </summary>
public sealed class BonusWatch
{
    private (int Speed, int Pace)? last;

    public IReadOnlyList<EarnedBonus> Update(int speedTotal, int paceTotal)
    {
        var earned = new List<EarnedBonus>();
        if (last is var (speed, pace))
        {
            if (speedTotal > speed)
            {
                earned.Add(new EarnedBonus(BonusKind.Speed, speedTotal - speed));
            }

            if (paceTotal > pace)
            {
                earned.Add(new EarnedBonus(BonusKind.Pace, paceTotal - pace));
            }
        }

        last = (speedTotal, paceTotal);
        return earned;
    }
}
