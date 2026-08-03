using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs.CombatCmdPatches;

[HarmonyPatch]
public class AttackCommandPatches
{
    private static readonly FieldInfo Field_DamagePerHit = AccessTools.Field(typeof(AttackCommand), "_damagePerHit");

    [HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.FromMonster))]
    [HarmonyPrefix]
    public static void Prefix(AttackCommand __instance)
    {
        var damagePerHit = (decimal)Field_DamagePerHit.GetValue(__instance)!;
        Field_DamagePerHit.SetValue(__instance, damagePerHit * (decimal)UncappedActsContext.CurrentScalingDmg);
    }
}
