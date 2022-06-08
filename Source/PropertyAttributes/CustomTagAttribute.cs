using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTagAttribute : PropertyAttribute
{
    public string Tag;

    public CustomTagAttribute(string InTag)
    {
        Tag = InTag;
    }
}
