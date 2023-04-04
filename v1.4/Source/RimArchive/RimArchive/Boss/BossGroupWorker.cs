using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace RimArchive;


//全部重写
//周期参考每次总力设为7天
//新想法：不限制每次招的CD，改为每七天更换一次boss池（暂时搁置）
public class BossGroupWorker
{
    public const int TimeBetweenAllBossgroups = 420000;
    public RaidDef def;
    public AcceptanceReport CanResolve(Pawn caller)
    {
        int num = Find.TickManager.TicksGame - RimArchiveMain.RaidManager.lastRaidCalled;
        if (num < TimeBetweenAllBossgroups)
        {
            return (AcceptanceReport)"BossgroupAvailableIn".Translate((NamedArgument)(TimeBetweenAllBossgroups - num).ToStringTicksToPeriod());
        }
        PawnKindDef pendingBossgroup = CallBossgroupUtility.GetPendingBossgroup();
        return pendingBossgroup != null ? (AcceptanceReport)"BossgroupIncoming".Translate((NamedArgument)pendingBossgroup.label) : (AcceptanceReport)true;
    }

    public AcceptanceReport ShouldSummonNow(Map map)
    {
        return (AcceptanceReport)true;
    }

    public void Resolve(Map map, int wave)
    {
        RimArchiveMain.RaidManager.Notify_RaidCalled(this.def);
        Slate vars = new Slate();
        vars.Set<RaidDef>("bossgroup", this.def);
        vars.Set<Map>(nameof(map), map);
        vars.Set<ThingDef>("reward", this.def.rewardDef);
        vars.Set<int>(nameof(wave), wave);
        QuestUtility.GenerateQuestAndMakeAvailable(this.def.quest, vars);
    }
}
