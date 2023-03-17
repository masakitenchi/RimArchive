using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
#pragma warning disable CS1591

namespace RimArchive.Components
{
    //Used as a document for each student.
    public class RimArchiveGameComponent : GameComponent
    {
        private Dictionary<StudentDef, Pawn> documents = new Dictionary<StudentDef, Pawn>();

        private HashSet<StudentDef> aliveStudents = new HashSet<StudentDef>();
        private HashSet<StudentDef> recruitedStudents = new HashSet<StudentDef>();

        /*public List<StudentDef> RecruitedStudents
        {
            get
            {
                return recruitedStudents ??= new List<StudentDef>();
            }
        }*/

        public HashSet<StudentDef> AliveStudents => aliveStudents;
        public HashSet<StudentDef> RecruitedStudents => recruitedStudents;
        

        public bool IsRecruitedOrAlive(StudentDef student) => IsAlive(student) || IsRecruited(student);
        public bool IsAlive(StudentDef student) => aliveStudents.Contains(student);
        public bool IsRecruited(StudentDef student) => recruitedStudents.Contains(student);

        //我为啥要用ref来着？
        public void Notify_StudentKilled(Pawn p)
        {
            documents.Add(p.kindDef as StudentDef, p);
            aliveStudents.RemoveWhere(s => p.kindDef == s);
            //防止id重复
            new Traverse(Find.WorldPawns).Field<HashSet<Pawn>>("pawnsDead").Value.RemoveWhere(x => x.Name == p.Name);
            Messages.Message("StudentDeadAngry".Translate(), MessageTypeDefOf.NegativeEvent);
        }
        public void Notify_StudentRecruited(ref StudentDef student)
        {
            recruitedStudents.Add(student);
            aliveStudents.Add(student);
        }

        //每次存档都根据isAlive更新 documents并将documents存档
        //去世的会通过Pawn.Kill触发更新函数所以不需要在这里更新
        //不对，没必要保存活着的，死了再触发防止被WorldPawn清理给删掉
        //估计还是得只保存必要的那些?
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<StudentDef>(ref aliveStudents, true, "AliveStudents", LookMode.Def);
            Scribe_Collections.Look<StudentDef>(ref recruitedStudents, true, "RecruitedStudents", LookMode.Def);
            Scribe_Collections.Look<StudentDef, Pawn>(ref documents, "document", LookMode.Def, LookMode.Deep);
        }

        public RimArchiveGameComponent(Game game) : base() { }
    }
}
