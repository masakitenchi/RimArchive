using RimWorld.Planet;

namespace RimArchive.WorldComponents;

internal class RimArchiveWorldComponent : WorldComponent
{
    //不知为何放在游戏初始化时初始化会NullReference
    public static readonly string Shale = RAFactionDefOf.Shale.ToString();
    public RimArchiveWorldComponent(World world)
        : base(world)
    {
    }
}
