using UnityEngine;
using Verse.Sound;

namespace RimArchive;

public class CompMoteEmitterIncreasingSize : CompMoteEmitter
{
    public CompProperties_MoteEmitterIncreasingSize Props => (CompProperties_MoteEmitterIncreasingSize)props;
    private CompInvadePillar cachedPillar;

    private CompInvadePillar PillarComp => cachedPillar ??= this.parent.GetComp<CompInvadePillar>();

    private float Scale => PillarComp.Radius * 2 / Props.mote.graphicData.drawSize.x;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
    }

    //Feels that only need DeSpawn()
    public void Notify_RadiusChanged()
    {
        this.mote.DeSpawn();
        this.Emit();
        this.mote?.Maintain();
    }

    public override void CompTick()
    {
        if (this.mote == null)
            this.Emit();
        if (this.mote == null)
            return;
        this.mote?.Maintain();
    }
    public override void Emit()
    {
        if (!this.parent.Spawned)
        {
            Log.Error("Thing tried spawning mote without being spawned!");
        }
        else
        {
            Vector3 offset = this.Props.offset + this.Props.RotationOffset(this.parent.Rotation);
            if (this.Props.offsetMin != Vector3.zero || this.Props.offsetMax != Vector3.zero)
                offset = this.Props.EmissionOffset;
            ThingDef moteDef = this.Props.RotationMote(this.parent.Rotation) ?? this.Props.mote;
            Vector3 vector3 = this.parent.DrawPos + offset;
            if (vector3.InBounds(this.parent.Map))
            {
                this.mote = MoteMaker.MakeStaticMote(vector3, this.parent.Map, moteDef, Scale, true);
            }
            if (this.mote != null && this.Props.useParentRotation)
                this.mote.exactRotation = this.parent.Rotation.AsAngle;
            if (this.Props.soundOnEmission.NullOrUndefined())
                return;
            this.Props.soundOnEmission.PlayOneShot(SoundInfo.InMap((TargetInfo)(Thing)this.parent));
        }
    }
}
