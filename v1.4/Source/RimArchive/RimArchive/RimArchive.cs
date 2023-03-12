using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions.Must;
using Verse;
using static RimArchive.Debug;

namespace RimArchive
{
    [StaticConstructorOnStartup]
    public class RimArchive : ModSettings
    {
        internal static readonly string packageId = "auxia.weaponpack.bluearchive";
#nullable enable
        internal static readonly List<PawnKindDef>? AllStudents;
        internal static readonly HashSet<string>? cachedSchools;
        public static readonly Dictionary<string, List<PawnKindDef>>? cachedAllStudentsBySchool;
#nullable disable

        static RimArchive()
        {
            AllStudents ??= DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).ToList();
            cachedSchools ??= DefDatabase<PawnKindDef>.AllDefs.Where(x => x.HasModExtension<SchoolNameModExtension>()).Select(x => x.GetModExtension<SchoolNameModExtension>()?.SchoolName).ToHashSet();
            cachedAllStudentsBySchool ??= AllStudents.GroupBy(x => x.GetModExtension<SchoolNameModExtension>().SchoolName).ToDictionary(group => group.Key, group => group.ToList());
            /*cachedAllStudentsBySchool = (from student in AllStudents
                                         group student by student.GetModExtension<SchoolNameModExtension>().SchoolName).ToDictionary(group => group.Key, group => group.ToList());*/
        }
    }
}
