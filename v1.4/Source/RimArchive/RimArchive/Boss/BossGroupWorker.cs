using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace RimArchive;

public class BossGroupWorker : BossgroupWorker
{
    new public const int TimeBetweenAllBossgroups = 420000;
    new public BossGroupDef def;
    public override AcceptanceReport CanResolve(Pawn caller)
    {
        int num = Find.TickManager.TicksGame - RimArchive.RaidManager.lastBossgroupCalled;
        if (num < TimeBetweenAllBossgroups)
        {
            return (AcceptanceReport)"BossgroupAvailableIn".Translate((NamedArgument)(120000 - num).ToStringTicksToPeriod());
        }
        PawnKindDef pendingBossgroup = CallBossgroupUtility.GetPendingBossgroup();
        return pendingBossgroup != null ? (AcceptanceReport)"BossgroupIncoming".Translate((NamedArgument)pendingBossgroup.label) : (AcceptanceReport)true;
    }

    public override void Resolve(Map map, int wave)
    {
        RimArchive.RaidManager.Notify_BossgroupCalled(this.def);
        Slate vars = new Slate();
        vars.Set<BossgroupDef>("bossgroup", this.def);
        vars.Set<Map>(nameof(map), map);
        vars.Set<ThingDef>("reward", this.def.rewardDef);
        vars.Set<int>(nameof(wave), wave);
        QuestUtility.GenerateQuestAndMakeAvailable(this.def.quest, vars);
    }
}
