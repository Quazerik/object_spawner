using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class Mapper_SheetToEditor : ScriptableObject
{
    public Mapper_Editor editor;
    string path;
    string jsonString;

    public Mapper_SheetToEditor Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        return this;
    }

    public void BuildEditor(Vector2 startPoint)
    {
        foreach (Mapper_Node node in editor.nodes)
        {
            if (node is Mapper_Node_Main)
                node.seInfo.depth = 0;
        }
        Mapper_Utility utility = Mapper_Utility.Create();

        path = utility.GetPath();
        jsonString = File.ReadAllText(path);
        List<Connection> connections = Connection.CreateFromJSON(jsonString);

        foreach (Connection con in connections)
        {
            if (!editor.nodes.Exists(x => x.nodeName == con.first_object))
            {
                editor.nodes.Add(ScriptableObject.CreateInstance<Mapper_Node_Object>().Initialize(editor, Vector2.zero, con.first_object));
            }
            if (!editor.nodes.Exists(x => x.nodeName == con.second_object))
            {
                editor.nodes.Add(ScriptableObject.CreateInstance<Mapper_Node_Object>().Initialize(editor, Vector2.zero, con.second_object));
            }
        }

        if (connections.Count > 0)
        {
            foreach (Mapper_Node node in editor.nodes)
            {
                if (node is Mapper_Node_Main)
                {
                    Mapper_Node objNode = editor.nodes.Find(x => x.nodeName == connections[0].first_object);
                    objNode.seInfo.depth = 1;
                    Mapper_Node.LinkNodes(node, objNode);
                    break;
                }
            }
        }

        List<Connection> temp = new List<Connection>(connections);

        //Set Depth
        while (temp.Count > 0)
        {
            List<Connection> removedCon = new List<Connection>();

            temp.RemoveAll(x =>
            {
                if ((editor.nodes.Find(n1 => n1.nodeName == x.first_object).seInfo.depth == -1 && editor.nodes.Find(n1 => n1.nodeName == x.second_object).seInfo.depth != -1) ||
                       (editor.nodes.Find(n1 => n1.nodeName == x.first_object).seInfo.depth != -1 && editor.nodes.Find(n1 => n1.nodeName == x.second_object).seInfo.depth == -1))
                {
                    removedCon.Add(x);
                    return true;
                }

                return false;
            });

            if (removedCon.Count == 0)
            {
                Mapper_Node node = editor.nodes.Find(x => x.nodeName == temp[0].first_object);
                node.seInfo.depth = 0;
            }

            foreach (Connection con in removedCon)
            {
                Mapper_Node first = editor.nodes.Find(x => x.nodeName == con.first_object);
                Mapper_Node second = editor.nodes.Find(x => x.nodeName == con.second_object);

                if (first.seInfo.depth != -1)
                    second.seInfo.depth = first.seInfo.depth + 1;
                else
                    first.seInfo.depth = second.seInfo.depth - 1;
            }
        }

        //Set Connection
        foreach (Connection con in connections)
        {
            Mapper_Node first = editor.nodes.Find(x => x.nodeName == con.first_object);
            Mapper_Node second = editor.nodes.Find(x => x.nodeName == con.second_object);

            ConnectionInfo conInfo = new ConnectionInfo(con.distance, -1, con.rotation_side, con.rotation_up);

            Mapper_Node.LinkNodes(first, second, conInfo);
        }

        //Set Position
        List<Mapper_Node> orderedNodes = editor.nodes.OrderBy(x => x.seInfo.depth).ToList();
        int column = orderedNodes[0].seInfo.depth;
        int row = 0;
        foreach (Mapper_Node node in orderedNodes)
        {
            if (column < node.seInfo.depth)
            {
                column = node.seInfo.depth;
                row = 0;
            }

            node.rect.x = startPoint.x + column * (node.node_width + 100);
            node.rect.y = startPoint.y + row * (node.node_height + 100);

            row++;
        }
    }
}