using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs.CreaturePatches;

[HarmonyPatch(typeof(Creature), nameof(Creature.ScaleMonsterHpForMultiplayer))]
public class Patch_ScaleMonsterHpForMultiplayer
{
    private static readonly MethodInfo Method_SetMaxHp = AccessTools.PropertySetter(typeof(Creature), nameof(Creature.MaxHp));

    [HarmonyPrefix]
    public static void Prefix(Creature __instance, EncounterModel? encounter, int playerCount, int actIndex)
    {
        if (playerCount == 1)
        {
            var scaledHp = (int)(__instance.MaxHp * UncappedActsContext.CurrentScalingHp);
            Method_SetMaxHp.Invoke(__instance, [scaledHp]);
            __instance.SetCurrentHpInternal(__instance.MaxHp);
        }
    }
}
