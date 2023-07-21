using RimArchive.ModExtension;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

namespace RimArchive.Abilities;

#region Obsolete(More like deprecated)

[Obsolete]
public class HediffCompProperties_DamageOverTime : HediffCompProperties
{
    public DamageDef damageDef;
    public float initialDamage;
    public float damage;
    public float damageInterval;
    public float duration;

    public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
    {
        if (!typeof(Hediff_DamageOverTime).IsAssignableFrom(parentDef.hediffClass))
        {
            parentDef.hediffClass = typeof(HediffWithComps);
            yield return $"{nameof(HediffCompProperties_DamageOverTime)} cannot be used without hediffClass being Hediff_DamageOverTime. Assigning {parentDef.defName}'s hediffClass to HediffWithComps";
        }
    }
}

[Obsolete("When move HediffCompProperties_DamageOverTime to DefModExtension the whole hediff class will inherit from Hediff directly")]
public abstract class HediffWithDamageDef : HediffWithComps
{
    public DamageDef sourceDamageDef;
}

[Obsolete]
public class CompProperties_AbilityLaunchProjectile : CompProperties_AbilityEffect
{
    public ThingDef projectileDef;
    public List<float> DamageList = new List<float>() { 1.0f };

    public CompProperties_AbilityLaunchProjectile() => this.compClass = typeof(CompAbilityEffect_LaunchProjectile);

}

[Obsolete]
public class CompAbilityEffect_LaunchProjectile : CompAbilityEffect
{
    new public CompProperties_AbilityLaunchProjectile Props => this.props as CompProperties_AbilityLaunchProjectile;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        this.LaunchProjectile(target);
    }

    protected void LaunchProjectile(LocalTargetInfo target)
    {
        Pawn pawn = parent.pawn;
    }
}

[Obsolete]
public class CompAbilityEffect_AddDoT : CompAbilityEffect_GiveHediff
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
    }
}

[Obsolete]
public class Projectile_AddsHediff : Projectile
{
    public override bool AnimalsFleeImpact => true;

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        //Log.Message($"Bullet impacted at {hitThing}");
        float BaseDmgAmnt = this.DamageAmount;
        Map map = this.Map;
        IntVec3 position = this.Position;
        base.Impact(hitThing, blockedByShield);
        BattleLogEntry_RangedImpact entryRangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
        Find.BattleLog.Add((LogEntry)entryRangedImpact);
        if (hitThing != null)
        {
            bool instigatorGuilty = !(this.launcher is Pawn launcher) || !launcher.Drafted;
            DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, BaseDmgAmnt, this.ArmorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, intendedTarget: this.intendedTarget.Thing, instigatorGuilty: instigatorGuilty);
            //RimArchive.CoroutineSingleton.StartCoroutine(RimArchive.CoroutineSingleton.DamageTarget(dinfo, hitThing, entryRangedImpact, DamageList));

        }
    }
}
#endregion


public class DamageWorker_AddHediffWithInstigator : DamageWorker
{
    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        if(victim is Pawn p)
        {
            HediffWithInstigator hediff = HediffMaker.MakeHediff(dinfo.Def.hediff, p) as HediffWithInstigator;
            hediff.Severity = dinfo.Amount;
            hediff.instigator = dinfo.Instigator;
            p.health.AddHediff(hediff, dinfo: new DamageInfo?(dinfo));
        }
        return new DamageResult();
    }
}

public abstract class HediffWithInstigator : Hediff
{
    public Thing instigator;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref instigator, "instigator");
    }
}
/// <summary>
/// DoT机制参考WoW。有补偿跳、在剩余时间小于一定百分比时再次施加可以将剩余伤害加到伤害池里（WIP）
/// </summary>
public class Hediff_DamageOverTime : HediffWithInstigator
{
    const float durationUpperLimitPercentage = 1.2f;
    DamageInfo TickDamage;
    float _damagePool;
    float _damage;
    int _damageIntvalTick;
    int _durationTick;
    int _tickCounter;

    public override bool ShouldRemove => _durationTick <= 0;


    public override void PostMake()
    {
        DamageOverTime ModExt = this.def.GetModExtension<DamageOverTime>();
        _damageIntvalTick = ((int)(ModExt.DamageInterval * GenTicks.TicksPerRealSecond));
        _tickCounter = _damageIntvalTick;
        _durationTick = (int)(ModExt.Duration * GenTicks.TicksPerRealSecond);
        _damage = ModExt.TickDamage;
        _damagePool = _durationTick / _damageIntvalTick * _damage;
        TickDamage = new DamageInfo(ModExt.DamageDef, ModExt.TickDamage, instigator: instigator);
        TickDamage.SetAllowDamagePropagation(false);
        TickDamage.SetIgnoreArmor(true);
        TickDamage.SetInstantPermanentInjury(false);
        if (ModExt.InitialDamage > 0)
            pawn.TakeDamage(new DamageInfo(ModExt.DamageDef, ModExt.InitialDamage, 99999f, instigator: instigator));
    }

    //原版仅检查def和Part是否一致，这里需要作出区分
    public override bool TryMergeWith(Hediff other)
    {
        if (other is Hediff_DamageOverTime otherDot && otherDot.def == this.def)
        {
            this._durationTick = (int)Math.Max(otherDot._durationTick * durationUpperLimitPercentage, otherDot._durationTick + this._durationTick);
            this._damagePool = Math.Max(otherDot._damagePool * durationUpperLimitPercentage, this._damagePool + otherDot._damagePool);
            this._tickCounter = this._damageIntvalTick;
            return true;
        }
        return false;
    }
    public override void Tick()
    {
        base.Tick();
        --_durationTick;
        --_tickCounter;
        if (_tickCounter <= 0 && _damagePool >= _damage)
        {
            _tickCounter = _damageIntvalTick;
            pawn.TakeDamage(new DamageInfo(TickDamage));
            _damagePool -= _damage;
        }
    }

    /// <summary>
    /// 移除Hediff前检查伤害池
    /// </summary>
    public override void PreRemoved()
    {
        if (_damagePool > 0)
        {
            Log.WarningOnce($"HediffDef {def.defName} hasn't dealt enough damage before it expires. This might have problem in future updates. Damage left in damage pool: {this._damagePool}", this.def.GetHashCode() * 114514);
            DamageInfo dmgLeft = new DamageInfo(TickDamage);
            dmgLeft.SetAmount(_damagePool);
            pawn.TakeDamage(dmgLeft);
            _damagePool = 0;
        }
    }

    public override string DebugString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("- Ticks left: " + this._durationTick);
        sb.AppendLine("- Damage left: " + this._damagePool);
        sb.AppendLine("- Ticks until next damage: " + this._tickCounter);
        sb.AppendLine(base.DebugString());
        return sb.ToString();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _damagePool, "damagePool");
        Scribe_Values.Look(ref _tickCounter, "tickCounter");
        Scribe_Values.Look(ref _damage, "tickDamage");
        Scribe_Values.Look(ref _damageIntvalTick, "damageInterval");
        Scribe_Values.Look(ref _durationTick, "ticksLeft");
    }
}

public class SplitInFrames : MonoBehaviour
{
    public static SplitInFrames Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator DamageTarget(DamageInfo dinfo, Thing hitThing, BattleLogEntry_RangedImpact entryRangedImpact, List<float> percentages = null, int DamageIntervalFrames = 20)
    {
        if (percentages == null) percentages = new List<float>() { 1.0f };
        float totalDamage = dinfo.Amount;
        int frameCount = 0;
        for (int i = 0; i < percentages.Count; i++)
        {
            while (frameCount < DamageIntervalFrames)
            {
                ++frameCount;
                yield return null;
            }

            frameCount = 0;
            dinfo.SetAmount(totalDamage * percentages[i]);
            hitThing.TakeDamage(dinfo).AssociateWithLog(entryRangedImpact);
        }
    }
}