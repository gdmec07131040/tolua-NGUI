using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
/// <summary>
/// -------------------------
/// |_______________________|32
/// |     |                 |
/// |     |                 |
/// |     |                 |
/// |     |                 |
/// |_____|_________________|
/// </summary>
internal class UIEditorDepth {
    enum DepthEnum {
        Public = 1000,
        Custom = 10000,
        TextAtlas = 20000,
        Label = 25000,
    }
    enum TypeEnum {
        Public,
        Custom,
        TextAtlas,
        Label,
    }
    string[] labels = new string[4] { "公共", "自定", "文字图集", "文字" };
    GameObject[] selectObj;
    EditorWindow mParent = null;
    Rect mPositon;
    float mUpdateDelay = 0f;
    Rect mHorizontalSplitterRect;
    bool mResizingHorizontalSplitter = false;
    float mHorizontalSplitterPercent;
    const float splitterWidth = 3f;
    Vector2 mScrollPositon = Vector2.zero;
    Vector2 mDimensions = Vector2.zero;
    TypeEnum mType;
    int depth;
    int offset;
    internal UIEditorDepth () {
        mHorizontalSplitterPercent = 0.3f;
    }
    internal void Update () {
        var t = Time.realtimeSinceStartup;
        if (t - mUpdateDelay > 0.1f || mUpdateDelay > t) {
            mUpdateDelay = t - 0.001f;
            mParent.Repaint ();
        }
    }
    internal void OnEnable (Rect pos, EditorWindow parent) {
        mPositon = pos;
        mParent = parent;
        //初始化界面 划分为两块区域
        mHorizontalSplitterRect = new Rect (
            (int) (mPositon.x + mPositon.width * mHorizontalSplitterPercent),
            mPositon.y,
            splitterWidth,
            mPositon.height);
    }
    internal void OnGUI (Rect pos) {
        mPositon = pos;
        //mParent.Repaint ();

        selectObj = Selection.gameObjects;

        if (selectObj.Length == 0) {
            var style = new GUIStyle (GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            GUI.Label (new Rect (mPositon.x, mPositon.y, mPositon.width, mPositon.height - 32f), "请选择UI对象", style);
        } else {

            HandleHorizontalResize ();
            //左边
            var leftPart = new Rect (
                mPositon.x,
                mPositon.y,
                mHorizontalSplitterRect.x,
                mPositon.height
            );
            float panelLeft = mHorizontalSplitterRect.x + splitterWidth;
            float panelWidth = mPositon.width - mHorizontalSplitterRect.x - splitterWidth;
            var rightPart = new Rect (
                panelLeft,
                mPositon.y,
                panelWidth,
                mPositon.height
            );

            DrawOutline (leftPart);
            DrawOutline (rightPart);
            DrawGameObjectList (leftPart);
            TypeToggle (rightPart);

            var style = new GUIStyle (GUI.skin.button);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            bool isClick = GUI.Button (new Rect (rightPart.x + splitterWidth, rightPart.height - 25f, panelWidth - splitterWidth * 2, 20f), "设置层级", style);
            if (isClick) {
                depth = depth + offset;
                SetUISpriteDepth ();
            }
        }

    }
    void TypeToggle (Rect rightPart) {
        switch (mType) {
            case TypeEnum.Public:
            default:
                depth = (int) DepthEnum.Public;
                break;
            case TypeEnum.Custom:
                depth = (int) DepthEnum.Custom;
                break;
            case TypeEnum.TextAtlas:
                depth = (int) DepthEnum.TextAtlas;
                break;
            case TypeEnum.Label:
                depth = (int) DepthEnum.Label;
                break;
        }
        var toolbar = new Rect (rightPart.x + splitterWidth, rightPart.y, rightPart.width - splitterWidth * 2, 20f);

        float toolbarWidth = rightPart.width - 5;
        mType = (TypeEnum) GUI.Toolbar (toolbar, (int) mType, labels);
        var intfield = new Rect (toolbar.x, rightPart.y + toolbar.height + 2f, toolbar.width, 16f);
        offset = EditorGUI.IntField (intfield, "Offset", offset);
    }
    void SetUISpriteDepth () {
        Selection.activeGameObject.GetComponent<UIWidget> ().depth = depth;
    }
    void DrawGameObjectList (Rect fullPos) {

        Rect pos = new Rect (fullPos.x + 1f, fullPos.y + 1, fullPos.width - 2f, fullPos.height - 2f);

        var style = new GUIStyle (GUI.skin.box);
        style.alignment = TextAnchor.MiddleCenter;
        style.wordWrap = false;
        style.normal.textColor = Color.white;
        if (mDimensions.y == 0 || mDimensions.x != pos.width - 16f) {
            //计算高度小于多少显示滑动条
            mDimensions.x = pos.width - 16f;
            mDimensions.y = 0;
            foreach (var item in selectObj) {
                mDimensions.y += style.CalcHeight (new GUIContent (item.name), mDimensions.x);
            }
        }

        mScrollPositon = GUI.BeginScrollView (pos, mScrollPositon, new Rect (0f, 0f, mDimensions.x, mDimensions.y));
        int counter = 0;
        float runningHeight = 2f;
        float height = 0f;
        float width = pos.width;

        foreach (var item in selectObj) {
            var content = new GUIContent (item.name);
            height = style.CalcHeight (content, mDimensions.x);
            counter++;
            GUI.Box (new Rect (2f, runningHeight, mDimensions.x, height), content, style);
            runningHeight += height;
        }
        GUI.EndScrollView ();

    }
    void HandleHorizontalResize () {
        mHorizontalSplitterRect.x = (int) (mPositon.width * mHorizontalSplitterPercent);
        mHorizontalSplitterRect.height = mPositon.height;

        EditorGUIUtility.AddCursorRect (mHorizontalSplitterRect, MouseCursor.ResizeHorizontal);
        if (Event.current.type == EventType.MouseDown && mHorizontalSplitterRect.Contains (Event.current.mousePosition))
            mResizingHorizontalSplitter = true;

        if (mResizingHorizontalSplitter) {
            mHorizontalSplitterPercent = Mathf.Clamp (Event.current.mousePosition.x / mPositon.width, 0.1f, 0.9f);
            mHorizontalSplitterRect.x = (int) (mPositon.width * mHorizontalSplitterPercent);
        }

        if (Event.current.type == EventType.MouseUp)
            mResizingHorizontalSplitter = false;
    }
    internal static void DrawOutline (Rect rect, float size = 1f) {
        Color color = new Color (0.6f, 0.6f, 0.6f, 1.333f);
        if (EditorGUIUtility.isProSkin) {
            color.r = 0.12f;
            color.g = 0.12f;
            color.b = 0.12f;
        }

        if (Event.current.type != EventType.Repaint)
            return;

        Color orgColor = GUI.color;
        GUI.color = GUI.color * color;
        GUI.DrawTexture (new Rect (rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture (new Rect (rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture (new Rect (rect.x, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);
        GUI.DrawTexture (new Rect (rect.xMax - size, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);

        GUI.color = orgColor;
    }
}