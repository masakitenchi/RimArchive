using UnityEngine;
using AlienRace;
using System.Diagnostics;
using System;
using static HarmonyLib.Code;

namespace RimArchive;

public class ScenPart_ForcedRace : ScenPart_PawnModifier
{
    public ThingDef race;
    protected override void ModifyNewPawn(Pawn p)
    {
        if(p.def != this.race)
            p.def = this.race;
    }

    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        race ??= PossibleRaces().RandomElement();
        Rect scenPartRect = listing.GetScenPartRect(this, RowHeight * 3.0f + 31.0f);
        if(Widgets.ButtonText(scenPartRect.TopPartPixels(RowHeight), (string) race.LabelCap))
        {
            FloatMenuUtility.MakeMenu<ThingDef>(PossibleRaces(), x => (string)x.LabelCap, x => () => { this.race = x; });
        }
    }

    protected override void ModifyPawnPostGenerate(Pawn p, bool redressed)
    {
        p.story.hairDef = HairDefOf.Bald;
        p.style.beardDef = BeardDefOf.NoBeard;
        p.genes.SetXenotype((p.def as ThingDef_AlienRace).alienRace.raceRestriction.xenotypeList.First());
        p.genes.ClearXenogenes();
        foreach (var gene in p.genes.Xenotype.AllGenes)
            p.genes.AddGene(gene, false);
    }

    private static IEnumerable<ThingDef> PossibleRaces()
    {
        if (ModLister.HasActiveModWithName("Humanoid Alien Races") || ModsConfig.IsActive("erdelf.humanoidalienraces"))
            return DefDatabase<ThingDef_AlienRace>.AllDefs.
                Where(x =>  x.race?.Humanlike ?? false);
        return DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false);
    }
}


//StartingPawnUtility 我干你娘
public class ScenPart_FixedStartingPawns : ScenPart_ConfigPage_ConfigureStartingPawnsBase
{
    new public int pawnChoiceCount = 0;


    public int pawnCount = 3;
    [MustTranslate]
    public string customSummary;

    //Count默认为1
    public List<PawnKindCount> possibleKindDefs = new List<PawnKindCount>();

    protected override int TotalPawnCount => possibleKindDefs.Sum(x => x.count);

    public override string Summary(Scenario scen) => this.customSummary ?? (string)"ScenPart_StartWithCertainColonists".Translate();

    //Except或许可以用在派生类里（如果将来有把这个再抽象成一层基类的必要的话）
    public IEnumerable<PawnKindDef> availableDefs => DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike && x.defaultFactionType != null && x.defaultFactionType.isPlayer).Except(possibleKindDefs.Select(x => x.kindDef));
    protected override void GenerateStartingPawns()
    {
        Log.Message("Generating Starting Pawns...");
        var num = 0;
        StartingPawnUtility.ClearAllStartingPawns();
        for(int i = 0; i< this.possibleKindDefs.Count; i++)
        {
            /* 凭什么？
             * private static void EnsureGenerationRequestInRangeOf(int index)
                {
                  while (StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Count <= index)
                    StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Add(StartingPawnUtility.DefaultStartingPawnRequest);
                }
             */
            StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Add(new PawnGenerationRequest(this.possibleKindDefs[i].kindDef));
            Pawn p = PawnGenerator.GeneratePawn(new PawnGenerationRequest(possibleKindDefs[i].kindDef,
                                faction: Faction.OfPlayer,
                                canGeneratePawnRelations: false,
                                colonistRelationChanceFactor: 0f));
            if (this.possibleKindDefs[i].kindDef is StudentDef s)
            {
                Log.Message("Postgen for students");
                StudentGenerationUtility.PostGen(p, s);
            }
            if (this.possibleKindDefs[i].kindDef == PawnKindDefOfLocal.RA_PawnKindDef_Sensei)
            {
                p.apparel.Wear(ThingMaker.MakeThing(ThingDefOf.Shittim_Chest_Apparel, GenStuff.RandomStuffFor(ThingDefOf.Shittim_Chest_Apparel)) as Apparel, false, false);
            }
            StartingPawnUtility.StartingAndOptionalPawns.Insert(i, p);
            StartingPawnUtility.GeneratePossessions(p);
            ++num;
            Log.Message($"Added {p} to Starting Pawn");
        }
        //再加上一个防止默认值被添加（StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Count <= index 而不是 < index）
        StartingPawnUtility.StartingAndOptionalPawnGenerationRequests.Add(new PawnGenerationRequest(this.possibleKindDefs.RandomElement().kindDef));
        while (num < TotalPawnCount && !StartingPawnUtility.WorkTypeRequirementsSatisfied())
        {
            Log.Message("Adding Extra Pawns to Starting Pawn");
            StartingPawnUtility.AddNewPawn();
            ++num;
        }
        Log.Message($"Generated {num} pawns");
    }
    public override int GetHashCode()
    {
        int hashCode = base.GetHashCode();
        foreach (PawnKindCount kindCount in this.possibleKindDefs)
            hashCode ^= kindCount.GetHashCode();
        return hashCode;
    }

    public override void PostIdeoChosen()
    {
        Find.GameInitData.startingPawnCount = this.TotalPawnCount;
        //限制可选人数
        Find.GameInitData.startingPawnsRequired = StartingPawnUtility.StartingAndOptionalPawns.Select(x => new PawnKindCount() { kindDef = x.kindDef }).ToList();
        if (ModsConfig.BiotechActive)
        {
            Current.Game.customXenotypeDatabase.customXenotypes.Clear();
            foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading)
            {
                foreach (Precept precept in ideo.PreceptsListForReading)
                {
                    if (precept is Precept_Xenotype preceptXenotype && preceptXenotype.customXenotype != null && !Current.Game.customXenotypeDatabase.customXenotypes.Contains(preceptXenotype.customXenotype))
                        Current.Game.customXenotypeDatabase.customXenotypes.Add(preceptXenotype.customXenotype);
                }
            }
        }
        if (ModsConfig.IdeologyActive && Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo != null)
        {
            foreach (Precept precept in Faction.OfPlayerSilentFail.ideos.PrimaryIdeo.PreceptsListForReading)
            {
                if (precept.def.defaultDrugPolicyOverride != null)
                    Current.Game.drugPolicyDatabase.MakePolicyDefault(precept.def.defaultDrugPolicyOverride);
            }
        }
        this.GenerateStartingPawns();
    }
    public override void DoEditInterface(Listing_ScenEdit listing)
    {
        Rect rect = listing.GetScenPartRect(this, RowHeight * (TotalPawnCount + 1)) with
        {
            height = RowHeight
        };
        for (int i = 0; i < possibleKindDefs.Count; i++)
        {
            PawnKindCount kindCount = possibleKindDefs[i];
            Rect MenuRect = new Rect(rect);
            MenuRect.xMax -= RowHeight;
            if (Widgets.ButtonText(MenuRect, kindCount.kindDef.LabelCap))
            {
                /*List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach(PawnKindDef def in availableDefs)
                {
                    PawnKindDef kindDef1 = def;
                    options.Add(new FloatMenuOption(kindDef1.LabelCap, (() => kindDef = kindDef1)));
                }
                if (options.Any())
                    Find.WindowStack.Add(new FloatMenu(options));*/
                FloatMenuUtility.MakeMenu(availableDefs, x => (string)x.LabelCap, delegate (PawnKindDef x)
                {
                    //Log.Message($"Trying to set {kindCount.kindDef.defName}@{i} to {x.defName}");
                    return () => kindCount.kindDef = x;
                });
            }
            if (Widgets.ButtonImage(rect with
            {
                xMin = MenuRect.xMax
            }, TexButton.DeleteX))
            {
                this.possibleKindDefs.RemoveAt(i);
                return;
            }
            rect.y += RowHeight;
        }
        if (Widgets.ButtonText(rect, "Add"))
        {
            FloatMenuUtility.MakeMenu(availableDefs, x => (string)x.LabelCap, delegate (PawnKindDef x)
            {
                return () => possibleKindDefs.Add(new PawnKindCount()
                {
                    kindDef = x
                });
            });
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref possibleKindDefs, "kindCounts", LookMode.Deep);
    }

}