using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public CharacterStats PlayerStats { get; private set; }
    public PlayerController PlayerController { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }
    public int Level { get; private set; }
    public int CurrentExp { get; private set; }
    public int NextLevelExp { get { return 20 + Level * 15; } }
    public bool IsGameOver { get; private set; }
    public bool IsVictory { get; private set; }
    public float SurvivalTime { get; private set; }
    public float SurvivalTarget { get; private set; } = 60f;
    public int KillCount { get; private set; }
    public bool IsPaused { get; private set; }

    public int DifficultyLevel
    {
        get
        {
            if (SurvivalTime < 15f) return 0;
            if (SurvivalTime < 30f) return 1;
            if (SurvivalTime < 45f) return 2;
            return 3;
        }
    }

    private GameUI gameUI;
    private UpgradeManager upgradeManager;
    private EnemySpawner enemySpawner;
    private Camera mainCamera;
    private Vector3 cameraShakeOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Level = 1;
        CurrentExp = 0;
        KillCount = 0;
        PlayerStats = GameData.GetSelectedCharacterStats();

        SetupCamera();
        CreateGround();
        CreateArenaBounds();
        CreatePlayer();
        CreateManagersAndUI();
    }

    private void LateUpdate()
    {
        if (mainCamera != null && PlayerController != null)
        {
            Vector3 playerPosition = PlayerController.transform.position;
            mainCamera.transform.position = new Vector3(playerPosition.x, playerPosition.y, -10f) + cameraShakeOffset;
        }
    }

    private void Update()
    {
        if (!IsGameOver && !IsVictory && !IsPaused)
        {
            SurvivalTime += Time.deltaTime;
            if (SurvivalTime >= SurvivalTarget)
            {
                Victory();
            }
        }
    }

    public void AddExperience(int amount)
    {
        if (IsGameOver || IsVictory)
        {
            return;
        }

        CurrentExp += amount;
        int pendingUpgradeCount = 0;
        while (CurrentExp >= NextLevelExp)
        {
            CurrentExp -= NextLevelExp;
            Level++;
            pendingUpgradeCount++;
        }

        RefreshUI();
        if (pendingUpgradeCount > 0 && upgradeManager != null)
        {
            upgradeManager.QueueUpgradeChoices(pendingUpgradeCount);
        }
    }

    public void AddKill()
    {
        if (!IsGameOver && !IsVictory)
        {
            KillCount++;
        }
    }

    public void TogglePause()
    {
        if (IsGameOver || IsVictory)
        {
            return;
        }

        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
    }

    public void RefreshUI()
    {
        if (gameUI != null)
        {
            gameUI.Refresh();
        }
    }

    public void GameOver()
    {
        if (IsGameOver || IsVictory)
        {
            return;
        }

        IsGameOver = true;
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        CleanupCombatObjects();
        Time.timeScale = 0f;
        if (gameUI != null)
        {
            gameUI.ShowGameOverPanel();
        }
    }

    public void Victory()
    {
        if (IsGameOver || IsVictory)
        {
            return;
        }

        IsVictory = true;
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        CleanupCombatObjects();
        Time.timeScale = 0f;
        if (gameUI != null)
        {
            gameUI.ShowVictoryPanel();
        }
    }

    public void ShakeCamera(float duration, float strength)
    {
        StartCoroutine(CameraShakeRoutine(duration, strength));
    }

    private System.Collections.IEnumerator CameraShakeRoutine(float duration, float strength)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float decay = 1f - (elapsed / duration);
            cameraShakeOffset = new Vector3(
                Random.Range(-1f, 1f) * strength * decay,
                Random.Range(-1f, 1f) * strength * decay,
                0f);
            yield return null;
        }
        cameraShakeOffset = Vector3.zero;
    }

    private void SetupCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 7.8f;
        mainCamera.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void CreateGround()
    {
        GameObject ground = new GameObject("Arcane Ruins Floor");
        SpriteRenderer renderer = ground.AddComponent<SpriteRenderer>();
        Sprite floorSprite = GameData.GetMapSprite("floor_tile");
        renderer.sprite = floorSprite != null ? floorSprite : GameData.GetSquareSprite();
        renderer.color = floorSprite != null ? Color.white : new Color(0.10f, 0.11f, 0.16f);
        renderer.sortingOrder = -10;
        if (floorSprite != null)
        {
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(24f, 14f);
            ground.transform.localScale = Vector3.one;
        }
        else
        {
            ground.transform.localScale = new Vector3(24f, 14f, 1f);
        }
    }

    private void CreateArenaBounds()
    {
        Color wallColor = new Color(0.18f, 0.19f, 0.26f);
        CreateSolidBlock("Wall Top", new Vector3(0f, 7f, 0f), new Vector3(24f, 1f, 1f), wallColor, -6, GameData.GetMapSprite("wall_tile"));
        CreateSolidBlock("Wall Bottom", new Vector3(0f, -7f, 0f), new Vector3(24f, 1f, 1f), wallColor, -6, GameData.GetMapSprite("wall_tile"));
        CreateSolidBlock("Wall Left", new Vector3(-12f, 0f, 0f), new Vector3(1f, 14f, 1f), wallColor, -6, GameData.GetMapSprite("wall_tile"));
        CreateSolidBlock("Wall Right", new Vector3(12f, 0f, 0f), new Vector3(1f, 14f, 1f), wallColor, -6, GameData.GetMapSprite("wall_tile"));

        Color obstacleColor = new Color(0.24f, 0.25f, 0.34f);
        Sprite obstacleSprite = GameData.GetMapSprite("obstacle_block");
        CreateSolidBlock("Broken Altar 1", new Vector3(-5f, 3f, 0f), new Vector3(4f, 1f, 1f), obstacleColor, -5, obstacleSprite);
        CreateSolidBlock("Broken Altar 2", new Vector3(5f, 1f, 0f), new Vector3(3f, 1f, 1f), obstacleColor, -5, obstacleSprite);
        CreateSolidBlock("Broken Pillar 1", new Vector3(-7f, -3f, 0f), new Vector3(3f, 1f, 1f), obstacleColor, -5, obstacleSprite);
        CreateSolidBlock("Broken Pillar 2", new Vector3(4f, -4f, 0f), new Vector3(3f, 1f, 1f), obstacleColor, -5, obstacleSprite);
        CreateSolidBlock("Central Altar", new Vector3(0f, 0f, 0f), new Vector3(2f, 1f, 1f), obstacleColor, -5, obstacleSprite);

        CreateRuinsDecorations();
    }

    private void CreateSolidBlock(string blockName, Vector3 position, Vector3 scale, Color color, int sortingOrder, Sprite sprite)
    {
        GameObject block = new GameObject(blockName);
        block.transform.position = position;
        block.transform.localScale = scale;

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : GameData.GetSquareSprite();
        renderer.color = sprite != null ? Color.white : color;
        renderer.sortingOrder = sortingOrder;
        if (sprite != null)
        {
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(scale.x, scale.y);
            block.transform.localScale = Vector3.one;
        }

        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        if (sprite != null)
        {
            collider.size = new Vector2(scale.x, scale.y);
        }

        block.AddComponent<Obstacle>();
    }

    private void CreateRuinsDecorations()
    {
        CreateCircleDecoration("Central Magic Circle", new Vector3(0f, 0f, 0f), 2.4f, new Color(0.35f, 0.45f, 1f, 0.22f), -8, GameData.GetMapSprite("magic_circle"));
        CreateCircleDecoration("Inner Magic Circle", new Vector3(0f, 0f, 0f), 1.35f, new Color(0.6f, 0.25f, 1f, 0.20f), -7);

        Vector3[] runePositions =
        {
            new Vector3(-9f, 4.8f, 0f),
            new Vector3(-3.5f, 5.2f, 0f),
            new Vector3(7.8f, 4.2f, 0f),
            new Vector3(9f, -3.6f, 0f),
            new Vector3(2.3f, -5.1f, 0f),
            new Vector3(-4.5f, -5f, 0f),
            new Vector3(-9.4f, -1.2f, 0f),
            new Vector3(6.3f, -1.8f, 0f)
        };

        for (int i = 0; i < runePositions.Length; i++)
        {
            float size = i % 2 == 0 ? 0.28f : 0.38f;
            Color color = i % 2 == 0 ? new Color(0.25f, 0.7f, 1f, 0.55f) : new Color(0.65f, 0.28f, 1f, 0.5f);
            CreateCircleDecoration("Arcane Rune", runePositions[i], size, color, -7, GameData.GetMapSprite("rune"));
        }
    }

    private void CreateCircleDecoration(string decorationName, Vector3 position, float size, Color color, int sortingOrder)
    {
        CreateCircleDecoration(decorationName, position, size, color, sortingOrder, null);
    }

    private void CreateCircleDecoration(string decorationName, Vector3 position, float size, Color color, int sortingOrder, Sprite sprite)
    {
        GameObject decoration = new GameObject(decorationName);
        decoration.transform.position = position;
        decoration.transform.localScale = new Vector3(size, size, 1f);

        SpriteRenderer renderer = decoration.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : GameData.GetCircleSprite();
        renderer.color = sprite != null ? Color.white : color;
        renderer.sortingOrder = sortingOrder;
    }

    private void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -2.2f, 0f);

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        Sprite characterSprite = GameData.GetCharacterSprite(PlayerStats.Type);
        renderer.sprite = characterSprite != null ? characterSprite : GameData.GetSquareSprite();
        renderer.color = characterSprite != null ? Color.white : PlayerStats.BodyColor;
        renderer.sortingOrder = 2;

        player.transform.localScale = characterSprite != null ? new Vector3(0.82f, 0.82f, 1f) : new Vector3(0.7f, 0.7f, 1f);

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.size = new Vector2(0.78f, 0.78f);

        PlayerHealth = player.AddComponent<PlayerHealth>();
        PlayerHealth.Initialize(this, PlayerStats.MaxHealth);

        PlayerHealthBar healthBar = player.AddComponent<PlayerHealthBar>();
        healthBar.Initialize(PlayerHealth);

        PlayerController = player.AddComponent<PlayerController>();
        PlayerController.Initialize(this, PlayerStats, PlayerHealth);
    }

    private void CreateManagersAndUI()
    {
        upgradeManager = gameObject.AddComponent<UpgradeManager>();
        upgradeManager.Initialize(this);

        gameUI = gameObject.AddComponent<GameUI>();
        gameUI.Initialize(this, upgradeManager);

        enemySpawner = gameObject.AddComponent<EnemySpawner>();
        enemySpawner.Initialize(this, PlayerController.transform);
    }

    private void CleanupCombatObjects()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i].gameObject);
        }

        Projectile[] projectiles = FindObjectsOfType<Projectile>();
        for (int i = 0; i < projectiles.Length; i++)
        {
            Destroy(projectiles[i].gameObject);
        }

        ArcaneRainArea[] rainAreas = FindObjectsOfType<ArcaneRainArea>();
        for (int i = 0; i < rainAreas.Length; i++)
        {
            Destroy(rainAreas[i].gameObject);
        }

        TemporaryEffect[] effects = FindObjectsOfType<TemporaryEffect>();
        for (int i = 0; i < effects.Length; i++)
        {
            Destroy(effects[i].gameObject);
        }
    }
}
