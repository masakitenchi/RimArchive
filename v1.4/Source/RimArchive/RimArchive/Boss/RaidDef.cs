using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimArchive;

/* BossGroupDef解说：
 * boss：顾名思义
 * waves：每波的具体信息（喽啰、boss数量、boss装备）
 * repeatWaveStartIndex：循环开始的waves索引
 * workerClass、workerInt、Worker属性：不用管
 * leaderDescription：boss介绍
 * quest：对应的QuestScriptDef（没错，召唤Boss会生成一个隐藏任务）
 * rewardDef：任务奖励
 * tmpEntries：用于召唤时的具体波次介绍
 */
//相较于原版的改动：
//删除repeatWaveStartIndex，改由玩家手动选择难度(WIP)
//RaidGroupWave新添bossOverride，可以覆盖默认的bossDef
public class RaidDef : Def
{
    private string iconPath;
    public Texture2D icon;
    public Type workerClass = typeof(BossGroupWorker);
    public BossDef boss;
    public List<RaidGroupWave> waves = new List<RaidGroupWave>();
    public ThingDef rewardDef;
    public QuestScriptDef quest;
    private string leaderDescription;

    private BossGroupWorker workerInt;
    private List<string> tmpEntries = new List<string>();

    public string LeaderDescription => (string)this.leaderDescription.Formatted(NamedArgumentUtility.Named(this.boss.kindDef, "LEADERKIND"));


    public BossGroupWorker Worker
    {
        get
        {
            if (this.workerInt == null && this.workerClass != (System.Type)null)
            {
                this.workerInt = (BossGroupWorker)Activator.CreateInstance(this.workerClass);
                this.workerInt.def = this;
            }
            return this.workerInt;
        }
    }

    public string GetWaveDescription(int waveIndex)
    {
        RaidGroupWave wave = waves[waveIndex];
        this.tmpEntries.Clear();
        string str = GenLabel.BestKindLabel(wave.bossOverride?.kindDef ?? this.boss.kindDef, Gender.None).CapitalizeFirst();
        if (!wave.bossApparel.NullOrEmpty<ThingDef>())
            str = (string)"BossWithApparel".Translate(str.Named("BOSS"), wave.bossApparel.Select<ThingDef, string>((Func<ThingDef, string>)(a => a.label)).ToCommaList(true).Named("APPAREL"));
        this.tmpEntries.Add(str + "(" + "BossHPMultiplier".Translate(wave.bossHPMultiplier.ToStringPercent()) + ")");
        foreach (PawnKindDefCount escort in wave.escorts)
            this.tmpEntries.Add(GenLabel.BestKindLabel(escort.kindDef, Gender.None).CapitalizeFirst() + " x" + (object)escort.count);
        return this.tmpEntries.ToLineList("  - ");
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string configError in base.ConfigErrors())
            yield return configError;
        if (this.boss == null && this.waves.Any(x => x.bossOverride == null))
            yield return "bosses required for all bossgroups";
        if (this.waves.NullOrEmpty())
            yield return "no waves defined.";
    }


    public void Init()
    {
        icon = iconPath != null ? ContentFinder<Texture2D>.Get(iconPath) : BaseContent.BadTex;
    }
}
