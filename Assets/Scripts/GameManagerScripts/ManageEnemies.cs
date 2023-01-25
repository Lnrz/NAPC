using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageEnemies : MonoBehaviour
{
    private Enemy[] enemies;
    private Color[] enemiesColor;
    private Coroutine[] blinkingCor;
    private bool enemiesNotPrepared = true;
    private bool isGameStarted = false;
    private bool loss = false;
    private GameUI gameUI;
    [SerializeField] private float spawnY = 0.5f;
    [SerializeField] private int minSpawnX = -1;
    [SerializeField] private int maxSpawnX = 1;
    [SerializeField] private int mapNum = 0;
    [SerializeField] private PlayerController player;
    [SerializeField] private ParticleSystem ghostDeathParticles;

    private void Start()
    {
        gameUI = gameObject.GetComponent<GameUI>();
        if (mapNum == 0)
        {
            Graph.BuildHardBrickMap();
        }
        else if (mapNum == 1)
        {
            Graph.BuildColdIceMap();
        }
    }

    private void Update()
    {
        if (enemiesNotPrepared)
        {
            GameObject[] enemiesGO;

            enemiesGO = GameObject.FindGameObjectsWithTag("Enemy");
            enemies = new Enemy[enemiesGO.Length];
            enemiesColor = new Color[enemiesGO.Length];
            blinkingCor = new Coroutine[enemiesGO.Length];
            for (int i = 0; i < enemiesGO.Length; i++)
            {
                enemies[i] = enemiesGO[i].GetComponent<Enemy>();
                enemiesColor[i] = GetRandomColor();
                enemies[i].ChangeColor(enemiesColor[i]);
            }
            DisableEnemyCollisions();
            enemiesNotPrepared = false;
        }
        if (!isGameStarted)
        {
            if (Input.anyKeyDown)
            {
                gameUI.StartGameUI();
                isGameStarted = true;
                player.Activate();
                for (int i = 0; i < enemies.Length; i++)
                {
                    StartCoroutine(WakeUpEnemy(i));
                }
            }
        }
    }

    private IEnumerator WakeUpEnemy(int index)
    {
        yield return new WaitForSeconds(index * 5);
        if (!loss)
        {
            enemies[index].Activate();
        }
    }

    private Color GetRandomColor()
    {
        float red = Random.Range(0.0f, 1.0f);
        float green = Random.Range(0.0f, 1.0f);
        float blue = Random.Range(0.0f, 1.0f);

        return new Color(red, green, blue);
    }

    public void PlayerHasPowerup()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].ChangeColor(Color.white);
            enemies[i].SwitchSprite();
            enemies[i].EnableFlee();
            blinkingCor[i] = StartCoroutine(enemies[i].StartBlinking());
        }
    }

    public void PowerupExpired()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!enemies[i].HasItAlreadyDied())
            {
                enemies[i].ChangeColor(enemiesColor[i]);
                enemies[i].SwitchSprite();
                enemies[i].DisableFlee();
                enemies[i].StopBlinking();
            }
            else
            {
                enemies[i].SetHasAlreadyDied(false);
            }
        }
    }

    public void Lose()
    {
        gameUI.DisplayGameEnd("Loss");
    }

    public void DisableEnemies()
    {
        loss = true;
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].Deactivate();
        }
    }

    public IEnumerator KillEnemy(GameObject enemy)
    {
        Enemy enemyScr = enemy.GetComponent<Enemy>();

        enemyScr.Deactivate();
        Instantiate(ghostDeathParticles, enemy.gameObject.transform.position, ghostDeathParticles.transform.rotation);
        enemy.transform.position = GetRandomSpawnPos();
        enemyScr.ResetEnemy();
        RecoverEnemyColorAndStopBlinking(enemyScr);
        enemyScr.SwitchSprite();
        enemyScr.DisableFlee();
        enemyScr.StopBlinking();
        enemyScr.SetHasAlreadyDied(true);
        yield return new WaitForSeconds(10.0f);
        enemyScr.Activate();
    }

    private void RecoverEnemyColorAndStopBlinking(Enemy enemy)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == enemy)
            {
                enemy.ChangeColor(enemiesColor[i]);
                if (blinkingCor[i] != null)
                {
                    StopCoroutine(blinkingCor[i]);
                }
            }
        }
    }

    private Vector3 GetRandomSpawnPos()
    {
        Vector3 spawnPos = new Vector3();

        spawnPos.x = Random.Range(minSpawnX, maxSpawnX + 1) + 0.5f;
        spawnPos.y = spawnY;
        return spawnPos;
    }

    public void FreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.Freeze();
        }
    }

    public void DefreezeEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.Defreeze();
        }
    }

    private void DisableEnemyCollisions()
    {
        for (int i = 0; i < enemies.Length - 1; i++)
        {
            for (int j = i; j < enemies.Length; j++)
            {
                Physics2D.IgnoreCollision(enemies[i].gameObject.GetComponent<Collider2D>(), enemies[j].GetComponent<Collider2D>(), true);
            }
        }
    }
}
