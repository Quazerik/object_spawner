using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper_EditorNodeData : ScriptableObject
{
    public string nodeName;

    public Mapper_EditorNodeData Initialize(string nodeName)
    {
        this.nodeName = nodeName;
        return this;
    }
}