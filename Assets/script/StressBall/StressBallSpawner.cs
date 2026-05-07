using UnityEngine;

public class StressBallSpawner : MonoBehaviour
{
    [Header("壓力球")]
    public GameObject[] stressBallPrefabs;

    [Header("玩家")]
    public Transform player;

    [Header("生成設定")]
    public float spawnInterval = 2f;

    public float minX = 95f;
    public float maxX = 123f;
    public float spawnY = 215f;

    [Header("數量")]
    public int minSpawnCount = 1;
    public int maxSpawnCount = 1;

    [Header("速度")]
    public float moveSpeed = 8f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnBall), 1f, spawnInterval);
    }

    void SpawnBall()
    {
        if (stressBallPrefabs == null || stressBallPrefabs.Length == 0)
        {
            Debug.Log("沒有壓力球 Prefab");
            return;
        }

        if (player == null)
        {
            Debug.Log("沒有指定 Player");
            return;
        }

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab =
                stressBallPrefabs[Random.Range(0, stressBallPrefabs.Length)];

            if (prefab == null) continue;

            float randomX = Random.Range(minX, maxX);

            Vector3 spawnPos = new Vector3(
                randomX,
                spawnY,
                player.position.z
            );

            GameObject ball = Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity
            );

            StressBall sb = ball.GetComponent<StressBall>();

            if (sb != null)
            {
                sb.moveSpeed = moveSpeed;

                Vector2 dir = player.position - spawnPos;
                sb.SetDirection(dir);
            }
        }
    }
}