using UnityEngine;

public class SupplementSpawner : MonoBehaviour
{
    [Header("補劑")]
    public GameObject[] supplementPrefabs;

    [Header("玩家")]
    public Transform player;

    [Header("生成設定")]
    public float spawnInterval = 1f;
    public float minX = 95f;
    public float maxX = 123f;
    public float spawnY = 280f;

    [Header("鎖定位置")]
    public float targetYOffset = 2.5f;

    [Header("數量")]
    public int minSpawnCount = 0;
    public int maxSpawnCount = 1;

    [Header("速度")]
    public float moveSpeed = 3.5f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnSupplement();
        }
    }

    void SpawnSupplement()
    {
        if (supplementPrefabs == null || supplementPrefabs.Length == 0)
        {
            Debug.LogWarning("沒有設定 supplementPrefabs");
            return;
        }

        int count = Random.Range(minSpawnCount, maxSpawnCount + 1);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, supplementPrefabs.Length);

            float randomX = Random.Range(minX, maxX);

            Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);

            GameObject obj = Instantiate(
                supplementPrefabs[index],
                spawnPos,
                Quaternion.identity
            );

            SupplementMove move = obj.GetComponent<SupplementMove>();

            if (move != null)
            {
                move.target = player;
                move.targetYOffset = targetYOffset;
                move.moveSpeed = moveSpeed;
            }
            else
            {
                Debug.LogWarning("補劑 Prefab 沒有掛 SupplementMove");
            }
        }
    }
}