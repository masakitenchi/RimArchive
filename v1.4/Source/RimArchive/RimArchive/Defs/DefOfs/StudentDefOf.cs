namespace RimArchive
{
    [DefOf]
    internal class StudentDefOf
    {

        static StudentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StudentDefOf));
        }
    }
}
