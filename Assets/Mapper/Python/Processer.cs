using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;
using Python.Runtime;

public class Processer
{
    const string ClientName = "com.unity.scripting.python.clients.processer";

    // [MenuItem("Mapper/Python/Client Name")]
    public static void SendClientName()
    {
        Debug.Log("running 'client_name'");
        using (Py.GIL())
        {
            dynamic x = PythonRunner.CallServiceOnClient(ClientName, "client_name");
            Debug.Log($"ran 'client_name' and got {x}");
        }
    }

    public static void SendSpacy(string obj, string text)
    {
        Debug.Log("running 'Spacy'");
        using (Py.GIL())
        {
            PythonRunner.CallServiceOnClient(ClientName, "try_spacy", obj, text);
        }
    }

    [MenuItem("Mapper/Activate API")]
    public static void SpawnClient()
    {
        PythonRunner.SpawnClient("Assets/Mapper/Python/processer.py",
                wantLogging: true);
    }
}
