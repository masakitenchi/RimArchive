using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System;
using UnityEngine;

namespace RimArchive;

//不会画光环特效，摆了
public class CompInvadePillar : ThingComp
{
    private float _currentRadius;
    private int _interval;
    private CompMoteEmitterIncreasingSize cachedEmitter;
    //private Mote mote;
    public CompProperties_InvadePillar Props => (CompProperties_InvadePillar)props;

    private CompMoteEmitterIncreasingSize Emitter => cachedEmitter ??= this.parent.GetComp<CompMoteEmitterIncreasingSize>();

    public float Radius => _currentRadius;
    public override void CompTick()
    {
        base.CompTick();
        (from pawn in parent.Map.mapPawns.AllPawnsSpawned
         where pawn.Position.InHorDistOf(parent.Position, _currentRadius)   
         select pawn).Do(delegate(Pawn x)
         {
             foreach(HediffDef hediff in Props.applyHediffs)
             {
                 x.health.AddHediff(hediff);
             }
         });
        if(parent.IsHashIntervalTick(_interval))
        {
            _currentRadius += 2.5f;
            Emitter.Notify_RadiusChanged();
            /*mote.Destroy();
            mote = MoteMaker.MakeStaticMote(base.parent.Position.ToVector3(), base.parent.Map, RimWorld.ThingDefOf.Mote_PowerBeam, _currentRadius);
            mote.Maintain();*/
        }
    }

    //构造函数被调用的时候还没有props
    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        _currentRadius = Props.InitialRadius;
        _interval = Props.upgradeInterval * GenTicks.TicksPerRealSecond;
    }

    public override string CompInspectStringExtra()
    {
        string str = base.CompInspectStringExtra();
        str += "\n" + "Current Radius:" + _currentRadius.ToString("F2");
        return str;
    }
}


/*public class MoteProperties_RotatingCircle : MoteProperties
{
    // 在这里定义特效的属性，例如半径、旋转速度、颜色等等
    public override void MoteSpawnInternal(MoteSpawnInfo spawnInfo)
    {
        // 从 spawnInfo 中获取需要的信息，例如位置、大小、旋转角度等等
        Vector3 position = spawnInfo.spawnLocs.FirstOrDefault();
        float radius = ((MoteProperties_RotatingCircle)spawnInfo.moteProps).radius;
        float rotationSpeed = ((MoteProperties_RotatingCircle)spawnInfo.moteProps).rotationSpeed;
        Color color = ((MoteProperties_RotatingCircle)spawnInfo.moteProps).color;

        // 绘制圆形特效
        Mote mote = (Mote)Activator.CreateInstance(this.moteClass);
        mote.exactPosition = position;
        mote.exactRotation = rotationSpeed * Find.TickManager.TicksGame;
        mote.exactScale = new Vector3(radius, 1f, radius);
        mote.instanceColor = color;
        mote.SetVelocity((Quaternion.AngleAxis(90f, Vector3.up) * mote.exactRotation * Vector3.forward).RotatedBy((double)mote.exactRotation, Vector3.up) * 0.2f);
        GenSpawn.Spawn(mote, position.ToIntVec3(), spawnInfo.spawnableThingsParent);
    }

}*/
