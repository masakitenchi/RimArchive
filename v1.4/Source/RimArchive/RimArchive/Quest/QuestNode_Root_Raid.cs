using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions.Must;
using Verse;

namespace RimArchive;

public class QuestNode_Root_Raid : QuestNode
{
    private static readonly IntRange MaxDelayTicksRange = new IntRange(7200, 14400);
    private static readonly IntRange MinDelayTicksRange = new IntRange(1800, 3600);
    //照抄一下，同时带上一些注释
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        Quest quest = QuestGen.quest;
        Map map = slate.Get<Map>("map");
        ThingDef reward = slate.Get<ThingDef>("reward");
        RaidDef raid = slate.Get<RaidDef>("bossgroup");
        int difficulty = slate.Get<int>("wave");
        Faction mech = Faction.OfMechanoids;
        TaggedString bossLabelCap = raid.waves[difficulty].bossOverride?.kindDef.LabelCap ?? raid.boss.kindDef.LabelCap;
        string bossLabel = raid.waves[difficulty].bossOverride?.kindDef.label ?? raid.boss.kindDef.label;
        //我想不到哪种情况下会没有这个派系。生成的时候就手动点掉了?
        if (mech == null)
        {
            List<FactionRelation> relation = new List<FactionRelation>();
            foreach (Faction existFaction in Find.FactionManager.AllFactionsListForReading)
            {
                relation.Add(new FactionRelation()
                {
                    other = existFaction,
                    kind = FactionRelationKind.Hostile
                });
            }
            mech = FactionGenerator.NewGeneratedFactionWithRelations(new FactionGeneratorParms(FactionDefOf.Mechanoid, hidden: true), relation);
            mech.temporary = true;
            Find.FactionManager.Add(mech);
        }
        List<Pawn> escorter = new List<Pawn>();
        RaidGroupWave wave = raid.waves[difficulty];
        PawnGenerationRequest bossReq = new PawnGenerationRequest(wave.bossOverride?.kindDef ?? raid.boss.kindDef, mech, forceGenerateNewPawn: true);
        //因为只有一个boss了所以这里没有循环
        Pawn boss = PawnGenerator.GeneratePawn(bossReq);
        Hediff damageRed = HediffMaker.MakeHediff(HediffDefOf.BA_BossDamageReduction, boss);
        damageRed.Severity = difficulty;
        boss.health.AddHediff(damageRed);
        //Nullable List still needs if to ensure it won't throw NullReferenceException
        if (!wave.bossApparel.NullOrEmpty())
        {
            foreach (var apparel in wave.bossApparel)
            {
                Apparel newApparel = ThingMaker.MakeThing(apparel) as Apparel;
                boss.apparel.Wear(newApparel, locked: true);
            }
        }
        Find.WorldPawns.PassToWorld(boss);
        foreach (var escort in wave.escorts)
        {
            PawnGenerationRequest escortReq = new PawnGenerationRequest(escort.kindDef, mech, forceGenerateNewPawn: true);
            GenerateXPawnAndPassToWorld(escortReq, escort.count, escorter);
        }
        slate.Set("mapParent", map.Parent);
        slate.Set("rewardStudied", Find.StudyManager.StudyComplete(reward));
        slate.Set<Pawn>("escortees", boss);
        IntVec3 dropCenterDistant = DropCellFinder.FindRaidDropCenterDistant(map);
        IEnumerable<Pawn> everyone = escorter.Concat(boss);
        foreach (Pawn p in everyone)
            map.attackTargetsCache.UpdateTarget(p);
        string newSignal = QuestGen.GenerateNewSignal("RaidArrives");
        QuestPart_RaidArrives questPart_RaidArrives = new QuestPart_RaidArrives
        {
            mapParent = map.Parent,
            raidDef = raid,
            minDelay = MinDelayTicksRange.RandomInRange,
            maxDelay = MaxDelayTicksRange.RandomInRange,
            inSignalEnable = QuestGen.slate.Get<string>("inSignal")
        };
        //没太搞懂AddPart和直接DropPods、Letter、Alert之类之间的区别
        //Quest_XXX 和QuestPart_XXX的区别？
        questPart_RaidArrives.outSignalsCompleted.Add(newSignal);
        quest.AddPart(questPart_RaidArrives);
        Quest quest1 = quest;
        MapParent mapParent = map.Parent;
        quest1.DropPods(mapParent, everyone, sendStandardLetter: false, inSignal: newSignal, dropSpot: dropCenterDistant, faction: mech);
        Quest quest2 = quest;
        quest2.Letter(LetterDefOf.NeutralEvent, label: "LetterLabelBossgroupSummoned".Translate(bossLabelCap), relatedFaction: mech, text: "LetterBossgroupSummoned".Translate((NamedArgument)mech.NameColored));
        quest2.Letter(LetterDefOf.Bossgroup, label: "LetterLabelBossgroupArrived".Translate(bossLabelCap), inSignal: newSignal, text: "LetterBossgroupArrived".Translate(mech.NameColored.ToString(), raid.LeaderDescription, bossLabel, mech.def.pawnsPlural, raid.GetWaveDescription(difficulty)), relatedFaction: mech, lookTargets: everyone);
        QuestPart_RaidGroup questPart_RaidGroup = new QuestPart_RaidGroup()
        {
            pawns = everyone.ToList(),
            faction = mech,
            mapParent = map.Parent,
            bosses = new List<Pawn>() { boss },
            stageLocation = dropCenterDistant,
            inSignal = newSignal
        };
        quest.AddPart(questPart_RaidGroup);
        quest.Alert("AlertBossgroupIncoming".Translate(bossLabelCap), "AlertBossgroupIncomingDesc".Translate(bossLabel), critical: true, inSignalDisable: newSignal);
        //这里貌似需要和上边某个字符串对应
        //slate.Set<Pawn>("escortees",boss); -> "escortees.KilledLeavingsLeft" 之类的
        string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("escortees.KilledLeavingsLeft");
        quest.ThingStudied(reward, () => quest.Letter(LetterDefOf.PositiveEvent, text: "[bossDefeatedLetterText]", label: "[bossDefeatedLetterLabel]"), () => quest.Letter(LetterDefOf.PositiveEvent, text: "[bossDefeatedStudyChipLetterText]", label: "[bossDefeatedLetterLabel]"), inSignal4);
        quest.AnyPawnAlive(everyone, elseAction: () => QuestGen_End.End(quest, QuestEndOutcome.Unknown), inSignal: QuestGenUtility.HardcodedSignalWithQuestID("escortees.Killed"));
        quest.End(QuestEndOutcome.Unknown, inSignal: QuestGenUtility.HardcodedSignalWithQuestID("mapParent.Destroyed"));

    }

    protected override bool TestRunInt(Slate slate) => slate.Exists("wave") && slate.Exists("bossgroup") && slate.Exists("map") && slate.Exists("reward");

    static void GenerateXPawnAndPassToWorld(PawnGenerationRequest request, int X, List<Pawn> listToAdd)
    {
        for (int i = 0; i < X; i++)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            listToAdd.Add(pawn);
            Find.WorldPawns.PassToWorld(pawn);
        }
    }
}
