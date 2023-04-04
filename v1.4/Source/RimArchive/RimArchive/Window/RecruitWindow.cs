using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using AlienRace;
using static RimArchive.RimArchiveMain;
using static RimArchive.DebugMessage;
using static Verse.Widgets;
using System.Text;
using UnityEngine.Profiling;
using UnityEngine.UI;
using RimArchive.GameComponents;
using System.Runtime.CompilerServices;

namespace RimArchive.Window
{
    /// <summary>
    /// This is the class that shows the recruit window. Might be more complicated once the main feature is finished. (e.g. Handle all the faction dialog here rather than using vanilla method)
    /// </summary>
    [HotSwappable]
    //目前尚未解决esc弹出菜单的问题。只能先继续沿用DiaNodeTree了
    public class RecruitWindow : Verse.Dialog_NodeTree
    {
        #region private field
        private static readonly float _scrollBarWidth = GenUI.ScrollBarWidth;
        private static readonly Vector2 _Margin = new Vector2(5f, 5f);
        private static readonly Vector2 _iconSize = new Vector2(120f, 120f);
        private static readonly Vector2 _lblSize = new Vector2(80f, 20f);
        private static Vector2 _dlgscrbr = Vector2.zero;
        private static Vector2 _icnscrbr = Vector2.zero;
        private static Vector2 _stdscrbr = Vector2.zero;
        private static Vector2 _profile = Vector2.zero;
        private static Vector2 _sclscrbr = Vector2.zero;
        private static int _SkillCount = DefDatabase<SkillDef>.DefCount;
        private static float _levelLabelWidth = -1f;
        private static float _cachedSchoolListHeight;
        private static bool _inStudentProfile = false;
        private static bool _clickedSchoolIcon = false;
        private static IconDef _currentSchool;
        private static Pawn _cachedStudent;
        private static StudentDef _currentStudent;
        #endregion

        internal static void Init()
        {
            _cachedSchoolListHeight = cachedSchools.Count * _iconSize.y;
            _currentSchool = cachedAllStudentsBySchool.Keys.First();
        }


        public override void OnCancelKeyPressed()
        {
            //Log.Message("Cancel Key Down Event Captured");
            if (!_inStudentProfile)
            {
                Close();
            }
            else
            {
                _cachedStudent.Discard();
                _currentStudent = null;
                _inStudentProfile = false;
            }
        }
        public RecruitWindow(DiaNode parent) : base(parent)
        {
            //openMenuOnCancel = false;
            //closeOnAccept = false;
            //closeOnCancel = false;
            //forcePause = false;
            //absorbInputAroundWindow = false;
        }

        public override Vector2 InitialSize
        {
            get
            {
                Vector2 res = new(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
                return res;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                Rect outRect = new Rect(inRect);
                //GUI.DrawTexture(outRect, BaseContent.BlackTex);
                //Sensei头像
                Rect sensei = new Rect(inRect.position, _iconSize);
                GUI.DrawTexture(sensei, Sensei, ScaleMode.ScaleAndCrop);
                //最右侧列出所有学校
                Rect schoolList = new Rect(inRect.xMax - _iconSize.x - 2 * _scrollBarWidth, inRect.y, _iconSize.x + 4 * _scrollBarWidth, inRect.height);
                //右侧的对话……栏?
                //暂定作为周常的boss栏
                outRect.x += sensei.width;
                outRect.height = sensei.height;
                outRect.width = inRect.width / 3;
                DrawRaidDialog(outRect);
                Rect studentsRect = new Rect(sensei.x, sensei.yMax, inRect.width, inRect.height - sensei.height);
                //加个if，不多画窗口了
                if (!_inStudentProfile)
                {
                    studentsRect.width -= schoolList.width - 4 * _Margin.x;
                    outRect.x += outRect.width;
                    outRect.xMax = studentsRect.xMax;
                    //学校介绍
                    DrawSchoolDescription(outRect);
                    //学校一览
                    DrawSchoolList(schoolList);
                    //根据当前选择的学校绘制可用学生
                    DrawBox(studentsRect, 1, BaseContent.WhiteTex);
                    studentsRect.x += _Margin.x;
                    studentsRect.y += _Margin.y;
                    DrawStudentList(studentsRect, cachedAllStudentsBySchool[_currentSchool]);
                }
                else
                {
                    //参考新Layout的话，需要分成以下几部分：
                    //右上角：校徽、全名、所属部活、etc.
                    //中下部：L2D大厅某一帧，左下角有一个所属校和名字的Rect；
                    //右下：特性、不能从事、Skill
                    //左下：技能
                    Rect profileRect = new Rect(outRect);
                    outRect.x += outRect.width;
                    outRect.xMax = inRect.xMax;
                    Rect hallRect = new Rect(studentsRect);
                    hallRect.xMin += studentsRect.width / 4;
                    hallRect.xMax -= studentsRect.width / 4;
                    Rect abilityRect = new Rect(studentsRect);
                    abilityRect.xMin = hallRect.xMax;
                    abilityRect.xMax = studentsRect.xMax;
                    Rect skillRect = new Rect(studentsRect);
                    skillRect.xMax = hallRect.x;
                    /*hallRect.size = studentsRect.size / 4 * 3;
                    hallRect = hallRect.CenteredOnXIn(studentsRect);
                    hallRect = hallRect.CenteredOnYIn(studentsRect);*/
                    //GUI.DrawTexture(outRect, BaseContent.YellowTex);
                    DrawBox(outRect, 1, BaseContent.WhiteTex);
                    DrawProfile(outRect);
                    DrawMemorialHall(hallRect);
                    //GUI.DrawTexture(skillRect, BaseContent.BlackTex);
                    DrawBox(skillRect, 1, BaseContent.WhiteTex);
                    DrawSkillAndTrait(skillRect);
                    //GUI.DrawTexture(abilityRect, BaseContent.WhiteTex);
                    DrawAbility(abilityRect);
                }
            }
            catch (Exception ex)
            {
                this.Close();
                Log.Error($"Exception when drawing GUI: {ex}");
            }

        }

        #region GUISCHOOL
        static void DrawRaidDialog(Rect outRect)
        {
            //outRect.width -= 1050f;
            //outRect.height -= 70f;
            GUI.DrawTexture(outRect, BaseContent.GreyTex);
            BeginGroup(outRect.ContractedBy(_Margin.x));
            Rect inRect = outRect.ContractedBy(_Margin.x).AtZero();
            float width = inRect.width / 3;
            for (int i = 0; i <= cachedAllBosses.Count - 1 && i < 3; i++)
            {
                Rect bossPic = new Rect(inRect.x + width * i, inRect.y, width, inRect.height);
                DrawTextureFitted(bossPic, cachedAllBosses.RandomElement().icon, 1f);
            }
            EndGroup();
            Widgets.LabelScrollable(outRect, "吃了吗您内今天也是好天气你是一个个什么啊漂亮得很呐人生路漫漫而修远兮吾将上下而求索关关雎鸠在河之洲窈窕淑女君子好逑".Translate(), ref _dlgscrbr);
        }

        static void DrawSchoolList(Rect outRect)
        {
            Rect viewRect = new Rect(outRect);
            //DrawBox(outRect, 1,BaseContent.WhiteTex);
            viewRect.height = _cachedSchoolListHeight;
            viewRect.width -= _scrollBarWidth;
            BeginScrollView(outRect, ref _icnscrbr, viewRect);
            int startRow = (int)Math.Floor((decimal)(_icnscrbr.y / _iconSize.y));
            startRow = (startRow < 0) ? 0 : startRow;
            //+iconSize.y平滑滚动条
            int endRow = startRow + (int)(Math.Ceiling((decimal)((outRect.height + _iconSize.y) / _iconSize.y)));
            endRow = (endRow > cachedSchools.Count) ? cachedSchools.Count : endRow;
            for (int i = startRow; i < endRow; i++)
            {
                Rect row = new Rect(viewRect.x, viewRect.y + i * _iconSize.y, _iconSize.x, _iconSize.y);
                //Text.WordWrap = false;
                if (cachedSchools[i].tex != null)
                {
                    //Widgets.DrawTextureFitted(new Rect(row.position, new Vector2(btnHeight, btnHeight)), operatorClasses[i].tex, 1f);
                    //Widgets.LabelFit(new Rect(row.position + new Vector2(btnHeight, 0f), new Vector2(btnWidth - btnHeight,btnHeight)), operatorClasses[i].label.Translate());
                    TooltipHandler.TipRegion(row, cachedSchools[i].LabelCap);
                    DrawTextureFitted(row, cachedSchools[i].tex, 1f);
                }
                else
                {
                    LabelFit(row, cachedSchools[i].label.Translate());
                }
                DrawHighlightIfMouseover(row);
                //可以设定成点击之后直到鼠标离开这个绘图区才允许Mouse.IsOver改写_currentSchool
                //搞定...?
                //还是差一点
                if (ButtonInvisible(row))
                {
                    _clickedSchoolIcon = true;
                    _currentSchool = cachedSchools[i];
                }
                else if (_clickedSchoolIcon)
                {
                    if (Mouse.IsOver(outRect))
                        continue;
                    else
                        _clickedSchoolIcon = false;
                }
                else if (Mouse.IsOver(row))
                {
                    _currentSchool = cachedSchools[i];
                }
                //Text.WordWrap = true;
            }
            EndScrollView();
        }


        static void DrawSchoolDescription(Rect outRect)
        {
            GUI.DrawTexture(outRect, BaseContent.BlackTex);
            LabelScrollable(outRect, _currentSchool.description, ref _sclscrbr);
        }

        //目前在拉伸版的1366*768下会爆红。具体原因未知
        void DrawStudentList(Rect outRect, List<StudentDef> students)
        {
            try
            {
                if (students.NullOrEmpty()) return;
                Rect viewRect = new Rect(outRect);
                ResolveTotalHeightAndReturnRowCount(ref viewRect, out int rowCount, out int columnCount);
                BeginScrollView(outRect, ref _stdscrbr, viewRect);
                int startRow = (int)Math.Floor((decimal)(_stdscrbr.y / (_iconSize.y + _lblSize.y)));
                startRow = (startRow < 0) ? 0 : startRow;
                //+iconSize.y平滑滚动条
                int endRow = startRow + (int)(Math.Ceiling((decimal)((outRect.height + _iconSize.y) / (_iconSize.y + _lblSize.y))));
                endRow = (endRow > rowCount) ? rowCount : endRow;
                Rect rect = new Rect(viewRect.x, viewRect.y, _iconSize.x, _iconSize.y + _lblSize.y);
                int studentNo = 0;
                for (int i = startRow; i < endRow; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        BeginGroup(rect);
                        Rect icon = new Rect(Vector2.zero, _iconSize);
                        DrawTextureFitted(icon, students[studentNo].Icon, 1f);
                        DrawHighlightIfMouseover(icon);
                        if (ButtonInvisible(icon))
                        {
                            _currentStudent = students[studentNo];
                            _cachedStudent = PawnGenerator.GeneratePawn(new PawnGenerationRequest(_currentStudent as PawnKindDef,
                                faction: Faction.OfPlayer,
                                canGeneratePawnRelations: false,
                                mustBeCapableOfViolence: true,
                                colonistRelationChanceFactor: 0f,
                                allowGay: false,
                                allowAddictions: false,
                                fixedGender: Gender.Female
                                ));
                            PostGen(_cachedStudent);
                            _inStudentProfile = true;
                        }
                        TooltipHandler.TipRegion(icon, students[studentNo].description);
                        //Label Tex
                        icon.y = icon.yMax;
                        GUI.DrawTexture(icon, BaseContent.GreyTex);
                        Verse.Text.Anchor = TextAnchor.UpperCenter;
                        LabelCacheHeight(ref icon, students[studentNo].label);
                        //Log.Message($"{students[studentNo].label}");
                        Verse.Text.Anchor = default;

                        //Log.Message($"x:{rect.x}, y:{rect.y}, width:{rect.width}, height:{rect.height}");
                        rect.x += _iconSize.x + _Margin.x;
                        EndGroup();
                        studentNo++;
                    }
                    rect.x = viewRect.x;
                    rect.y += _iconSize.y + _Margin.y + _lblSize.y;
                }
                EndScrollView();

            }
            catch
            {
                //Log.Error($"Current State:\nviewRect:{viewRect}")
                _inStudentProfile = false;
                this.Close();
                Log.Error("Currently this window will throw error when using streched 1366*768 fullscreen (When you choose 1366*768 fullscreen in screen with native resolution > 1366*768). We're sorry about that and will investigate.");
            }
        }

        static void ResolveTotalHeightAndReturnRowCount(ref Rect viewRect, out int rowCount, out int columnCount)
        {
            int studentCount = cachedAllStudentsBySchool[_currentSchool].Count;
            columnCount = (int)(viewRect.width / (_iconSize.x + _Margin.x));
            if (columnCount > studentCount)
                columnCount = studentCount;
            rowCount = Mathf.CeilToInt((float)studentCount / columnCount);
            viewRect.height = (_iconSize.y + _Margin.y) * rowCount;
            viewRect.width = (_iconSize.x + _Margin.x) * columnCount;
            //Log.Message($"School:{_currentSchool}, {studentCount} people, {rowCount} rows, {columnCount} columns\n viewRect: {viewRect.x}x, {viewRect.y}y, {viewRect.height} height, {viewRect.width} width");
        }
        #endregion

        #region PROFILE
        static void DrawMemorialHall(Rect outRect)
        {
            //备选项：
            GUI.DrawTexture(outRect.ContractedBy(_Margin.x), _currentStudent.Memorial, ScaleMode.ScaleAndCrop, true, 0, Color.white, 0, 10);
            //GUI.DrawTexture(hallRect, _currentStudent.Memorial);
            //Rect recruitBtn = new Rect(outRect);
            //GUI.DrawTexture(recruitBtn, BaseContent.WhiteTex);

        }

        private static Vector2 _descriptionscrbr;
        static void DrawProfile(Rect outRect)
        {
            BeginGroup(outRect);
            //资料部分：左上->校徽， 右侧：<description>，最右侧：部活...这样？
            Rect schoolIcon = new Rect(outRect.AtZero());
            Rect description = new Rect(outRect.AtZero());
            schoolIcon.size = new Vector2(outRect.height, outRect.height);
            description.width -= schoolIcon.width;
            description.x += schoolIcon.width + 2f;
            //是否可以保证_currentSchool必定为当前页面学生的学校呢...
            DrawTextureFitted(schoolIcon, _currentSchool.tex, 1f);
            DrawLineVertical(schoolIcon.xMax, 0f, schoolIcon.height);
            LabelScrollable(description, _currentStudent.description, ref _descriptionscrbr, false);
            EndGroup();
        }


        private static Vector2 _traitscrbr;
        private static Vector2 _storyscrbr;
        void DrawSkillAndTrait(Rect outRect)
        {
            Verse.Text.Font = GameFont.Small;
            BeginGroup(outRect);
            Rect skillRect = new Rect(outRect.AtZero().ContractedBy(_Margin.x));
            //GUI.DrawTexture(abilityRect, BaseContent.WhiteTex);
            BeginGroup(skillRect);
            Rect skillTab = new Rect(skillRect.AtZero());

            //GUI.DrawTexture(skillTab, BaseContent.BadTex);
            //skillTab.height = skillTab.height / _SkillCount;
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                float x = Verse.Text.CalcSize(allDefsListForReading[i].skillLabel.CapitalizeFirst()).x;
                if (x > _levelLabelWidth)
                {
                    _levelLabelWidth = x;
                }
            }
            //SkillUI.DrawSkillsOf(_cachedStudent, Vector2.zero, (Current.ProgramState != ProgramState.Playing) ? SkillUI.SkillDrawMode.Menu : SkillUI.SkillDrawMode.Gameplay,skillTab);
            //Rect skillRectEach = new Rect(abilityRect.ContractedBy(_Margin.x));
            skillTab.SplitHorizontally(skillTab.height / 3 * 2, out Rect skillTabGroup, out Rect TraitsGroup);
            #region skillTab
            BeginGroup(skillTab);
            skillTabGroup.position = skillTab.AtZero().position;
            //GUI.DrawTexture(skillTabGroup, BaseContent.BadTex);
            //平均分布技能条
            skillTabGroup.height = skillTabGroup.height / _SkillCount;
            Rect skillbar = new Rect(skillTabGroup);
            skillbar.xMin += _levelLabelWidth * 2;
            Rect passionIcon = new Rect(skillbar);
            passionIcon.x = skillbar.x;
            passionIcon.width = SkillUI.SkillHeight;
            //passionIcon.xMax = skillbar.x;
            Rect label = new Rect(skillTabGroup.AtZero());
            foreach (SkillRecord skill in _cachedStudent.skills.skills)
            {
                FillableBar(skillbar, (float)skill.Level / SkillRecord.MaxLevel, BaseContent.GreyTex, BaseContent.ClearTex, true);
                DrawTextureFitted(passionIcon, IconforPassion(skill.passion), 1f);
                LabelFit(label, string.Concat(skill.def.LabelCap, "   ", skill.Level.ToString()));
                //LabelCacheHeight(ref skillRectEach, skill.def.defName.Translate());
                skillbar.y += skillTabGroup.height;
                passionIcon.y += skillTabGroup.height;
                label.y += skillTabGroup.height;
            }
            EndGroup();
            #endregion
            //Traits & BackStory
            #region
            BeginGroup(TraitsGroup);
            DrawLineHorizontal(0f, 0f, TraitsGroup.width);
            DrawLineVertical(TraitsGroup.width / 2, 0f, TraitsGroup.height);
            Rect outtrait = TraitsGroup.AtZero().LeftHalf().ContractedBy(2f);
            Rect outstory = TraitsGroup.AtZero().RightHalf().ContractedBy(2f);
            Rect viewtrait;
            Rect viewstory;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Traits".Translate());
            _cachedStudent.story.traits.allTraits.Select(x => stringBuilder.AppendLine(x.LabelCap));
            LabelScrollable(outtrait, stringBuilder.ToString(), ref _traitscrbr);
            stringBuilder.Clear();
            stringBuilder.AppendLine("BackStories".Translate());
            _cachedStudent.story.AllBackstories.Select(x => stringBuilder.AppendLine(x.LabelCap));
            LabelScrollable(outstory, stringBuilder.ToString(), ref _storyscrbr);
            stringBuilder.Clear();
            //BeginScrollView(outtrait, ref _traitscrbr, outtrait.ContractedBy(2f), false);
            //EndScrollView();
            //BeginScrollView(outstory, ref _storyscrbr, outstory.ContractedBy(2f), false);
            //EndScrollView();
            EndGroup();
            #endregion
            EndGroup();
            EndGroup();
        }

        void DrawAbility(Rect outRect)
        {
            BeginGroup(outRect);
            Rect inRect = outRect.AtZero().ContractedBy(_Margin.x);
            DrawBox(inRect, 1, BaseContent.WhiteTex);
            Rect recruitBtn = outRect.AtZero().CenteredOnYIn(outRect);
            recruitBtn.size = _iconSize;
            if (ButtonImageFitted(recruitBtn, SSR))
            {
                //存活
                if (RimArchiveMain.StudentDocument.IsAlive(_currentStudent))
                {
                    Messages.Message("StudentAlreadyRecruited".Translate(_cachedStudent.NameFullColored), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    //未存活但招募过
                    if (RimArchiveMain.StudentDocument.IsRecruited(_currentStudent))
                    {
                        RimArchiveMain.StudentDocument.DocumentedStudent(_currentStudent, ref _cachedStudent);
                        DebugMessage.DbgMsg("Re-recruiting");
                        DbgMsg($"Pawn name:{_cachedStudent.Name.ToStringFull}, Gender:{_cachedStudent.gender}");
                    }
                    _inStudentProfile = false;
                    RimArchiveMain.StudentDocument.Notify_StudentRecruited(_currentStudent);
                    this.Close();
                    Map currentmap = Find.CurrentMap;
                    IntVec3 intVec3 = DropCellFinder.TradeDropSpot(currentmap);
                    ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                    activeDropPodInfo.innerContainer.TryAddOrTransfer(_cachedStudent as Thing);
                    DropPodUtility.MakeDropPodAt(intVec3, currentmap, activeDropPodInfo);
                    Messages.Message("StudentArrived".Translate(_cachedStudent.NameFullColored), new LookTargets(intVec3, currentmap), MessageTypeDefOf.PositiveEvent);
                }
            }
            /*if (StudentDocument.IsRecruited(_currentStudent))
            {
                if (ButtonImageFitted(recruitBtn, SSR))
                {
                    Messages.Message("StudentAlreadyRecruited".Translate(_cachedStudent.NameFullColored), MessageTypeDefOf.NeutralEvent);
                }
            }
            else if (ButtonImageFitted(recruitBtn, SSR))
            {
                _inStudentProfile = false;
                StudentDocument.Notify_StudentRecruited(ref _currentStudent);
                this.Close();
                Map currentmap = Find.CurrentMap;
                IntVec3 intVec3 = DropCellFinder.TradeDropSpot(currentmap);
                ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                activeDropPodInfo.innerContainer.TryAddOrTransfer(_cachedStudent as Thing);
                DropPodUtility.MakeDropPodAt(intVec3, currentmap, activeDropPodInfo);
                Messages.Message("StudentArrived".Translate(_cachedStudent.NameFullColored), new LookTargets(intVec3, currentmap), MessageTypeDefOf.PositiveEvent);
            }*/
            EndGroup();
        }
        //Show WorkTags
        #endregion

        #region Misc Methods
        static void PostGen(Pawn p)
        {
            try
            {
                //Adjust skills
                foreach (SkillRecord record in p.skills.skills)
                {
                    record.Level = _currentStudent.skills.FirstOrFallback(x => x.skill == record.def).level;
                    record.passion = _currentStudent.skills.FirstOrFallback(x => x.skill == record.def).passion;
                }
                //Remove harmful hediffs or addiction
                //p.health.hediffSet.hediffs = p.health.hediffSet.hediffs.Where(x => !(x.def.isBad || x.def.IsAddiction)).ToList();
                p.health.hediffSet.hediffs.RemoveAll(x=> x.def.isBad || x.def.IsAddiction);
                p.Name = _currentStudent.name;
                PawnBioAndNameGenerator.FillBackstorySlotShuffled(p, BackstorySlot.Childhood, _currentStudent.backstoryFiltersOverride, Faction.OfPlayer.def);
                PawnBioAndNameGenerator.FillBackstorySlotShuffled(p, BackstorySlot.Adulthood, _currentStudent.backstoryFiltersOverride, Faction.OfPlayer.def);
                p.story.headType = _currentStudent.forcedHeadType;
                p.story.hairDef = _currentStudent.forcedHair;
                p.story.bodyType = BodyTypeDefOf.Thin;
                p.story.traits.allTraits.Clear();
                foreach (TraitRequirement trait in _currentStudent.forcedTraits)
                {
                    p.story.traits.GainTrait(new Trait(trait.def, trait.degree ?? 0, true));
                }
                p.apparel.WornApparel.RemoveAll(x => x.def.apparel.bodyPartGroups.Any(t => t == BodyPartGroupDefOf.FullHead || t == BodyPartGroupDefOf.UpperHead));
                p.apparel.LockAll();
                #region Relations
                //处理人际关系
                //逻辑有点乱，首先：
                //这个要双向查找保证不漏，或者说保证任意顺序招募都可以确保关系
                //那么一开始要先一对多，然后多对一
                //还是需要简洁一点的代码，或者说数据结构
                //在StudentDef.Init里加了双向添加的代码，试试看
                List<Pawn> students = Find.CurrentMap.mapPawns.AllPawns.Where(x => x.kindDef is StudentDef).ToList();
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
                //DebugMessage log for backstory. Maybe vanilla cannot recognize har's backstory? but with HAR it should inject into vanilla code, doesn't it?
                //DbgMsg($"Pawn {p.Name}: \nrace:{p.kindDef.race}\n kindDef {p.kindDef},\n backstoryoverride: {string.Join("\n", p.kindDef.backstoryFiltersOverride.First().categories.Select(x => x + "\n"))}");

            }
            catch (Exception ex)
            {
                DebugMessage.DbgErr($"{ex} with {ex.Message}");
            }
        }
        static Texture2D IconforPassion(Passion passion) => passion switch
        {
            Passion.None => BaseContent.ClearTex,
            Passion.Minor => SkillUI.PassionMinorIcon,
            Passion.Major => SkillUI.PassionMajorIcon,
            _ => throw new NullReferenceException("No such Passion Enum")
        };
        #endregion
    }
}
