using RimArchive.Defs;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimArchive.Components
{
    //Used as a document for each student.
    internal class RimArchiveGameComponent : GameComponent
    {
        //Checks all students that's been recruited
        private Dictionary<StudentDef, bool> isAlive = new Dictionary<StudentDef, bool>();
        private Dictionary<StudentDef, bool> isRecruited = new Dictionary<StudentDef, bool>();

        private List<StudentDef> recruitedStudents;
        private List<StudentDef> studentsTmp;
        private List<bool> aliveTmp;
        private List<bool> recruitTmp;

        public List<StudentDef> RecruitedStudents
        {
            get
            {
                return recruitedStudents;
            }
        }


        public void Notify_StudentRecruited(StudentDef student)
        {
            recruitedStudents.Add(student);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<StudentDef, bool>(ref isAlive, "AliveStudents", LookMode.Def, LookMode.Value, ref studentsTmp, ref aliveTmp);
            Scribe_Collections.Look<StudentDef, bool>(ref isRecruited, "RecruitedStudents", LookMode.Def, LookMode.Value, ref studentsTmp, ref recruitTmp);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                isAlive ??= new Dictionary<StudentDef, bool>();
                isRecruited ??= new Dictionary<StudentDef, bool>();
            }
        }

        public RimArchiveGameComponent(Game game) : base() { }
    }
}
