using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NacmanMenu : MonoBehaviour
{
    SpriteRenderer spriteRend;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        spriteRend = collision.gameObject.GetComponent<SpriteRenderer>();

        spriteRend.enabled = false;
    }
}
