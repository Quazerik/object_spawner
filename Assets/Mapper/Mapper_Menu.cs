using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mapper
{
    public class Mapper_Menu : EditorWindow
    {
        private string path = "Assets/Resources/Objects/";
        private List<string> objects = new List<string>();
        private string textToProcess = "Input Your Text!";

        [MenuItem("Mapper/Text Processing Tool")]
        private static void ShowWindow()
        {
            var window = GetWindow<Mapper_Menu>();
            window.titleContent = new GUIContent("Editor");
            window.Show();
        }

        [MenuItem("Mapper/Open Editor")]
        private static void OpenEditor()
        {
            Mapper_Editor.Init();
        }

        private void OnGUI()
        {

            if (GUILayout.Button("Get objects!"))
            {
                objects.Clear();

                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] info = dir.GetFiles("*.*");

                foreach (FileInfo f in info)
                {
                    if (f.Extension == ".fbx" || f.Extension == ".prefab")
                    {
                        string tempName = f.Name;
                        string extension = f.Extension;
                        string strippedName = tempName.Replace(extension, "");
                        objects.Add(strippedName);
                    }
                }

                // foreach (var obj in objects)
                // {
                //     Debug.Log(obj);
                // }
            }

            textToProcess = GUILayout.TextArea(textToProcess);

            if (GUILayout.Button("Process!"))
            {
                Processer.SendSpacy(ObjectsToStr(), textToProcess);
                Debug.Log("Processed!");
            }

            if (GUILayout.Button("Spawning!"))
            {
                Mapper_Placer.Parser();
                Debug.Log("Spawned!");
            }

            if (GUILayout.Button("Open Mapper Editor!"))
            {
                Mapper_Editor.Init(true);
            }
        }

        private string ObjectsToStr()
        {
            string toSend = "";

            foreach (string obj in objects)
            {
                toSend = toSend + obj + " ";
            }

            toSend = toSend.Remove(toSend.Length - 1);

            //Debug.Log(toSend);

            return toSend;
        }

        private Object[] LoadObject()
        {
            Object[] loadedObj;

            loadedObj = Resources.LoadAll("Objects", typeof(Mesh));

            foreach (var obj in loadedObj)
            {
                Mesh t = (Mesh)obj;
                Bounds bounds = t.bounds;
                Debug.Log(obj.name + " " + bounds.size.z + " " + bounds.size.y);
            }

            return loadedObj;
        }
    }
}