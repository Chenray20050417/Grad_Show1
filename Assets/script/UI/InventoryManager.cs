using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("物品欄設定")]
    public int maxItemCount = 3;

    [Header("物品欄圖片")]
    public Image[] slotImages;

    [Header("物品欄 Slot")]
    public RectTransform[] slots;

    [Header("補劑圖示")]
    public Sprite proteinIcon;
    public Sprite creatineIcon;
    public Sprite riceIcon;
    public Sprite testosteroneIcon;
    public Sprite vitaminIcon;

    [Header("目前數量")]
    public int currentItemCount;

    private List<SupplementType> items = new List<SupplementType>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseItem(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseItem(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseItem(2);
    }

    public bool AddItem(SupplementType type)
    {
        if (items.Count >= maxItemCount)
        {
            Debug.Log("物品欄滿了");
            return false;
        }

        items.Add(type);
        currentItemCount = items.Count;

        UpdateUI();

        Debug.Log("加入物品欄：" + type);
        return true;
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.Log("這格沒有道具：" + (index + 1));
            return;
        }

        SupplementType type = items[index];

        Debug.Log("使用補劑：" + type);

        ApplyEffect(type);

     
        items.RemoveAt(index);
        currentItemCount = items.Count;

        UpdateUI();
    }

    private void ApplyEffect(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                if (GameTimer.Instance != null)
                {
                    GameTimer.Instance.AddTime(10f);
                    Debug.Log("蛋白粉效果：時間 +10 秒");
                }
                else
                {
                    Debug.LogError("找不到 GameTimer.Instance");
                }
                break;

            case SupplementType.Creatine:
                if (HealthUI.Instance != null)
                {
                    HealthUI.Instance.HealPercent(0.15f);
                    Debug.Log("肌酸效果：回血 15%");
                }
                break;

            case SupplementType.Rice:
                Debug.Log("白飯保留");
                break;

            case SupplementType.Testosterone:
                Debug.Log("睪固酮效果：無敵 5 秒");
                break;

            case SupplementType.Vitamin:
                Debug.Log("維他命保留");
                break;
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < items.Count)
            {
                slotImages[i].sprite = GetIcon(items[i]);
                slotImages[i].enabled = true;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = false;
            }
        }
    }

public Sprite GetIcon(SupplementType type)
    {
        switch (type)
        {
            case SupplementType.Protein:
                return proteinIcon;

            case SupplementType.Creatine:
                return creatineIcon;

            case SupplementType.Rice:
                return riceIcon;

            case SupplementType.Testosterone:
                return testosteroneIcon;

            case SupplementType.Vitamin:
                return vitaminIcon;

            default:
                return null;
        }
    }
}