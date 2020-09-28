using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAndEditorInfo
{
    public bool isPlaced;
    public int depth;

    public GameObject nodeGO;

    public SceneAndEditorInfo(bool isPlaced = false, int depth = -1, GameObject gameObject = null)
    {
        this.isPlaced = isPlaced;
        this.depth = depth;
        this.nodeGO = gameObject;
    }

    public void ToPlaceOnScene()
    {
        isPlaced = false;
    }
}
public class Mapper_Node : ScriptableObject
{
    public SceneAndEditorInfo seInfo;

    public const int NODE_WIDTH = 100;
    public const int NODE_HEIGHT = 100;

    public int node_width = NODE_WIDTH;
    public int node_height = NODE_HEIGHT;

    public Mapper_Editor editor;

    public string nodeName;

    public Rect rect;

    GUIStyle nodeStyle;

    public List<Mapper_NodeConnector> connectors;

    public bool isDragging = false;
    public Vector2 dragStart;
    public Vector2 dragDelta;

    public virtual Mapper_Node Initialize(Mapper_Editor editor, Vector2 pos, string nodeName)
    {
        seInfo = new SceneAndEditorInfo();

        connectors = new List<Mapper_NodeConnector>();
        connectors.Add(ScriptableObject.CreateInstance<Mapper_NodeConnector>().Initialize(this, ConType.cChild));
        connectors.Add(ScriptableObject.CreateInstance<Mapper_NodeConnector>().Initialize(this, ConType.cParent));

        this.nodeName = nodeName;
        this.editor = editor;
        InitializeDefaultRect(pos);
        return this;
    }
    public void InitializeDefaultRect(Vector2 pos)
    {
        this.rect = new Rect(
            pos.x - node_width / 2,
            pos.y - node_height / 2,
            node_width,
            node_height);
    }

    public bool Draw()
    {
        nodeStyle = new GUIStyle((GUIStyle)"flow node 2");
        nodeStyle.alignment = TextAnchor.MiddleCenter;
        nodeStyle.fontSize = 14;
        nodeStyle.fontStyle = FontStyle.Bold;
        nodeStyle.padding.top = 40;
        nodeStyle.padding.left = 4;
        nodeStyle.normal.textColor = new Color(0f, 0f, 0f, 0.5f);

        GUI.Box(rect, nodeName, nodeStyle);

        bool mouseOver = rect.Contains(Event.current.mousePosition);

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && (Event.current.modifiers & EventModifiers.Alt) != 0)
        {
            DeleteNode();
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            OnPress();
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            OnRelease();
        }

        if (isDragging && Event.current.isMouse)
            OnDraggedWindow(Event.current.delta);

        return true;
    }

    public void DrawConnectors()
    {
        if (connectors != null)
        {
            for (int i = 0; i < connectors.Count; i++)
            {
                Vector2 pos = new Vector2(rect.x + rect.width, rect.y + rect.height / 4);
                connectors[i].Draw(pos);
            }
        }
    }

    public void DrawConnections()
    {
        foreach (Mapper_NodeConnector con in connectors)
            con.CheckConnection(editor);
    }

    public bool MouseOverNode()
    {
        if (!editor.nodeView.MouseInsideNodeView(offset: true))
            return false;

        return rect.Contains(Event.current.mousePosition);
    }

    public void OnPress()
    {
        if (MouseOverNode() && Event.current.isMouse)
        {
            StartDragging();
            Event.current.Use();
        }
    }

    public void StartDragging()
    {
        isDragging = true;
        dragStart = new Vector2(rect.x, rect.y);
        dragDelta = Vector2.zero;
    }

    public void OnDraggedWindow(Vector2 delta)
    {
        dragDelta += delta;
        Vector2 finalDelta = new Vector2(rect.x, rect.y);
        rect.x = dragStart.x + dragDelta.x;
        rect.y = dragStart.y + dragDelta.y;
        Event.current.Use();
    }

    public void OnRelease()
    {
        if (MouseOverNode())
        {
            editor.nodeView.selection.Select(this);
        }

        if (isDragging)
        {
            isDragging = false;
            Vector2 tmp = new Vector2(rect.x, rect.y);
            rect.x = dragStart.x;
            rect.y = dragStart.y;
            rect.x = tmp.x;
            rect.y = tmp.y;
        }

        if (Mapper_NodeConnector.nodeWaitingForConnection != null)
            return;

        bool hover = MouseOverNode();
        bool stationary = dragDelta.sqrMagnitude < 7;

        if (hover && stationary)
        {
            if (MouseOverNode())
            {
                editor.nodeView.selection.Select(this);
            }
        }
    }

    public static void LinkNodes(Mapper_Node parentNode, Mapper_Node childNode, ConnectionInfo conInfo = null)
    {
        Mapper_NodeConnector parentOutputNode = null;
        Mapper_NodeConnector childInputNode = null;

        foreach (Mapper_NodeConnector nodeCon in parentNode.connectors)
        {
            if (nodeCon.conType == ConType.cParent)
            {
                parentOutputNode = nodeCon;
            }
        }

        foreach (Mapper_NodeConnector nodeCon in childNode.connectors)
        {
            if (nodeCon.conType == ConType.cChild)
            {
                childInputNode = nodeCon;
            }
        }

        parentOutputNode.LinkTo(childInputNode, conInfo);
    }

    public void DeleteNode()
    {
        if (this is Mapper_Node_Main)
            return;

        if (MouseOverNode() && Event.current.isMouse)
        {
            editor.nodeView.selection.Deselect();

            foreach (Mapper_NodeConnector nodeConnector in this.connectors)
            {
                if (nodeConnector.conType == ConType.cChild)
                {
                    // for (int i = 0; i < nodeConnector.parentLines.Count; i++)
                    //     nodeConnector.parentLines[i].DisconnectLine(true);
                    while (nodeConnector.parentLines.Count > 0)
                        nodeConnector.parentLines[0].DisconnectLine(true);
                }

                if (nodeConnector.conType == ConType.cParent)
                {
                    // for (int i = 0; i < nodeConnector.childLines.Count; i++)
                    //     nodeConnector.childLines[i].DisconnectLine(true);
                    while (nodeConnector.childLines.Count > 0)
                        nodeConnector.childLines[0].DisconnectLine(true);
                }
            }

            // Debug.Log(editor.nodes.Count);
            editor.nodes.Remove(this);
            // Debug.Log(editor.nodes.Count);

            ScriptableObject.DestroyImmediate(this);
        }
    }
}
