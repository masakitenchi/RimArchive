namespace RimArchive;

/// <summary>
/// 所有医疗箱、饭桶、毒饭桶的基类
/// </summary>
public abstract class TrapLikeBuilding : ThingWithComps
{
    /// <summary>
    /// 每踩上去一次只进行一次判定
    /// </summary>
    protected HashSet<Pawn> touchingPawns = new HashSet<Pawn>();

    /// <summary>
    /// 很中庸的查询接触的生物的函数，不是虚函数故不应重写
    /// </summary>
    protected void UpdateTouchingPawns()
    {
        if (this.Spawned)
        {
            List<Thing> pawnList = this.Position.GetThingList(this.Map);
            pawnList.RemoveAll(x => x is not Pawn);
            for (int i = 0; i < pawnList.Count; i++)
            {
                if (pawnList[i] is Pawn p && this.touchingPawns.Add(p))
                {
                    //DebugMessage.DbgMsg($"Checking Springs for {p.Name}");
                    this.CheckSpring(p);
                }
            }
            /*for (int j = 0; j < this.touchingPawns.Count; j++)
            {
                Pawn pawn2 = this.touchingPawns[j];
                if (pawn2 == null || !pawn2.Spawned || pawn2.Position != this.Position)
                {
                    this.touchingPawns.Remove(pawn2);
                }
            }*/
            touchingPawns.RemoveWhere(x => x is null || !x.Spawned || x.Position != this.Position);
        }
    }

    /// <summary>
    /// UpdateTouchingPawns中会调用CheckSpring<br/>
    /// 各自的CompProperties不同故留给各个类实现
    /// </summary>
    protected abstract void CheckSpring(Pawn p);

    /// <summary>
    /// 同理，除了查询接触的生物和基类的Tick以外，陷阱不应该有其他的功能。
    /// </summary>
    public sealed override void Tick()
    {
        UpdateTouchingPawns();
        base.Tick();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
        //DebugMessage.DbgMsg($"{this} despawned");
    }
}