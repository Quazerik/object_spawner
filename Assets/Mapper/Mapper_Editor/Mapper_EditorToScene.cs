using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapper_EditorToScene : ScriptableObject
{
    public GameObject folder;
    public Mapper_Editor editor;
    string objPath = "Objects/";

    public Mapper_EditorToScene Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        return this;
    }

    public void BuildScene()
    {
        GameObject find = GameObject.Find("folder");

        if (find != null)
        {
            DestroyImmediate(find);
        }

        folder = new GameObject("folder");

        Mapper_Node_Main mainNode = null;

        foreach (Mapper_Node node in editor.nodes)
        {
            node.seInfo.ToPlaceOnScene();

            if (node is Mapper_Node_Main)
            {
                mainNode = node as Mapper_Node_Main;
            }
        }

        Mapper_Node objNode = null;

        if (mainNode != null)
            objNode = PlaceObjAsMain(mainNode);
        else
            return;

        ToPlaceObj(objNode);
    }

    public Mapper_Node PlaceObjAsMain(Mapper_Node_Main mainNode)
    {
        Mapper_NodeConnector outputConnector = null;
        Mapper_Node childNode = null;

        foreach (Mapper_NodeConnector con in mainNode.connectors)
        {
            if (con.conType == ConType.cParent)
            {
                outputConnector = con;
            }
        }

        childNode = outputConnector.childLines[0].childNode.node;

        Object prefab = Resources.Load(objPath + childNode.nodeName);
        Vector3 pos = new Vector3((float)mainNode.mainInfo.posInWorldX, (float)mainNode.mainInfo.posInWorldY, (float)mainNode.mainInfo.posInWorldZ);
        GameObject go = (GameObject)Instantiate(prefab, pos, Quaternion.identity, folder.transform);
        go.name = childNode.nodeName;
        go.transform.Rotate(0, Random.Range(0.0f, 360.0f), 0, Space.World);

        childNode.seInfo.isPlaced = true;
        childNode.seInfo.nodeGO = go;

        return childNode;
    }

    public void ToPlaceObj(Mapper_Node objNode)
    {
        Mapper_NodeConnector outputConnector = null;
        Mapper_NodeConnector inputConnector = null;
        Mapper_Node childNode = null;

        foreach (Mapper_NodeConnector con in objNode.connectors)
        {
            if (con.conType == ConType.cParent)
            {
                outputConnector = con;
            }
            if (con.conType == ConType.cChild)
            {
                inputConnector = con;
            }
        }

        for (int i = 0; i < outputConnector.childLines.Count; i++)
        {
            childNode = outputConnector.childLines[i].childNode.node;
            if (objNode.seInfo.isPlaced == true && childNode.seInfo.isPlaced == false)
            {
                PlaceObj(objNode, childNode, outputConnector.childLines[i]);
                ToPlaceObj(childNode);
            }
        }

        if (objNode is Mapper_Node_Object)
        {
            for (int i = 0; i < inputConnector.parentLines.Count; i++)
            {
                childNode = inputConnector.parentLines[i].parentNode.node;
                if (objNode.seInfo.isPlaced == true && childNode.seInfo.isPlaced == false)
                {
                    PlaceObj(objNode, childNode, inputConnector.parentLines[i], true);
                    ToPlaceObj(childNode);
                }
            }
        }
    }

    public void PlaceObj(Mapper_Node pNode, Mapper_Node cNode, Mapper_NodeConnectionLine conLine, bool reverse = false)
    {
        GameObject parentGo = pNode.seInfo.nodeGO;

        Object prefab = Resources.Load(objPath + cNode.nodeName);

        if (prefab == null)
            return;

        GameObject childGO = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, folder.transform);
        childGO.name = cNode.nodeName;
        childGO.transform.Rotate(0, Random.Range(0.0f, 360.0f), 0, Space.World);

        float distance;
        float exactDistance;
        float rotationSide;
        float rotationUp;

        ConnectionInfo conInfo = conLine.connectionInfo;

        if (conInfo.distance == -1)
        {
            distance = Random.Range(0.0f, 2f);
        }
        else
        {
            distance = Random.Range(conInfo.distance - 1f <= 0 ? 0 : conInfo.distance - 0.5f, conInfo.distance + 0.5f);
        }

        if (conInfo.exactDistance >= 0)
        {
            exactDistance = conInfo.exactDistance;
            distance = -1;
        }
        else
        {
            exactDistance = -1;
        }

        if (conInfo.rotationSide == -1)
        {
            rotationSide = Random.Range(0.0f, 360.0f);
        }
        else
        {
            int reverseValue = reverse == false ? 0 : 180;
            rotationSide = Random.Range(conInfo.rotationSide - 25f - reverseValue, conInfo.rotationSide + 25f - reverseValue);
        }

        if (conInfo.rotationUp != -1)
        {
            int reverseValue = reverse == false ? 0 : 180;
            rotationUp = conInfo.rotationUp - reverseValue;
        }

        BoxCollider parentMesh = parentGo.GetComponent<BoxCollider>();
        BoxCollider childMesh = childGO.GetComponent<BoxCollider>();

        float parentRad = (parentMesh.bounds.size.x > parentMesh.bounds.size.z ? 0.7f * parentMesh.bounds.size.x + 0.3f * parentMesh.bounds.size.z : parentMesh.bounds.size.z + 0.3f * parentMesh.bounds.size.x) / 2;
        float childRad = (childMesh.bounds.size.x > childMesh.bounds.size.z ? 0.7f * childMesh.bounds.size.x + 0.3f * childMesh.bounds.size.z : childMesh.bounds.size.z + 0.3f * childMesh.bounds.size.x) / 2;

        if (conInfo.rotationUp == -1)
        {
            if (distance != -1)
                childGO.transform.position = new Vector3(parentGo.transform.position.x + (parentRad + childRad) * (1 + distance * 2), parentGo.transform.position.y, parentGo.transform.position.z);
            else
                childGO.transform.position = new Vector3(parentGo.transform.position.x + (parentRad + childRad) + exactDistance, parentGo.transform.position.y, parentGo.transform.position.z);

            childGO.transform.RotateAround(parentGo.transform.position, Vector3.up, rotationSide);
        }
        else
        {
            float height = parentMesh.bounds.size.y;
            Vector3 vec = new Vector3(parentGo.transform.position.x, parentGo.transform.position.y, parentGo.transform.position.z);

            if (conInfo.rotationUp == 0)
            {
                vec.y -= height;
                childGO.transform.position = vec;
            }
            else if (conInfo.rotationUp == 180)
            {
                vec.y += height;
                childGO.transform.position = vec;
            }
        }

        cNode.seInfo.isPlaced = true;
        cNode.seInfo.nodeGO = childGO;
    }
}