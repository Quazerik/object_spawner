using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Mapper_Placer : MonoBehaviour
{
    public static void Parser()
    {
        GameObject find = GameObject.Find("folder");

        if (find != null)
        {
            DestroyImmediate(find);
        }

        GameObject folder = new GameObject("folder");

        List<GameObject> objects = new List<GameObject>();

        string objPath = "Objects/";

        string path = Application.dataPath + "/Mapper/result.json";
        string jsonString = File.ReadAllText(path);

        List<Connection> connections = Connection.CreateFromJSON(jsonString);

        foreach (var el in connections)
        {
            if (el.first_object != el.second_object)
            {
                if (!objects.Exists(x => x.name == el.first_object))
                {
                    Object prefab = Resources.Load(objPath + el.first_object);
                    GameObject go = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, folder.transform);
                    go.name = el.first_object;
                    go.transform.Rotate(0, Random.Range(0.0f, 360.0f), 0, Space.World);
                    objects.Add(go);
                }
                if (!objects.Exists(x => x.name == el.second_object))
                {
                    Object prefab = Resources.Load(objPath + el.second_object);
                    GameObject go = (GameObject)Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, folder.transform);
                    go.name = el.second_object;
                    go.transform.Rotate(0, Random.Range(0.0f, 360.0f), 0, Space.World);
                    objects.Add(go);
                }
            }
        }

        List<string> holder = new List<string>();

        Distantion_Placing(objects.Find(x => x.name == connections[0].first_object),
        objects.Find(x => x.name == connections[0].second_object), connections[0]);

        holder.Add(connections[0].first_object);
        holder.Add(connections[0].second_object);

        connections.RemoveAt(0);

        while (connections.Count > 0)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                if (holder.Exists(x => x == connections[i].first_object))
                {
                    Distantion_Placing(objects.Find(x => x.name == connections[i].first_object),
                    objects.Find(x => x.name == connections[i].second_object), connections[i]);

                    holder.Add(connections[i].second_object);

                    connections.RemoveAt(i);

                    break;
                }

                if (holder.Exists(x => x == connections[i].second_object))
                {
                    Distantion_Placing(objects.Find(x => x.name == connections[i].second_object),
                    objects.Find(x => x.name == connections[i].first_object), connections[i]);

                    holder.Add(connections[i].first_object);

                    connections.RemoveAt(i);

                    break;
                }
            }
        }
    }

    private static void Distantion_Placing(GameObject main, GameObject toPlace, Connection conn)
    {
        float distance;
        float rotation_side;
        float rotation_up;
        int bool_is_internal;

        if (conn.distance == -1)
        {
            distance = Random.Range(0.0f, 3f);
        }
        else
        {
            distance = Random.Range(conn.distance - 1f <= 0 ? 0 : conn.distance - 1f, conn.distance + 1f);
        }

        if (conn.rotation_side == -1)
        {
            rotation_side = Random.Range(0.0f, 360.0f);
        }
        else
        {
            rotation_side = Random.Range(conn.rotation_side - 25f, conn.rotation_side + 25f);
        }

        if (conn.rotation_up == -1)
        {
            rotation_up = 0;
        }
        else
        {
            rotation_up = conn.rotation_up;
        }

        bool_is_internal = conn.bool_is_internal;

        BoxCollider mainMesh = main.GetComponent<BoxCollider>();
        BoxCollider toPlaceMesh = toPlace.GetComponent<BoxCollider>();

        float mainRad = (mainMesh.bounds.size.x > mainMesh.bounds.size.z ? 0.7f * mainMesh.bounds.size.x + 0.3f * mainMesh.bounds.size.z : mainMesh.bounds.size.z + 0.3f * mainMesh.bounds.size.x) / 2;
        float toPlaceRad = (toPlaceMesh.bounds.size.x > toPlaceMesh.bounds.size.z ? 0.7f * toPlaceMesh.bounds.size.x + 0.3f * toPlaceMesh.bounds.size.z : toPlaceMesh.bounds.size.z + 0.3f * toPlaceMesh.bounds.size.x) / 2;

        toPlace.transform.position = new Vector3(main.transform.position.x + (mainRad + toPlaceRad) * (1 + distance * 2), main.transform.position.y, main.transform.position.z);
        toPlace.transform.RotateAround(main.transform.position, Vector3.up, rotation_side);
    }
}
