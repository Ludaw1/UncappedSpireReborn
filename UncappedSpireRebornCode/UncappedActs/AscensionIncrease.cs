using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

// Direct port of Tobiline's AscensionIncrease.cs. Reaches into private RunManager/AscensionManager
// fields via reflection because there's no public API to raise ascension mid-run. Verify field
// names ("_level", "maxAscensionAllowed") still match in your BaseLib/game version - if they've
// been renamed this will throw a NullReferenceException at runtime rather than fail to compile.
public static class AscensionIncrease
{
    private static readonly FieldInfo Field_Level = AccessTools.Field(typeof(AscensionManager), "_level");
    private static readonly FieldInfo Field_MaxAscensionAllowed = AccessTools.Field(typeof(AscensionManager), "maxAscensionAllowed");

    public static void IncrementAscension(Player player)
    {
        var ascensionManager = RunManager.Instance.AscensionManager;
        var level = (int)Field_Level.GetValue(ascensionManager)!;
        var maxAscensionAllowed = (int)Field_MaxAscensionAllowed.GetValue(ascensionManager)!;

        if (level >= maxAscensionAllowed)
        {
            MainFile.Logger.Info("[UncappedActs] Already at max ascension; skipping increment.");
            return;
        }

        var ascensionLevelSetter = AccessTools.PropertySetter(typeof(RunState), nameof(RunState.AscensionLevel));
        ascensionLevelSetter.Invoke(player.RunState, [player.RunState.AscensionLevel + 1]);

        var ascensionManagerSetter = AccessTools.PropertySetter(typeof(RunManager), nameof(RunManager.AscensionManager));
        ascensionManagerSetter.Invoke(RunManager.Instance, [new AscensionManager(player.RunState.AscensionLevel)]);

        MainFile.Logger.Info($"[UncappedActs] Ascension increased to {player.RunState.AscensionLevel}.");

        // NOTE: Tobiline's version also applies immediate side effects for specific ascension levels
        // reached (e.g. TightBelt -> subtract a potion slot, AscendersBane -> add the curse card).
        // Deliberately left out here since those thresholds/behaviors are game-specific and worth
        // deciding deliberately rather than blindly porting - happy to add once you confirm which
        // ascension effects you want to actually fire on a manual increment like this.
    }
}
