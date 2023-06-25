using RimWorld;
using System.Collections.Generic;
using Verse;
using HarmonyLib;
using System.Linq;
using static RimArchive.DebugMessage;
using System;
using System.Text;
using UnityEngine;

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
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static void Postfix(Pawn __instance)
    {
        RimArchiveMain.RaidManager.Notify_PawnKilled(__instance);
    }
}
#endregion

public class RaidManager : GameComponent
{
    private static readonly List<RaidDef> allRaids = DefDatabase<RaidDef>.AllDefsListForReading;
    public int lastRaidCalled = -9999999;
    private int? difficultyComing;
    private Dictionary<RaidDef, int> highestDifficulty;
    private HashSet<BossDef> killedBosses;
    private List<RaidDef> raidTmp;
    private List<int> difficultyTmp;
    private RaidDef currentRaid;

    public bool RaidIncoming => difficultyComing.HasValue && difficultyComing.Value >= 0;

    public IEnumerable<FloatMenuOption> Waves
    {
        get
        {
            for (int i = 0; i < CurrentRaid.waves.Count; i++)
            {
                RaidGroupWave wave = CurrentRaid.waves[i];
                FloatMenuOption option = new FloatMenuOption(wave.GetWaveDescription(i), () => this.StartRaid(wave), this.CurrentRaid.icon, Color.white);
                if (RaidIncoming)
                {
                    option.Label += "\n" + "RaidIncoming".Translate();
                    option.action = null;
                }
                yield return option;
            }
        }
    }
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
            DbgMsg("Re-shuffling by GameComponentTick");
            CurrentRaid = allRaids.RandomElement();
        }
    }

    public List<ValueTuple<string, int>> BestScore
    {
        get
        {
            List<ValueTuple<string, int>> results = new List<ValueTuple<string, int>>();
            foreach (var a in highestDifficulty)
            {
                results.Add((ValueTuple<string, int>)(a.Key.label, a.Value));
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
        //Log.Message($"Pawn:{pawn?.def.defName}");
        if (!RaidIncoming)
            return;
        BossDef bossForRaid = GetBossForRaid(pawn.kindDef);
        //Log.Message($"BossForRaid : {bossForRaid}");
        if (bossForRaid == null)
            return;
        //Log.Message($"Passed if statement : {bossForRaid == null}");
        this.killedBosses.Add(bossForRaid);
        if(!this.highestDifficulty.TryAdd(CurrentRaid, difficultyComing.Value))
            this.highestDifficulty[CurrentRaid] = Math.Max(difficultyComing.Value, this.highestDifficulty[CurrentRaid]);
        difficultyComing = null;
    }

    //看来这块确实有点麻烦，要想让多个bossDef(准确来说，是多个PawnKindDef)对应一个RaidDef需要额外的努力
    private BossDef GetBossForRaid(PawnKindDef kindDef)
    {
        return CurrentRaid.waves.FirstOrFallback(x => x.bossOverride?.kindDef == kindDef)?.bossOverride ?? (CurrentRaid.boss.kindDef == kindDef ? CurrentRaid.boss : null);
    }

    private BossDef GetBossForRaid(RaidDef raidDef, int waveInt) => raidDef.waves[waveInt].bossOverride ?? raidDef.boss;

    public void StartRaid(RaidGroupWave wave)
    {
        currentRaid.Worker.Resolve(Find.CurrentMap, CurrentRaid.waves.FindIndex(x => x == wave));
    }

    public override void ExposeData()
    {
        base.ExposeData();
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
                killedBosses ??= new HashSet<BossDef>();
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
            killedBosses ??= new HashSet<BossDef>();
        }
    }

    public RaidManager(Game game)
    {

    }


    #region DebugAction

    public void DebugResetRaid()
    {
        this.killedBosses.Clear();
        this.difficultyComing = null;
    }

    public void DebugRandomRaid() => this.CurrentRaid = allRaids.RandomElement();
    #endregion
}
