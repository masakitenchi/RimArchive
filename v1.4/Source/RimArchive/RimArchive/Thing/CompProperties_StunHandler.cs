using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace RimArchive;

public class CompProperties_StunHandler : CompProperties
{
    public float BaseStunThreshold = 0f;
    public float BreakStunDuration = 0f;
    public float currentDuration = 0f;
    /// <summary>
    /// Tries to nullify current stun and add it to the stun meter.
    /// </summary>
    /// <param name="stunDuration">the duration of current stun if not nullified</param>
    /// <returns>True if successfully nullified; False otherwise</returns>
    public bool TryAddStunDuration(Pawn pawn, float stunDuration, out float BreakStunDuration)
    {
        //Log.Message($"Threshold: {this.BaseStunThreshold}, stunDuration:{stunDuration}, current: {currentDuration}");
        BreakStunDuration = 0f;
        if (currentDuration + stunDuration > BaseStunThreshold)
        {
            MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "ThresholdBroken".Translate(currentDuration, BaseStunThreshold), Color.white);
            currentDuration = 0f;
            BreakStunDuration = this.BreakStunDuration;
            return false;
        }
        currentDuration += stunDuration;
        MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "CurrentTotal".Translate(currentDuration, BaseStunThreshold), Color.white);
        return true;
    }
}
