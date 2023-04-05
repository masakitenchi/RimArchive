﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using System.Linq;
using static RimArchive.DebugMessage;
using System;
using System.Text;

namespace RimArchive.GameComponents;


/* GameComponent_BossGroup解说：
 * lastBossGroupCalled：上一次召唤时的Tick数
 * timesCalledBossgroups：顾名思义，Dic<BossGroupDef,int>
 * allBosses：暂存所有boss
 * killedBosses：顾名思义
 * bossgroupTmp、calledTmp：分别为timesCalledBossgroups的Key和Value
 */

#region Harmony
[HarmonyPatch]
public static class Harmony_RaidManager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameComponent_Bossgroup), nameof(GameComponent_Bossgroup.Notify_PawnKilled))]
    public static void Postfix(Pawn pawn)
    {
        RimArchiveMain.RaidManager.Notify_PawnKilled(pawn);
    }
}
#endregion

public class RaidManager : GameComponent
{
    private static readonly List<RaidDef> allRaids = DefDatabase<RaidDef>.AllDefsListForReading;
    public int lastRaidCalled = -9999999;
    private int? lastRaidUpdated;
    private int? difficultyComing;
    private Dictionary<RaidDef, int> highestDifficulty;
    private List<BossDef> killedBosses;
    private List<RaidDef> raidTmp;
    private List<int> difficultyTmp;
    private RaidDef currentRaid;

    public bool RaidIncoming => difficultyComing.HasValue && difficultyComing.Value >= 0;
    public RaidDef CurrentRaid
    {
        get
        {
            currentRaid ??= allRaids.RandomElement();
            return currentRaid;
        }
        private set
        {
            currentRaid = value;
        }
    }

    public override void GameComponentTick()
    {
        if (Find.TickManager.TicksGame % GenDate.TicksPerTwelfth == 0)
        {
            DbgMsg("Re-shuffling by GameComponentUpdate");
            CurrentRaid = allRaids.RandomElement();
        }
    }

    public List<ValueTuple<string, int>> BestScore
    {
        get
        {
            List<ValueTuple<string, int>> results = new List<ValueTuple<string, int>>();
            foreach(var a in highestDifficulty)
            {
                results.Add((ValueTuple<string,int>)(a.Key.label, a.Value));
            }
            return results;
        }
    }

    public void Notify_RaidCalled(RaidDef raidDef, int difficulty)
    {
        difficultyComing = difficulty;
        lastRaidCalled = Find.TickManager.TicksGame;
    }

    public void Notify_PawnKilled(Pawn pawn)
    {
        BossDef bossForRaid = GetBossForRaid(pawn.kindDef);
        if (bossForRaid == null || this.killedBosses.Contains(bossForRaid))
            return;
        this.killedBosses.Add(bossForRaid);
    }

    //看来这块确实有点麻烦，要想让多个bossDef(准确来说，是多个PawnKindDef)对应一个RaidDef需要额外的努力
    private BossDef GetBossForRaid(PawnKindDef kindDef)
    {
        //RaidDef overrideRaid = allRaids.First(x => x.waves.Any(t => t.bossOverride?.kindDef == kindDef));


        return allRaids.FirstOrDefault(x => x.boss.kindDef == kindDef)?.boss;
    }

    private BossDef GetBossForRaid(RaidDef raidDef, int waveInt) => raidDef.waves[waveInt].bossOverride ?? raidDef.boss;

    public void StartRaid(RaidGroupWave wave)
    {
        currentRaid.Worker.Resolve(Find.CurrentMap, currentRaid.waves.FindIndex(x=> x == wave));
    }
    public RaidManager(Game game)
    {

    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastRaidUpdated, "lastRaidUpdated", 0);
        Scribe_Values.Look(ref lastRaidCalled, "lastRaidCalled", -9999999);
        Scribe_Defs.Look(ref currentRaid, "currentRaid");
        Scribe_Collections.Look<BossDef>(ref this.killedBosses, "killedBosses", LookMode.Def);
        Scribe_Collections.Look<RaidDef, int>(ref highestDifficulty, "highestDifficulty", LookMode.Def, LookMode.Value, ref raidTmp, ref difficultyTmp);
        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            HashSet<PawnKindDef> valueHashSet = null;
            Scribe_Collections.Look(ref valueHashSet, "killedBossgroupMechs");
            if (valueHashSet != null)
            {
                //实在想不到什么时候这几行会运行
                DebugMessage.DbgMsg("ValueHashSet != null");
                killedBosses ??= new List<BossDef>();
                foreach (PawnKindDef def in valueHashSet)
                {
                    BossDef bossForKind = GetBossForRaid(def);
                    if (bossForKind != null && !killedBosses.Contains(bossForKind))
                    {
                        killedBosses.Add(bossForKind);
                    }
                }
            }
        }
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            highestDifficulty ??= new Dictionary<RaidDef, int>();
            killedBosses ??= new List<BossDef>();
        }
    }


    #region DebugAction

    public void DebugResetRaid() => this.killedBosses.Clear();

    public void DebugRandomRaid() => this.CurrentRaid = allRaids.RandomElement();
    #endregion
}
