using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonColor : MonoBehaviour
{
    private Image imageComp;
    private Color originalColor;

    private void Start()
    {
        imageComp = gameObject.GetComponent<Image>();
        originalColor = imageComp.color;
    }

    public void NoTransparency()
    {
        originalColor.a = 1;
        imageComp.color = originalColor;
    }

    public void Transparency()
    {
        originalColor.a = 150.0f/255.0f;
        imageComp.color = originalColor;
    }
}
