using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainInfo
{
    public double posInWorldX;
    public double posInWorldY;
    public double posInWorldZ;

    public MainInfo(double x, double y, double z)
    {
        posInWorldX = x;
        posInWorldY = y;
        posInWorldZ = z;
    }
}

public class Mapper_Node_Main : Mapper_Node
{
    public MainInfo mainInfo;
    public override Mapper_Node Initialize(Mapper_Editor editor, Vector2 pos, string nodeName)
    {
        seInfo = new SceneAndEditorInfo();

        connectors = new List<Mapper_NodeConnector>();
        connectors.Add(ScriptableObject.CreateInstance<Mapper_NodeConnector>().Initialize(this, ConType.cParent));

        this.nodeName = nodeName;
        this.editor = editor;
        InitializeDefaultRect(pos);

        mainInfo = new MainInfo(0, 0, 0);

        return this;
    }
}
