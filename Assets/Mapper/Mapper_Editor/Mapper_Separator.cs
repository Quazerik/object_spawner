using UnityEngine;
using UnityEditor;
using System.Collections;

public class Mapper_Separator : ScriptableObject
{
    public Rect rect;
    int minX;
    public int MinX
    {
        get
        {
            return minX;
        }
        set
        {
            minX = value;
            ClampX();
        }
    }

    int maxX;
    public int MaxX
    {
        get
        {
            return maxX;
        }
        set
        {
            maxX = value;
            ClampX();
        }
    }

    public void Draw(int yPos, int height)
    {
        rect.y = yPos;
        rect.height = height;
        rect.width = 7;

        GUI.Box(rect, "", EditorStyles.textField);
    }

    void ClampX()
    {
        rect.x = Mathf.Clamp(rect.x, minX, maxX);
    }
}