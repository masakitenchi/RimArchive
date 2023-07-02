namespace RimArchive;

public class CompProperties_MoteEmitterIncreasingSize : CompProperties_MoteEmitter
{
    public float InitialSize = 0f;
    public CompProperties_MoteEmitterIncreasingSize() => compClass = typeof(CompMoteEmitterIncreasingSize);
}
