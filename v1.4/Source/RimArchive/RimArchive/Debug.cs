using Verse;

namespace RimArchive
{
    internal static class Debug
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
}
