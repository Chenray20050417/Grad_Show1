using UnityEngine;

public class StressBallSpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        AimAtPlayer,
        Horizontal
    }

    [Header("壓力球")]
    public GameObject[] stressBallPrefabs;

    [Header("玩家")]
    public Transform player;

    [Header("生成設定")]
    public float spawnInterval = 2f;

    public float minX = 95f;
    public float maxX = 123f;
    public float spawnY = 215f;

    [Header("第三關水平飛行")]
    public SpawnMode spawnMode = SpawnMode.AimAtPlayer;
    public bool spawnFromRight = true;
    public float horizontalSpawnX = 170f;
    public bool usePlayerCenteredHorizontalSpawn = false;
    public float horizontalSpawnOffset = 16f;
    public float horizontalMinY = 188f;
    public float horizontalMaxY = 208f;

    [Header("第三關左右交換")]
    public bool autoSwitchHorizontalSide = false;
    public float sideSwitchInterval = 20f;

    [Header("鎖定位置")]
    public float targetYOffset = 2.5f; // 鎖定主角頭上方

    [Header("數量")]
    public int minSpawnCount = 1;
    public int maxSpawnCount = 1;

    [Header("速度")]
    public float moveSpeed = 8f;

    private float sideSwitchTimer = 0f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnBall), 1f, spawnInterval);
    }

    void Update()
    {
        if (!autoSwitchHorizontalSide || spawnMode != SpawnMode.Horizontal) return;

        sideSwitchTimer += Time.deltaTime;
        if (sideSwitchTimer < sideSwitchInterval) return;

        sideSwitchTimer = 0f;
        spawnFromRight = !spawnFromRight;
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

            Vector3 spawnPos = GetSpawnPosition();

            GameObject ball = Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity
            );

            StressBall sb = ball.GetComponent<StressBall>();

            if (sb != null)
            {
                sb.moveSpeed = moveSpeed;
                sb.SetDirection(GetMoveDirection(spawnPos));
            }
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnMode == SpawnMode.Horizontal)
        {
            float y = Random.Range(horizontalMinY, horizontalMaxY);
            float x = GetHorizontalSpawnX();

            return new Vector3(x, y, player.position.z);
        }

        float randomX = Random.Range(minX, maxX);

        return new Vector3(
            randomX,
            spawnY,
            player.position.z
        );
    }

    private Vector2 GetMoveDirection(Vector3 spawnPos)
    {
        if (spawnMode == SpawnMode.Horizontal)
            return spawnFromRight ? Vector2.left : Vector2.right;

        Vector2 targetPos = new Vector2(
            player.position.x,
            player.position.y + targetYOffset
        );

        return targetPos - (Vector2)spawnPos;
    }

    private float GetHorizontalSpawnX()
    {
        if (!usePlayerCenteredHorizontalSpawn)
            return spawnFromRight ? horizontalSpawnX : -horizontalSpawnX;

        float centerX = player != null ? player.position.x : 0f;
        float offset = Mathf.Abs(horizontalSpawnOffset);
        return centerX + (spawnFromRight ? offset : -offset);
    }
}
