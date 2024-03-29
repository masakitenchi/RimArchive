﻿namespace RimArchive;

public class CompProperties_DisappearAfterTicks : CompProperties
{
    public int TicksToDisappear = 0;
    public CompProperties_DisappearAfterTicks() => compClass = typeof(CompDisappearAfterTicks);
}

public class CompDisappearAfterTicks : ThingComp
{
    private int _ticksLeft;
    CompProperties_DisappearAfterTicks Props => (CompProperties_DisappearAfterTicks)props;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        this._ticksLeft = Props.TicksToDisappear;
    }

    public override void CompTick()
    {
        _ticksLeft--;
        if (_ticksLeft <= 0)
        {
            this.parent.Destroy();
        }
    }
}