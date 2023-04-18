﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimArchive;

public class CompProperties_UseMedikit : CompProperties
{
    public CompProperties_UseMedikit() => compClass = typeof(Comp_UseMedikit);
}
public class Comp_UseMedikit : CompUseEffect
{
    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        Log.Message("Step1");
        TaggedString taggedString = HealPawnWound.HealWound(usedBy);
    }


}

public class HealPawnWound
{
    public static TaggedString HealWound(Pawn pawn)
    {
        StringBuilder HealWound = new StringBuilder();
        float num = 10f;
        for (int i = 0; i < num; i++)
        {
            Hediff hediff = HealPawnWound.FindInjury(pawn, null);
            if (hediff != null)
            {
                Log.Message("Step2");
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

//备用方法：类似陷阱的触发方式
public class CompProperties_contackMedkit : CompProperties
{
    public CompProperties_contackMedkit() => compClass = typeof(contactMedikit);
}

public class contactMedikit : Thing

{
    public void DoEffect(Pawn pawn)
    {
        Log.Message("Step1");
        TaggedString taggedString = HealPawnWound.HealWound(pawn);
    }
    public void CheckSpring(Pawn p)
    {
        if (Rand.Chance(this.SpringChance(p)))
        {
            Map map = base.Map;
            this.DoEffect(p);
            if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
            {
                Messages.Message("Heal Success".Translate(), MessageTypeDefOf.PositiveEvent);
                this.Destroy(DestroyMode.Vanish);
            }
        }
    }
    protected float SpringChance(Pawn p)
    {
        float num = 1f;
        if (this.WhoContact(p))
        {
            if (p.Faction == null)
            {
                num = 0f;
            }
            else if (p.Faction == base.Faction)
            {
                num = 1f;
            }
            else
            {
                num = 0f;
            }
        }
        num *= this.GetStatValue(StatDefOf.TrapSpringChance, true, -1) * p.GetStatValue(StatDefOf.PawnTrapSpringChance, true, -1);
        return Mathf.Clamp01(num);
    }
    public bool WhoContact(Pawn p)
    {
        return (p.Faction != null && !p.Faction.HostileTo(base.Faction)) || (p.Faction == null && p.RaceProps.Animal && !p.InAggroMentalState) || (p.guest != null && p.guest.Released) || (!p.IsPrisoner && base.Faction != null && p.HostFaction == base.Faction) || (p.RaceProps.Humanlike && p.IsFormingCaravan()) || (p.IsPrisoner && p.guest.ShouldWaitInsteadOfEscaping && base.Faction == p.HostFaction) || (p.Faction == null && p.RaceProps.Humanlike);
    }

    public override void Tick()
    {
        if (base.Spawned)
        {
            List<Thing> thingList = base.Position.GetThingList(base.Map);
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
                if (pawn2 == null || !pawn2.Spawned || pawn2.Position != base.Position)
                {
                    this.touchingPawns.Remove(pawn2);
                }
            }
        }
        base.Tick();
    }




    private List<Pawn> touchingPawns = new List<Pawn>();

}



    

