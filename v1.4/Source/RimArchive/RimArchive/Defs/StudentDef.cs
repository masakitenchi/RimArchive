using RimWorld;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using System.Xml;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using System.Linq;

namespace RimArchive;

/// <summary>
/// Extends from PawnKindDef. This should give it more editable field.
/// </summary>
public class StudentDef : PawnKindDef
{
    public class DirectRelationWith
    {
        public List<StudentDef> others = new List<StudentDef>();
        public PawnRelationDef relation;

        //另一个教程。List类型的通过在循环内调用RegisterListWantsCrossRef(this.listName, node.InnerText)读取。
        //list需要是整个list，但每次循环的InnerText都会变化
        public void LoadDataFromXmlCustom(XmlNode node)
        {
            //DebugMessage.DbgMsg($"Node:{node.Name}");
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "relation", node.Name);
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                //DebugMessage.DbgMsg($"ChildNode:{node.ChildNodes[i].InnerText}");
                DirectXmlCrossRefLoader.RegisterListWantsCrossRef(this.others, node.ChildNodes[i].InnerText);
                //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, $"others[{i}]", node.ChildNodes[i].InnerText);

            }
        }
    }
    /// <summary>
    /// The name class used in vanilla. Contains 3 fields: first, last, nick.
    /// </summary>
    public NameTriple name;
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
    /// <summary>
    /// Memorial Hall Picture
    /// </summary>
    public Texture2D Memorial;
    /// <summary>
    /// Skill level and passion
    /// </summary>
    new public List<PassionSkill> skills;
#nullable enable
    /// <summary>
    /// Having this weapon in inventory will receive a +5 mood buff
    /// </summary>
    public ThingDef? ownWeapon;
    /// <summary>
    /// Directly set relation with other students
    /// </summary>
    public List<DirectRelationWith>? relations = new List<DirectRelationWith>();
#nullable disable
#pragma warning disable CS1591
    public HeadTypeDef forcedHeadType;
    new public HairDef forcedHair;
#pragma warning restore CS1591

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        #region Graphic
        Match match = RimArchiveMain.studentNameRegex.Match(this.defName);
        if (!match.Success)
        {
            Log.Error($"Error when parsing {this.defName}: Regex cannot match");
            return;
        }
        //Example:
        //BA_Shiroko_RidingSuit => <Prefix> = BA, <FullName> = Shiroko_RidingSuit
        Icon = ContentFinder<Texture2D>.Get("Icons/" + match.Groups["FullName"].Value);
        Portrait = ContentFinder<Texture2D>.Get("Portraits/Resized/" + match.Groups["FullName"].Value);
        Memorial = ContentFinder<Texture2D>.Get("Memorial/Yuuka");
        if (Icon == null)
        {
            Log.Error($"Cannot find icon tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        if (Portrait == null)
        {
            Log.Error($"Cannot find portrait tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        if (Memorial == null)
        {
            Log.Error($"Cannot find memorial hall tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        #endregion

        #region resolve relations
        foreach (var relationWith in relations)
        {
            foreach (StudentDef student in relationWith.others)
            {
                if (!student.relations.Any(x => x.relation == relationWith.relation))
                {
                    student.relations.Add(new DirectRelationWith() { relation = relationWith.relation, others = new List<StudentDef>() { this } });
                }
                else if (!student.relations.Any(x => x.others.Contains(this)))
                {
                    student.relations.First(x => x.relation.Equals(relationWith.relation)).others.Add(this);
                }
            }
        }
        #endregion
    }

    /*internal void Init()
    {
        #region Graphic
        Match match = RimArchiveMain.studentNameRegex.Match(this.defName);
        if (!match.Success)
        {
            Log.Error($"Error when parsing {this.defName}: Regex cannot match");
            return;
        }
        //Example:
        //BA_Shiroko_RidingSuit => <Prefix> = BA, <FullName> = Shiroko_RidingSuit
        Icon = ContentFinder<Texture2D>.Get("Icons/" + match.Groups["FullName"].Value);
        Portrait = ContentFinder<Texture2D>.Get("Portraits/Resized/" + match.Groups["FullName"].Value);
        Memorial = ContentFinder<Texture2D>.Get("Memorial/Yuuka");
        if (Icon == null)
        {
            Log.Error($"Cannot find icon tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        if (Portrait == null)
        {
            Log.Error($"Cannot find portrait tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        if (Memorial == null)
        {
            Log.Error($"Cannot find memorial hall tex named {match.Groups["Name"].Value} for {this.defName}. All Matches:\n{match.Value}");
        }
        #endregion

        #region resolve relations
        foreach (var relationWith in relations)
        {
            foreach (StudentDef student in relationWith.others)
            {
                if(!student.relations.Any(x => x.relation ==  relationWith.relation))
                {
                    student.relations.Add(new DirectRelationWith() { relation = relationWith.relation, others = new List<StudentDef>() { this } });
                }
                else if (!student.relations.Any(x => x.others.Contains(this)))
                {
                    student.relations.First(x => x.relation.Equals(relationWith.relation)).others.Add(this);
                }
            }
        }
        #endregion
    }*/
}


/// <summary>
/// Used to set skill level and passion. Must be the subnode of StudentDef.
/// </summary>
public class PassionSkill
{
    /// <summary>
    /// Skill name. Must match the defName of the SkillDef
    /// </summary>
    public SkillDef skill;
    /// <summary>
    /// The level of this skill. Integer.
    /// </summary>
    public int level;
    /// <summary>
    /// Passion level of this skill. 0 - no interest, 1 - minor interest, 2 - major interest
    /// </summary>
    public Passion passion;


    /*  A good Example for how to custom xml loading.
     *  All types that game created needs to be cross-refed, so those must use RegisterObjectWantsCrossRef, as well as be public or game cannot find it
     *  Other types (including enums) goes to ParseHelper
     *  The arguments are also very important. Don't mess with node name
     */
#pragma warning disable CS1591
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        //Log.Message($"root is : {xmlRoot.Name}");
        if (xmlRoot == null)
            return;
        //Log.Message($"Node is: {xmlRoot.Name}\n content:{xmlRoot.InnerXml}\n");
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
        this.level = ParseHelper.FromString<int>(xmlRoot["level"].InnerText);
        //Log.Message($"level :{xmlnode["level"].InnerText}");
        //DirectXmlCrossRefLoader.TryResolveDef<SkillDef>(xmlnode["skill"].InnerText, FailMode.LogErrors);
        //Log.Message($"skill:{xmlnode["skill"].InnerText}");
        this.passion = ParseHelper.FromString<Passion>(xmlRoot["fireLevel"].InnerText);
        //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "passion", xmlnode["fireLevel"].InnerText);
        //Log.Message($"passion :{xmlnode["fireLevel"].InnerText}");
    }
}
