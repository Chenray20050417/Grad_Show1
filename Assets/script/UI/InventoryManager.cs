using UnityEngine;
using UnityEngine.UI;

public enum SupplementType
{
    Protein,
    Creatine,
    Rice,
    Testosterone,
    Vitamin
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slot Icon")]
    public Image[] slotImages = new Image[3];

    [Header("補劑 Icon")]
    public Sprite proteinIcon;
    public Sprite creatineIcon;
    public Sprite riceIcon;
    public Sprite testosteroneIcon;
    public Sprite vitaminIcon;

    private SupplementType[] items = new SupplementType[3];
    private bool[] hasItem = new bool[3];

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ClearUI();
    }

    public bool AddItem(SupplementType type)
    {
        for (int i = 0; i < 3; i++)
        {
            if (!hasItem[i])
            {
                items[i] = type;
                hasItem[i] = true;

                UpdateUI();

                Debug.Log("加入道具到第 " + (i + 1) + " 格：" + type);
                return true;
            }
        }

        Debug.Log("物品欄滿了");
        return false;
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= 3)
            return;

        if (!hasItem[index])
        {
            Debug.Log("第 " + (index + 1) + " 格沒有道具");
            return;
        }

        Debug.Log("使用第 " + (index + 1) + " 格道具：" + items[index]);

        hasItem[index] = false;
        items[index] = default;

        UpdateUI();
    }

    void UpdateUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slotImages[i] == null)
                continue;

            if (hasItem[i])
            {
                slotImages[i].enabled = true;
                slotImages[i].sprite = GetIcon(items[i]);
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].enabled = false;
                slotImages[i].sprite = null;
            }
        }
    }

    void ClearUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].enabled = false;
                slotImages[i].sprite = null;
            }
        }
    }

    Sprite GetIcon(SupplementType type)
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