using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagScript : MonoBehaviour
{
    [SerializeField] private List<string> tags;

    public bool IsItTagged(string tag)
    {
        if (tags == null)
        {
            return false;
        }
        return tags.Contains(tag);
    }
}
