using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInfo
{
    public string objectName;
    public string description;

    public ObjectInfo(string objN, string desc = "")
    {
        objectName = objN;
        description = desc;
    }
}

public class Mapper_Node_Object : Mapper_Node
{
    public ObjectInfo objectInfo;
    public Mapper_Node Initialize(Mapper_Editor editor, Vector2 pos, string nodeName, ObjectInfo objectInfo = null)
    {
        seInfo = new SceneAndEditorInfo();

        connectors = new List<Mapper_NodeConnector>();
        connectors.Add(ScriptableObject.CreateInstance<Mapper_NodeConnector>().Initialize(this, ConType.cChild));
        connectors.Add(ScriptableObject.CreateInstance<Mapper_NodeConnector>().Initialize(this, ConType.cParent));

        this.nodeName = nodeName;
        this.editor = editor;
        InitializeDefaultRect(pos);

        if (objectInfo == null)
        {
            this.objectInfo = new ObjectInfo(nodeName);
        }
        else
        {
            this.objectInfo = objectInfo;
        }

        return this;
    }
}
