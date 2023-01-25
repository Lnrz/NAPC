using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnGhostColor : MonoBehaviour
{
    private Color originalColor;
    private Image imageComp;

    private void Start()
    {
        imageComp = gameObject.GetComponent<Image>();
        originalColor = imageComp.color;
    }

    public void BecomeWhite()
    {
        imageComp.color = Color.white;
    }

    public void RestoreColor()
    {
        imageComp.color = originalColor;
    }
}
