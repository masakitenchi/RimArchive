using Verse;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RimArchive;

//果然还是不想和原版的混在一块
public class BossDef : Def
{
    public PawnKindDef kindDef;
    public int appearAfterTicks = int.MaxValue;

    public override IEnumerable<string> ConfigErrors()
    {
        BossDef bossDef = this;
        foreach (string str in base.ConfigErrors())
        yield return str;
        if (DefDatabase<BossDef>.AllDefs.Any((BossDef b) => b != this && b.kindDef == kindDef))
            yield return bossDef.kindDef.label + " appears in two BossDefs, only one PawnKind per boss is currently supported.";
    }
}
