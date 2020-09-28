using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ConnectionInfo
{
    public int distance;
    public int exactDistance;
    public int rotationSide;
    public int rotationUp;
    public ConnectionInfo(int distance = -1, int exactDistance = -1, int rotationSide = -1, int rotationUp = -1)
    {
        this.distance = distance;
        this.exactDistance = exactDistance;
        this.rotationSide = rotationSide;
        this.rotationUp = rotationUp;
    }
}

public class Mapper_NodeConnectionLine : ScriptableObject
{
    const int BOX_SIZE = 14;
    public ConnectionInfo connectionInfo;
    public Mapper_Editor editor;
    public Mapper_NodeConnector parentNode;
    public Mapper_NodeConnector childNode;
    public Rect rect;

    public Mapper_NodeConnectionLine Initialize(Mapper_NodeConnector parentNode, Mapper_NodeConnector childNode, Mapper_Editor editor, ConnectionInfo connectionInfo = null)
    {
        this.editor = editor;
        this.parentNode = parentNode;
        this.childNode = childNode;

        if (connectionInfo == null)
            this.connectionInfo = new ConnectionInfo();
        else
            this.connectionInfo = connectionInfo;

        return this;
    }

    public void Draw()
    {
        Handles.BeginGUI();
        Handles.color = Color.black;

        Vector2 startPoint = parentNode.GetConnectionPoint();
        Vector2 endPoint = childNode.GetConnectionPoint();

        startPoint = editor.nodeView.ViewSpaceToScreenSpace(startPoint);
        endPoint = editor.nodeView.ViewSpaceToScreenSpace(endPoint);

        // Debug.Log("Line pos: " + startPoint + " " + endPoint);

        Handles.DrawLine(startPoint, endPoint);
        Handles.EndGUI();

        Vector2 middlePoint = new Vector2((startPoint.x + endPoint.x) / 2, (startPoint.y + endPoint.y) / 2);

        rect = new Rect(middlePoint.x - BOX_SIZE / 2, middlePoint.y - BOX_SIZE / 2, BOX_SIZE, BOX_SIZE);

        GUIStyle conNodeStyle = new GUIStyle((GUIStyle)"flow node 2");
        GUI.Box(rect, string.Empty, conNodeStyle);

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && (Event.current.modifiers & EventModifiers.Alt) != 0)
        {
            DisconnectLine();
            return;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            ClickedOnSmallBox();
        }
    }

    public void DrawBox()
    {
        GUIStyle conNodeStyle = new GUIStyle((GUIStyle)"flow node 2");

        Vector2 startPoint = parentNode.GetConnectionPoint();
        Vector2 endPoint = childNode.GetConnectionPoint();
        Vector2 middlePoint = new Vector2((startPoint.x + endPoint.x) / 2, (startPoint.y + endPoint.y) / 2);

        rect = new Rect(middlePoint.x - BOX_SIZE / 2, middlePoint.y - BOX_SIZE / 2, BOX_SIZE, BOX_SIZE);

        GUI.Box(rect, string.Empty, conNodeStyle);

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && (Event.current.modifiers & EventModifiers.Alt) != 0)
        {
            DisconnectLine();
            return;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            ClickedOnSmallBox();
        }
    }

    public void ClickedOnSmallBox()
    {
        if (MouseOverConLine())
        {
            editor.nodeView.selection.Select(this);
        }
    }

    public bool MouseOverConLine()
    {
        if (!editor.nodeView.MouseInsideNodeView(offset: true))
            return false;

        return rect.Contains(Event.current.mousePosition);
    }

    public void DisconnectLine(bool need = false)
    {
        if (need || MouseOverConLine())
        {
            editor.nodeView.selection.Deselect();

            // Debug.Log("Destroying");

            parentNode.childLines.Remove(this);
            childNode.parentLines.Remove(this);

            ScriptableObject.DestroyImmediate(this);
        }
    }
}