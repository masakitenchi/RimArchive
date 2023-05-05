using RimWorld;
using Verse;

namespace RimArchive;

public class CompProperties_DisappearAfterTicks : CompProperties
{
    public int TicksToDisappear = 0;
    public CompProperties_DisappearAfterTicks() => compClass = typeof(CompDisappearAfterTicks);
}
