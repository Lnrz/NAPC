using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursorHider : MonoBehaviour
{
    private float waitBeforeHiding = 3.5f;
    private bool isCoroutineActive = false;
    private Coroutine coroutine;

    void Update()
    {
        if (Cursor.visible)
        {
            if (MouseIsNotMoving())
            {
                if (!isCoroutineActive)
                {
                    coroutine = StartCoroutine(HideMouseCursor());
                    isCoroutineActive = true;
                }
            }
            else
            {
                if (isCoroutineActive)
                {
                    StopCoroutine(coroutine);
                    isCoroutineActive = false;
                }
            }
        }
        else if (!MouseIsNotMoving())
        {
            Cursor.visible = true;
            isCoroutineActive = false;
        }
    }

    private bool MouseIsNotMoving()
    {
        return Input.GetAxis("Mouse X") == 0 && Input.GetAxis("Mouse Y") == 0;
    }

    private IEnumerator HideMouseCursor()
    {
        yield return new WaitForSeconds(waitBeforeHiding);
        Cursor.visible = false;
    }
}
