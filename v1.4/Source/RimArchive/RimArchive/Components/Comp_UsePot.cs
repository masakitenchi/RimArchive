using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using Verse;
using System.Linq;
using System.Text;
using CombatExtended;
using System.Collections;
#pragma warning disable CS1591

namespace RimArchive
{
    /// <summary>
    /// 汤锅一次性只治疗2次伤，与医疗箱作区别
    /// </summary>
    public class HealPawnWound_Pot
    {
        public static TaggedString HealWound(Pawn pawn)
        {
            StringBuilder HealWound = new StringBuilder();
            float num = 2f;
            for (int i = 0; i < num; i++)
            {
                Hediff hediff = HealPawnWound_Pot.FindInjury(pawn, null);
                if (hediff != null)
                {
                    HealWound.Append(HealthUtility.Cure(hediff));
                }
            }
            return HealWound.ToString();
        }

        private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediff_Injury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury2 = hediffs[i] as Hediff_Injury;
                if (hediff_Injury2 != null && hediff_Injury2.Visible && hediff_Injury2.def.everCurableByItem && (allowedBodyParts == null || allowedBodyParts.Contains(hediff_Injury2.Part)) && (hediff_Injury == null || hediff_Injury2.Severity > hediff_Injury.Severity))
                {
                    hediff_Injury = hediff_Injury2;
                }
            }
            return hediff_Injury;
        }
    }

    /// <summary>
    /// 依然是类陷阱触发的方式，需要尝试用List来避免被同一个pawn重复使用
    /// </summary>
    public class CompProperties_contactPot : CompProperties
    {
        public CompProperties_contactPot() => compClass = typeof(CompContactPot);
        
    }

    public class CompContactPot : ThingComp
    {
        float times = 4f;
        public void DoEffect(Pawn pawn)
        {
            Messages.Message(HealPawnWound.HealWound(pawn), MessageTypeDefOf.PositiveEvent);
           
            times--;
            if (times <= 0)
            {
                this.parent.Destroy();
            }

        }
    }

    public class contactPot : Building

    {
        public List<Pawn> PawnsAlreadyHeal = new List<Pawn>();

        public void CheckSpring(Pawn p)///这里是实现道具触发的地方
        {

            if (Rand.Chance(this.SpringChance(p)))///检测
            {
                if (PawnsAlreadyHeal.Contains(p))
                {
                    return;
                }
                else
                {
                    PawnsAlreadyHeal.Add(p);
                    Map map = base.Map;
                    this.TryGetComp<CompContactPot>().DoEffect(p);
                    if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
                    {
                        Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);///治疗并将p加入列表中
                    }
                }
            }
        }
        
        protected float SpringChance(Pawn p)
        {
            return p.Faction == Faction.OfPlayer ? 1 : 0;
        }
        public bool WhoContact(Pawn p)
        {
            return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) || (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState) || (p.guest != null && p.guest.Released) || (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction) || (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction) || (p.Faction == null && p.RaceProps.Humanlike);
        }

        public override void Tick()
        {
            if (this.Spawned)
            {
                List<Thing> thingList = this.Position.GetThingList(this.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && !this.touchingPawns.Contains(pawn))
                    {
                        this.touchingPawns.Add(pawn);
                        this.CheckSpring(pawn);
                    }
                }
                for (int j = 0; j < this.touchingPawns.Count; j++)
                {
                    Pawn pawn2 = this.touchingPawns[j];
                    if (pawn2 == null || !pawn2.Spawned || pawn2.Position != this.Position)
                    {
                        this.touchingPawns.Remove(pawn2);
                    }
                }
            }
            base.Tick();
        }




        private List<Pawn> touchingPawns = new List<Pawn>();

        








    }

}
