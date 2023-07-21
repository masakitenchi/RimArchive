using RimArchive.WorldComponents;
using UnityEngine;

namespace RimArchive;

public class CompProperties_CommConsole : CompProperties
{
    /// <summary>
    /// 目前无实际作用
    /// </summary>
    public bool ShaleOnly = true;
    public CompProperties_CommConsole() => this.compClass = typeof(CompCommConsole);
}

public class CompCommConsole : ThingComp
{
    public CompProperties_CommConsole Props => this.props as CompProperties_CommConsole;

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        yield return new Gizmo_CommConsole();
    }
}

[StaticConstructorOnStartup]
public class Gizmo_CommConsole : Command_Action
{
    public Gizmo_CommConsole()
    {
        this.icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/CommsConsole_south");
        this.defaultLabel = "ContactShale".Translate();
        this.defaultDesc = "ContactShaleDesc".Translate();
        this.action = delegate
        {
            Find.TickManager.Pause();
            Find.WindowStack.Add(new global::RimArchive.Window.RecruitWindow());
        };
    }

}
