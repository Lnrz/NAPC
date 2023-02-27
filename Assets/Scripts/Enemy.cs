using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private AudioManager audioManager;
    private float speed = 2.0f;
    private float turnRange = 0.005f;
    private float animationLength = 0.2f;
    private Vector3 direction = Vector3.zero;
    private Vector3 lastDirection = Vector3.zero;
    private bool isActive = false;
    private bool firstTime = true;
    private bool hasCollided = false;
    private bool useAltSprite = false;
    private bool isBlinking = false;
    private bool hasExitedFromSpawn = false;
    private bool isFrozen = false;
    private bool isBehaviourRandom = true;
    private bool isChangeBehaviourActive = false;
    private bool isRouteValid = false;
    private bool pursueIsDirectionValid = false;
    private bool forcedRouteUpdate = false;
    private bool isFleeingEnabled = false;
    private bool hasAlreadyDied = false;
    private int pursueIndex = 0;
    private IList<Vector2> route;
    private Coroutine changeBehaviourCoroutine;
    private bool[] block = { false, false, false, false }; //0 - Right , 1 - Left, 2 - Up, 3 - Down
    private bool[] dBlock = { false, false, false, false };
    private bool[] freeOn = { false, false, false, false };
    private float[,] limits = { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
    private ICollection<Collider2D> ghostWallColliders = new LinkedList<Collider2D>();
    private SpriteRenderer spriteRend;
    [SerializeField] private Sprite firstSprite;
    [SerializeField] private Sprite secondSprite;
    [SerializeField] private Sprite firstAltSprite;
    [SerializeField] private Sprite secondAltSprite;
    [SerializeField] private Sprite frozenSprite;
    [SerializeField] private Sprite frozenSpriteSecond;
    [SerializeField] private Sprite frozenSpriteThird;
    [SerializeField] private SpriteRenderer toppingRend;
    [SerializeField] private GameObject player;
    [SerializeField] private float spawnBoxXMAX = 3.5f;
    [SerializeField] private float spawnBoxXmin = -2.5f;
    [SerializeField] private float spawnBoxYMAX = 2.5f;
    [SerializeField] private float spawnBoxYmin = -1.5f;

    private void Start()
    {
        GameObject audioManagerObj;

        audioManagerObj = GameObject.Find("AudioManager");
        audioManager = audioManagerObj.GetComponent<AudioManager>();

        spriteRend = GetComponent<SpriteRenderer>();

        StartCoroutine(AnimationOne());
    }

    void Update()
    {
        if (isActive && !isFrozen)
        {
            if (firstTime)
            {
                direction = ChooseDirection();
                firstTime = false;
            }
            if (!hasExitedFromSpawn && CheckSpawnZone())
            {
                hasExitedFromSpawn = true;
                foreach (Collider2D coll in ghostWallColliders)
                {
                    Physics2D.IgnoreCollision(coll, gameObject.GetComponent<Collider2D>(), false);
                }
                ghostWallColliders = new LinkedList<Collider2D>();
                if (!isChangeBehaviourActive)
                {
                    changeBehaviourCoroutine = StartCoroutine(ChangeBehaviour());
                    isChangeBehaviourActive = true;
                }
            }
            if (isFleeingEnabled)
            {
                Flee();
            }
            else if (isBehaviourRandom)
            {
                ChangeDirection();
            }
            else
            {
                Pursue();
            }
            lastDirection.Set(direction.x, direction.y, 0);
            transform.position += speed * Time.deltaTime * direction;
        }
    }

    public void EnableFlee()
    {
        isFleeingEnabled = true;
    }

    public void DisableFlee()
    {
        isFleeingEnabled = false;
    }

    private void Flee()
    {
        Vector2 dist;

        dist = player.transform.position - gameObject.transform.position;
        if (dist.magnitude <= 1.3f)
        {
            Vector2 goodDir;

            goodDir = CalculateGoodDirection(dist);
            if (!IsItAlreadyGoingInAGoodDirection(goodDir) || !IsDirectionValid() || IsItPossibleToTurn(goodDir))
            {
                ChangeDirectionToGoodDirection(goodDir);
            }
        }
        else
        {
            ChangeDirection();
        }
    }

    private bool IsItPossibleToTurn(Vector2 goodDir)
    {
        if (goodDir.x == 1)
        {
            if (goodDir.y == 1)
            {
                return IsItPossibleToTurn(0, 2);
            }
            else if (goodDir.y == -1)
            {
                return IsItPossibleToTurn(0, 3);
            }
            else
            {
                return IsItPossibleToTurn(0, 2, 3, (int)lastDirection.y);
            }
        }
        else if (goodDir.x == -1)
        {
            if (goodDir.y == 1)
            {
                return IsItPossibleToTurn(1, 2);
            }
            else if (goodDir.y == -1)
            {
                return IsItPossibleToTurn(1, 3);
            }
            else
            {
                return IsItPossibleToTurn(1, 2, 3, (int)lastDirection.y);
            }
        }
        else
        {
            if (goodDir.y == 1)
            {
                return IsItPossibleToTurn(2, 0, 1, (int)lastDirection.x);
            }
            else if (goodDir.y == -1)
            {
                return IsItPossibleToTurn(3, 0, 1, (int)lastDirection.x);
            }
            else
            {
                return false;
                //Impossible, it would require the overlap of nacman and the ghost
            }
        }
    }

    //second = up - right
    //secondOp = bottom - left
    private bool IsItPossibleToTurn(int first, int second, int secondOp, int lastDirectionMem)
    {
        int num;

        num = 0;
        if (!block[first] && IsWithinLimits(first, (first < 2) ? gameObject.transform.position.y : gameObject.transform.position.x))
        {
            num++;
        }
        if (!block[second] && IsWithinLimits(second, (second< 2) ? gameObject.transform.position.y : gameObject.transform.position.x) && lastDirectionMem != -1)
        {
            num++;
        }
        if (!block[secondOp] && IsWithinLimits(secondOp, (secondOp< 2) ? gameObject.transform.position.y : gameObject.transform.position.x) && lastDirectionMem != 1)
        {
            num++;
        }
        return num > 1;
    }

    private bool IsItPossibleToTurn(int first, int second)
    {
        return !block[first] && IsWithinLimits(first, (first < 2)? gameObject.transform.position.y : gameObject.transform.position.x) &&
                !block[second] && IsWithinLimits(second, (second < 2) ? gameObject.transform.position.y : gameObject.transform.position.x);
    }

    private bool IsDirectionValid()
    {
        if (direction.x == 1)
        {
            return !block[0];
        }
        else if (direction.x == -1)
        {
            return !block[1];
        }
        else if (direction.y == 1)
        {
            return !block[2];
        }
        else
        {
            return !block[3];
        }
    }

    private void ChangeDirectionToGoodDirection(Vector2 goodDir)
    {
        if (goodDir.x == 1)
        {
            if (goodDir.y == 1)
            {
                GoodDirectionChangeTo(0, 2);
            }
            else if (goodDir.y == -1)
            {
                GoodDirectionChangeTo(0, 3);
            }
            else
            {
                GoodDirectionChangeTo(0, 2, 3);
            }
        }
        else if (goodDir.x == -1)
        {
            if (goodDir.y == 1)
            {
                GoodDirectionChangeTo(1, 2);
            }
            else if (goodDir.y == -1)
            {
                GoodDirectionChangeTo(1, 2);
            }
            else
            {
                GoodDirectionChangeTo(1, 2, 3);
            }
        }
        else
        {
            if (goodDir.y == 1)
            {
                GoodDirectionChangeTo(0, 1, 2);
            }
            else if (goodDir.y == -1)
            {
                GoodDirectionChangeTo(0, 1, 3);
            }
            else
            {
                //Impossible, it would require the overlap of nacman and the ghost
            }
        }
    }

    private void GoodDirectionChangeTo(int first, int second, int third)
    {
        if (!block[first] && !block[second] && !block[third])
        {
            switch (Random.Range(0, 3))
            {
                case 0:
                    DirectionTo(first);
                    break;
                case 1:
                    DirectionTo(second);
                    break;
                case 2:
                    DirectionTo(third);
                    break;
            }
        }
        else if (block[first])
        {
            GoodDirectionChangeTo(second, third);
        }
        else if (block[second])
        {
            GoodDirectionChangeTo(first, third);
        }
        else
        {
            GoodDirectionChangeTo(first, second);
        }
    }

    private void GoodDirectionChangeTo(int first, int second)
    {
        if (!block[first] && !block[second])
        {
            if (Random.Range(0, 2) != 0)
            {
                DirectionTo(first);
            }
            else
            {
                DirectionTo(second);
            }
        }
        else if (!block[first])
        {
            DirectionTo(first);
        }
        else if (!block[second])
        {
            DirectionTo(second);
        }
        else
        {
            direction = Vector2.zero;
        }
    }

    private void DirectionTo(int dir)
    {
        switch (dir)
        {
            case 0:
                direction = Vector3.right;
                break;
            case 1:
                direction = Vector3.left;
                break;
            case 2:
                direction = Vector3.up;
                break;
            case 3:
                direction = Vector3.down;
                break;
        }
    }

    private bool IsItAlreadyGoingInAGoodDirection(Vector2 goodDir)
    {
        return (direction.x != 0 && (direction.x == goodDir.x || goodDir.x == 0)) ||
                (direction.y != 0 && (direction.y == goodDir.y || goodDir.y == 0));
    }

    private Vector2 CalculateGoodDirection(Vector2 dist)
    {
        Vector2 res;

        res = new Vector2();
        if (dist.x > 0.5f)
        {
            res.x = -1;
        }
        else if (dist.x < -0.5f)
        {
            res.x = 1;
        }
        if (dist.y > 0.5f)
        {
            res.y = -1;
        }
        else if (dist.y < -0.5f)
        {
            res.y = 1;
        }
        return res;
    }

    private void Pursue()
    {
        if (!isRouteValid)
        {
            PursueGetRoute();
            isRouteValid = true;
            StartCoroutine(PursueUpdateRoute());
        }
        if (pursueIndex >= route.Count)
        {
            forcedRouteUpdate = true;
            PursueGetRoute();
            pursueIndex = 0;
            pursueIsDirectionValid = false;
        }
        if (!pursueIsDirectionValid)
        {
            PursueChangeDirection();
            pursueIsDirectionValid = true;
        }
        if (PursueHasItReachedNode())
        {
            pursueIndex++;
            pursueIsDirectionValid = false;
        }
    }

    private void PursueGetRoute()
    {
        GraphNode ghostNode;
        GraphNode playerNode;

        ghostNode = Graph.GetApproximateNode(gameObject.transform.position);
        playerNode = Graph.GetApproximateNode(player.transform.position);
        route = AStarAlgorithm.AStarAlgo(ghostNode, playerNode);
        StreamlineRoute();
    }

    private void StreamlineRoute()
    {
        Vector2 ghostPos;

        ghostPos = NormalizeGhostPosition();
        while (route.Count > 1 && AreNodesAligned(ghostPos, route[0], route[1]))
        {
            route.Remove(route[0]);
        }
        for (int start = 0; start < route.Count - 2; start++)
        {
            while (route.Count > start + 2 && AreNodesAligned(route[start], route[start + 1], route[start + 2]))
            {
                route.Remove(route[start + 1]);
            }
        }
    }

    private Vector2 NormalizeGhostPosition()
    {
        Vector2 res;

        res = new Vector2();
        res.x = Mathf.Floor(gameObject.transform.position.x) + 0.5f;
        res.y = Mathf.Floor(gameObject.transform.position.y) + 0.5f;

        return res;
    }

    private bool AreNodesAligned(Vector2 first, Vector2 second, Vector2 third)
    {
        return (first.x == second.x && second.x == third.x) ||
                (first.y == second.y && second.y == third.y);
    }

    private void PursueChangeDirection()
    {
        Vector2 dir;
        
        dir = route[pursueIndex] - (Vector2)gameObject.transform.position;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                direction = Vector3.right;
            }
            else
            {
                direction = Vector3.left;
            }
        }
        else
        {
            if (dir.y > 0)
            {
                direction = Vector3.up;
            }
            else
            {
                direction = Vector3.down;
            }
        }
    }

    private bool PursueHasItReachedNode()
    {
        Vector2 dist;

        dist = route[pursueIndex] - (Vector2)gameObject.transform.position;
        return dist.magnitude <= 2 * turnRange;
    }

    private IEnumerator PursueUpdateRoute()
    {
        yield return new WaitForSeconds(3.0f);
        if (!forcedRouteUpdate)
        {
            isRouteValid = false;
            pursueIsDirectionValid = false;
            pursueIndex = 0;
        }
        else
        {
            forcedRouteUpdate = false;
        }
    }

    private IEnumerator ChangeBehaviour()
    {
        yield return new WaitForSeconds(10.0f + ChangeBehaviourRandomVariation());
        SetBehaviourToRan(false);
        yield return new WaitForSeconds(15.0f + ChangeBehaviourRandomVariation());
        SetBehaviourToRan(true);
        yield return new WaitForSeconds(10.0f + ChangeBehaviourRandomVariation());
        changeBehaviourCoroutine = StartCoroutine(ChangeBehaviour());
    }

    private float ChangeBehaviourRandomVariation()
    {
        return Random.Range(0.0f, 10.0f);
    }

    private void SetBehaviourToRan(bool mode)
    {
        isBehaviourRandom = mode;
        pursueIndex = 0;
        isRouteValid = false;
        pursueIsDirectionValid = false;
        forcedRouteUpdate = false;
    }

    private bool CheckSpawnZone()
    {
        return gameObject.transform.position.x >= spawnBoxXMAX ||
            gameObject.transform.position.x <= spawnBoxXmin ||
            gameObject.transform.position.y >= spawnBoxYMAX ||
            gameObject.transform.position.y <= spawnBoxYmin;
    
    }

    private Vector3 ChooseDirection()
    {
        int index;

        for (int i = 0; i < freeOn.Length; i++)
        {
            freeOn[i] = false;
        }
        do
        {
            index = Random.Range(0, 4);
        }
        while (block[index]);
        switch (index)
        {
            case 0:
                return Vector3.right;
            case 1:
                return Vector3.left;
            case 2:
                return Vector3.up;
            default:
                return Vector3.down;
        }
    }

    private void ChangeDirection()
    {
        if (hasCollided)
        {
            direction = Vector3.zero;
        }
        if (freeOn[0] && IsWithinLimits(0, transform.position.y))
        {
            if ((Random.Range(0, 2) == 0 || hasCollided) && lastDirection.x != -1)
            {
                direction = Vector3.right;
            }
            freeOn[0] = false;
        }
        if (freeOn[1] && IsWithinLimits(1, transform.position.y))
        {
            if ((Random.Range(0, 2) == 0 || hasCollided) && lastDirection.x != 1)
            {
                direction = Vector3.left;
            }
            freeOn[1] = false;
        }
        if (freeOn[2] && IsWithinLimits(2, transform.position.x))
        {
            if ((Random.Range(0, 2) == 0 || hasCollided) && lastDirection.y != -1)
            {
                direction = Vector3.up;
            }
            freeOn[2] = false;
        }
        if (freeOn[3] && IsWithinLimits(3, transform.position.x))
        {
            if ((Random.Range(0, 2) == 0 || hasCollided) && lastDirection.y != 1)
            {
                direction = Vector3.down;
            }
            freeOn[3] = false;
        }
        if (hasCollided && direction.Equals(Vector3.zero))
        {
            direction = ChooseDirection();
        }
        hasCollided = false;
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
            block[index] = true;
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
            freeOn[index] = true;
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


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasExitedFromSpawn && collision.gameObject.CompareTag("Wall") && HasGhostWallTag(collision.gameObject))
        {
            Physics2D.IgnoreCollision(collision.collider, gameObject.GetComponent<Collider2D>(), true);
            ghostWallColliders.Add(collision.collider);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 diff;

            diff = GetCollisionDiff(collision);
            LockDirection(diff);
        }
    }

    private bool HasGhostWallTag(GameObject wall)
    {
        TagScript tags = wall.GetComponent<TagScript>();
        if (tags == null)
        {
            return false;
        }
        return tags.IsItTagged("ghostwall");
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


    private Vector2 GetCollisionDiff(Collision2D collision)
    {
        Vector2 pos = transform.position;
        Vector2 collPoint = collision.collider.ClosestPoint(pos);
        Vector2 diff = collPoint - pos;

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
                freeOn[0] = false;
            }
            else
            {
                SwitchBlockOn(1);
                badDirection = Vector3.left;
                ResetLimits(1);
                freeOn[1] = false;
            }
        }
        else
        {
            if (diff.y > 0)
            {
                SwitchBlockOn(2);
                badDirection = Vector3.up;
                ResetLimits(2);
                freeOn[2] = false;
            }
            else
            {
                SwitchBlockOn(3);
                badDirection = Vector3.down;
                ResetLimits(3);
                freeOn[3] = false;
            }
        }
        if (direction.Equals(badDirection))
        {
            hasCollided = true;
        }
    }

    private IEnumerator AnimationOne()
    {
        yield return new WaitForSeconds(animationLength);
        if (!isFrozen)
        {
            if (!useAltSprite)
            {
                spriteRend.sprite = firstSprite;
            }
            else
            {
                spriteRend.sprite = firstAltSprite;
            }
        }
        StartCoroutine(AnimationTwo());
    }

    private IEnumerator AnimationTwo()
    {
        yield return new WaitForSeconds(animationLength);
        if (!isFrozen)
        {
            if (!useAltSprite)
            {
                spriteRend.sprite = secondSprite;
            }
            else
            {
                spriteRend.sprite = secondAltSprite;
            }
        }
        StartCoroutine(AnimationOne());
    }

    public void ChangeColor(Color newColor)
    {
        spriteRend.color = newColor;
    }

    public void Activate()
    {
        isActive = true;
    }

    public void ResetEnemy()
    {
        hasExitedFromSpawn = false;
        firstTime = true;
        block = new bool[]{ false, false, false, false };
        dBlock = new bool[]{ false, false, false, false };
        freeOn = new bool[]{ false, false, false, false };
        limits = new float[,]{ { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };
        direction = Vector3.zero;
        lastDirection = Vector3.zero;
        if (changeBehaviourCoroutine != null)
        {
            StopCoroutine(changeBehaviourCoroutine);
        }
        isChangeBehaviourActive = false;
        SetBehaviourToRan(true);
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void SwitchSprite()
    {
        useAltSprite = !useAltSprite;
    }

    public IEnumerator StartBlinking()
    {
        yield return new WaitForSeconds(13.2f);
        isBlinking = true;
        StartCoroutine(BlinkAnimationOne());
    }

    private IEnumerator BlinkAnimationOne()
    {
        if (isBlinking)
        {
            spriteRend.color = new Color(spriteRend.color.r, spriteRend.color.g, spriteRend.color.b, 0.15f);
        }
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(BlinkAnimationTwo());
    }

    private IEnumerator BlinkAnimationTwo()
    {
        spriteRend.color = new Color(spriteRend.color.r, spriteRend.color.g, spriteRend.color.b, 1.0f);
        yield return new WaitForSeconds(0.15f);
        if (isBlinking)
        {
            StartCoroutine(BlinkAnimationOne());
        }
    }

    public void StopBlinking()
    {
        isBlinking = false;
    }

    public void Freeze()
    {
        isFrozen = true;
        toppingRend.sprite = frozenSprite;
        StartCoroutine(FreezeDecay());
    }

    public void Defreeze()
    {
        toppingRend.sprite = null;
        isFrozen = false;
    }

    private IEnumerator FreezeDecay()
    {
        yield return new WaitForSeconds(10.0f);
        audioManager.PlayIceCrackOneSound();
        toppingRend.sprite = frozenSpriteSecond;
        yield return new WaitForSeconds(3.2f);
        audioManager.PlayIceCrackTwoSound();
        toppingRend.sprite = frozenSpriteThird;
    }

    public void SetHasAlreadyDied(bool mode)
    {
        hasAlreadyDied = mode;
    }

    public bool HasItAlreadyDied()
    {
        return hasAlreadyDied;
    }
}