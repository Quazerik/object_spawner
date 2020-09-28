using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewableMonoBehaviour : MonoBehaviour
{
    protected static T Create<T>(GameObject gameObject = null) where T : MonoBehaviour
    {
        GameObject obj = gameObject;

        if (obj == null)
            obj = new GameObject(typeof(T).ToString());

        return obj.AddComponent<T>();
    }
}

public class Mapper_Utility : NewableMonoBehaviour
{
    private string path;

    public string GetPath()
    {
        return path;
    }

    public static Mapper_Utility Create(GameObject gameObject = null)
    {
        GameObject find = GameObject.Find("Mapper_Utility");

        if (find != null)
        {
            DestroyImmediate(find);
        }

        Mapper_Utility utility = Create<Mapper_Utility>(gameObject);
        utility.path = Application.dataPath + "/Mapper/result.json";
        return utility;
    }
}