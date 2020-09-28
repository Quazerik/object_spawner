using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Mapper_InfoWindow : ScriptableObject
{
    public Mapper_Editor editor;
    public Mapper_SelectionInfo selection;

    public int maxWidth;

    public Mapper_InfoWindow Initialize(Mapper_Editor editor)
    {
        this.editor = editor;
        selection = editor.nodeView.selection;
        return this;
    }

    public void OnLocalGUI(int yOffset, int in_MaxWidth)
    {
        maxWidth = in_MaxWidth;

        Rect infoRect = new Rect(0f, yOffset, maxWidth, Screen.height / EditorGUIUtility.pixelsPerPoint - yOffset - 20);

        int offset = 0;

        GUI.BeginGroup(infoRect);
        offset = GUISeparator(offset);
        offset = GUISetInfo(offset);
        offset = GUISeparator(offset);
        GUI.EndGroup();
    }

    public int GUISeparator(int yOffset)
    {
        GUI.Box(new Rect(0, yOffset, maxWidth, 1), "", EditorStyles.textField);
        return yOffset + 1;
    }

    public int GUISetInfo(int offset)
    {
        int tempOffset = offset;
        if (selection.mainNode != null)
        {
            Rect tempR = new Rect(0, tempOffset, maxWidth, 17);
            EditorGUI.LabelField(tempR, "Main node is selected!");
            tempR.y += 17;
            tempR.y = GUISeparator((int)tempR.y);
            tempR.y += 5;
            EditorGUI.LabelField(tempR, "Position X:");
            tempR.y += 20;
            selection.mainNode.mainInfo.posInWorldX = Convert.ToDouble(GUI.TextField(tempR, (string)selection.mainNode.mainInfo.posInWorldX.ToString()));
            tempR.y += 20;
            EditorGUI.LabelField(tempR, "Position Y:");
            tempR.y += 20;
            selection.mainNode.mainInfo.posInWorldY = Convert.ToDouble(GUI.TextField(tempR, (string)selection.mainNode.mainInfo.posInWorldY.ToString()));
            tempR.y += 20;
            EditorGUI.LabelField(tempR, "Position Z:");
            tempR.y += 20;
            selection.mainNode.mainInfo.posInWorldZ = Convert.ToDouble(GUI.TextField(tempR, (string)selection.mainNode.mainInfo.posInWorldZ.ToString()));
            tempOffset = GUISeparator((int)tempR.y);
            return tempOffset;
        }
        else if (selection.objectNode != null)
        {
            Rect tempR = new Rect(0, offset, maxWidth, 17);
            EditorGUI.LabelField(tempR, @"Object «" + selection.objectNode.objectInfo.objectName + @"» is selected!");
            tempR.y += 17;
            tempR.y = GUISeparator((int)tempR.y);
            tempR.y += 5;
            EditorGUI.LabelField(tempR, "Description:");
            tempR.y += 20;
            selection.objectNode.objectInfo.description = GUI.TextField(tempR, selection.objectNode.objectInfo.description);
            tempOffset = GUISeparator((int)tempR.y);
            return tempOffset + 17;
        }
        else if (selection.connectionLine != null)
        {
            Rect tempR = new Rect(0, tempOffset, maxWidth, 17);
            EditorGUI.LabelField(tempR, @"Connection between «" + selection.connectionLine.parentNode.node.nodeName + @"» and «" + selection.connectionLine.childNode.node.nodeName + "» is selected!");
            tempR.y += 17;
            tempR.y = GUISeparator((int)tempR.y);
            tempR.y += 5;
            EditorGUI.LabelField(tempR, "Diameter position:");
            tempR.y += 20;
            selection.connectionLine.connectionInfo.distance = Convert.ToInt32(GUI.TextField(tempR, (string)selection.connectionLine.connectionInfo.distance.ToString()));
            tempR.y += 20;
            EditorGUI.LabelField(tempR, "(Higher Priority) Exact position:");
            tempR.y += 20;
            selection.connectionLine.connectionInfo.exactDistance = Convert.ToInt32(GUI.TextField(tempR, (string)selection.connectionLine.connectionInfo.exactDistance.ToString()));
            tempR.y += 20;
            EditorGUI.LabelField(tempR, "Degree side rotation:");
            tempR.y += 20;
            selection.connectionLine.connectionInfo.rotationSide = Convert.ToInt32(GUI.TextField(tempR, (string)selection.connectionLine.connectionInfo.rotationSide.ToString()));
            tempR.y += 20;
            EditorGUI.LabelField(tempR, "Degree up rotation:");
            tempR.y += 20;
            selection.connectionLine.connectionInfo.rotationUp = Convert.ToInt32(GUI.TextField(tempR, (string)selection.connectionLine.connectionInfo.rotationUp.ToString()));
            tempOffset = GUISeparator((int)tempR.y);
            return tempOffset;
        }
        else
        {
            Rect tempR = new Rect(0, offset, maxWidth, 17);
            EditorGUI.LabelField(tempR, "Nothing is selected!");
            return tempOffset + 17;
        }
        // return tempOffset;
    }
}
