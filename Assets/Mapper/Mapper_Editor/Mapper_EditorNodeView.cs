using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Mapper_ViewArea
{
    private static float editorWindowTabHeight = 19.0f;

    public static void Begin(Rect screenCoordsArea, Vector2 cameraPos)
    {
        GUI.EndGroup();

        editorWindowTabHeight = screenCoordsArea.y;

        GUI.BeginGroup(screenCoordsArea);

        Rect offsetRect = screenCoordsArea;
        offsetRect.x -= cameraPos.x;
        offsetRect.y -= cameraPos.y;
        offsetRect.width = int.MaxValue / 2;
        offsetRect.height = int.MaxValue / 2;
        
        GUI.BeginGroup(offsetRect);
    }

    public static void End()
    {
        GUI.EndGroup();
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0.0f, editorWindowTabHeight, Screen.width, Screen.height));
    }
}

public class Mapper_EditorNodeView : ScriptableObject
{
    public Mapper_Editor editor;
    public Mapper_SelectionInfo selection;

    public Vector2 cameraPos = Vector3.zero;

    bool panCamera = false;

    public Rect rect;

    public Vector2 nodeSpaceMousePos;
    public Vector2 viewSpaceMousePos;

    public Mapper_EditorNodeView Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        selection = ScriptableObject.CreateInstance<Mapper_SelectionInfo>().Initialize(editor);
        rect = new Rect();
        cameraPos = new Vector2(32500, 32500);
        return this;
    }

    public void OnLocalGUI(Rect r)
    {
        // Debug.Log(r.y);
        editor.mousePosition = Event.current.mousePosition;
        rect = r;

        if ((Event.current.rawType == EventType.MouseUp) && (Event.current.button == 0))
        {
            Mapper_NodeConnector.nodeWaitingForConnection = null;
        }

        if (Event.current.type == EventType.Repaint)
        {
            nodeSpaceMousePos = ScreenSpaceToViewSpace(Event.current.mousePosition);
        }

        Mapper_ViewArea.Begin(rect, cameraPos);
        {
            if (Event.current.type == EventType.Repaint)
            {
                viewSpaceMousePos = ViewSpaceToScreenSpace(Event.current.mousePosition);
            }

            if (editor.nodes != null)
            {
                for (int i = editor.nodes.Count - 1; i >= 0; i--)
                {
                    if (!editor.nodes[i].Draw())
                        break;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    for (int i = 0; i < editor.nodes.Count; i++)
                        editor.nodes[i].DrawConnectors();
                }

                // for (int i = 0; i < editor.nodes.Count; i++)
                // {
                //     Mapper_Node n = editor.nodes[i];
                //     n.DrawConnections();
                // }
            }

            UpdateCameraPanning();

        }
        Mapper_ViewArea.End();
    }

    void UpdateCameraPanning()
    {
        if ((Event.current.rawType == EventType.MouseUp) && (Event.current.button == 0))
        {
            panCamera = false;
        }

        bool insideNodeView = MouseInsideNodeView(true);
        bool dragging = (Event.current.type == EventType.MouseDrag && panCamera);
        bool connecting = Mapper_NodeConnector.IsConnecting();

        if (connecting)
        {
            Vector2 mousePosInNodeViewScreenSpace = ViewSpaceToScreenSpace(Event.current.mousePosition) - Vector2.right * editor.separatorLeft.rect.xMax;

            float dragPanMargin = 32f;
            float panSpeed = 0.2f;
            float leftMag = Mathf.Clamp(-mousePosInNodeViewScreenSpace.x + dragPanMargin, 0f, dragPanMargin);
            float rightMag = Mathf.Clamp(mousePosInNodeViewScreenSpace.x - rect.width + dragPanMargin, 0f, dragPanMargin);
            float topMag = Mathf.Clamp(-mousePosInNodeViewScreenSpace.y + dragPanMargin, 0f, dragPanMargin);
            float bottomMag = Mathf.Clamp(mousePosInNodeViewScreenSpace.y - rect.height + dragPanMargin, 0f, dragPanMargin);
            cameraPos += new Vector2(rightMag - leftMag, bottomMag - topMag) * panSpeed;
        }

        bool doingSomethingElse = connecting;
        bool dragInside = dragging && insideNodeView;

        if (dragInside && !doingSomethingElse)
        {
            cameraPos -= Event.current.delta;
            SnapCamera();

            Event.current.Use();
        }

        if ((Event.current.rawType == EventType.MouseUp) && (Event.current.button == 0))
        {
            panCamera = true;
        }
    }

    public Vector2 ViewSpaceToScreenSpace(Vector2 vec)
    {
        Vector2 vecRight = new Vector2(editor.separatorLeft.rect.xMax, editor.separatorLeft.rect.y);
        return (vec - cameraPos + vecRight + new Vector2(rect.x, rect.y));
    }

    public Vector2 ScreenSpaceToViewSpace(Vector2 vec)
    {
        Vector2 vecRight = new Vector2(editor.separatorLeft.rect.xMax, editor.separatorLeft.rect.y);
        return (vec + cameraPos - vecRight - new Vector2(rect.x, rect.y));
    }

    public bool MouseInsideNodeView(bool offset = false)
    {

        if (offset)
        {
            return rect.Contains(viewSpaceMousePos);
        }
        else
        {
            return rect.Contains(Event.current.mousePosition);
        }

    }

    public Vector2 GetNodeSpaceMousePos()
    {
        return nodeSpaceMousePos;
    }

    public Vector2 GetMiddle()
    {
        Vector2 temp = cameraPos;

        // Debug.Log(temp);

        temp.x += rect.width / 2;
        temp.y += rect.height / 2;

        // Debug.Log(temp);

        return temp;
    }

    void SnapCamera()
    {
        cameraPos.x = Mathf.Round(cameraPos.x);
        cameraPos.y = Mathf.Round(cameraPos.y);
    }
}