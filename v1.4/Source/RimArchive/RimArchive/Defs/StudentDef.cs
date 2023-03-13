using RimWorld;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace RimArchive.Defs
{
    /// <summary>
    /// Extends from PawnKindDef. This should give it more editable field.
    /// </summary>
    public class StudentDef : PawnKindDef
    {
        /// <summary>
        /// The School where this student is belong to;
        /// </summary>
        public string School;
        /// <summary>
        /// Icon texture.
        /// </summary>
        public Texture2D Icon;
        /// <summary>
        /// Portrait texture.
        /// </summary>
        public Texture2D Portrait;

        internal void Init()
        {
            Match match = RimArchive.studentNameRegex.Match(this.defName);
            if (!match.Success)
            {
                Log.Error($"Error when parsing {this.defName}: Regex cannot match");
                return;
            }
            //Example:
            //BA_Shiroko_RidingSuit => <Prefix>.BA, <FullName>=>Shiroko_RidingSuit
            Icon = ContentFinder<Texture2D>.Get("Icons/" + match.Groups["FullName"].Value);
            Portrait = ContentFinder<Texture2D>.Get("Portraits/" + match.Groups["FullName"].Value);
            if (Icon == null)
            {
                Log.Error($"Cannot find icon tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
            }
            if (Portrait == null)
            {
                Log.Error($"Cannot find portrait tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
            }
        }
    }
}
