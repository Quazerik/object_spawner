using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Mapper_Editor : EditorWindow
{
    public Mapper_EditorToScene editorToScene;
    public Mapper_SheetToEditor sheetToEditor;

    public Mapper_InfoWindow infoWindow;
    public Mapper_EditorNodeView nodeView;
    public Mapper_EditorNodeBrowser nodeBrowser;

    public static Mapper_Editor instance;

    public List<Mapper_Node> nodes;

    public List<Mapper_EditorNodeData> browserNodes;

    GUIStyle windowStyle;
    GUIStyle titleStyle;
    GUIStyle versionStyle;

    public Mapper_Separator separatorLeft;

    public Mapper_Separator separatorRight;

    public Rect previousPosition;

    public List<string> objects;

    public Vector2 mousePosition = Vector2.zero;

    public void GetObjects()
    {
        string path = "Assets/Resources/Objects/";
        objects = new List<string>();

        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.*");

        foreach (FileInfo f in info)
        {
            if (f.Extension == ".fbx" || f.Extension == ".prefab")
            {
                string tempName = f.Name;
                string extension = f.Extension;
                string strippedName = tempName.Replace(extension, "");
                objects.Add(strippedName);
            }
        }
    }

    public void InitializeNodeObjects()
    {
        browserNodes = new List<Mapper_EditorNodeData>();

        GetObjects();

        foreach (string obj in objects)
        {
            browserNodes.Add(ScriptableObject.CreateInstance<Mapper_EditorNodeData>().Initialize(obj));
        }
    }

    public static bool Init(bool load = false)
    {
        Mapper_Editor objectEditor = (Mapper_Editor)EditorWindow.GetWindow(typeof(Mapper_Editor));
        Mapper_Editor.instance = objectEditor;
        bool loaded = objectEditor.InitializeInstance(load);
        if (!loaded)
            return false;
        return true;
    }

    public bool InitializeInstance(bool load)
    {
        InitializeNodeObjects();

        windowStyle = new GUIStyle(EditorStyles.textField);
        windowStyle.margin = new RectOffset(0, 0, 0, 0);
        windowStyle.padding = new RectOffset(0, 0, 0, 0);

        titleStyle = new GUIStyle(EditorStyles.largeLabel);
        titleStyle.fontSize = 24;

        versionStyle = new GUIStyle(EditorStyles.miniBoldLabel);
        versionStyle.alignment = TextAnchor.MiddleLeft;
        versionStyle.fontSize = 9;
        versionStyle.normal.textColor = Color.gray;
        versionStyle.padding.left = 1;
        versionStyle.padding.top = 1;
        versionStyle.padding.bottom = 1;
        versionStyle.margin.left = 1;
        versionStyle.margin.top = 3;
        versionStyle.margin.bottom = 1;

        this.editorToScene = ScriptableObject.CreateInstance<Mapper_EditorToScene>().Initialize(this);
        this.sheetToEditor = ScriptableObject.CreateInstance<Mapper_SheetToEditor>().Initialize(this);
        this.nodeView = ScriptableObject.CreateInstance<Mapper_EditorNodeView>().Initialize(this);
        this.infoWindow = ScriptableObject.CreateInstance<Mapper_InfoWindow>().Initialize(this);
        this.nodeBrowser = ScriptableObject.CreateInstance<Mapper_EditorNodeBrowser>().Initialize(this);
        this.separatorLeft = ScriptableObject.CreateInstance<Mapper_Separator>();
        this.separatorRight = ScriptableObject.CreateInstance<Mapper_Separator>();

        separatorLeft.rect = new Rect(340, 0, 0, 0);
        separatorRight.rect = new Rect(Screen.width - 130f, 0, 0, 0);

        this.nodes = new List<Mapper_Node>();
        nodes.Add(ScriptableObject.CreateInstance<Mapper_Node_Main>().Initialize(this, new Vector2(32700, 32900), "Main"));
        // nodes.Add(ScriptableObject.CreateInstance<Mapper_Node>().Initialize(this, new Vector2(32768 - 400, 32768 - 300),"Obj1"));
        // nodes.Add(ScriptableObject.CreateInstance<Mapper_Node>().Initialize(this, new Vector2(32400, 32800),"Obj2"));

        this.previousPosition = position;

        if (load)
            sheetToEditor.BuildEditor(nodeView.GetMiddle());

        return true;
    }

    void Update()
    {
        if (focusedWindow == this)
            Repaint();
    }

    void OnGUI()
    {
        if (position != previousPosition)
        {
            previousPosition = position;
        }

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Mapper_Node n = nodes[i];
                n.DrawConnections();
            }

        }

        Rect pRect = new Rect(0, 0, Screen.width, Screen.height);
        pRect.width = separatorLeft.rect.x;
        DrawPreviewPanel(pRect);

        separatorLeft.MinX = 320;
        separatorLeft.MaxX = (int)(Screen.width / 2f - separatorLeft.rect.width);
        separatorLeft.Draw((int)pRect.y, (int)pRect.height);

        pRect.x = separatorLeft.rect.x + separatorLeft.rect.width;
        pRect.width = separatorRight.rect.x - separatorLeft.rect.x - separatorLeft.rect.width;
        pRect.yMin += 19;

        nodeView.OnLocalGUI(pRect);

        separatorRight.MinX = (int)(Screen.width / EditorGUIUtility.pixelsPerPoint) - 150;
        separatorRight.MaxX = (int)(Screen.width / EditorGUIUtility.pixelsPerPoint) - 32;
        separatorRight.Draw((int)pRect.y, (int)pRect.height);

        pRect.x += pRect.width + separatorRight.rect.width;
        pRect.width = (Screen.width / EditorGUIUtility.pixelsPerPoint) - separatorRight.rect.x - separatorRight.rect.width;

        nodeBrowser.OnLocalGUI(pRect);
    }

    public void DrawPreviewPanel(Rect r)
    {
        Rect btnRect = new Rect(r);

        btnRect.y += 4;
        btnRect.height = 17;
        btnRect.width /= 4;
        btnRect.x += btnRect.width;
        btnRect.width *= 2f;

        GUIStyle btnStyle = EditorStyles.miniButton;

        if (GUI.Button(btnRect, "Build!", btnStyle))
        {
            editorToScene.BuildScene();
        }

        infoWindow.OnLocalGUI((int)btnRect.yMax + 4, (int)r.width);
    }
}