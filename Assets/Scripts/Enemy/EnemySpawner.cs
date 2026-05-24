using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private GameManager gameManager;
    private Transform player;
    private float nextSpawnTime;
    private bool isSpawning;
    private int activeEnemyCount;

    private const float SpawnPadding = 1.4f;

    public void Initialize(GameManager manager, Transform playerTransform)
    {
        gameManager = manager;
        player = playerTransform;
        isSpawning = true;
        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (!isSpawning || gameManager == null || gameManager.IsGameOver || Time.timeScale == 0f)
        {
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            int diff = gameManager.DifficultyLevel;
            float interval = GetSpawnInterval(diff);
            nextSpawnTime = Time.time + interval;
            if (activeEnemyCount < GetMaxEnemies(diff))
            {
                SpawnEnemy(diff);
            }
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
    }

    private float GetSpawnInterval(int diff)
    {
        if (diff == 0) return 2.0f;
        if (diff == 1) return 1.7f;
        if (diff == 2) return 1.35f;
        return 1.1f;
    }

    private int GetMaxEnemies(int diff)
    {
        if (diff == 0) return 10;
        if (diff == 1) return 13;
        if (diff == 2) return 16;
        return 20;
    }

    private float GetHpMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.2f;
        if (diff == 2) return 1.5f;
        return 1.8f;
    }

    private float GetDamageMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.1f;
        if (diff == 2) return 1.25f;
        return 1.4f;
    }

    private float GetSpeedMultiplier(int diff)
    {
        if (diff == 0) return 1.0f;
        if (diff == 1) return 1.05f;
        if (diff == 2) return 1.12f;
        return 1.2f;
    }

    private void SpawnEnemy(int diff)
    {
        Vector3 position = GetRandomEdgePosition();
        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.transform.position = position;

        EnemyController enemy = enemyObject.AddComponent<EnemyController>();

        float hpMul = GetHpMultiplier(diff);
        float dmgMul = GetDamageMultiplier(diff);
        float spdMul = GetSpeedMultiplier(diff);

        bool isElite = false;
        float eliteChance = 0f;
        if (gameManager.SurvivalTime >= 45f) eliteChance = 0.2f;
        else if (gameManager.SurvivalTime >= 30f) eliteChance = 0.1f;
        if (Random.value < eliteChance) isElite = true;

        if (isElite)
        {
            hpMul *= 1.8f;
            dmgMul *= 1.5f;
            spdMul *= 0.9f;
        }

        bool spawnRanged = gameManager.SurvivalTime > 15f && Random.value < 0.2f;

        if (spawnRanged)
        {
            int hp = Mathf.RoundToInt(25f * hpMul);
            int dmg = Mathf.RoundToInt(8f * dmgMul);
            int exp = Mathf.RoundToInt(15f * (isElite ? 2f : 1f));
            float spd = 3f * spdMul;
            float size = isElite ? 0.95f : 0.7f;
            Color eliteColor = isElite ? new Color(1f, 0.2f, 0.1f) : new Color(0.85f, 0.35f, 0.15f);

            enemy.Initialize(gameManager, this, player, isElite ? "Elite Archer" : "Archer", hp, spd, dmg, exp, eliteColor, size, null);
            enemy.SetRangedBehavior(5f, 1.5f, Mathf.RoundToInt(8f * dmgMul), 5f, 3f);
        }
        else
        {
            bool spawnBat = Random.value > 0.5f;

            if (spawnBat)
            {
                int hp = Mathf.RoundToInt(30f * hpMul);
                int dmg = Mathf.RoundToInt(8f * dmgMul);
                int exp = Mathf.RoundToInt(12f * (isElite ? 2f : 1f));
                float spd = 4f * spdMul;
                float size = isElite ? 1.15f : 0.85f;
                Color eliteColor = isElite ? new Color(1f, 0.25f, 0.55f) : new Color(0.6f, 0.18f, 0.9f);

                enemy.Initialize(gameManager, this, player, isElite ? "Elite Bat" : "Bat", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Bat"));
            }
            else
            {
                int hp = Mathf.RoundToInt(110f * hpMul);
                int dmg = Mathf.RoundToInt(10f * dmgMul);
                int exp = Mathf.RoundToInt(10f * (isElite ? 2f : 1f));
                float spd = 2.5f * spdMul;
                float size = isElite ? 1.0f : 0.75f;
                Color eliteColor = isElite ? new Color(0.9f, 0.7f, 0.15f) : new Color(0.1f, 0.85f, 0.25f);

                enemy.Initialize(gameManager, this, player, isElite ? "Elite Slime" : "Slime", hp, spd, dmg, exp, eliteColor, size, GameData.GetEnemySprite("Slime"));
            }
        }

        RegisterEnemy();
    }

    private Vector3 GetRandomEdgePosition()
    {
        int side = Random.Range(0, 4);
        float x;
        float y;

        if (side == 0)
        {
            x = Random.Range(GameUtility.MinX + SpawnPadding, GameUtility.MaxX - SpawnPadding);
            y = GameUtility.MaxY - SpawnPadding;
        }
        else if (side == 1)
        {
            x = Random.Range(GameUtility.MinX + SpawnPadding, GameUtility.MaxX - SpawnPadding);
            y = GameUtility.MinY + SpawnPadding;
        }
        else if (side == 2)
        {
            x = GameUtility.MinX + SpawnPadding;
            y = Random.Range(GameUtility.MinY + SpawnPadding, GameUtility.MaxY - SpawnPadding);
        }
        else
        {
            x = GameUtility.MaxX - SpawnPadding;
            y = Random.Range(GameUtility.MinY + SpawnPadding, GameUtility.MaxY - SpawnPadding);
        }

        return new Vector3(x, y, 0f);
    }
}
