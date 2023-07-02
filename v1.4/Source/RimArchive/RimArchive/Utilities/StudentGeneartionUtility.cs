using System;

namespace RimArchive;


public static class StudentGenerationUtility
{
    public static void PostGen(Pawn p, StudentDef studentDef)
    {
        if (p.def != studentDef.race) p.def = studentDef.race;
        try
        {
            //Adjust skills
            foreach (SkillRecord record in p.skills.skills)
            {
                record.Level = studentDef.skills.FirstOrFallback(x => x.skill == record.def).level;
                record.passion = studentDef.skills.FirstOrFallback(x => x.skill == record.def).passion;
            }
            //Remove harmful hediffs or addiction
            //p.health.hediffSet.hediffs = p.health.hediffSet.hediffs.Where(x => !(x.def.isBad || x.def.IsAddiction)).ToList();
            p.health.hediffSet.hediffs.RemoveAll(x => x.def.isBad || x.def.IsAddiction);
            p.Name = studentDef.name;
            PawnBioAndNameGenerator.FillBackstorySlotShuffled(p, BackstorySlot.Childhood, studentDef.backstoryFiltersOverride, Faction.OfPlayer.def);
            PawnBioAndNameGenerator.FillBackstorySlotShuffled(p, BackstorySlot.Adulthood, studentDef.backstoryFiltersOverride, Faction.OfPlayer.def);
            p.story.headType = studentDef.forcedHeadType;
            p.story.hairDef = studentDef.forcedHair;
            p.story.bodyType = BodyTypeDefOf.Thin;
            p.story.traits.allTraits.Clear();
            foreach (TraitRequirement trait in studentDef.forcedTraits)
            {
                p.story.traits.GainTrait(new Trait(trait.def, trait.degree ?? 0, true));
            }
            //p.apparel.WornApparel.RemoveAll(x => x.def.apparel.bodyPartGroups.Any(t => t == BodyPartGroupDefOf.FullHead || t == BodyPartGroupDefOf.UpperHead));
            p.apparel.LockAll();
            #region Relations
            //处理人际关系
            //逻辑有点乱，首先：
            //这个要双向查找保证不漏，或者说保证任意顺序招募都可以确保关系
            //那么一开始要先一对多，然后多对一
            //还是需要简洁一点的代码，或者说数据结构
            //在StudentDef.Init里加了双向添加的代码，试试看

            //改为通用代码后需要处理在初始界面（没有地图）时的问题了
            List<Pawn> students = Find.CurrentMap?.mapPawns.AllPawns.Where(x => x.kindDef is StudentDef).ToList();
            if (students is null) return;
            foreach (var relation in (p.kindDef as StudentDef).relations)
            {
                //DebugMessage.DbgMsg($"relation: {relation.relation.defName}");
                //DebugMessage.DbgMsg($"Pawns: {string.Join("\n", relation.others.Select(x => x.defName))}");
                foreach (Pawn other in students.Where(x => !p.relations.DirectRelationExists(relation.relation, x) && relation.others.Contains(x.kindDef as StudentDef)))
                {
                    p.relations.AddDirectRelation(relation.relation, other);
                }
            }
            //查找地图上的每一个student看关系里是否包含要招募的学生
            /*foreach(var student in students.Where(x => (x.kindDef as StudentDef).relations.Exists(t => t.others.Contains(p.kindDef as StudentDef))))
            {
                foreach(var relation in (student.kindDef as StudentDef).relations.Where(x => x.others.Contains(p.kindDef as StudentDef)))
                {
                    if(!p.relations.DirectRelationExists(relation.relation, student))
                        p.relations.AddDirectRelation(relation.relation, student);
                }
            }*/
            #endregion

        }
        catch (Exception ex)
        {
            DebugMessage.DbgErr($"{ex} with {ex.Message}");
        }
    }

}
