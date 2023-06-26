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
    /// 依然是类陷阱触发的方式，需要尝试用List来避免被同一个pawn重复使用
    /// </summary>
    public class CompProperties_ContactPot : CompProperties
    {
        public CompProperties_ContactPot() => compClass = typeof(CompContactPot);
        public int HealPawnCount = 4;
        public int WoundsToHealEach = 2;
    }

    public class CompContactPot : ThingComp
    {
        private int _healsleft;
        public CompProperties_ContactPot Props => this.props as CompProperties_ContactPot;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this._healsleft = Props.HealPawnCount;
        }
        public void DoEffect(Pawn pawn)
        {
            if (HealingUtility.TryHealWounds(pawn, out var Message, Props.WoundsToHealEach))
            {
                Messages.Message(Message, MessageTypeDefOf.PositiveEvent);
                _healsleft--;
            }
            if (_healsleft <= 0)
            {
                this.parent.Destroy();
            }
        }
    }

    public class ContactPot : TrapLikeBuilding
    {
        public HashSet<Pawn> PawnsAlreadyHealed = new HashSet<Pawn>();

        protected override void CheckSpring(Pawn p)///这里是实现道具触发的地方
        {
            if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)///检测
            {
                if (!PawnsAlreadyHealed.Add(p)) return;
                this.TryGetComp<CompContactPot>().DoEffect(p);
                Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);///治疗并将p加入列表中
            }
        }

    }

    /// <summary>
    /// 汤锅一次性只治疗2次伤，与医疗箱作区别
    /// </summary>
    /*public class HealPawnWound_Pot
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
    }*/
}
