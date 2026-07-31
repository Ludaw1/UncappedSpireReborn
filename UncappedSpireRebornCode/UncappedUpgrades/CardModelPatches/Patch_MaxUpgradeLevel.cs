using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedUpgrades.CardModelPatches;

// Step 1: remove the upgrade level cap entirely for every card.
// No per-card curation yet - that comes later once this is confirmed working.
[HarmonyPatch(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel), MethodType.Getter)]
public static class Patch_MaxUpgradeLevel
{
    public static void Postfix(CardModel __instance, ref int __result)
    {
        __result = int.MaxValue;
    }
}