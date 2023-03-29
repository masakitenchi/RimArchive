using RimWorld;
using UnityEngine;
using Verse;

namespace RimArchive;

public class CompStunHandler : ThingComp
{
    private float currentDuration;
    private float BaseStunThreshold;
    private float BreakStunDuration;
    public CompProperties_StunHandler Props => (CompProperties_StunHandler)props;

    //60Tick/s, 250Tick/TickRare, 2000Tick/TickLong
    public override void CompTick()
    {
        if (parent.IsHashIntervalTick(600))
            Props.currentDuration -= 30f;
    }

    //构造函数被调用的时候还没有props
    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        this.currentDuration = 0;
        this.BaseStunThreshold = Props.BaseStunThreshold;
        this.BreakStunDuration = Props.BreakStunDuration;
    }

    /// <summary>
    /// Tries to nullify current stun and add it to the stun meter.
    /// </summary>
    /// <param name="stunDuration">the duration of current stun if not nullified</param>
    /// <returns>True if successfully nullified; False otherwise</returns>
    public bool TryAddStunDuration(float stunDuration, out float BreakStunDuration)
    {
        //Log.Message($"Threshold: {this.BaseStunThreshold}, stunDuration:{stunDuration}, current: {currentDuration}");
        BreakStunDuration = 0f;
        if (currentDuration + stunDuration > BaseStunThreshold)
        {
            MoteMaker.ThrowText(new Vector3((float)parent.Position.x + 1f, parent.Position.y, (float)parent.Position.z + 1f), parent.Map, "ThresholdBroken".Translate(currentDuration, BaseStunThreshold), Color.white);
            currentDuration = 0f;
            BreakStunDuration = this.BreakStunDuration;
            return false;
        }
        currentDuration += stunDuration;
        MoteMaker.ThrowText(new Vector3((float)parent.Position.x + 1f, parent.Position.y, (float)parent.Position.z + 1f), parent.Map, "CurrentTotal".Translate(currentDuration, BaseStunThreshold), Color.white);
        return true;
    }
}
