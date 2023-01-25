using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameUI gameUI;
    private ManageEnemies enemyManager;
    private WinScript winScript;
    private float speed = 2.25f;
    private float turnRange = 0.28f;
    private float triggerDistance = 0.5f;
    private float animationLength = 0.2f;
    private float powerupLength = 15.0f;
    private Vector3 direction = Vector3.zero;
    private Vector3 lastDirection = Vector3.zero;
    private bool hasCollided = false;
    private bool hasPowerUp = false;
    private bool isActive = false;
    private bool isDestroyEnabled = false;
    private bool isFreezeEnabled = false;
    private bool isAnimationEnabled = true;
    private bool areCollisionsDisabled = false;
    private int destroyUsesLeft = 3;
    private int skin;
    private bool[] block = { false, false , false, false }; //0 - Right , 1 - Left, 2 - Up, 3 - Down
    private bool[] dBlock = { false, false, false, false };
    private float[,] limits = { {0, 0}, {0, 0}, {0, 0}, {0, 0} };
    private SpriteRenderer spriteRend;
    private Sprite openMouth;
    private Sprite closedMouth;
    private Animator playerAnim;
    [SerializeField] private Texture2D destroyCursor;
    [SerializeField] private Sprite[] openMouths;
    [SerializeField] private Sprite[] closedMouths;

    [SerializeField] private ParticleSystem parts;

    private void Start()
    {
        GameObject gameManager;

        gameManager = GameObject.Find("Game Manager");
        gameUI = gameManager.GetComponent<GameUI>();
        enemyManager = gameManager.GetComponent<ManageEnemies>();
        winScript = gameManager.GetComponent<WinScript>();

        playerAnim = gameObject.GetComponent<Animator>();

        spriteRend = gameObject.GetComponent<SpriteRenderer>();
        skin = PlayerPrefs.GetInt("skin", 0);
        openMouth = openMouths[skin];
        closedMouth = closedMouths[skin];

        ResetCursor();
        StartCoroutine(AnimationClosedMouth());
    }

    void Update()
    {
        if (isActive)
        {
            if (isDestroyEnabled && destroyUsesLeft != 0 && Input.GetMouseButtonDown(0))
            {
                DestroyBlock();
                if (destroyUsesLeft == 0)
                {
                    ResetCursor();
                }
            }
            UpdateLimits();
            ChangeDirection();
            ChangeRotation();
            transform.position += speed * Time.deltaTime * direction;
        }
    }

    private void UpdateLimits()
    {
        for (int i = 0; i < 4; i++)
        {
            if (limits[i, 0] != 0)
            {
                if (i < 2)
                {
                    SetLimits(i, gameObject.transform.position.y);
                }
                else
                {
                    SetLimits(i, gameObject.transform.position.x);
                }
            }
        }
    }

    private void DestroyBlock()
    {
        Vector3 mousePos;
        Collider2D coll;
        GameObject block;
        TagScript tags;

        mousePos = Input.mousePosition;
        mousePos.x = mousePos.x * 28 / Screen.width - 14;
        mousePos.y = mousePos.y * 14 / Screen.height - 7;
        coll = Physics2D.OverlapCircle(mousePos, 0.1f);
        if (coll != null)
        {
            block = coll.gameObject;
            tags = block.GetComponent<TagScript>();
            if (block.CompareTag("Wall") && (tags == null || !tags.IsItTagged("undestroyable")))
            {
                Destroy(block);
                Graph.AddNode(block.transform.position);
                destroyUsesLeft--;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Coin") && collision.Distance(this.GetComponent<Collider2D>()).distance <= -triggerDistance)
        {
            Destroy(collision.gameObject);
            gameUI.AddPoints(5);
            winScript.AddCoin();
        }
        else if (!isFreezeEnabled && !hasPowerUp && collision.gameObject.CompareTag("Powerup") && collision.Distance(this.GetComponent<Collider2D>()).distance <= -triggerDistance)
        {
            Destroy(collision.gameObject);
            hasPowerUp = true;
            enemyManager.PlayerHasPowerup();
            StartCoroutine(PowerUpDuration());
        }
        else if (collision.gameObject.CompareTag("PowerupD") && collision.Distance(this.GetComponent<Collider2D>()).distance <= -triggerDistance)
        {
            Destroy(collision.gameObject);
            isDestroyEnabled = true;
            ChangeCursorToDestroy();
        }
        else if (!hasPowerUp && collision.gameObject.CompareTag("PowerupF") && collision.Distance(this.GetComponent<Collider2D>()).distance <= -triggerDistance)
        {
            Destroy(collision.gameObject);
            isFreezeEnabled = true;
            enemyManager.FreezeEnemies();
            StartCoroutine(FreezeCountdown());
        }
    }

    private IEnumerator FreezeCountdown()
    {
        yield return new WaitForSeconds(15.0f);
        enemyManager.DefreezeEnemies();
        isFreezeEnabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!areCollisionsDisabled)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                Vector2 diff;

                diff = GetCollisionDiff(collision);
                LockDirection(diff);
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                if (!isFreezeEnabled)
                {
                    if (!hasPowerUp || collision.gameObject.GetComponent<Enemy>().HasItAlreadyDied())
                    {
                        enemyManager.DisableEnemies();
                        isAnimationEnabled = false;
                        isActive = false;
                        areCollisionsDisabled = true;
                        playerAnim.enabled = true;
                        playerAnim.SetBool("Death", true);
                        playerAnim.SetInteger("Skin", skin);
                    }
                    else
                    {
                        StartCoroutine(enemyManager.KillEnemy(collision.gameObject));
                        gameUI.AddPoints(15);
                    }
                }
            }
        }
    }

    private void EndGameLoss()
    {
        enemyManager.Lose();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 diff;

            diff = GetCollisionDiff(collision);
            UnlockDirection(diff);
        }
    }

    private void ChangeDirection()
    {
        if (!direction.Equals(Vector3.zero))
        { 
            lastDirection.x = direction.x;
            lastDirection.y = direction.y;
        }
        if (!hasCollided)
        {
            direction = GetDirectionInput();
        }
        else
        {
            direction = Vector3.zero;
            hasCollided = false;
        }
    }

    private Vector3 GetDirectionInput()
    {
        if (RightInput() && !block[0] && IsWithinLimits(0, transform.position.y))
        {
            return Vector3.right;
        }
        else if (LeftInput() && !block[1] && IsWithinLimits(1, transform.position.y))
        {
            return Vector3.left;
        }
        else if (UpInput() && !block[2] && IsWithinLimits(2, transform.position.x))
        {
            return Vector3.up;
        }
        else if (BottomInput() && !block[3] && IsWithinLimits(3, transform.position.x))
        {
            return Vector3.down;
        }
        else
        {
            return direction;
        }
    }

    private bool RightInput()
    {
        return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.Keypad6);
    }

    private bool LeftInput()
    {
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Keypad4);
    }

    private bool UpInput()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Keypad8);
    }

    private bool BottomInput()
    {
        return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Keypad5);
    }

    private void ChangeRotation()
    {
        if (direction.x == 1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (direction.x == -1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (transform.rotation.z == 0)
        {
            if (direction.y == 1)
            {
                transform.Rotate(0, 0, 90);
            }
            else if (direction.y == -1)
            {
                transform.Rotate(0, 0, -90);
            }
        }
        if (direction.y != 0 && direction.y == -lastDirection.y)
        {
            transform.Rotate(0, 180, 0);
        }
    }

    private Vector2 GetCollisionDiff(Collision2D collision)
    {
        Vector2 pos;
        Vector2 collPoint;
        Vector2 diff;

        pos = gameObject.transform.position;
        collPoint = collision.gameObject.transform.position;
        diff = collPoint - pos;

        return diff;
    }

    private void LockDirection(Vector2 diff)
    {
        Vector3 badDirection;

        if (System.Math.Abs(diff.x) > System.Math.Abs(diff.y))
        {
            if (diff.x > 0)
            {
                SwitchBlockOn(0);
                badDirection = Vector3.right;
                ResetLimits(0);
            }
            else
            {
                SwitchBlockOn(1);
                badDirection = Vector3.left;
                ResetLimits(1);
            }
        }
        else
        {
            if (diff.y > 0)
            {
                SwitchBlockOn(2);
                badDirection = Vector3.up;
                ResetLimits(2);
            }
            else
            {
                SwitchBlockOn(3);
                badDirection = Vector3.down;
                ResetLimits(3);
            }
        }
        if (direction.Equals(badDirection))
        {
            hasCollided = true;
        }
    }

    private void UnlockDirection(Vector2 diff)
    {
        if (System.Math.Abs(diff.x) > System.Math.Abs(diff.y))
        {
            if (diff.x > 0)
            {
                SwitchBlockOff(0);
                SetLimits(0, transform.position.y);
            }
            else
            {
                SwitchBlockOff(1);
                SetLimits(1, transform.position.y);
            }
        }
        else
        {
            if (diff.y > 0)
            {
                SwitchBlockOff(2);
                SetLimits(2, transform.position.x);
            }
            else
            {
                SwitchBlockOff(3);
                SetLimits(3, transform.position.x);
            }
        }
    }

    private void SwitchBlockOn(int index)
    {
        if (block[index])
        {
            dBlock[index] = true;
        }
        else
        {
            block[index]= true;
        }
    }

    private void SwitchBlockOff(int index)
    {
        if (dBlock[index])
        {
            dBlock[index] = false;
        }
        else
        {
            block[index] = false;
        }
    }

    private void SetLimits(int index, float position)
    {
        float roundedPos = Mathf.Floor(position);
        
        limits[index, 0] = roundedPos + 0.5f - turnRange / 2;
        limits[index, 1] = roundedPos + 0.5f + turnRange / 2;
    }

    private bool IsWithinLimits(int index, float position)
    {
        if (limits[index, 0] == 0)
        {
            return true;
        }
        return limits[index, 0] <= position && limits[index, 1] >= position;
    }

    private void ResetLimits(int index)
    {
        limits[index, 0] = 0;
        limits[index, 1] = 0;
    }

    private IEnumerator PowerUpDuration()
    {
        yield return new WaitForSeconds(powerupLength);
        enemyManager.PowerupExpired();
        hasPowerUp = false;
    }

    private IEnumerator AnimationOpenMouth()
    {
        if (isAnimationEnabled)
        {
            if (!direction.Equals(Vector3.zero))
            {
                spriteRend.sprite = openMouth;
            }
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(AnimationClosedMouth());
        }
    }

    private IEnumerator AnimationClosedMouth()
    {
        if (isAnimationEnabled)
        {
            spriteRend.sprite = closedMouth;
            yield return new WaitForSeconds(animationLength);
            StartCoroutine(AnimationOpenMouth());
        }
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
    }

    private void ChangeCursorToDestroy()
    {
        Vector2 hotspot;

        hotspot = new Vector2(8, 8);

        Cursor.SetCursor(destroyCursor, hotspot, CursorMode.Auto);
        Cursor.visible = true;
    }

    private void ResetCursor()
    {
        Vector2 hotspot;

        hotspot = Vector2.zero;

        Cursor.SetCursor(null, hotspot, CursorMode.Auto);
    }
}