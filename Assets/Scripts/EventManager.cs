using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] private GameObject nacmanPrefab;
    [SerializeField] private GameObject[] ghostsPrefab = new GameObject[4];
    private GameObject nacman;
    private GameObject[] ghosts = new GameObject[3];
    private bool isItFirstCall = true;
    private bool leftDir = false;
    private bool[] arrived = {true, true, true};
    private int eventNum;
    private int[] ghostsDir = new int[3];
    private int[] ghostsSteps = new int[3];
    private float startXPos = -12.0f;
    private float startYPos = 7.0f;
    private float speed = 2.5f;
    private float xRange = 9.5f;
    private float yRange = 4.5f;
    private float minNacGhostDis = 1.0f;
    private float maxNacGhostDis = 3.0f;
    private float[] nexPos = new float[3];

    void Start()
    {
        ChooseEvent();
        leftDir = randomBool();
    }

    void Update()
    {
        switch (eventNum)
        {
            case 0:
                NacmanAlone();
                break;
            case 1:
                GhostNacman();
                break;
            case 2:
                NacmanGhost();
                break;
            case 3:
                Ghosts();
                break;
        }
    }

    private void NacmanAlone()
    {
        if (isItFirstCall)
        {
            StartNacmanAlone();
        }
        nacman.transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (nacman.transform.position.x > -startXPos || nacman.transform.position.x < startXPos)
        {
            Destroy(nacman);
            ResetEvent();
        }
    }

    private void GhostNacman()
    {
        if (isItFirstCall)
        {
            StartGhostNacman();
        }
        if (nacman != null)
        {
            nacman.transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        ghosts[0].transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (nacman != null && (nacman.transform.position.x > -startXPos || nacman.transform.position.x < startXPos))
        {
            Destroy(nacman);
        }
        if ((!leftDir && ghosts[0].transform.position.x > -startXPos) || (leftDir && ghosts[0].transform.position.x < startXPos))
        {
            Destroy(ghosts[0]);
            ResetEvent();
        }
    }

    private void NacmanGhost()
    {
        if (isItFirstCall)
        {
            StartNacmanGhost();
        }
        if (ghosts[0] != null)
        {
            ghosts[0].transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        nacman.transform.Translate(Vector3.right * speed * Time.deltaTime);
        if (ghosts[0] != null && (ghosts[0].transform.position.x > -startXPos || ghosts[0].transform.position.x < startXPos))
        {
            Destroy(ghosts[0]);
        }
        if ((!leftDir && nacman.transform.position.x > -startXPos) || (leftDir && nacman.transform.position.x < startXPos))
        {
            Destroy(nacman);
            ResetEvent();
        }
    }

    private void Ghosts()
    {
        if (isItFirstCall)
        {
            StartGhosts();
        }
        for (int i = 0; i < 3; i++)
        {
            if (ghosts[i] != null)
            {
                ManageSteps(i);
                ghosts[i].transform.Translate(GetGhostDirection(i) * speed * Time.deltaTime);
                if (ghosts[i].transform.position.x < startXPos ||
                    ghosts[i].transform.position.x > -startXPos ||
                    ghosts[i].transform.position.y > startYPos ||
                    ghosts[i].transform.position.y < -startYPos)
                {
                    Destroy(ghosts[i]);
                }
            }
        }
        if (ghosts[0] == null && ghosts[1] == null && ghosts[2] == null)
        {
            for (int i = 0; i < 3; i++)
            {
                arrived[i] = true;
            }
            ResetEvent();
        }
    }

    private void ManageSteps(int i)
    {
        if (arrived[i])
        {
            if (ghostsSteps[i] == 0)
            {
                CalculateDirAndSteps(i);
            }
            CalcNexPos(i);
            arrived[i] = false;
        }
        if (HasReachedNexPos(i))
        {
            ghostsSteps[i] -= 1;
            arrived[i] = true;
        }
    }

    private void CalcNexPos(int i)
    {
        if (ghostsDir[i] % 2 == 0)
        {
            nexPos[i] = ghosts[i].transform.position.x;
            if (ghostsDir[i] == 0)
            {
                nexPos[i] += 1;
            }
            else
            {
                nexPos[i] -= 1;
            }
        }
        else
        {
            nexPos[i] = ghosts[i].transform.position.y;
            if (ghostsDir[i] == 1)
            {
                nexPos[i] -= 1;
            }
            else
            {
                nexPos[i] += 1;
            }
        }
    }

    private bool HasReachedNexPos(int i)
    {
        switch (ghostsDir[i])
        {
            case 0:
                return ghosts[i].transform.position.x >= nexPos[i];
            case 1:
                return ghosts[i].transform.position.y <= nexPos[i];
            case 2:
                return ghosts[i].transform.position.x <= nexPos[i];
            case 3:
                return ghosts[i].transform.position.y >= nexPos[i];
            default:
                return false;
        }
    }

    private void CalculateDirAndSteps(int i)
    {
        int newDir;

        ghostsSteps[i] = Random.Range(3, 8);
        do
        {
            newDir = Random.Range(0, 4);
        }
        while (Mathf.Abs(ghostsDir[i] - newDir) == 2);
        ghostsDir[i] = newDir;
    }

    private Vector3 GetGhostDirection(int i)
    {
        switch (ghostsDir[i])
        {
            case (0):
                return Vector3.right;
            case (1):
                return Vector3.down;
            case (2):
                return Vector3.left;
            case (3):
                return Vector3.up;
            default:
                return Vector3.zero;
        }
    }

    private void ChooseEvent()
    {
        eventNum = Random.Range(0,4);
    }

    private bool randomBool()
    {
        return Random.Range(0, 2) % 2 == 0;
    }

    private void ResetEvent()
    {
        isItFirstCall = true;
        ChooseEvent();
        leftDir = randomBool();
    }

    private void StartNacmanAlone()
    {
        Vector3 startPosition;

        if (!leftDir)
        {
            startPosition = new Vector3(startXPos, Random.Range(-yRange, yRange));
            nacman = Object.Instantiate(nacmanPrefab, startPosition, Quaternion.identity);
        }
        else
        {
            startPosition = new Vector3(-startXPos, Random.Range(-yRange, yRange));
            nacman = Object.Instantiate(nacmanPrefab, startPosition, Quaternion.Euler(0, 180, 0));
        }
        isItFirstCall = false;
    }

    private void StartGhostNacman()
    {
        Vector3 nacmanStartPosition;
        Vector3 ghostStartPosition;

        if (!leftDir)
        {
            nacmanStartPosition = new Vector3(startXPos, Random.Range(-yRange, yRange));
            ghostStartPosition = new Vector3(nacmanStartPosition.x - Random.Range(minNacGhostDis, maxNacGhostDis), nacmanStartPosition.y);

            nacman = Object.Instantiate(nacmanPrefab, nacmanStartPosition, Quaternion.identity);
            ghosts[0] = Object.Instantiate(ghostsPrefab[Random.Range(0, 3)], ghostStartPosition, Quaternion.identity);
        }
        else
        {
            nacmanStartPosition = new Vector3(-startXPos, Random.Range(-yRange, yRange));
            ghostStartPosition = new Vector3(nacmanStartPosition.x + Random.Range(minNacGhostDis, maxNacGhostDis), nacmanStartPosition.y);

            nacman = Object.Instantiate(nacmanPrefab, nacmanStartPosition, Quaternion.Euler(0, 180, 0));
            ghosts[0] = Object.Instantiate(ghostsPrefab[Random.Range(0, 3)], ghostStartPosition, Quaternion.Euler(0, 180, 0));
        }
        isItFirstCall = false;
    }

    private void StartNacmanGhost()
    {
        Vector3 ghostStartPosition;
        Vector3 nacmanStartPosition;

        if (!leftDir)
        {
            ghostStartPosition = new Vector3(startXPos, Random.Range(-yRange, yRange));
            nacmanStartPosition = new Vector3(ghostStartPosition.x - Random.Range(minNacGhostDis, maxNacGhostDis), ghostStartPosition.y);

            ghosts[0] = Object.Instantiate(ghostsPrefab[3], ghostStartPosition, Quaternion.identity);
            nacman = Object.Instantiate(nacmanPrefab, nacmanStartPosition, Quaternion.identity);
        }
        else
        {
            ghostStartPosition = new Vector3(-startXPos, Random.Range(-yRange, yRange));
            nacmanStartPosition = new Vector3(ghostStartPosition.x + Random.Range(minNacGhostDis, maxNacGhostDis), ghostStartPosition.y);

            nacman = Object.Instantiate(nacmanPrefab, nacmanStartPosition, Quaternion.Euler(0, 180, 0));
            ghosts[0] = Object.Instantiate(ghostsPrefab[3], ghostStartPosition, Quaternion.Euler(0, 180, 0));
        }
        isItFirstCall = false;
    }

    private void StartGhosts()
    {
        Vector3 startPos = new Vector3();

        for (int i = 0; i < 3; i++)
        {
            ghostsDir[i] = Random.Range(0,4);
            ghostsSteps[i] = Random.Range(3, 8);
            if (ghostsDir[i] % 2 == 0)
            {
                startPos.x = (ghostsDir[i] == 0) ? startXPos : -startXPos;
                startPos.y = Random.Range(-yRange, yRange);
            }
            else
            {
                startPos.x = Random.Range(-xRange, xRange);
                startPos.y = (ghostsDir[i] == 1) ? startYPos : -startYPos;
            }
            ghosts[i] = Object.Instantiate(ghostsPrefab[i], startPos, Quaternion.identity);
        }
        isItFirstCall = false;
    }
}
