namespace RimArchive;

[DefOf]
public class StudentDefOf
{

    static StudentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(StudentDefOf));
    }
}

[DefOf]
public class PawnKindDefOfLocal
{
    public static PawnKindDef RA_PawnKindDef_Sensei;

    static PawnKindDefOfLocal()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOfLocal));
    }
}