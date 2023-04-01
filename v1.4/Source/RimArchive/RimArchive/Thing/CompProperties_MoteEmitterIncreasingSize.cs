using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimArchive;

public class CompProperties_MoteEmitterIncreasingSize : CompProperties_MoteEmitter
{
    public float InitialSize = 0f;
    public CompProperties_MoteEmitterIncreasingSize() => compClass = typeof(CompMoteEmitterIncreasingSize);
}
