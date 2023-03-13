using System.Collections.Generic;
using Verse;

namespace RimArchive.Components
{
    //Used as a document for each student. Thanks @SamaraFleurety's code for reference
    internal class RimArchiveGameComponent : GameComponent
    {
        //Checks all students that's been recruited
        public static Dictionary<PawnKindDef, bool> HasEverExisted = new Dictionary<PawnKindDef, bool>();
        //
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<PawnKindDef, bool>(ref HasEverExisted, "StudentExistence");

        }

        public RimArchiveGameComponent(Game game) : base() { }
    }
}
