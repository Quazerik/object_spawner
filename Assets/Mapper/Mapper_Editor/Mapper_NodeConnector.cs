using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ConType { cChild, cParent };

public class Mapper_NodeConnector : ScriptableObject
{
    public static Mapper_NodeConnector nodeWaitingForConnection = null;
    public ConType conType;
    public Mapper_Node node;
    public Rect rect;
    public List<Mapper_NodeConnectionLine> childLines;
    public List<Mapper_NodeConnectionLine> parentLines;

    public Mapper_NodeConnector Initialize(Mapper_Node node, ConType conType)
    {
        childLines = new List<Mapper_NodeConnectionLine>();
        parentLines = new List<Mapper_NodeConnectionLine>();
        this.node = node;
        this.conType = conType;
        return this;
    }

    public void Draw(Vector2 pos)
    {
        rect = new Rect(pos.x, pos.y, 16, node.rect.height / 2);

        if (conType == ConType.cChild)
        {
            rect.x -= node.rect.width + rect.width;
        }

        GUI.Box(rect, string.Empty);
    }

    public void CheckConnection(Mapper_Editor editor)
    {
        if (conType == ConType.cParent)
        {
            DrawConnection(editor);
        }

        if (Clicked())
        {
            Mapper_NodeConnector.nodeWaitingForConnection = this;
            Event.current.Use();
        }

        if (!ConnectionInProgress())
        {
            if (Released())
                TryMakeConnection();
            return;
        }

        if (Event.current.type == EventType.Repaint && Mapper_NodeConnector.nodeWaitingForConnection != null)
        {
            Color c = Color.black;

            bool input = (conType == ConType.cChild);
            Vector2 start = input ? GetConnectionPoint() : MousePos();
            Vector2 end = input ? MousePos() : GetConnectionPoint(); ;

            DrawTempLine(start, end);
        }
    }

    public Vector2 GetConnectionPoint()
    {
        if (conType == ConType.cParent)
            return new Vector2(rect.xMax, rect.center.y);
        else
            return new Vector2(rect.xMin + 1, rect.center.y);
    }

    public Vector2 MousePos()
    {
        if (node.editor == null)
            return Vector2.zero;
        return node.editor.nodeView.GetNodeSpaceMousePos();
    }

    public bool Hovering(bool world)
    {
        if (!node.editor.nodeView.MouseInsideNodeView(world))
            return false;
        Rect r = GetExpanded(rect, 4);
        return r.Contains(world ? Event.current.mousePosition : MousePos());
    }

    public Rect GetExpanded(Rect r, float px)
    {
        r.y -= px;
        r.x -= px;
        r.width += 2 * px;
        r.height += 2 * px;
        return r;
    }

    public bool Clicked(int button = 0)
    {
        bool hovering = Hovering(false);
        bool click = (Event.current.type == EventType.MouseDown && Event.current.button == button);
        bool clickedCont = hovering && click;
        return clickedCont;
    }

    public bool Released()
    {
        bool cont = Hovering(false);
        bool release = (Event.current.type == EventType.MouseUp);
        return cont && release;
    }

    public bool ConnectionInProgress()
    {
        return (Mapper_NodeConnector.nodeWaitingForConnection == this && IsConnecting());
    }

    public static bool IsConnecting()
    {
        if (Mapper_NodeConnector.nodeWaitingForConnection == null)
            return false;
        return true;
    }

    void DrawConnection(Mapper_Editor editor)
    {
        // foreach (Mapper_NodeConnectionLine line in childLines)
        // {
        //     line.Draw();
        // }

        for (int i = 0; i < childLines.Count; i++)
        {
            childLines[i].Draw();
        }
    }

    public void TryMakeConnection()
    {
        if (Mapper_NodeConnector.nodeWaitingForConnection == null)
            return;

        if (nodeWaitingForConnection == this)
        {
            Mapper_NodeConnector.nodeWaitingForConnection = null;
            return;
        }

        if (nodeWaitingForConnection.conType == this.conType)
        {
            Mapper_NodeConnector.nodeWaitingForConnection = null;
            return;
        }
        LinkTo(Mapper_NodeConnector.nodeWaitingForConnection);

        Mapper_NodeConnector.nodeWaitingForConnection = null;
    }

    public void LinkTo(Mapper_NodeConnector otherNode, ConnectionInfo connectionInfo = null)
    {
        if (conType == ConType.cChild)
        {
            otherNode.LinkTo(this);
            return;
        }

        Mapper_NodeConnectionLine temp = ScriptableObject.CreateInstance<Mapper_NodeConnectionLine>().Initialize(this, otherNode, node.editor, connectionInfo);
        childLines.Add(temp);
        otherNode.parentLines.Add(temp);
    }

    public void DrawTempLine(Vector2 start, Vector2 end)
    {
        Handles.BeginGUI();
        Handles.color = Color.black;

        Vector2 startPoint = node.editor.nodeView.ViewSpaceToScreenSpace(start);
        Vector2 endPoint = node.editor.nodeView.ViewSpaceToScreenSpace(end);

        Handles.DrawLine(startPoint, endPoint);
        Handles.EndGUI();
    }
}