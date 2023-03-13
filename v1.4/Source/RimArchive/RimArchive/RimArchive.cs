using RimArchive.Defs;
using RimArchive.Window;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using static RimArchive.Debug;
using static RimArchive.Defs.StudentDef;

namespace RimArchive
{
    [StaticConstructorOnStartup]
    public class RimArchive
    {
        public class RASettings
        {
            

        }
        /// <summary>
        /// Regex for StudentDef
        /// </summary>
        public static readonly Regex studentNameRegex = new Regex(@"(?<Prefix>BA)_(?<FullName>\w*)");
        /// <summary>
        /// The Regex for all Defs. Group Name should explain everything
        /// </summary>
        public static readonly Regex fileNameRegex = new Regex(@"(?<modName>\w*?)_(?<category>\w*?)_(?<Name>\w*)");
        /// <summary>
        /// Sensei's icon
        /// </summary>
        public static readonly Texture2D Sensei = ContentFinder<Texture2D>.Get("Icons/Sensei");
        //
        internal static readonly string packageId = "auxia.weaponpack.bluearchive";
#nullable enable
        //Each student belongs to a different PawnKindDef, but should share the same race
        internal static readonly List<StudentDef> AllStudents = new List<StudentDef>();
        //Cache school for Recruit Window
        internal static readonly List<IconDef> cachedSchools = new List<IconDef>();
        /// <summary>
        /// This should help for searching students from a certain school
        /// </summary>
        public static readonly Dictionary<string, List<StudentDef>>? cachedAllStudentsBySchool = new Dictionary<string, List<StudentDef>>();
#nullable disable

        static RimArchive()
        {
            //Should be of use sometime
            //But what if some other mod also add this extension? Meh
            packageId = DefDatabase<StudentDef>.AllDefs.Where(x => x.HasModExtension<RA_StudentModExtension>()).First().modContentPack.PackageId;
            foreach (StudentDef student in DefDatabase<StudentDef>.AllDefs.Where(x=>x.HasModExtension<RA_StudentModExtension>()))
            {
                AllStudents.Add(student);
                //One day we also need to use a new def
                student.Init();
            }
            foreach(var school in DefDatabase<IconDef>.AllDefsListForReading)
            {
                cachedSchools.Add(school);
                school.ResoleFields();
                cachedAllStudentsBySchool.Add(school.name, (from x in AllStudents where x.GetModExtension<RA_StudentModExtension>().School == school.name select x).ToList());
            }
            RecruitWindow.Init();

        }
    }
}
