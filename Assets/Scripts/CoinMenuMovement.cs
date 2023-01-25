using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMenuMovement : MonoBehaviour
{
    private bool hasReachedTop = false;
    private float top;
    private float bottom;
    private float speed = 0.28f;
    private bool movementEnabled = false;

    private void Start()
    {
        top = transform.position.y + 0.15f;
        bottom = transform.position.y - 0.15f;
        StartCoroutine(RandomWait());
    }


    void Update()
    {
        if (movementEnabled)
        {
            if (!hasReachedTop)
            {
                transform.Translate(Vector3.up * speed * Time.deltaTime);
                hasReachedTop = transform.position.y > top;
            }
            else
            {
                transform.Translate(Vector3.down * speed * Time.deltaTime);
                hasReachedTop = transform.position.y > bottom;
            }
        }
    }

    private IEnumerator RandomWait()
    {
        yield return new WaitForSeconds(Random.Range(0.0f, 0.25f));
        movementEnabled = true;
    }
}
