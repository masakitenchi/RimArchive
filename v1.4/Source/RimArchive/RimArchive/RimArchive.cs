using RimArchive.Window;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        //The Regex for all Defs. Group Name should explain everything
        internal static readonly Regex fileNameRegex = new Regex(@"(?<modName>\w*?)_(?<category>\w*?)_(?<Name>\w*)");
        //Sensei's icon
        public static readonly Texture2D Sensei = ContentFinder<Texture2D>.Get("Icons/Sensei");
        //
        internal static readonly string packageId = "auxia.weaponpack.bluearchive";
#nullable enable
        //Each student belongs to a different PawnKindDef, but should share the same race
        internal static readonly List<PawnKindDef> AllStudents;
        //Cache school for Recruit Window
        internal static readonly List<IconDef> cachedSchools = new List<IconDef>();
        //This should help for searching students from a certain school
        public static readonly Dictionary<string, List<PawnKindDef>>? cachedAllStudentsBySchool = new Dictionary<string, List<PawnKindDef>>();
#nullable disable

        static RimArchive()
        {
            //Should be of use sometime
            //But what if some other mod also add this extension? Meh
            packageId = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).First().modContentPack.PackageId;
            AllStudents ??= DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).ToList();
            foreach(var school in DefDatabase<IconDef>.AllDefsListForReading)
            {
                cachedSchools.Add(school);
                school.ResoleFields();
                cachedAllStudentsBySchool.Add(school.name, (from x in AllStudents where x.GetModExtension<SchoolNameModExtension>().School == school.name select x).ToList());
            }
            RecruitWindow.Init();

        }
    }
}
