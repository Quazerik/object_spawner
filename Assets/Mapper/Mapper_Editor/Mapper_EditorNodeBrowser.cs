using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper_EditorNodeBrowser : ScriptableObject
{
    public Mapper_Editor editor;

    public Vector2 scrollPos;

    GUIStyle styleButton;

    List<Mapper_EditorNodeData> nodesData;

    bool initializedStyles = false;

    float innerHeight = 256;

    public void CheckInitializeStyles()
    {
        if (initializedStyles && styleButton.fixedHeight == 24)
            return;
        InitializeStyles();
    }

    private void InitializeStyles()
    {
        styleButton = new GUIStyle(GUI.skin.textField);
        styleButton.alignment = TextAnchor.MiddleLeft;
        styleButton.normal.textColor = Color.black;
        styleButton.fixedHeight = 24;
        styleButton.fontSize = 14;
        styleButton.margin.top = 0;
        styleButton.margin.bottom = 0;

        initializedStyles = true;
    }

    public Mapper_EditorNodeBrowser Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        nodesData = editor.browserNodes;
        return this;
    }

    public void OnLocalGUI(Rect rect)
    {
        CheckInitializeStyles();

        Rect panelRect = new Rect(rect);
        Rect scrollRect = new Rect(panelRect);

        scrollRect.height = Mathf.Max(panelRect.height, innerHeight);
        scrollRect.width -= 15;

        Rect btnRect = new Rect(panelRect.x + 4, panelRect.y + 4, rect.width - 20, 24);
        innerHeight = 0;
        float innerStartY = 0f;

        scrollPos = GUI.BeginScrollView(panelRect, scrollPos, scrollRect, false, true);
        {
            foreach (Mapper_EditorNodeData entry in nodesData)
            {
                DrawButton(entry, ref btnRect);
            }

            if (Event.current.type == EventType.Layout)
            {
                innerHeight = btnRect.yMax - innerStartY;
            }
        }

        GUI.EndScrollView();
    }

    public void DrawButton(Mapper_EditorNodeData entry, ref Rect btnRect)
    {
        GUI.color = Color.white;

        bool mouseOver = btnRect.Contains(Event.current.mousePosition);

        if (mouseOver && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            editor.nodes.Add(ScriptableObject.CreateInstance<Mapper_Node_Object>().Initialize(editor, editor.nodeView.GetMiddle(), entry.nodeName));
        }

        GUI.Label(btnRect, entry.nodeName, styleButton);

        GUI.enabled = true;
        btnRect.y += btnRect.height;
    }
}