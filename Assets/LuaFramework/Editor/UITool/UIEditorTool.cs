using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class UIEditorTool : EditorWindow {
    [SerializeField]
    enum Mode {
        Depth,
        Mode2,
        Mode3,
    }
    Mode mMode;
    GameObject[] selectObJ;
    internal UIEditorDepth mUIDepth;
    static UIEditorTool s_instance;
    internal static UIEditorTool instance {
        get {
            if (s_instance == null)
                s_instance = GetWindow<UIEditorTool> ();
            return s_instance;
        }
    }

    [MenuItem ("Window/UIEditorTool")]
    static void ShowWindow () {
        s_instance = null;
        instance.titleContent = new GUIContent ("UIEditorTool");
        instance.Show ();
    }

    /// <summary>
    /// 初始化用
    /// </summary>
    void OnEnable () {
        Rect subPos = GetSubWindowArea ();
        if (mUIDepth == null)
            mUIDepth = new UIEditorDepth ();
        mUIDepth.OnEnable (subPos, this);
    }
    void Update () {
        switch (mMode) {
            case Mode.Depth:
            default:
                mUIDepth.Update ();
                break;
            case Mode.Mode2:
                break;
            case Mode.Mode3:
                break;
        }
    }
    void OnGUI () {
        ModeToggle ();
        //Debug.Log ("mMode = " + mMode);
        switch (mMode) {
            case Mode.Depth:
            default:
                mUIDepth.OnGUI (GetSubWindowArea ());
                break;
            case Mode.Mode2:
                break;
            case Mode.Mode3:
                break;

        }
    }

    void ModeToggle () {
        GUILayout.BeginHorizontal ();
        switch (mMode) {
            case Mode.Depth:
                break;
            case Mode.Mode2:
                break;
            case Mode.Mode3:
                break;
        }

        string[] labels = new string[3] { "层级", "待加", "待加" };
        float toolbarWidth = position.width - 5;
        mMode = (Mode) GUILayout.Toolbar ((int) mMode, labels, "LargeButton", GUILayout.Width (toolbarWidth));
        GUILayout.EndHorizontal ();
    }
    Rect GetSubWindowArea () {
        float padding = 32f;
        Rect subPos = new Rect (0f, padding, position.width, position.height);
        return subPos;
    }
}