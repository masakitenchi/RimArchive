using RimWorld;
using System.Collections.Generic;
using Verse;
using HarmonyLib;

namespace RimArchive.GameComponents;


/* GameComponent_BossGroup解说：
 * lastBossGroupCalled：上一次召唤时的Tick数
 * timesCalledBossgroups：顾名思义，Dic<BossGroupDef,int>
 * allBosses：暂存所有boss
 * killedBosses：顾名思义
 * bossgroupTmp、calledTmp：分别为timesCalledBossgroups的Key和Value
 */
[HarmonyPatch]
public class RaidManager : GameComponent
{
    public int lastRaidCalled = -9999999;
    private List<BossDef> allBosses;
    private List<BossDef> killedBosses;
    private Dictionary<RaidDef, int> timesCalledRaid;
    private List<RaidDef> raidTmp;
    private List<int> calledTmp;

    public List<BossDef> Raids
    {
        get
        {
            if (allBosses.NullOrEmpty())
            {
                allBosses = new List<BossDef>();
                allBosses.AddRange(DefDatabase<BossDef>.AllDefs);
            }
            return allBosses;
        }
    }

    public void Notify_RaidCalled(RaidDef raidDef)
    {
        lastRaidCalled = Find.TickManager.TicksGame;
        if (timesCalledRaid.ContainsKey(raidDef))
            timesCalledRaid[raidDef]++;
        else
            timesCalledRaid.Add(raidDef, 0);
    }
    public void Notify_PawnKilled(Pawn pawn)
    {
        BossDef bossForRaid = GetBossForKind(pawn.kindDef);
        if (bossForRaid == null || this.killedBosses.Contains(bossForRaid))
            return;
        this.killedBosses.Add(bossForRaid);
    }

    private BossDef GetBossForKind(PawnKindDef kindDef) => allBosses.FirstOrDefault(x => x.kindDef == kindDef);

    public RaidManager(Game game)
    {

    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastRaidCalled, "lastRaidCalled", -9999999);
        Scribe_Collections.Look<BossDef>(ref this.killedBosses, "killedBosses", LookMode.Def);
        Scribe_Collections.Look<RaidDef, int>(ref timesCalledRaid, "timesCalledRaid", LookMode.Def, LookMode.Value, ref raidTmp, ref calledTmp);
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
                    BossDef bossForKind = GetBossForKind(def);
                    if (bossForKind != null && !killedBosses.Contains(bossForKind))
                    {
                        killedBosses.Add(bossForKind);
                    }
                }
            }
        }
        if(Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            timesCalledRaid ??= new Dictionary<RaidDef, int>();
            killedBosses ??= new List<BossDef> ();
        }
    }

    #region Harmony
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameComponent_Bossgroup), nameof(GameComponent_Bossgroup.Notify_PawnKilled))]
    public static void Postfix(Pawn pawn)
    {
        RimArchiveMain.RaidManager.Notify_PawnKilled(pawn);
    }
    #endregion

    #region DebugAction
    
    public void DebugResetRaid() => this.killedBosses.Clear();
    #endregion
}
