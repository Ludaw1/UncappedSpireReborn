using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedUpgrades.CardModelPatches;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel), MethodType.Getter)]
public static class Patch_MaxUpgradeLevel
{
    private const bool LogCardIdsOnFirstSight = true;
    private static readonly HashSet<ModelId> AlreadyLogged = [];

    public static void Postfix(CardModel __instance, ref int __result)
    {
        string branch;

        if (UpgradeCapsConfig.EnergyOnlyCards.Contains(__instance.Id))
        {
            __result = __instance.EnergyCost.Canonical;
            branch = "energy-only";
        }
        else if (UpgradeCapsConfig.CardUpgradeMaxMap.TryGetValue(__instance.Id, out var maxLevel))
        {
            __result = maxLevel;
            branch = $"explicit cap ({maxLevel})";
        }
        else
        {
            __result = int.MaxValue;
            branch = "uncapped (default)";
        }

        if (LogCardIdsOnFirstSight && AlreadyLogged.Add(__instance.Id))
        {
            MainFile.Logger.Info($"[UpgradeCaps] {__instance.Id} -> {branch}");
        }
    }
}