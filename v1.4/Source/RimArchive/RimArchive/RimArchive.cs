using RimArchive.Window;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static RimArchive.Debug;

namespace RimArchive
{
    [StaticConstructorOnStartup]
    public class RimArchive
    {
        public class RASettings
        {
            

        }
        //Sensei's icon
        public static readonly Texture2D Sensei = ContentFinder<Texture2D>.Get("Icons/Sensei");
        //
        internal static readonly string packageId = "auxia.weaponpack.bluearchive";
#nullable enable
        //Each student belongs to a different PawnKindDef, but should share the same race
        internal static readonly List<PawnKindDef> AllStudents;
        //Cache school for Recruit Window
        internal static readonly List<SchoolDef> cachedSchools = new List<SchoolDef>();
        //This should help for searching students from a certain school
        public static readonly Dictionary<string, List<PawnKindDef>>? cachedAllStudentsBySchool;
#nullable disable

        static RimArchive()
        {
            //Should be of use sometime
            //But what if some other mod also add this extension? Meh
            packageId = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).First().modContentPack.PackageId;
            AllStudents ??= DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).ToList();
            foreach(var school in DefDatabase<SchoolDef>.AllDefsListForReading)
            {
                cachedSchools.Add(school);
                school.ResolveTex();
            }
            cachedAllStudentsBySchool ??= AllStudents.GroupBy(x => x.GetModExtension<SchoolNameModExtension>().SchoolName).ToDictionary(group => group.Key, group => group.ToList());

            RecruitWindow.Init();
        }
    }
}
