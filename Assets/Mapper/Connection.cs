using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Connection
{
    public string first_object;
    public string fObj_desc;
    public string second_object;
    public string sObj_desc;
    public int distance;
    public int rotation_side;
    public int rotation_up;
    public int bool_is_internal;

    public static List<Connection> CreateFromJSON(string jsonString)
    {
        List<Connection> connections = new List<Connection>();

        string parsingStr = jsonString.Replace("},{", "} {");
        parsingStr = parsingStr.Replace("[", "");
        parsingStr = parsingStr.Replace("]", "");

        string[] arr = parsingStr.Split(' ');

        foreach (var str in arr)
        {
            connections.Add(JsonUtility.FromJson<Connection>(str));
        }

        return connections;
    }
}
