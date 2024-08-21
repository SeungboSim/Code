using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InGameManager : MonoBehaviour
{
    GameManager gameManager;

    public static InGameManager instance;
    [Header("# Game Control")]
    public bool isLive;
    public bool isItemSpawn = false;
    public bool isGameOver = false;
    public float gameTime;
    public float maxGameTime;
    public List<Exp> expObjects;
    public bool dashState;
    bool isPaused = false;
    bool isQuilted = false;
    public bool isBossSpawned = false;
    bool isBossStage = false;
    WaitForSeconds delay = new WaitForSeconds(0.05f);
    public Enemy boss;

    [Header("# Player Info")]
    public int characterID;
    public float maxHealth = 100;
    public float health;
    public int level = 0;
    public int kill;
    public float exp;
    public float expRate = 1;
    public float dashRate = 1;
    public int[] nextExp;
    public int earnedGold;
    public int maxDashCount;
    public int dashCount;
    public int maxRefreshCount = 1;
    public int refreshCount = 1;
    public float deffence;

    [Header("# 초기값")]
    public float originalPowerRate;
    public float originalSpeedRate;
    public float originalDeffence;
    public float originalAttackRate;
    public float originalExpRate;

    [Header("# Game Object")]
    public Player player;
    public PoolManager pool;
    public F_TextPoolManager fPool;
    public Spawner spawner;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public GameObject uiHUD;
    public GameObject expSlider;
    public GameObject timerText;
    public Slider bossHpSlider;
    public GameObject bossArea;
    public Transform uiPause;
    public Transform uiJoy;
    public GameObject enemyCleaner;
    public Image dashButtonImage;
    public Image dashButtonBackgroundImage;
    public RewardView rewardView;
    public GameObject goToRobbyButton;
    public Text dashCountText;
    public Text stageInfoText;
    public Text refreshCountText;

    private void Awake()
    {
        gameManager = GameManager.instance;
        instance = this;
        Application.targetFrameRate = 60;
        expObjects = new List<Exp>();
    }

    private void Start()
    {
        characterID = gameManager.PlayerInfoManager.userData.lastCharacterID;
        maxGameTime = gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageTime;
        maxDashCount = gameManager.PlayerInfoManager.dashCount;
        dashCount = maxDashCount;
        originalPowerRate = gameManager.PlayerInfoManager.powerRate * 0.01f;
        originalSpeedRate = gameManager.PlayerInfoManager.speedRate * 0.01f;
        originalDeffence = gameManager.PlayerInfoManager.deffence;
        deffence = originalDeffence;
        originalAttackRate = gameManager.PlayerInfoManager.attackRate * 0.01f;
        originalExpRate = gameManager.PlayerInfoManager.expRate * 0.01f;
        expRate = 1 + originalExpRate;
        maxRefreshCount = gameManager.PlayerInfoManager.refreshCount;
        refreshCount = maxRefreshCount;
        maxHealth = 100 + gameManager.PlayerInfoManager.health;
        dashCountText.text = string.Format("{0} / {1}", dashCount, maxDashCount);
        stageInfoText.text = string.Format("Stage - {0}", gameManager.PlayerInfoManager.selectedStageNum);
        refreshCountText.text = string.Format("{0} / {1}", refreshCount, maxRefreshCount);
        GameStart(characterID);
    }
    public void PlayTouchSound()
    {
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Touch);
    }
    public void GameStart(int id)
    {
        characterID = id;
        health = maxHealth;
        player.gameObject.SetActive(true);
        uiHUD.SetActive(true);
        if(gameManager.PlayerInfoManager.weaponID != -1)
        {
            uiLevelUp.Select((int)gameManager.PlayerInfoManager.weaponType);
        }
        else
        {
            uiLevelUp.Select(Random.Range(0, 9));
        }

        isLive = true;
        Resume();
        GetExp(1 / expRate);
        gameManager.AudioManager.PlayBGM(1, true);
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Select);
        StartCoroutine(IngameRoutine());
    }
    IEnumerator IngameRoutine() // 인게임 메인 루틴
    {
        while (!isGameOver)
        {
            if (isLive)
            {
                gameTime += Time.deltaTime;
                if (health <= 0)
                    GameOver();
                if (gameTime > maxGameTime)
                {
                    if (!isBossSpawned)
                    {
                        timerText.SetActive(false);
                        isBossSpawned = true;
                        enemyCleaner.SetActive(true);
                        StartCoroutine(SpawnBoss());
                    }
                    if (isBossStage)
                    {
                        bossHpSlider.value = boss.health / boss.maxHealth;
                        if (boss.health <= 0)
                            GameVictory();
                    }
                }
            }
            yield return null;
        }
    }
    IEnumerator SpawnBoss() // 보스 소환
    {
        bossArea.transform.position = player.transform.position;
        bossArea.SetActive(true);
        yield return new WaitForSeconds(1f);
        gameManager.AudioManager.PlayBGM(2, true);
        enemyCleaner.SetActive(false);
        spawner.BossSpawn();
        expSlider.SetActive(false);
        bossHpSlider.gameObject.SetActive(true);
        while (bossHpSlider.value < boss.health / boss.maxHealth)
        {
            bossHpSlider.value += Time.deltaTime * 2f;
            yield return null;
        }
        isBossStage = true;
    }
    public void GameOver() // 게임 오버 시
    {
        isLive = false;
        isGameOver = true;
        player.Dead();
        uiPause.localScale = Vector3.zero;
        player.rigid.simulated = false;
        StartCoroutine(GameOverRoutine());
    }   
    IEnumerator GameOverRoutine() // 게임 오버 시 루틴
    {
        yield return new WaitForSeconds(1f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        uiHUD.SetActive(false);

        gameManager.AudioManager.PlayBGM(0, false);
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Lose);

        uiResult.resultTexts[0].text = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(gameTime / 60), Mathf.FloorToInt(gameTime % 60));
        uiResult.resultTexts[1].text = string.Format("{0:F0}", kill);

        foreach (RewardBox box in rewardView.rewardBoxes)
        {
            box.gameObject.SetActive(false);
        }
        rewardView.rewardBoxes.Clear();

        if (earnedGold > 0 && !isQuilted)
        {
            rewardView.AddBox("Icon_Gold", (int)(earnedGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f)));
            gameManager.PlayerInfoManager.userData.gold += (int)(earnedGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f));
            gameManager.FirebaseDBManager.WriteUserData("gold", gameManager.PlayerInfoManager.userData.gold);
        }
        StartCoroutine(ShowRewardItemsRoutine());
    }
    public void QuitGame() // 게임 종료
    {
        isQuilted = true;
        gameManager.AudioManager.EffectBGM(false);
        GameOver();
    }
    public void GameVictory() // 게임 승리 시
    {
        if (!gameManager.NetworkManager.NetworkCheck())
            return;

        StartCoroutine(GameVictoryRoutine());
    }
    IEnumerator GameVictoryRoutine() // 게임 승리 시 루틴
    {
        isLive = false;
        isGameOver = true;
        enemyCleaner.SetActive(true);
        player.rigid.simulated = false;
        uiHUD.SetActive(false);
        yield return new WaitForSeconds(1f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        uiResult.resultTexts[0].text = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(gameTime / 60), Mathf.FloorToInt(gameTime % 60));
        uiResult.resultTexts[1].text = string.Format("{0:F0}", kill);
        uiHUD.SetActive(false);

        gameManager.AudioManager.PlayBGM(0, false);
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Win);

        if (gameManager.PlayerInfoManager.userData.clearStage + 1 == gameManager.PlayerInfoManager.selectedStageNum)
        {
            gameManager.PlayerInfoManager.userData.clearStage = gameManager.PlayerInfoManager.selectedStageNum;
            gameManager.PlayerInfoManager.userData.gold += (int)(gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageClearGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f));
            rewardView.rewardBoxes[0].countText.text = $"x {(int)(gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageClearGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f))}";
            rewardView.rewardBoxes[1].countText.text = $"x {(int)(gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageClearExp * (1 + gameManager.PlayerInfoManager.userExpRate * 0.01f))}";
            gameManager.FirebaseDBManager.WriteUserData("clearStage", gameManager.PlayerInfoManager.userData.clearStage);
        }
        else
        {
            rewardView.rewardBoxes[0].gameObject.SetActive(false);
            rewardView.rewardBoxes.RemoveAt(0);
            rewardView.rewardBoxes[0].countText.text = $"x {(int)(gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageClearExp * (1 + gameManager.PlayerInfoManager.userExpRate * 0.01f))}";
        }
        gameManager.PlayerInfoManager.userData.gold += (int)(earnedGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f));
        gameManager.FirebaseDBManager.WriteUserData("gold", gameManager.PlayerInfoManager.userData.gold);
        gameManager.PlayerInfoManager.GetExp((int)(gameManager.PlayerInfoManager.stageDatas[gameManager.PlayerInfoManager.selectedStageNum].stageClearExp * (1 + gameManager.PlayerInfoManager.userExpRate * 0.01f)));
        gameManager.FirebaseDBManager.WriteUserData("level", gameManager.PlayerInfoManager.userData.level);
        gameManager.FirebaseDBManager.WriteUserData("maxEnergy", gameManager.PlayerInfoManager.userData.maxEnergy);
        gameManager.FirebaseDBManager.WriteUserData("exp", gameManager.PlayerInfoManager.userData.exp);

        if (earnedGold > 0)
            rewardView.AddBox("Icon_Gold", (int)(earnedGold * (1 + gameManager.PlayerInfoManager.userGoldRate * 0.01f)));

        goToRobbyButton.SetActive(true);
        StartCoroutine(ShowRewardItemsRoutine());
    }
    IEnumerator ShowRewardItemsRoutine()
    {
        int index = 0;
        while (index < rewardView.rewardBoxes.Count)
        {
            rewardView.rewardBoxes[index].gameObject.SetActive(true);
            index++;
            yield return delay;
        }
        goToRobbyButton.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Stop();
    }
    public void GoToRobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    public void GetExp(float exp)
    {
        if (!isLive)
            return;

        this.exp += exp * expRate;

        if (this.exp >= nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            this.exp -= nextExp[Mathf.Min(level, nextExp.Length - 1)];
            level++;
            uiLevelUp.Show();
        }
    }
    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
        uiJoy.localScale = Vector3.zero;
        dashButtonImage.transform.localScale = Vector3.zero;
    }
    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        uiJoy.localScale = Vector3.one;
        dashButtonImage.transform.localScale = Vector3.one;
    }
    public void GameQuit()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    public void DashButtonClicked() // 대쉬 버튼 클릭 시
    {
        if (dashCount != 0 && isLive && !dashState && (player.inputVec.x != 0 || player.inputVec.y != 0))
        {
            dashCount--;
            dashCountText.text = string.Format("{0} / {1}", dashCount, maxDashCount);
            if (maxDashCount > 1)
            {
                if (dashCount == 0)
                {
                    dashButtonImage.color = new Color32(255, 255, 255, 30);
                    dashButtonBackgroundImage.color = new Color32(200, 200, 200, 20);
                }
                else
                {
                    if (dashCount == maxDashCount - 1)
                    {
                        dashButtonImage.fillAmount = 0;
                        StartCoroutine(DashCooltimeRoutine());
                    }
                }
            }
            else
            {
                dashButtonImage.color = new Color32(255, 255, 255, 30);
                dashButtonBackgroundImage.color = new Color32(200, 200, 200, 20);
                dashButtonImage.fillAmount = 0;
                StartCoroutine(DashCooltimeRoutine());
            }

            StartCoroutine(DashRoutine());
        }
    }
    IEnumerator DashRoutine() // 대쉬 진행 중
    {
        dashState = true;
        player.rigid.velocity = player.inputVec.normalized * 60;
        yield return delay;
        dashState = false;
    }
    IEnumerator DashCooltimeRoutine() // 대쉬 쿨타임 시 루틴
    {
        while (dashCount < maxDashCount)
        {
            while (dashButtonImage.fillAmount < 1)
            {
                dashButtonImage.fillAmount += Time.deltaTime / (5 - 5 * (dashRate - 1));
                yield return null;
            }

            if (dashCount == 0)
            {
                dashButtonImage.color = new Color32(255, 255, 255, 200);
                dashButtonBackgroundImage.color = new Color32(255, 255, 255, 180);
            }
            if (dashCount < maxDashCount)
            {
                dashCount++;
                dashCountText.text = string.Format("{0} / {1}", dashCount, maxDashCount);
                if (dashCount == maxDashCount)
                    break;
                else
                    dashButtonImage.fillAmount = 0;
            }
        }
    }
    public void PauseButtonClicked()
    {
        if (!isGameOver)
        {
            uiPause.localScale = Vector3.one;
            Stop();
            gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Touch);
            gameManager.AudioManager.EffectBGM(true);
        }
    }
    public void ResumeButtonClicked()
    {
        uiPause.localScale = Vector3.zero;
        Resume();
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Touch);
        gameManager.AudioManager.EffectBGM(false);
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)            // 앱이 비활성화 되었을 때 처리
        {
            isPaused = true;
            if (isLive)
                PauseButtonClicked();
        }
        else
        {
            if (isPaused)                // 앱이 활성화 되었을 때 처리
            {
                isPaused = false;
            }
        }
    }
}
