using System;
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
    private static void ResetRaidData() => RimArchiveMain.RaidManager.DebugResetRaid();
    [DebugAction("ReShuffleRaid", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ReShuffleRaid() => RimArchiveMain.RaidManager.DebugRandomRaid();

}
