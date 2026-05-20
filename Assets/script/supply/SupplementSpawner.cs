using UnityEngine;

public class SupplementSpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        FallToPlayer,
        Horizontal
    }

    [Header("補劑")]
    public GameObject[] supplementPrefabs;

    [Header("玩家")]
    public Transform player;

    [Header("生成設定")]
    public float spawnInterval = 1f;
    public float minX = 95f;
    public float maxX = 123f;
    public float spawnY = 280f;
    public SpawnMode spawnMode = SpawnMode.FallToPlayer;
    public bool spawnFromRight = true;
    public float horizontalSpawnX = 125f;
    public bool usePlayerCenteredHorizontalSpawn = false;
    public float horizontalSpawnOffset = 16f;
    public float horizontalMinY = 195f;
    public float horizontalMaxY = 195f;

    [Header("第三關左右交換")]
    public bool autoSwitchHorizontalSide = false;
    public float sideSwitchInterval = 20f;

    [Header("鎖定位置")]
    public float targetYOffset = 2.5f;

    [Header("數量")]
    public int minSpawnCount = 0;
    public int maxSpawnCount = 1;

    [Header("速度")]
    public float moveSpeed = 3.5f;

    private float timer = 0f;
    private float sideSwitchTimer = 0f;

    void Update()
    {
        UpdateHorizontalSideSwitch();

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnSupplement();
        }
    }

    void UpdateHorizontalSideSwitch()
    {
        if (!autoSwitchHorizontalSide || spawnMode != SpawnMode.Horizontal) return;

        sideSwitchTimer += Time.deltaTime;
        if (sideSwitchTimer < sideSwitchInterval) return;

        sideSwitchTimer = 0f;
        spawnFromRight = !spawnFromRight;
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
            GameObject prefab = supplementPrefabs[index];
            if (prefab == null) continue;

            Vector3 spawnPos = GetSpawnPosition();

            GameObject obj = Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity
            );

            SupplementMove move = obj.GetComponent<SupplementMove>();

            if (move != null)
            {
                move.moveMode = spawnMode == SpawnMode.Horizontal
                    ? SupplementMove.MoveMode.Direction
                    : SupplementMove.MoveMode.ChaseTarget;
                move.target = spawnMode == SpawnMode.Horizontal ? null : player;
                move.targetYOffset = targetYOffset;
                move.moveSpeed = moveSpeed;
                move.moveDirection = spawnFromRight ? Vector2.left : Vector2.right;
            }
            else
            {
                Debug.LogWarning("補劑 Prefab 沒有掛 SupplementMove");
            }
        }
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnMode == SpawnMode.Horizontal)
        {
            float y = Random.Range(horizontalMinY, horizontalMaxY);
            float x = GetHorizontalSpawnX();
            float z = player != null ? player.position.z : 0f;
            return new Vector3(x, y, z);
        }

        float randomX = Random.Range(minX, maxX);
        return new Vector3(randomX, spawnY, 0f);
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
