global using HarmonyLib;
global using RimWorld;
global using System.Collections.Generic;
global using System.Linq;
global using Verse;
using RimArchive.Abilities;
using RimArchive.GameComponents;
using RimArchive.Window;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RimArchive
{
    /// <summary>
    /// Loads all assets before GC.
    /// </summary>
    [StaticConstructorOnStartup]
    public class RimArchive
    {
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
        /// <summary>
        /// The button for recruitment
        /// </summary>
        public static readonly Texture2D SSR = ContentFinder<Texture2D>.Get("Things/Gacha/SSR");

        /// <summary>
        /// Used to access the Document system in current game
        /// </summary>
        public static StudentDocument StudentDocument => Current.Game.GetComponent<StudentDocument>();
        public static RaidManager RaidManager => Current.Game.GetComponent<RaidManager>();
        //
        internal static readonly string packageId;
        //Each student belongs to a different PawnKindDef, but should share the same race
        internal static readonly List<StudentDef> AllStudents = new List<StudentDef>();
        //Cache school for Recruit Window
        internal static readonly List<IconDef> cachedSchools = new List<IconDef>();
        /// <summary>
        /// This should help for searching students from a certain school
        /// </summary>
        public static readonly Dictionary<IconDef, List<StudentDef>> cachedAllStudentsBySchool = new Dictionary<IconDef, List<StudentDef>>();

        public static readonly HashSet<RaidDef> cachedAllBosses = new HashSet<RaidDef>();

        public static GameObject gameObject = new GameObject("Auxia.RimArchive");

        public static SplitInFrames CoroutineSingleton => gameObject.GetComponent<SplitInFrames>();

        public static RimArchiveMod ModSingleton => RimArchiveMod.instance;

        static RimArchive()
        {
            //Should be of use sometime
            //But what if some other mod also add this extension? Meh
            packageId ??= DefDatabase<StudentDef>.AllDefs.First().modContentPack.PackageId;
            cachedAllBosses = DefDatabase<RaidDef>.AllDefs.ToHashSet();
            DefDatabase<RaidDef>.AllDefs.Do(delegate (RaidDef def) { def.Init(); });
            foreach (StudentDef student in DefDatabase<StudentDef>.AllDefs)
            {
                AllStudents.Add(student);
                student.Init();
            }
            foreach (var school in DefDatabase<IconDef>.AllDefsListForReading)
            {
                cachedSchools.Add(school);
                school.ResolveFields();
                cachedAllStudentsBySchool.Add(school, (from x in AllStudents where x.School == school.name select x).ToList());
            }
            RecruitWindow.Init();
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<global::RimArchive.Abilities.SplitInFrames>();
        }
    }

    public class RimArchiveMod : Mod
    {
        public static RimArchiveMod instance;
        public static Setting settings;
        public RimArchiveMod(ModContentPack content) : base(content)
        {
            if (instance == null)
                instance = this;
            settings = instance.GetSettings<Setting>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            ls.CheckboxLabeled("NeedSilverNearBeacon".Translate(), ref settings.SilverNeedToBeLaunchable, "NeedSilverNearBeaconDesc".Translate());
            ls.End();
        }

        public override string SettingsCategory() => "RimArchive".Translate();

    }

    public class Setting : ModSettings
    {
        public bool SilverNeedToBeLaunchable = false;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SilverNeedToBeLaunchable, "NeedSilverNearBeacon");
        }
    }
}
