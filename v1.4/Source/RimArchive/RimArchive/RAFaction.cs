using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using static RimWorld.TradeUtility;
using static RimArchive.RimArchive;
using RimArchive.Window;

namespace RimArchive
{

#pragma warning disable CS1591
    [DefOf]
    public static class RAFactionDefOf
    {
        public static readonly FactionDef Shale;

        static RAFactionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RAFactionDefOf));
        }
    }
#pragma warning restore CS1591

    /// <summary>
    /// Here process the faction dialog menu. Will move to elsewhere
    /// </summary>
    [HotSwappable]
    public class RAFaction
    {
        //private static readonly int RandomGatchaCost = 1000;
        //private static readonly int ChooseSchoolGatchaCost = 1500;
        private readonly Pawn requestor;
        private readonly Faction faction;
        private readonly DiaNode root;

        /// <summary>
        /// Looks like a ctor. But maybe I should change that.
        /// </summary>
        /// <param name="requestor"></param>
        /// <param name="faction"></param>
        /// <param name="root"></param>
        public RAFaction(Pawn requestor, Faction faction, DiaNode root)
        {
            this.requestor = requestor;
            this.faction = faction;
            this.root = root;
        }

        public DiaOption CreateInitialDiaMenu()
        {
            bool disabled = Faction.OfPlayer.HostileTo(faction);
            TaggedString HostileFail = "HostileFail".Translate(faction.NameColored);
            return new DiaOption("RAExchangeOpenOption".Translate())
            {
                disabled = disabled,
                disabledReason = HostileFail,
                linkLateBind = () => ShowDiaNode()
            };
        }

        private DiaNode ShowDiaNode()
        {
            DiaNode node = new DiaNode("RAExchangeRoot".Translate());
            node.options.Add(new DiaOption("RAStudentRecruitMenu".Translate())
            {
                resolveTree = true,
                action = () =>
                {
                    Find.TickManager.Pause();
                    Find.WindowStack.Add(new RecruitWindow(node));
                }
            });
            node.options.Add(new DiaOption("RABack".Translate())
            {
                linkLateBind = () => root
            });
            return node;
        }

        /*private DiaNode StudentRecruitment(DiaNode parent)
        {
            DiaNode node = new DiaNode("RAStudentRecruitment".Translate());
            node.options.Add(new DiaOption("StudentGachaRandom".Translate(RandomGatchaCost.ToString()))
            {
                disabled = !(DebugSettings.godMode || ColonyHasEnoughSilver(requestor.Map, RandomGatchaCost)),
                disabledReason = "NotEnoughMoney".Translate(),
                action = delegate
                {
                    IntVec3 intVec3 = DropCellFinder.TradeDropSpot(requestor.Map);
                    Pawn student = ChooseRandomStudentFrom(AllStudents);
                    ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                    activeDropPodInfo.innerContainer.TryAddOrTransfer(student as Thing);
                    DropPodUtility.MakeDropPodAt(intVec3, requestor.Map, activeDropPodInfo);
                    TradeUtility.LaunchSilver(requestor.Map, RandomGatchaCost);
                    Messages.Message("StudentArrived".Translate(student.NameFullColored), new LookTargets(intVec3, requestor.Map), MessageTypeDefOf.PositiveEvent);
                },
                resolveTree = true
            });
            node.options.Add(new DiaOption("StudentGachaCertainSchool".Translate(ChooseSchoolGatchaCost.ToString()))
            {
                disabled = !DebugSettings.godMode && !ColonyHasEnoughSilver(requestor.Map, ChooseSchoolGatchaCost),
                disabledReason = "NotEnoughMoney".Translate(),
                link = SelectSchool(node)
            });
            node.options.Add(new DiaOption("RABack".Translate())
            {
                linkLateBind = () => parent
            });
            return node;
        }*/

        /*private DiaNode SelectSchool(DiaNode parent)
        {
            DiaNode node = new DiaNode("ChooseSchool".Translate());
            foreach (string school in cachedSchools)
            {
                node.options.Add(new DiaOption(school.Translate())
                {
                    disabled = !cachedAllStudentsBySchool.ContainsKey(school),
                    disabledReason = "NoAvailableStudentInSchool".Translate(),
                    action = delegate
                    {
                        IntVec3 intVec3 = DropCellFinder.TradeDropSpot(requestor.Map);
                        ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                        Pawn student = ChooseRandomStudentFrom((from x in cachedAllStudentsBySchool[school]
                                                                select x).ToList());
                        activeDropPodInfo.innerContainer.TryAddOrTransfer(student as Thing);
                        DropPodUtility.MakeDropPodAt(intVec3, requestor.Map, activeDropPodInfo);
                        TradeUtility.LaunchSilver(requestor.Map, ChooseSchoolGatchaCost);
                        Messages.Message("StudentArrived".Translate(student.NameFullColored), new LookTargets(intVec3, requestor.Map), MessageTypeDefOf.PositiveEvent);
                    },
                    resolveTree = true
                });
            }
            node.options.Add(new DiaOption("RABack".Translate())
            {
                linkLateBind = () => parent
            });
            return node;
        }*/

        private Pawn ChooseRandomStudentFrom(List<PawnKindDef> students)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(students.RandomElement(), Faction.OfPlayer);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            return pawn;
        }

        private int CurrentSilver
        {
            get
            {
                return (from x in TradeUtility.AllLaunchableThingsForTrade(requestor.Map)
                        where x.def == RimWorld.ThingDefOf.Silver
                        select x).Sum(x => x.stackCount);
            }
        }
    }
}
