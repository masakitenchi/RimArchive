using System;
using System.Runtime.CompilerServices;
using Verse;

namespace RimArchive;

internal static class DebugMessage
{
    internal static void DbgMsg(string message)
    {
        Log.Message("[RimArchive]" + message);
    }
    internal static void DbgWrn(string message)
    {
        Log.Warning("[RimArchive]" + message);
    }
    internal static void DbgErr(string message)
    {
        Log.Error("[RimArchive]" + message);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class DebugActionAttribute : Verse.DebugActionAttribute
{
    public DebugActionAttribute(
      string name = null,
      bool requiresRoyalty = false,
      bool requiresIdeology = false,
      bool requiresBiotech = false,
      int displayPriority = 0,
      bool hideInSubMenu = false) : base(category: "RimArchive", name, requiresRoyalty, requiresIdeology, requiresBiotech, displayPriority, hideInSubMenu) { }
}
public static class DebugTools
{
    [DebugAction("ResetRaidData", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ResetRaidData() => RimArchive.RaidManager.DebugResetRaid();
    [DebugAction("ReShuffleRaid", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ReShuffleRaid() => RimArchive.RaidManager.DebugRandomRaid();

    /*[DebugAction("OutputCurrentHediff", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void HediffOutput()
    {
        if(!RimArchiveMain.HediffGen.stages.NullOrEmpty())
        {
            StringBuilder sb = new StringBuilder();
            foreach (HediffStage stage in RimArchiveMain.HediffGen.stages)
            {
                sb.AppendLine(stage.minSeverity.ToString());
                foreach(StatModifier factor in stage.statFactors)
                {
                    sb.AppendLine(factor.stat.defName);
                    sb.AppendLine(factor.value.ToString());
                }
                sb.AppendLine();
            }
            Log.Message(sb.ToString());
        }
    }*/

}

[HarmonyPatch]
public static class VerbTrackerPatch
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.GetGizmos))]
    [HarmonyPostfix]
    public static IEnumerable<Gizmo> Pawn_EquipmentTracker_Postfix(IEnumerable<Gizmo> commands)
    {
        foreach (var command in commands)
        {
            if (command is Command_VerbTarget verb)
            {
                verb.defaultLabel = verb.verb.verbProps.defaultProjectile.ToString();
                Log.WarningOnce($"Command is : {verb.verb}, projectile is : {verb.verb.GetProjectile().defName}", verb.verb.GetProjectile().GetHashCode() * 1919810);
                yield return verb;
            }
        }
    }

/*    [HarmonyPatch(typeof(Targeter), "OrderPawnForceTarget")]
    [HarmonyPrefix]
    public static bool Targeter_Prefix(Targeter __instance, ITargetingSource targetingSource)
    {
        Log.Message($"pawn: {(targetingSource as Pawn).Name}, verb");
    }
*/
    /*[HarmonyPatch(typeof(Command_VerbTarget), nameof(Command_VerbTarget.GizmoUpdateOnMouseover))]
    [HarmonyPostfix]
    public static void Command_VerbTarget_GizmoUpdateOnMouseover_Postfix(Command_VerbTarget __instance)
    {
        Log.Message($"This is : {__instance.ToString()}, verb: {__instance.verb}");
    }*/

    [HarmonyPatch(typeof(Command_VerbTarget), nameof(Command_VerbTarget.ProcessInput))]
    [HarmonyPostfix]
    public static void Command_VerbTarget_ProcessInput_Postfix(Command_VerbTarget __instance)
    {
        Log.Message($"Clicking {__instance.verb + " --- " + __instance.verb.GetProjectile().defName}");
    }

    [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands))]
    [HarmonyPostfix]
    public static IEnumerable<Command>  CompEquippable_Postfix(IEnumerable<Command> __result, CompEquippable __instance)
    {
        foreach (var command in __result)
        {
            if(command is Command_VerbTarget verb)
                Log.WarningOnce($"parent :{__instance.parent}, verbs: \n{verb + "---" + verb.verb.verbProps.defaultProjectile.defName})", verb.verb.verbProps.defaultProjectile.GetHashCode() * 114514);
            yield return command;
        }
    }

    [HarmonyPatch(typeof(Verb), nameof(Verb.OrderForceTarget))]
    [HarmonyPostfix]
    public static void Verb_Postfix(Verb __instance, LocalTargetInfo target)
    {
        Log.Message($"Verb: {__instance + " --- " + __instance.GetProjectile().defName}, source: {(__instance.CasterIsPawn ? __instance.CasterPawn : __instance.Caster)} ");
    }
}