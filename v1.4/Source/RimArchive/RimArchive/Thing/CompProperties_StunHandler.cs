namespace RimArchive;

public class CompProperties_StunHandler : CompProperties
{
    public float BaseStunThreshold = 0f;
    public float BreakStunDuration = 0f;
    public float currentDuration = 0f;
    public CompProperties_StunHandler() => compClass = typeof(CompStunHandler);
}
