﻿using RimWorld;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using Verse;

namespace RimArchive
{
    public class RA_StudentModExtension : DefModExtension
    {
        public string School;
        public Texture2D Icon;
        public Texture2D Portrait;

        public void ResolveTexFor(string defName)
        {
            Match match = new Regex(@"(?<Prefix>BA)_(?<FullName>\w*)").Match(defName);
            if (!match.Success)
            {
                Log.Error($"Error when parsing {defName}: Regex cannot match");
                return;
            }
            //Example:
            //BA_Shiroko_RidingSuit => <Prefix>.BA, <FullName>=>Shiroko_RidingSuit
            Icon = ContentFinder<Texture2D>.Get("Icons/" + match.Groups["FullName"].Value);
            Portrait = ContentFinder<Texture2D>.Get("Portraits/" + match.Groups["FullName"].Value);
            if (Icon == null)
            {
                Log.Error($"Cannot find icon tex named {match.Groups["Name"].Value} for {defName}. All Matches:\n{match.Value}");
            }
            if( Portrait == null)
            {
                Log.Error($"Cannot find portrait tex named {match.Groups["Name"].Value} for {defName}. All Matches:\n{match.Value}");
            }
        }
    }
}
