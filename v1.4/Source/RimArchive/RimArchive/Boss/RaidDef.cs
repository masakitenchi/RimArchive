using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimArchive;

public class RaidDef : RimWorld.BossgroupDef
{
    private string iconPath;
    public Texture2D icon;
    new public Type workerClass = typeof(BossGroupWorker);

    private BossGroupWorker workerInt;
    private List<string> tmpEntries = new List<string>();
    new public BossgroupWorker Worker
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

    new public string GetWaveDescription(int waveIndex)
    {
        BossGroupWave wave = this.GetWave(this.GetWaveIndex(waveIndex));
        this.tmpEntries.Clear();
        string str = GenLabel.BestKindLabel(this.boss.kindDef, Gender.None).CapitalizeFirst();
        if (!wave.bossApparel.NullOrEmpty<ThingDef>())
            str = (string)"BossWithApparel".Translate(str.Named("BOSS"), wave.bossApparel.Select<ThingDef, string>((Func<ThingDef, string>)(a => a.label)).ToCommaList(true).Named("APPAREL"));
        this.tmpEntries.Add(str + " x" + (object)wave.bossCount);
        foreach (PawnKindDefCount escort in wave.escorts)
            this.tmpEntries.Add(GenLabel.BestKindLabel(escort.kindDef, Gender.None).CapitalizeFirst() + " x" + (object)escort.count);
        return this.tmpEntries.ToLineList("  - ");
    }

    public void Init()
    {
        icon = iconPath != null ? ContentFinder<Texture2D>.Get(iconPath) : null;
    }
}
