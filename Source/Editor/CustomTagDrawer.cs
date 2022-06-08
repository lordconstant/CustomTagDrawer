using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[CustomPropertyDrawer(typeof(CustomTagAttribute))]
public class CustomTagDrawer : PropertyDrawer
{
    const string m_pathPrefix = "/GeneratedTags/";
    const string m_fileType = ".txt";

    static Dictionary<string, string[]> m_tags = new Dictionary<string, string[]>();

    int m_selectedIdx = -1;

    NewTagPopup m_addTagPopup = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(m_addTagPopup == null)
        {
            m_addTagPopup = new NewTagPopup();
            m_addTagPopup.OnAddTag += OnAddTag;
        }

        CustomTagAttribute tagAtt = attribute as CustomTagAttribute;

        string[] curTags = null;

        if (!m_tags.TryGetValue(tagAtt.Tag, out curTags))
        {
            curTags = Load(tagAtt.Tag + m_fileType);

            if (curTags == null)
                curTags = new string[1] { "Add New Tag..." };
            else
            {
                for(int i = 0; i < curTags.Length; i++)
                {
                    if (curTags[i] != property.stringValue)
                        continue;

                    m_selectedIdx = i;
                    break;
                }
            }

            m_tags.Add(tagAtt.Tag, curTags);
        }
        else if(m_selectedIdx < 0)
        {
            for (int i = 0; i < curTags.Length; i++)
            {
                if (curTags[i] != property.stringValue)
                    continue;

                m_selectedIdx = i;
                break;
            }
        }

        EditorGUI.BeginProperty(position, label, property);
        {
            string curTag = property.stringValue;

            EditorGUI.LabelField(position, label);
            Rect indentRect = EditorGUI.IndentedRect(position);
            float offset = (position.x - indentRect.x) + EditorGUIUtility.labelWidth;
            position.x += offset;
            position.width -= offset;
            int selectedIdx = EditorGUI.Popup(position, m_selectedIdx, curTags);

            if(selectedIdx == curTags.Length-1)
            {
                PopupWindow.Show(position, m_addTagPopup);
            }
            else if(selectedIdx >= 0 && selectedIdx < curTags.Length)
            {
                m_selectedIdx = selectedIdx;
                property.stringValue = curTags[m_selectedIdx];
            }
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

    public void OnAddTag(string InTag)
    {
        CustomTagAttribute tagAtt = attribute as CustomTagAttribute;

        string[] curTags = m_tags[tagAtt.Tag];

        string[] newTags = new string[curTags.Length + 1];

        for(int i = 0; i < curTags.Length-1; i++)
        {
            newTags[i] = curTags[i];
        }

        newTags[curTags.Length - 1] = InTag;
        newTags[curTags.Length] = "Add New Tag...";

        Save(tagAtt.Tag + m_fileType, newTags);

        m_tags[tagAtt.Tag] = newTags;

        m_selectedIdx = curTags.Length-1;

        m_addTagPopup.editorWindow.Close();
    }

    string[] Load(string InFilename)
    {
        string fullPath = Path.Combine(GetFullPath(), InFilename);
        string[] loadedTags = null;

        if (!Directory.Exists(GetFullPath()))
        {
            return loadedTags;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Read);
        
        if (file.Length > 0)
            loadedTags = bf.Deserialize(file) as string[];
        file.Close();

        return loadedTags;
    }

    void Save(string InFilename, string[] InTags)
    {
        string fullPath = Path.Combine(GetFullPath(), InFilename);

        if (!Directory.Exists(GetFullPath()))
            Directory.CreateDirectory(GetFullPath());

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
        bf.Serialize(file, InTags);
        file.Close();
    }

    string GetFullPath()
    {
        return Application.dataPath + m_pathPrefix;
    }
}

public delegate void OnAddTagClicked(string InTag);

public class NewTagPopup : PopupWindowContent
{
    public string NewTagName;
    public OnAddTagClicked OnAddTag;

    public override Vector2 GetWindowSize()
    {
        return new Vector2(200, 44);
    }

    public override void OnGUI(Rect rect)
    {
        NewTagName = EditorGUILayout.TextField(NewTagName);
        if(GUILayout.Button("Add Tag"))
        {
            OnAddTag.Invoke(NewTagName);
        }
    }
}