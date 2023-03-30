using Verse;

namespace RimArchive
{
    internal static class Debug
    {
        internal static void DbgMsg(string message)
        {
            Log.Message("[RimArchiveMain]" + message);
        }
        internal static void DbgWrn(string message)
        { 
            Log.Warning("[RimArchiveMain]" + message);
        }
        internal static void DbgErr(string message) 
        {
            Log.Error("[RimArchiveMain]" + message);
        }
    }
}
