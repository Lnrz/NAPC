using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour
{
    private float xPos = 0;
    private float yPos = 0;
    private Vector3 pos = new Vector3();

    void Update()
    {
        if (xPos != gameObject.transform.position.x)
        {
            xPos = gameObject.transform.position.x;
            yPos = gameObject.transform.position.y;
            xPos = Mathf.Floor(xPos) + 0.5f;
            yPos = Mathf.Floor(yPos) + 0.5f;
            pos.Set(xPos, yPos, gameObject.transform.position.z);
            gameObject.transform.position = pos;
        }
    }
}