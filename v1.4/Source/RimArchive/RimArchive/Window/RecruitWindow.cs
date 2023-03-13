using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static RimArchive.RimArchive;
using static Verse.Widgets;
using System.Text;

namespace RimArchive.Window
{
    [HotSwappable]
    public class RecruitWindow : Verse.Window
    {
        private static readonly float _scrollBarWidth = GenUI.ScrollBarWidth;
        private static readonly float _xMargin = 5f;
        private static readonly float _yMargin = 5f;
        private static readonly Vector2 _iconSize = new Vector2(120f, 120f);
        private static readonly Vector2 _lblSize = new Vector2(80f, 15f);
        private static Vector2 _dlgscrbr = Vector2.zero;
        private static Vector2 _icnscrbr = Vector2.zero;
        private static Vector2 _stdscrbr = Vector2.zero;
        private static string _currentSchool;
        private DiaNode parent;

        private static float _cachedSchoolListHeight;

        internal static void Init()
        {
            _cachedSchoolListHeight = cachedSchools.Count * _iconSize.y;
            _currentSchool = cachedAllStudentsBySchool.Keys.First();
        }


        public RecruitWindow(DiaNode nodeRoot) : base()
        {
            this.parent = nodeRoot;
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
            Rect outRect = new Rect(inRect);
            //GUI.DrawTexture(outRect, BaseContent.BlackTex);
            //Sensei头像
            Rect sensei = new Rect(inRect.position, _iconSize);
            GUI.DrawTexture(sensei, Sensei, ScaleMode.ScaleAndCrop);
            //右侧的对话……栏?
            outRect.x += sensei.width;
            outRect.height = sensei.height;
            outRect.width -= sensei.width;
            DrawDialog(outRect);
            //最右侧列出所有学校
            Rect schoolList = new Rect(inRect.width - _iconSize.x - 6 * _xMargin, inRect.y, _iconSize.x + 4 * _xMargin, inRect.height);
            DrawSchoolList(schoolList);
            //根据当前选择的学校绘制可用学生
            Rect studentsRect = new Rect(sensei.x, sensei.yMax, inRect.xMax - schoolList.width - 4 * _xMargin, inRect.height - sensei.height);
            DrawBox(studentsRect, 1, BaseContent.WhiteTex);
            studentsRect.x += _xMargin;
            studentsRect.y += _yMargin;
            DrawStudentList(studentsRect, cachedAllStudentsBySchool[_currentSchool]);

        }
        static void DrawDialog(Rect outRect)
        {
            outRect.width -= 1050f;
            outRect.height -= 70f;
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
                if (ButtonInvisible(row)) _currentSchool = cachedSchools[i].name;
                //Text.WordWrap = true;
            }
            EndScrollView();
        }

        static void DrawStudentList(Rect outRect, List<PawnKindDef> students)
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
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    //TooltipHandler.TipRegion(rect, students[i].description);
                    BeginGroup(rect);
                    Rect icon = new Rect(0f, 0f, _iconSize.x, _iconSize.y);
                    DrawTextureFitted(icon, BaseContent.GreyTex, 1f);
                    DrawHighlightIfMouseover(icon);
                    icon.y += icon.height;
                    GUI.DrawTexture(icon, BaseContent.BlackTex);

                    //Log.Message($"x:{rect.x}, y:{rect.y}, width:{rect.width}, height:{rect.height}");
                    rect.x += _iconSize.x + _xMargin;
                    if (rect.xMax  > viewRect.width)
                        rect.x = viewRect.x;
                    EndGroup();
                }
                rect.y += _iconSize.y + _yMargin + _lblSize.y;
            }
            EndScrollView();
        }

        static void ResolveTotalHeightAndReturnRowCount(ref Rect viewRect, out int rowCount, out int columnCount)
        {
            int studentCount = cachedAllStudentsBySchool[_currentSchool].Count;
            columnCount = (int)(viewRect.width / (_iconSize.x + _xMargin));
            if (columnCount > studentCount)
                columnCount = studentCount;
            rowCount = Mathf.CeilToInt((float)studentCount / columnCount);
            viewRect.height = (_iconSize.y + _yMargin) * rowCount;
            viewRect.width = (_iconSize.x + _xMargin) * columnCount;
            //Log.Message($"School:{_currentSchool}, {studentCount} people, {rowCount} rows, {columnCount} columns\n viewRect: {viewRect.x}x, {viewRect.y}y, {viewRect.height} height, {viewRect.width} width");
        }
    }
}
