using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper_SelectionInfo : ScriptableObject
{
    public Mapper_Editor editor;
    public Mapper_Node_Main mainNode;
    public Mapper_Node_Object objectNode;
    public Mapper_NodeConnectionLine connectionLine;

    public Mapper_SelectionInfo Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        mainNode = null;
        objectNode = null;
        connectionLine = null;

        return this;
    }

    public void Deselect()
    {
        mainNode = null;
        objectNode = null;
        connectionLine = null;
    }

    public void Select(Mapper_NodeConnectionLine connectionLine)
    {
        Deselect();
        this.connectionLine = connectionLine;
    }

    public void Select(Mapper_Node node)
    {
        // Debug.Log("Node selected!");
        Deselect();
        if (node is Mapper_Node_Main)
            mainNode = node as Mapper_Node_Main;
        else
            objectNode = node as Mapper_Node_Object;
    }
}