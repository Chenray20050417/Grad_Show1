using UnityEngine;

public class SupplementPickup : MonoBehaviour
{
    [Header("補劑種類")]
    public SupplementType type;

    [Header("破掉特效")]
    public GameObject proteinEffect;
    public GameObject creatineEffect;
    public GameObject riceEffect;
    public GameObject testosteroneEffect;
    public GameObject vitaminEffect;

    private bool picked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        if (other.CompareTag("PushHitBox"))
        {
            picked = true;

            if (InventoryManager.Instance == null)
            {
                picked = false;
                return;
            }

            bool success = InventoryManager.Instance.AddItem(type);

            if (success)
{
    Debug.Log("顯示補劑說明：" + type);

    if (SupplementEffectUI.Instance != null)
    {
        SupplementEffectUI.Instance.ShowSupplementEffect(type);
    }
    else
    {
        Debug.LogError("找不到 SupplementEffectUI.Instance");
    }

    Destroy(gameObject);
}
        }
        else
        {
            BreakEffect();
            Destroy(gameObject);
        }
    }

    private void BreakEffect()
    {
        GameObject effect = null;

        switch (type)
        {
            case SupplementType.Protein:
                effect = proteinEffect;
                break;

            case SupplementType.Creatine:
                effect = creatineEffect;
                break;

            case SupplementType.Rice:
                effect = riceEffect;
                break;

            case SupplementType.Testosterone:
                effect = testosteroneEffect;
                break;

            case SupplementType.Vitamin:
                effect = vitaminEffect;
                break;
        }

        if (effect != null)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
        }
    }
}