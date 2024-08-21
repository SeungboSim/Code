using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    GameManager gameManager;
    public Image characterIamge;
    public Text levelText, energyText, energyRechargingText, goldText, crystalText, postAlertText;
    public Slider expSlider;
    public GameObject postAlertImage;
    public GameObject[] lobbyPanels;
    public GameObject energyNoticePanel;
    public Button ExitStagePanelButton;
    public StoreScreen storePanel;
    public AbilityUpgradeScreen abilityScreen;
    public NoticeScreen noticeScreen;
    public PostScreen postScreen;
    public EquipSelectScreen equipSelectScreen;
    public GameObject waitAdScreen;

    bool isPaused = false;

    bool isUpdateComplete;
    bool loadGameSceneState;
    float updateDelayTimer;
    float remainder;

    private void Awake()
    {
        instance = this;
        gameManager = GameManager.instance;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UserDataCheck();
        UpdateEnergyValue();
        PostCheck();
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;         // 이벤트에서 함수를 제거해 리소스 누수 방지
    }
    private void Start()
    {
        characterIamge.sprite = gameManager.PlayerInfoManager.characterDatas[gameManager.PlayerInfoManager.userData.lastCharacterID].characterIcon;
        goldText.text = gameManager.PlayerInfoManager.userData.gold.ToString();
        crystalText.text = gameManager.PlayerInfoManager.userData.crystal.ToString();

        AbilityCheck(); // 어빌리티 업데이트
        Time.timeScale = 1;
        
        gameManager.AudioManager.PlayBGM(0, true);
        gameManager.AudioManager.EffectBGM(false);
    }
    public void PlayTouchSound()
    {
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Touch);
    }
    public void AbilityCheck()
    {
        gameManager.PlayerInfoManager.ResetAbility();
        if (gameManager.PlayerInfoManager.weaponID != -1) // 무기 적용
        {
            Equip weapon = gameManager.PlayerInfoManager.equipList.Find(x => x.id == gameManager.PlayerInfoManager.weaponID);
            gameManager.PlayerInfoManager.powerRate += (int)(gameManager.PlayerInfoManager.equipDatas.Find(x => (int)x.itemType == weapon.type).values[weapon.level]);
            gameManager.PlayerInfoManager.weaponType = (WeaponType)((int)weapon.type);
        }
        else
            gameManager.PlayerInfoManager.weaponType = WeaponType.Nothing;

        if (gameManager.PlayerInfoManager.hatID != -1) // 방어구 적용
        {
            Equip hat = gameManager.PlayerInfoManager.equipList.Find(x => x.id == gameManager.PlayerInfoManager.hatID);
            gameManager.PlayerInfoManager.deffence += (int)(gameManager.PlayerInfoManager.equipDatas.Find(x => (int)x.itemType == hat.type).values[hat.level]);
        }
        if (gameManager.PlayerInfoManager.shirtID != -1) // 방어구 적용
        {
            Equip shirt = gameManager.PlayerInfoManager.equipList.Find(x => x.id == gameManager.PlayerInfoManager.shirtID);
            gameManager.PlayerInfoManager.deffence += (int)(gameManager.PlayerInfoManager.equipDatas.Find(x => (int)x.itemType == shirt.type).values[shirt.level]);
        }
        if (gameManager.PlayerInfoManager.shoesID != -1) // 방어구 적용
        {
            Equip shoes = gameManager.PlayerInfoManager.equipList.Find(x => x.id == gameManager.PlayerInfoManager.shoesID);
            gameManager.PlayerInfoManager.speedRate += (int)(gameManager.PlayerInfoManager.equipDatas.Find(x => (int)x.itemType == shoes.type).values[shoes.level]);
        }
        for (int i = 0; i < abilityScreen.abilityButtons_First.Count; i++)
        {
            if (i < gameManager.PlayerInfoManager.userData.abilityLevel)
            {
                switch (abilityScreen.abilityButtons_First[i].type)
                {
                    case AbilityType.Power:
                        gameManager.PlayerInfoManager.powerRate += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Speed:
                        gameManager.PlayerInfoManager.speedRate += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Defence:
                        gameManager.PlayerInfoManager.deffence += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Health:
                        gameManager.PlayerInfoManager.health += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Rate:
                        gameManager.PlayerInfoManager.attackRate += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Exp:
                        gameManager.PlayerInfoManager.expRate += abilityScreen.abilityButtons_First[i].value;
                        break;
                    case AbilityType.Gold:
                        gameManager.PlayerInfoManager.userGoldRate += abilityScreen.abilityButtons_First[i].value;
                        break;
                    default:
                        break;
                }
            }
        }
        for (int i = 0; i < abilityScreen.abilityButtons_Second.Count; i++)
        {
            if (i < gameManager.PlayerInfoManager.userData.abilityLevel2)
            {
                switch (abilityScreen.abilityButtons_Second[i].type)
                {
                    case AbilityType.Power:
                        gameManager.PlayerInfoManager.powerRate += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.Speed:
                        gameManager.PlayerInfoManager.speedRate += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.Defence:
                        gameManager.PlayerInfoManager.deffence += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.Health:
                        gameManager.PlayerInfoManager.health += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.UserExp:
                        gameManager.PlayerInfoManager.userExpRate += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.Gold:
                        gameManager.PlayerInfoManager.userGoldRate += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.DashCount:
                        gameManager.PlayerInfoManager.dashCount += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    case AbilityType.RefreshCount:
                        gameManager.PlayerInfoManager.refreshCount += abilityScreen.abilityButtons_Second[i].value;
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void LateUpdate()
    {
        if (gameManager.PlayerInfoManager.userData.energy < gameManager.PlayerInfoManager.userData.maxEnergy && !isPaused && isUpdateComplete)
        {
            energyRechargingText.gameObject.SetActive(true);
            energyRechargingText.text = string.Format("{0:D2}:{1:D2}", Mathf.FloorToInt(remainder / 60), Mathf.FloorToInt(remainder % 60));
            remainder -= Time.deltaTime;
            if (remainder <= 0)
            {
                remainder = 0;
                updateDelayTimer += Time.deltaTime;
                if (updateDelayTimer > 1)
                {
                    updateDelayTimer = 0;
                    isUpdateComplete = false;
                    UpdateEnergyValue();
                }
            }
        }
        else
        {
            updateDelayTimer = 0;
            energyRechargingText.gameObject.SetActive(false);
        }
    }
    public void UpdateEnergyValue()
    {
        if (!Application.isEditor)
        {
            if (!gameManager.NetworkManager.NetworkCheck())
                return;

            if (gameManager.PlayerInfoManager.userData.energy >= gameManager.PlayerInfoManager.userData.maxEnergy)
            {
                ChargingEnergyCheck(true);
                return;
            }

            ChargingEnergyCheck(false);
            RemainderTimeCheck(); // 남은 시간 계산
        }
    }
    public void ChargingEnergyCheck(bool isJustUpdate)
    {
        if (isJustUpdate)
        {
            gameManager.TimeManager.calculateEnergyValue = 0;
        }
        else
        {
            gameManager.TimeManager.calculateEnergyValue = -1;
            gameManager.TimeManager.ChargingEnergyCalculate(gameManager.PlayerInfoManager.userData.rechargingTime);
        }
        StartCoroutine(ChargingEnergyCheckRoutine());
    }
    IEnumerator ChargingEnergyCheckRoutine()
    {
        while (gameManager.TimeManager.calculateEnergyValue == -1)
        {
            yield return null;
        }

        energyText.text = string.Format("{0} / {1}", gameManager.PlayerInfoManager.userData.energy, gameManager.PlayerInfoManager.userData.maxEnergy);
    }
    void RemainderTimeCheck()
    {
        gameManager.TimeManager.calculateRemainderValue = -1;
        gameManager.TimeManager.RemainderCalculate(gameManager.PlayerInfoManager.userData.rechargingTime);
        StartCoroutine(RemainderTimeCheckRoutine());
    }
    IEnumerator RemainderTimeCheckRoutine()
    {
        while (gameManager.TimeManager.calculateRemainderValue == -1)
        {
            yield return null;
        }
        remainder = 1800 - gameManager.TimeManager.calculateRemainderValue;
        if (!isUpdateComplete)
            isUpdateComplete = true;
    }
    public void PostCheck()
    {
        postScreen.PostCheck();
    }
    IEnumerator PostCheckRoutine()
    {
        while (!gameManager.FirebaseDBManager.postCheckDelay)
        {
            yield return null;
        }
        if (gameManager.PlayerInfoManager.postList.Count > gameManager.PlayerInfoManager.postHistroyList.Count - 1)
        {
            postAlertImage.SetActive(true);
            PostAlertTextUpdate();
        }
        else
        {
            postAlertImage.SetActive(false);
        }
    }
    public void PostAlertTextUpdate()
    {
        if(postScreen.posts.Count > 0)
        {
            postAlertImage.SetActive(true);
            postAlertText.text = postScreen.posts.Count.ToString();
        }
        else
            postAlertImage.SetActive(false);
    }
    public void OpenPostScreen()
    {
        lobbyPanels[4].transform.localScale = Vector3.one;
        foreach(PostBox box in postScreen.posts)
        {
            box.transform.SetParent(postScreen.gameObject.transform);
        }
    }
    public void ClosePostScreen()
    {
        lobbyPanels[4].transform.localScale = Vector3.zero;
    }
    public void UserDataCheck()
    {
        levelText.text = gameManager.PlayerInfoManager.userData.level.ToString();
        expSlider.value = ((float)gameManager.PlayerInfoManager.userData.exp / (float)gameManager.PlayerInfoManager.nextExp[gameManager.PlayerInfoManager.userData.level]);
        goldText.text = gameManager.PlayerInfoManager.userData.gold.ToString();
        crystalText.text = gameManager.PlayerInfoManager.userData.crystal.ToString();
    }
    public void GameStart(int index)
    {
        PlayTouchSound();
        if (!Application.isEditor)
        {
            if (!gameManager.NetworkManager.NetworkCheck())
                return;

            if (!loadGameSceneState)
            {
                if (gameManager.PlayerInfoManager.userData.energy >= gameManager.PlayerInfoManager.stageDatas[index].stageEnergy) // 게임 진행에 필요한 에너지 소모
                {
                    gameManager.PlayerInfoManager.selectedStageNum = index;
                    gameManager.PlayerInfoManager.userData.energy -= gameManager.PlayerInfoManager.stageDatas[index].stageEnergy;
                    gameManager.FirebaseDBManager.WriteUserData("energy", gameManager.PlayerInfoManager.userData.energy);
                    ExitStagePanelButton.interactable = false;
                    loadGameSceneState = true;
                    if (remainder == 0)
                        gameManager.TimeManager.RecharginTimeWrite(CalculateType.RechargingTime);
                    else
                        SceneManager.LoadScene("InGameScene");
                }
                else // 에너지 부족 안내 및 에너지 상점 이동
                {
                    lobbyPanels[3].SetActive(false);
                    lobbyPanels[1].SetActive(true);
                    storePanel.CloseAllPurchaseScreens();
                    storePanel.screens[4].SetActive(true);
                    energyNoticePanel.SetActive(true);
                }
            }
        }
        else
        {
            gameManager.PlayerInfoManager.selectedStageNum = index;
            ExitStagePanelButton.interactable = false;
            loadGameSceneState = true;
            SceneManager.LoadScene("InGameScene");
        }
    }
    public void NoticeButtonClicked(int scale)
    {
        lobbyPanels[6].transform.localScale = Vector3.one * scale;
    }
    public void CloseAllPanel()
    {
        for (int i = 1; i < lobbyPanels.Length; i++)
        {
            lobbyPanels[i].SetActive(false);
        }
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)            // 앱이 비활성화 되었을 때 처리
        {
            isPaused = true;
        }
        else
        {
            if (isPaused)                // 앱이 활성화 되었을 때 처리
            {
                UpdateEnergyValue();
                PostCheck();
                isPaused = false;
            }
        }
    }
    public void DeleteID()
    {
        gameManager.FirebaseDBManager.dataRemoveDelay = true;
        gameManager.FirebaseDBManager.RemoveID();
        StartCoroutine(DeleteIDRoutine());
    }
    IEnumerator DeleteIDRoutine()
    {
        while (gameManager.FirebaseDBManager.dataRemoveDelay)
        {
            yield return null;
        }        
        PlayerPrefs.DeleteKey("LogInData");
        Application.Quit();
    }
}
