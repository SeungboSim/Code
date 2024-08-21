using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    GameManager gameManager;
    InGameManager inGameManager;

    public GameObject bossPref;
    public List<Enemy> enemyList;
    public Transform[] spawnPoints;
    public StageData stageData;
    public float levelTime;
    public Text enemyCountText;

    int level;
    float timer;
    bool isItemSpawn;

    private void Awake()
    {
        gameManager = GameManager.instance;
        inGameManager = InGameManager.instance;
        stageData = gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum];
        levelTime = inGameManager.maxGameTime / stageData.spawnData.Length;
        enemyList = new List<Enemy>();
    }

    private void Start()
    {
        StartCoroutine(SpawnerRoutine());
    }

    private void Update()
    {
        enemyCountText.text = enemyList.Count.ToString();
    }

    IEnumerator SpawnerRoutine()
    {
        while (!(inGameManager.isGameOver || inGameManager.isBossSpawned))
        {
            if (!inGameManager.isLive)
                yield return null;

            timer += Time.deltaTime;
            level = Mathf.Min(Mathf.FloorToInt(inGameManager.gameTime / levelTime), stageData.spawnData.Length - 1);

            if (timer > stageData.spawnData[level].spawnTime)
            {
                timer = 0;
                Spawn();
            }
            if (inGameManager.kill != 0 && inGameManager.kill % 20 == 0)
            {
                if (!isItemSpawn)
                {
                    isItemSpawn = true;
                    int rand = Random.Range(3, 8);
                    if (rand == 6 || rand == 7)
                        rand = 5;
                    GameObject item = inGameManager.pool.Get(rand);
                    item.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
                    if (item.name.Contains("Gold"))
                    {
                        item.GetComponent<Gold>().Init(stageData.spawnData[level].gold);
                    }
                }
            }
            else
            {
                if (isItemSpawn)
                    isItemSpawn = false;
            }
            yield return null;
        }
    }
    void Spawn()
    {
        if (enemyList.Count < 800)
        {
            Enemy enemy = inGameManager.pool.Get(0).GetComponent<Enemy>();
            enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
            enemy.Init(stageData.spawnData[level].enemyData[Random.Range(0, stageData.spawnData[level].enemyData.Length)]);
            enemyList.Add(enemy);
        }
    }
    public void BossSpawn()
    {
        Enemy boss = Instantiate(bossPref).GetComponent<Enemy>();
        boss.transform.position = spawnPoints[0].position;
        boss.Init(stageData.bossData);
        inGameManager.boss = boss;
        enemyList.Add(boss);
    }
}
