using RimWorld;

namespace RimArchive
{
    [DefOf]
    internal class SchoolDefOf
    {
        static SchoolDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SchoolDefOf));
        }

        SchoolDefOf()
        {

        }
    }
}
