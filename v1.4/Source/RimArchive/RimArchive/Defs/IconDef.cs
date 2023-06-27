﻿using System.Text.RegularExpressions;
using UnityEngine;

namespace RimArchive;

#pragma warning disable CS1591
public class IconDef : Def
{
#pragma warning disable CS0649
    public string name;
    private string textureFolder;
#pragma warning restore CS0649
    public Texture2D tex;

    public void ResolveFields()
    {
        Match match = RimArchiveMain.fileNameRegex.Match(this.defName);
        if (!match.Success)
        {
            Log.Error($"Error when parsing {this.defName}: Regex cannot match");
            return;
        }
        //Example:
        //RA_School_Shanhaijing => <modName>.RA , <category>.School, <Name>.Shanhaijing
        //So resolvedTexPath will be SchoolIcon/Shanhaijing

        string resolvedTexPath = this.textureFolder + "/" + match.Groups["Name"].Value;
        this.name = match.Groups["Name"].Value;
        this.tex = ContentFinder<Texture2D>.Get(resolvedTexPath, false);
        if (this.tex == null)
        {
            Log.Error($"Cannot find tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
    }
}
