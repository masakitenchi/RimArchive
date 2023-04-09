using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimArchive;

public class QuestPart_RaidArrives : QuestPartActivable
{
    public int minDelay;
    public int maxDelay;
    public MapParent mapParent;
    public RaidDef raidDef;

    public override void QuestPartTick()
    {
        int ticksGame = Find.TickManager.TicksGame;
        if (this.enableTick + this.minDelay > ticksGame)
            return;
        if (this.enableTick + this.maxDelay <= ticksGame)
        {
            this.Complete();
        }
        else
        {
            if (!this.mapParent.IsHashIntervalTick(2500))
                return;
            this.Complete();
        }
    }

    public override void DoDebugWindowContents(Rect innerRect, ref float curY)
    {
        if (this.State != QuestPartState.Enabled)
            return;
        int numTicks1 = this.enableTick + this.minDelay - Find.TickManager.TicksGame;
        int numTicks2 = this.enableTick + this.maxDelay - Find.TickManager.TicksGame;
        string str1 = "";
        if (numTicks1 >= 0)
            str1 = str1 + "\nTicks until available: " + (object)numTicks1 + " (" + numTicks1.ToStringTicksToPeriodVerbose() + ")";
        string str2 = str1 + "\nTicks until forced arrival: " + (object)numTicks2 + " (" + numTicks2.ToStringTicksToPeriodVerbose() + ")" ;
        Vector2 vector2 = Text.CalcSize(str2);
        Rect rect1 = new Rect(innerRect.x, curY, innerRect.width, vector2.y);
        Widgets.Label(rect1, str2);
        curY += rect1.height + 4f;
        Rect rect2 = new Rect(innerRect.x, curY, 500f, 25f);
        if (Widgets.ButtonText(rect2, "End " + this.ToString()))
            this.Complete();
        curY += rect2.height;
    }
}

public class QuestPart_RaidGroup : QuestPart_MakeLord
{
    public List<Pawn> bosses = new List<Pawn>();
    public IntVec3 stageLocation;


    protected override Lord MakeLord() => LordMaker.MakeNewLord(faction, new LordJob_BossgroupAssaultColony(faction, stageLocation, bosses), mapParent.Map);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref stageLocation, "stageLocation");
        Scribe_Collections.Look(ref bosses, "bosses", LookMode.Reference);
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
            return;
        this.bosses.RemoveAll(x => x == null);
    }
}