using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
#pragma warning disable CS1591

namespace RimArchive.GameComponents;

//Used as a document for each student.
public class StudentDocument : GameComponent
{
	private Faction _Shaleint;


	private Dictionary<StudentDef, Pawn> documents = new Dictionary<StudentDef, Pawn>();

	private HashSet<StudentDef> aliveStudents = new HashSet<StudentDef>();
	private HashSet<StudentDef> recruitedStudents = new HashSet<StudentDef>();

	private int hostileDaysAsTick;
	private bool inAngryHostile;
	/*public List<StudentDef> RecruitedStudents
    {
        get
        {
            return recruitedStudents ??= new List<StudentDef>();
        }
    }*/

	public HashSet<StudentDef> AliveStudents => aliveStudents;
	public HashSet<StudentDef> RecruitedStudents => recruitedStudents;


    public Pawn GetDocumentedStudent(StudentDef student)
    {
        return documents[student];
    }

	//public bool IsRecruitedOrAlive(StudentDef student) => IsAlive(student) || HasEverBeenRecruited(student);
	public bool IsAlive(StudentDef student) => aliveStudents.Contains(student);
	public bool HasEverBeenRecruited(StudentDef student) => recruitedStudents.Contains(student);

	//我为啥要用ref来着？
	public bool Notify_StudentKilled(Pawn p)
	{
		try
		{
			documents.Add(p.kindDef as StudentDef, p);
			aliveStudents.RemoveWhere(s => p.kindDef == s);
			//防止id重复
			new Traverse(Find.WorldPawns).Field<HashSet<Pawn>>("pawnsDead").Value.RemoveWhere(x => x.Name == p.Name);
			float hostiledays = Rand.Range(0.1f, 0.2f);
			Messages.Message("StudentHeavilyInjuredAngry".Translate(p.Name, hostiledays.ToString("F1")), MessageTypeDefOf.NegativeEvent);
			hostileDaysAsTick = (int)(hostiledays * GenDate.TicksPerDay);
			Faction shale = Find.FactionManager.AllFactions.Where(x => x.def == RAFactionDefOf.Shale).First();
			inAngryHostile = true;
			if (shale == null)
			{
				DebugMessage.DbgErr("Cannot find Shale as faction");
				return false;
			}
			shale.TryAffectGoodwillWith(Faction.OfPlayer, shale.GoodwillToMakeHostile(Faction.OfPlayer));
			return true;
		}
		catch (Exception ex)
		{
			DebugMessage.DbgErr($"Exception: {ex}");
			return false;
		}
	}

	public void Notify_StudentRecruited(StudentDef student)
	{
		recruitedStudents.Add(student);
		aliveStudents.Add(student);
		documents.Remove(student);
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
		Scribe_Values.Look<int>(ref hostileDaysAsTick, "hostileDaysAsTick");
	}



	//以后或许可以加一个补丁用来阻止这段时间内将声望冲到中立的行为
	//不然每Tick都检查一边这些感觉太吃性能了
	public override void GameComponentTick()
	{
		if (!inAngryHostile)
			return;
		else if (inAngryHostile)
		{

		}
		if (hostileDaysAsTick > 0)
			hostileDaysAsTick--;
		if (hostileDaysAsTick == 0)
		{
			Faction shale = Find.FactionManager.AllFactions.FirstOrDefault(x => x.def == RAFactionDefOf.Shale);
			shale?.TryAffectGoodwillWith(Faction.OfPlayer, -shale.GoodwillWith(Faction.OfPlayer));
			inAngryHostile = false;
		}
	}
	public StudentDocument(Game game) : base()
	{

	}
}
