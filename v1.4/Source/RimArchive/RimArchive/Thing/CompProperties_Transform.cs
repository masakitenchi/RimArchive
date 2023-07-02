

using System;
using UnityEngine;

namespace RimArchive;

public class CompProperties_Transform : CompProperties
{
    public ThingDef ToApparel;
    public ThingDef ToBuilding;

    public CompProperties_Transform() => this.compClass = typeof(CompTransform);

    //public CompProperties_Transform(Type compClass) => this.compClass = compClass;
}


public class CompTransform : ThingComp
{
    public CompProperties_Transform Props => this.props as CompProperties_Transform;

    public ThingDef stuff;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        if (this.parent.def.MadeFromStuff)
            this.stuff = this.parent.Stuff;
    }
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if(this.parent is Building or MinifiedThing&& this.parent.Spawned)
            yield return new Gizmo_Transform(this.parent);       
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        if (this.parent is Apparel pad && pad.Wearer != null)
            yield return new Gizmo_Transform(this.parent);
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Defs.Look(ref stuff, "stuff");
    }
}


public class Gizmo_Transform : Command
{
    public Thing parent; 
    public Gizmo_Transform(Thing thing)
    {
        this.parent = thing;
        this.defaultDesc = "RA_TransformDesc".Translate();
        this.defaultLabel = "RA_TransformLabel".Translate();
        this.icon = ContentFinder<Texture2D>.Get("Building/ShittimChest/Pad_south");
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        CompTransform thingComp = parent.TryGetComp<CompTransform>();
        if (this.parent is Building building)
        {
            Apparel belt = ThingMaker.MakeThing(thingComp.Props.ToApparel, thingComp.stuff) as Apparel;
            if (belt != null)
            {
                GenPlace.TryPlaceThing(belt, parent.Position, parent.Map, ThingPlaceMode.Near);
                building.DeSpawn();
            }
        }
        else if (this.parent is Apparel apparel)
        {
            if(apparel.Wearer != null)
            {
                if (!apparel.Wearer.apparel.TryDrop(apparel)) throw new Exception("Cannot Drop Apparel");
            }
            Building Pad = ThingMaker.MakeThing(thingComp.Props.ToBuilding, thingComp.stuff) as Building;
            if (Pad != null)
            {
                Pad.SetFaction(Faction.OfPlayer);
                GenPlace.TryPlaceThing(Pad, parent.Position, parent.Map, ThingPlaceMode.Near);
                apparel.DeSpawn();
            }
        }
    }
}