using RimWorld.QuestGen;

namespace RimArchive;


//全部重写
//周期参考每次总力设为5天
public class BossGroupWorker
{
    public const int TimeBetweenAllBossgroups = 300000;
    public RaidDef def;
    public AcceptanceReport CanResolve(Pawn caller)
    {
        int num = Find.TickManager.TicksGame - RimArchive.RaidManager.lastRaidCalled;
        if (num < TimeBetweenAllBossgroups)
        {
            return (AcceptanceReport)"BossgroupAvailableIn".Translate((NamedArgument)(TimeBetweenAllBossgroups - num).ToStringTicksToPeriod());
        }
        PawnKindDef pendingBossgroup = CallBossgroupUtility.GetPendingBossgroup();
        return pendingBossgroup != null ? (AcceptanceReport)"BossgroupIncoming".Translate((NamedArgument)pendingBossgroup.label) : (AcceptanceReport)true;
    }

    public AcceptanceReport ShouldSummonNow()
    {
        return RimArchive.RaidManager.RaidIncoming ? (AcceptanceReport)"RaidActive".Translate() : (AcceptanceReport)true;
    }

    public void Resolve(Map map, int wave)
    {
        RimArchive.RaidManager.Notify_RaidCalled(this.def, wave);
        Slate vars = new Slate();
        vars.Set<RaidDef>("bossgroup", this.def);
        vars.Set<Map>(nameof(map), map);
        vars.Set<ThingDef>("reward", this.def.rewardDef);
        vars.Set<int>(nameof(wave), wave);
        QuestUtility.GenerateQuestAndMakeAvailable(this.def.quest, vars);
    }
}
