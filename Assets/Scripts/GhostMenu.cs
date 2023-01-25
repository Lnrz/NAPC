using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMenu : MonoBehaviour
{
    SpriteRenderer spriteRend;

    private void OnTriggerExit2D(Collider2D collision)
    {
        spriteRend = collision.gameObject.GetComponent<SpriteRenderer>();

        spriteRend.enabled = true;
    }
}
