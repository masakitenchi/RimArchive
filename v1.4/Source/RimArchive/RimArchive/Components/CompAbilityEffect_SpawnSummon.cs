using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimArchive

{
    public class CompProperties_SpawnSummon : CompProperties_AbilityEffect
    {

        public CompProperties_SpawnSummon() => compClass = typeof(CompAbilityEffect_SpawnSummon);

        public string ThingToSummon;

    }

    public class CompAbilityEffect_SpawnSummon : CompAbilityEffect
    {
        public List<IntVec2> One;

        new public CompProperties_SpawnSummon Props => this.props as CompProperties_SpawnSummon;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            List<Thing> list = new List<Thing>();
            list.AddRange(this.SummonTarget(target, map).SelectMany((IntVec3 c) => from t in c.GetThingList(map)
                                                                                   where t.def.category == ThingCategory.Item
                                                                                   select t));
            foreach (Thing thing in list)
            {
                thing.DeSpawn(DestroyMode.Vanish);
            }
            foreach (IntVec3 intVec in this.SummonTarget(target, map))
            {
                GenSpawn.Spawn(DefDatabase<ThingDef>.AllDefsListForReading.Find(x => x.defName == Props.ThingToSummon), intVec, map, WipeMode.Vanish);
                FleckMaker.ThrowDustPuffThick(intVec.ToVector3Shifted(), map, Rand.Range(1.5f, 3f), CompAbilityEffect_SpawnSummon.DustColor);
                CompAbilityEffect_Teleport.SendSkipUsedSignal(intVec, this.parent.pawn);
            }
        }

        private IEnumerable<IntVec3> SummonTarget(LocalTargetInfo target, Map map)
        {
            IntVec3 intVec2 = target.Cell + new IntVec3(0, 0, 0);
            if (intVec2.InBounds(map))
            {
                yield return intVec2;
            }
            List<IntVec2>.Enumerator enumerator = default(List<IntVec2>.Enumerator);
            yield break;
            yield break;

        }

        public static Color DustColor = new Color(0.55f, 0.85f, 0.55f, 1f);

        private ThingDef thing;

        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            this.thing = DefDatabase<ThingDef>.AllDefsListForReading.Find(x => x.defName == Props.ThingToSummon);
        }

    }


}
