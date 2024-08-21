using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public LevelUp uiLevelUp;
    
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textName;
    Text textLevel;
    Text textDesc;

    private void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textName = texts[0];
        textLevel = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;
    }
    private void OnEnable()
    {
        TextReWrite();
    }
    public void TextReWrite()
    {
        if (level >= data.damages.Length - 1)
            return;

        textLevel.text = "Lv." + (level + 1);

        switch (data.itemType)
        {
            case ItemData.ItemType.Ruler:
            case ItemData.ItemType.Spring:
            case ItemData.ItemType.Brush:
                textDesc.text = string.Format(data.itemDesc, data.counts[level + 1]);
                break;
            case ItemData.ItemType.Glue:
            case ItemData.ItemType.Broomstick:
                textDesc.text = string.Format(data.itemDesc, 100 + data.counts[level + 1]);
                break;
            case ItemData.ItemType.PencilSharpener:
            case ItemData.ItemType.Watch:
            case ItemData.ItemType.DrawingPaper:
            case ItemData.ItemType.WaterBottle:
            case ItemData.ItemType.RubberBand:
            case ItemData.ItemType.Magnet:
            case ItemData.ItemType.Shoe:
                textDesc.text = string.Format(data.itemDesc, data.damages[level + 1] * 100);
                break;
            default:
                textDesc.text = string.Format(data.itemDesc);
                break;
        }
    }
    public void OnClick()
    {
        switch (data.itemType)
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
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else
                {
                    weapon.LevelUp(data.damages[level + 1], data.counts[level + 1]);
                }
                level++;
                break;
            case ItemData.ItemType.PencilSharpener:
            case ItemData.ItemType.Watch:
            case ItemData.ItemType.DrawingPaper:
            case ItemData.ItemType.WaterBottle:
            case ItemData.ItemType.RubberBand:
            case ItemData.ItemType.Magnet:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    gear.LevelUp(data.damages[level + 1]);
                }
                level++;
                break;
            case ItemData.ItemType.Heal:
                InGameManager.instance.health = InGameManager.instance.maxHealth;
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
                GameObject newTranscendenceWeapon = new GameObject();
                weapon = newTranscendenceWeapon.AddComponent<Weapon>();
                weapon.Init(data);
                break;
        }

        TextReWrite();

        if (level == 5)
        {
            GetComponent<Button>().interactable = false;
        }
        uiLevelUp.SelectedItemDataUpdate(this);
    }
}
