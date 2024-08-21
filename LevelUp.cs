using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelUp : MonoBehaviour
{
    GameManager gameManager;
    InGameManager inGameManager;
    RectTransform rect;

    public List<Item> ItemList = new List<Item>();              // 기본 아이템들의 리스트
    [SerializeField]
    List<Item> items = new List<Item>();                        // 레벨업 시 보이는 아이템들의 리스트
    public List<Item> transcendenceWeapons = new List<Item>();  // 초월 무기 아이템들의 리스트
    List<int> completedItems = new List<int>();                 // 아이템의 레벨이 최고레벨일 경우 해당 아이템 ID를 저장하는 리스트
    List<int> combinedItems = new List<int>();                 // 초월 조합을 마칭 아이템 ID를 저장하는 리스트

    public GameObject selectedItemPref;

    public GameObject selectedWeaponBox;
    public GameObject selectedWeaponBox_Pause;
    List<SelectedItem> selectedWeapon = new List<SelectedItem>();           // 유저가 고른 무기 아이템을 보여주는 UI 오브젝트 리스트
    List<SelectedItem> selectedWeapon_Pause = new List<SelectedItem>();     // Pause창에서 유저가 고른 무기 아이템을 보여주는 UI 오브젝트 리스트

    public GameObject selectedGearBox;
    public GameObject selectedGearBox_Pause;
    List<SelectedItem> selectedGear = new List<SelectedItem>();             // 유저가 고른 지원 아이템을 보여주는 UI 오브젝트 리스트    
    List<SelectedItem> selectedGear_Pause = new List<SelectedItem>();       // Pause창에서유저가 고른 지원 아이템을 보여주는 UI 오브젝트 리스트    

    public Button refreshButton;

    private void Awake()
    {
        gameManager = GameManager.instance;
        inGameManager = InGameManager.instance;
        rect = GetComponent<RectTransform>();
        items.AddRange(ItemList);
    }

    public void Show()
    {
        rect.localScale = Vector3.one;
        inGameManager.Stop();
        if (inGameManager.refreshCount > 0)
            EnableRefreshButton();
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.LevelUp);
        gameManager.AudioManager.EffectBGM(true);
    }
    void EnableRefreshButton()
    {
        refreshButton.interactable = true;
        refreshButton.gameObject.GetComponent<Image>().color = new Color32(104, 157, 238, 200);
        refreshButton.gameObject.GetComponentInChildren<Text>().color = new Color32(255, 255, 255, 255);
    }
    void DisableRefreshButton()
    {
        refreshButton.interactable = false;
        refreshButton.gameObject.GetComponent<Image>().color = new Color32(100, 100, 100, 100);
        refreshButton.gameObject.GetComponentInChildren<Text>().color = new Color32(0, 0, 0, 100);
    }
    public void Hide()
    {
        rect.localScale = Vector3.zero;
        inGameManager.Resume();
        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Select);
        gameManager.AudioManager.EffectBGM(false);
    }
    public void Select(int index)
    {
        items[index].OnClick();
    }
    public void Next()
    {
        foreach (Item item in items)
            item.gameObject.SetActive(false);

        List<Item> cloneList = new List<Item>();
        cloneList = items.ToList();
        int trascendenceWeaponAmont = 0;
        if (cloneList.Count > 3)
            cloneList.Remove(cloneList.Find(x => x.data.itemId == 100));
        foreach (Item item in items)
        {
            if (item.data.itemId >= 41 && item.data.itemId < 60 && trascendenceWeaponAmont < 3)
            {
                item.gameObject.SetActive(true);
                cloneList.Remove(item);
                trascendenceWeaponAmont++;
            }
        }
        for (int i = 0; i < MathF.Min(3, items.Count) - trascendenceWeaponAmont; i++)
        {
            int rand = Random.Range(0, cloneList.Count);
            cloneList[rand].gameObject.SetActive(true);
            cloneList.RemoveAt(rand);
        }
    }
    public void SelectedItemInfoUpdate(Item item, List<SelectedItem> list, GameObject box, int index)
    {
        SelectedItem newItem = Instantiate(selectedItemPref, box.transform).GetComponent<SelectedItem>();
        newItem.id = item.data.itemId;
        newItem.icon.sprite = item.data.itemIcon;
        newItem.levelBar[0].SetActive(true);
        newItem.SetColor(index);
        list.Add(newItem);
    }
    public void SelectedItemDataUpdate(Item clickeditem)
    {
        Item item = items.Find(x => x == clickeditem);
        switch (item.data.itemType)
        {
            case ItemData.ItemType.Ruler:
            case ItemData.ItemType.Eraser:
            case ItemData.ItemType.Pencil:
            case ItemData.ItemType.Glue:
            case ItemData.ItemType.Clip:
            case ItemData.ItemType.Chalk:
            case ItemData.ItemType.Spring:
            case ItemData.ItemType.Brush:
            case ItemData.ItemType.Broomstick:
                if (item.level == 1)
                {
                    SelectedItemInfoUpdate(item, selectedWeapon, selectedWeaponBox, 0);
                    SelectedItemInfoUpdate(item, selectedWeapon_Pause, selectedWeaponBox_Pause, 0);

                    if (selectedWeapon.Count == 5)
                        ExceptUnselectedWeapon();
                }
                else
                {
                    int index = selectedWeapon.FindIndex(x => x.id == item.data.itemId);
                    if (index != -1)
                    {
                        for (int i = 0; i < item.level; i++)
                        {
                            selectedWeapon[index].levelBar[i].SetActive(true);
                            selectedWeapon_Pause[index].levelBar[i].SetActive(true);
                        }
                    }
                    if (item.level == 5)
                    {
                        Item completedItem = items.Find(x => x.data.itemId == item.data.itemId);
                        completedItems.Add(completedItem.data.itemId);
                        items.Remove(completedItem);
                        completedItem.gameObject.SetActive(false);
                        CompletedItemDataCheck();
                    }
                }
                break;
            case ItemData.ItemType.PencilSharpener:
            case ItemData.ItemType.Watch:
            case ItemData.ItemType.DrawingPaper:
            case ItemData.ItemType.WaterBottle:
            case ItemData.ItemType.RubberBand:
            case ItemData.ItemType.Magnet:
            case ItemData.ItemType.Shoe:
                if (item.level == 1)
                {
                    SelectedItemInfoUpdate(item, selectedGear, selectedGearBox, 1);
                    SelectedItemInfoUpdate(item, selectedGear_Pause, selectedGearBox_Pause, 1);

                    if (selectedGear.Count == 5)
                        ExceptUnselectedGear();
                }
                else
                {
                    int index = selectedGear.FindIndex(x => x.id == item.data.itemId);
                    if (index != -1)
                    {
                        for (int i = 0; i < item.level; i++)
                        {
                            selectedGear[index].levelBar[i].SetActive(true);
                            selectedGear_Pause[index].levelBar[i].SetActive(true);
                        }
                    }
                    if (item.level == 5)
                    {
                        Item completedItem = items.Find(x => x.data.itemId == item.data.itemId);
                        completedItems.Add(completedItem.data.itemId);
                        items.Remove(completedItem);
                        completedItem.gameObject.SetActive(false);
                        CompletedItemDataCheck();
                    }
                }
                break;
            case ItemData.ItemType.Compass:
            case ItemData.ItemType.MeasuringTape:
            case ItemData.ItemType.BlackboardEraser:
            case ItemData.ItemType.SoccerBall:
            case ItemData.ItemType.BallpointPen:
            case ItemData.ItemType.SharpPencil:
            case ItemData.ItemType.Stapler:
            case ItemData.ItemType.SuperGlue:
            case ItemData.ItemType.SketchBook:
            case ItemData.ItemType.Palette:
            case ItemData.ItemType.Mop:
                for (int i = 0; i < item.data.combination.Length; i++)
                {
                    SelectedItem beforeSelected = selectedWeapon.Find(x => x.id == item.data.combination[i]);
                    selectedWeapon.Remove(beforeSelected);
                    Destroy(beforeSelected.gameObject);

                    SelectedItem beforeSelected_pause = selectedWeapon_Pause.Find(x => x.id == item.data.combination[i]);
                    selectedWeapon_Pause.Remove(beforeSelected_pause);
                    Destroy(beforeSelected_pause.gameObject);

                    Weapon beforeWeapon = inGameManager.player.weapons.Find(x => x.id == item.data.combination[i]);
                    inGameManager.player.weapons.Remove(beforeWeapon);
                    Destroy(beforeWeapon.gameObject);

                    int beforeCompletedItemID = completedItems.Find(x => x == item.data.combination[i]);
                    combinedItems.Add(beforeCompletedItemID);
                    completedItems.Remove(beforeCompletedItemID);
                }
                Item trascendenceWeaponItem = items.Find(x => x.data.itemId == item.data.itemId);
                trascendenceWeaponItem.gameObject.SetActive(false);
                items.Remove(trascendenceWeaponItem);

                SelectedItemInfoUpdate(item, selectedWeapon, selectedWeaponBox, 2);
                SelectedItemInfoUpdate(item, selectedWeapon_Pause, selectedWeaponBox_Pause, 2);

                if (selectedWeapon.Count < 5)
                    ResetWeaponList();

                RemoveTranscendenceWeapon();
                break;
            default:
                break;
        }
        Next();
        Hide();
    }
    void RemoveWeaponItems()
    {
        for (int i = 0; i < 20; i++)         // 무기 아이템의 ID 값 범위
        {
            int index = items.FindIndex(x => x.data.itemId == i);
            if (index != -1)
                items.RemoveAt(index);
        }
    }
    void RemoveGearItems()
    {
        for (int i = 21; i < 40; i++)         // 지원 아이템의 ID 값 범위
        {
            int index = items.FindIndex(x => x.data.itemId == i);
            if (index != -1)
                items.RemoveAt(index);
        }
    }
    void ResetWeaponList()
    {
        RemoveWeaponItems();
        for (int i = 0; i < 20; i++)         // 무기 아이템의 ID 값 범위
        {
            int index = ItemList.FindIndex(x => x.data.itemId == i);
            if (index != -1)
            {
                if (completedItems.FindIndex(x => x == i) == -1 && combinedItems.FindIndex(x => x == i) == -1)
                    items.Add(ItemList[index]);
            }
        }
        Next();
    }
    void ExceptUnselectedWeapon()
    {
        List<Item> itemsClone = new List<Item>();
        foreach (SelectedItem select in selectedWeapon)
        {
            foreach (Item item in items)
            {
                if (item.data.itemId == select.id)
                    itemsClone.Add(item);

                item.gameObject.SetActive(false);
            }
        }
        RemoveWeaponItems();
        items.AddRange(itemsClone);
        Next();
    }
    void ExceptUnselectedGear()
    {
        List<Item> itemsClone = new List<Item>();
        foreach (SelectedItem select in selectedGear)
        {
            foreach (Item item in items)
            {
                if (item.data.itemId == select.id)
                    itemsClone.Add(item);

                item.gameObject.SetActive(false);
            }
        }
        RemoveGearItems();
        items.AddRange(itemsClone);
        Next();
    }
    public void RemoveTranscendenceWeapon()
    {
        for (int i = 41; i < 60; i++)         // 초월 무기 아이템의 ID 값 범위
        {
            int index = items.FindIndex(x => x.data.itemId == i);
            if (index != -1)
            {
                items[index].gameObject.SetActive(false);
                items.RemoveAt(index);
            }
        }
        CompletedItemDataCheck();
    }
    void CompletedItemDataCheck()
    {
        if (completedItems.Contains(0))     // 막대자
        {
            if (completedItems.Contains(2))         // 컴퍼스
            {
                if (items.FindIndex(x => x.data.itemId == 41) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 41));
            }
            if (completedItems.Contains(25))         // 줄자
            {
                if (items.FindIndex(x => x.data.itemId == 42) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 42));
            }
        }
        if (completedItems.Contains(1))     // 지우개
        {
            if (completedItems.Contains(5))
            {
                if (items.FindIndex(x => x.data.itemId == 43) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 43));
            }
            if (completedItems.Contains(27))
            {
                if (items.FindIndex(x => x.data.itemId == 44) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 44));
            }
        }
        if (completedItems.Contains(2))     // 연필
        {
            if (completedItems.Contains(6))
            {
                if (items.FindIndex(x => x.data.itemId == 45) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 45));
            }
            if (completedItems.Contains(21))
            {
                if (items.FindIndex(x => x.data.itemId == 46) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 46));
            }
        }
        if (completedItems.Contains(3))     // 풀
        {
            if (completedItems.Contains(22))
            {
                if (items.FindIndex(x => x.data.itemId == 47) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 47));
            }
        }
        if (completedItems.Contains(4))     // 클립
        {
            if (completedItems.Contains(26))
            {
                if (items.FindIndex(x => x.data.itemId == 48) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 48));
            }
        }
        if (completedItems.Contains(6))     // 스프링
        {
            if (completedItems.Contains(23))
            {
                if (items.FindIndex(x => x.data.itemId == 49) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 49));
            }
        }
        if (completedItems.Contains(7))     // 붓
        {
            if (completedItems.Contains(24))
            {
                if (items.FindIndex(x => x.data.itemId == 50) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 50));
            }
        }
        if (completedItems.Contains(8))     // 빗자루
        {
            if (completedItems.Contains(24))
            {
                if (items.FindIndex(x => x.data.itemId == 51) == -1)
                    items.Add(transcendenceWeapons.Find(x => x.data.itemId == 51));
            }
        }
    }
    public void RefreshButtonClicked()
    {
        List<Item> cloneList = new List<Item>();
        cloneList = items.ToList();

        int exceptItemCount = 0;
        foreach (Item item in items)
        {
            if (item.gameObject.activeSelf)
            {
                cloneList.Remove(item);
                item.gameObject.SetActive(false);
                exceptItemCount++;
            }
        }

        if (cloneList.Count > 3)
            cloneList.Remove(cloneList.Find(x => x.data.itemId == 100));

        for (int i = 0; i < MathF.Min(3, items.Count - exceptItemCount); i++)
        {
            int rand = Random.Range(0, cloneList.Count);
            cloneList[rand].gameObject.SetActive(true);
            cloneList.RemoveAt(rand);
        }

        inGameManager.refreshCount--;
        inGameManager.refreshCountText.text = string.Format("{0} / {1}", inGameManager.refreshCount, inGameManager.maxRefreshCount);
        if (inGameManager.refreshCount <= 0)
        {
            DisableRefreshButton();
        }

        gameManager.AudioManager.PlaySfx(AudioManager.Sfx.Select);
    }
}
