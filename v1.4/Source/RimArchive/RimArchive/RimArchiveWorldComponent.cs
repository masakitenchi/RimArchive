using RimWorld;
using RimWorld.Planet;
using System.Linq;
using UnityEngine.Assertions.Must;
using Verse;

namespace RimArchive
{
    internal class RimArchiveWorldComponent : WorldComponent
    {
        public static readonly string Shale = RAFactionDefOf.Shale.ToString();
        public RimArchiveWorldComponent(World world) 
            : base(world)
        {
        }
    }
}
