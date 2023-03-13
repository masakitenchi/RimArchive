using RimWorld;
using Verse;
using UnityEngine;
using System.Text.RegularExpressions;

namespace RimArchive
{
    internal class SchoolDef : Def
    {
        private Regex fileNameRegex = new Regex(@"(?<modName>\w*?)_(?<category>\w*?)_(?<Name>\w*)");
#pragma warning disable CS0649
        private string textureFolder;
#pragma warning restore CS0649
        public Texture2D tex;

        public void ResolveTex()
        {
            Match match = fileNameRegex.Match(this.defName);
            if (!match.Success)
            {
                Log.Error($"Error when parsing {this.defName}: invalid pattern");
                return;
            }
            //Example:
            //RA_School_Shanhaijing => <modName>.RA , <category>.School, <Name>.Shanhaijing
            //So resolvedTexPath will be SchoolIcon/Shanhaijing

            string resolvedTexPath = this.textureFolder + "/" + match.Groups["Name"].Value;
            this.tex = ContentFinder<Texture2D>.Get(resolvedTexPath, false);
            if (this.tex == null)
            {
                Log.Error($"Cannot find tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
            }
        }
    }
}
