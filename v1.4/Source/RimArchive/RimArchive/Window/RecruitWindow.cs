using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RimArchive.RimArchive;
using static Verse.Widgets;
using System.Text;
using RimArchive.Defs;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace RimArchive.Window
{
    /// <summary>
    /// This is the class that shows the recruit window. Might be more complicated once the main feature is finished. (e.g. Handle all the faction dialog here rather than using vanilla method)
    /// </summary>
    [HotSwappable]
    //目前尚未解决esc弹出菜单的问题。只能先继续沿用DiaNodeTree了
    public class RecruitWindow : Verse.Dialog_NodeTree
    {
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
        private static float _timer = 0f;
        private static float _levelLabelWidth = -1f;
        private static bool _inStudentProfile = false;
        private static bool _clickedSchoolIcon = false;
        private static IconDef _currentSchool;
        private static StudentDef _currentStudent;
        private static Pawn _cachedStudent;
        private DiaNode parent;

        private static float _cachedSchoolListHeight;

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
                _currentStudent = null;
                _cachedStudent = null;
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
                outRect.x += sensei.width;
                outRect.height = sensei.height;
                outRect.width = inRect.width / 3;
                DrawDialog(outRect);
                Rect studentsRect = new Rect(sensei.x, sensei.yMax, inRect.width, inRect.height - sensei.height);
                //加个if，不多画窗口了
                //这么搞貌似会导致内存溢出？？？明天得重写一下了
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
                    Rect skillRect = new Rect(studentsRect);
                    skillRect.xMin = hallRect.xMax;
                    skillRect.xMax = studentsRect.xMax;
                    Rect abilityRect = new Rect(studentsRect);
                    abilityRect.xMax = hallRect.x;
                    /*hallRect.size = studentsRect.size / 4 * 3;
                    hallRect = hallRect.CenteredOnXIn(studentsRect);
                    hallRect = hallRect.CenteredOnYIn(studentsRect);*/
                    GUI.DrawTexture(outRect, BaseContent.YellowTex);
                    //DrawProfile(outRect);
                    //备选项：
                    GUI.DrawTexture(hallRect, _currentStudent.Memorial, ScaleMode.ScaleToFit, true, 0, Color.white, 0, 10);
                    //GUI.DrawTexture(hallRect, _currentStudent.Memorial);
                    //DrawMemorialHall(hallRect);
                    //GUI.DrawTexture(abilityRect, BaseContent.BlackTex);
                    DrawBox(abilityRect, 1, BaseContent.WhiteTex);
                    DrawSkillAndTrait(abilityRect);
                    //GUI.DrawTexture(skillRect, BaseContent.WhiteTex);
                    DrawAbility(skillRect);
                }
            }
            catch (Exception ex)
            {
                this.Close();
                Log.Error($"Exception when drawing GUI: {ex}");
            }

        }

        #region GUISCHOOL
        static void DrawDialog(Rect outRect)
        {
            //outRect.width -= 1050f;
            //outRect.height -= 70f;
            GUI.DrawTexture(outRect, BaseContent.GreyTex);
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
                if (Mouse.IsOver(row) || ButtonInvisible(row))
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
                                canGeneratePawnRelations: false,
                                mustBeCapableOfViolence: true,
                                colonistRelationChanceFactor: 0f,
                                allowGay: false,
                                allowAddictions: false,
                                fixedGender: Gender.Female
                                ));
                            PostGen(ref _cachedStudent);
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

        }

        static void DrawProfile(Rect outRect)
        {

        }


        private static Vector2 _traitscrbr;
        private static Vector2 _storyscrbr;
        void DrawSkillAndTrait(Rect outRect)
        {
            Verse.Text.Font = GameFont.Small;
            BeginGroup(outRect);
            Rect skillRect = new Rect(outRect.AtZero().ContractedBy(_Margin.x));
            //GUI.DrawTexture(skillRect, BaseContent.WhiteTex);
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
            //Rect skillRectEach = new Rect(skillRect.ContractedBy(_Margin.x));
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
                FillableBar(skillbar, (float)skill.Level / SkillRecord.MaxLevel, BaseContent.GreyTex, BaseContent.BlackTex, true);
                DrawTextureFitted(passionIcon, getIconforPassion(skill.passion), 1f);
                LabelFit(label, string.Concat(skill.def.LabelCap,"   ",skill.Level.ToString()));
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
            Rect inRect = outRect.ContractedBy(_Margin.x);
            //Show Traits
            /*foreach(TraitRequirement outtrait in _currentStudent.forcedTraits)
            {

            }*/
            EndGroup();
        }
        //Show WorkTags
        #endregion

        #region Misc Methods
        static void PostGen(ref Pawn p)
        {
            //Adjust skills
            foreach (SkillRecord record in p.skills.skills)
            {
                record.Level = _currentStudent.skills.Where(x => x.skill == record.def).First().level;
                record.passion = _currentStudent.skills.Where(x => x.skill == record.def).First().passion;
            }
            //Remove harmful hediffs or addiction
            p.health.hediffSet.hediffs = p.health.hediffSet.hediffs.Where(x => !(x.def.isBad || x.def.IsAddiction)).ToList();
            p.Name = _currentStudent.name;
        }
        static Texture2D getIconforPassion(Passion passion)
        {
            switch (passion)
            {
                case Passion.None:
                    return BaseContent.ClearTex;
                case Passion.Minor:
                    return SkillUI.PassionMinorIcon;
                case Passion.Major:
                    return SkillUI.PassionMajorIcon;
                default:
                    throw new NullReferenceException("No such Passion Enum");
            }
        }


        #endregion
    }
}
