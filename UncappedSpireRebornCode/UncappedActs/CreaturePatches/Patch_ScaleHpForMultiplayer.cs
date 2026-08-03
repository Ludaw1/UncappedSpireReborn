using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs.CreaturePatches;

[HarmonyPatch(typeof(Creature), nameof(Creature.ScaleHpForMultiplayer))]
public class Patch_ScaleHpForMultiplayer
{
    [HarmonyPrefix]
    public static void Prefix(Creature __instance, ref decimal hp, EncounterModel? encounter, int playerCount, int actIndex)
    {
        hp *= (decimal)UncappedActsContext.CurrentScalingHp;
    }
}
