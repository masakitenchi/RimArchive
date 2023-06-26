using RimWorld;
using Verse;

namespace RimArchive;

/// <summary>
/// 所有治疗类道具的基类
/// </summary>
public class CompProperties_Healing : CompProperties
{
    /// <summary>
    /// 是否限单次使用
    /// </summary>
    public bool SingleUse = true;
    /// <summary>
    /// 每次治疗x个伤口
    /// </summary>
    public int WoundsPerHeal = 1;
    /// <summary>
    /// 使用次数
    /// </summary>
    public int UseCount;

    public CompProperties_Healing() => this.compClass = typeof(CompHealing);
}

public class CompHealing : ThingComp
{
    public CompProperties_Healing Props => this.props as CompProperties_Healing;
    private int _healsleft;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        this._healsleft = Props.SingleUse ? 1 : Props.UseCount;
    }


    public void DoEffect(Pawn pawn)
    {
        if (HealingUtility.TryHealWounds(pawn, out var Message, Props.WoundsPerHeal))
        {
            Messages.Message(Message, MessageTypeDefOf.PositiveEvent);
            --_healsleft;
        }
        DebugMessage.DbgMsg($"{parent.def.defName} has {_healsleft} heals left");
        if (_healsleft <= 0)
            this.parent.Destroy(DestroyMode.Vanish);
    }
}
