namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

// Holds a reference to the active run's UncappedActsModifier so Harmony patches
// (which don't naturally have access to "the current run") can read chapter/scaling info.
public static class UncappedActsContext
{
    public static UncappedActsModifier? State { get; set; }

    public static bool IsCurrentlyInRun => State != null;

    public static int CurrentChapter => IsCurrentlyInRun ? State!.CurrentChapter : 1;

    // Scaling compounds per completed chapter: chapter 1 is always 1x.
    public static float CurrentScalingHp =>
        (float)Math.Pow(UncappedActsConfig.ScalingHpIncrement, CurrentChapter - 1);

    public static float CurrentScalingDmg =>
        (float)Math.Pow(UncappedActsConfig.ScalingDmgIncrement, CurrentChapter - 1);
}
