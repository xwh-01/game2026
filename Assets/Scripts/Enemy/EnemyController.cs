using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private GameManager gameManager;
    private EnemySpawner enemySpawner;
    private Transform player;
    private int currentHealth;
    private int maxHealth;
    private float moveSpeed;
    private int contactDamage;
    private int expReward;
    private float nextDamageTime;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private bool isDead;
    private EnemyHealthBar healthBar;
    private float knockbackEndTime;
    private Vector2 knockbackVelocity;
    private const float DamageInterval = 0.8f;
    private const float KnockbackForce = 7f;
    private const float KnockbackDuration = 0.1f;

    private bool isRanged;
    private float attackRange;
    private float shootInterval;
    private float nextShootTime;
    private int bulletDamage;
    private float bulletSpeed;
    private float bulletLife;
    private float externalSpeedMultiplier = 1f;
    private float currentSlowMultiplier = 1f;

    public void SetExternalSpeedMultiplier(float multiplier)
    {
        currentSlowMultiplier = multiplier;
    }

    public void SetRangedBehavior(float range, float cooldown, int bDamage, float bSpeed, float bLife)
    {
        isRanged = true;
        attackRange = range;
        shootInterval = cooldown;
        bulletDamage = bDamage;
        bulletSpeed = bSpeed;
        bulletLife = bLife;
        nextShootTime = Time.time + 1f;
    }

    private void FixedUpdate()
    {
        if (gameManager == null || gameManager.IsGameOver || player == null || Time.timeScale == 0f)
        {
            StopMovement();
            return;
        }

        if (Time.time < knockbackEndTime)
        {
            body.velocity = knockbackVelocity;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (isRanged)
        {
            float stopDistance = attackRange * 0.7f;
            if (distance > attackRange)
            {
                body.velocity = direction * moveSpeed * currentSlowMultiplier;
            }
            else if (distance < stopDistance)
            {
                body.velocity = -direction * moveSpeed * 0.6f * currentSlowMultiplier;
            }
            else
            {
                body.velocity = Vector2.zero;
            }

            if (Time.time >= nextShootTime)
            {
                nextShootTime = Time.time + shootInterval;
                ShootAtPlayer(direction);
            }

            return;
        }

        body.velocity = direction * moveSpeed * currentSlowMultiplier;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 1f, 1f, 0.65f), 0.45f, 0.08f, 5);
        CombatEffectFactory.CreateDamageText(transform.position + new Vector3(0f, 0.5f, 0f), damage.ToString(), new Color(1f, 0.9f, 0.2f, 1f));
        StopCoroutine("HitFlash");
        StartCoroutine("HitFlash");

        if (player != null && body != null)
        {
            Vector2 knockDir = ((Vector2)(transform.position - player.position)).normalized;
            knockbackVelocity = knockDir * KnockbackForce;
            knockbackEndTime = Time.time + KnockbackDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null || Time.time < nextDamageTime || gameManager == null || gameManager.IsGameOver)
        {
            return;
        }

        nextDamageTime = Time.time + DamageInterval;
        playerHealth.TakeDamage(contactDamage);
    }

    private void ShootAtPlayer(Vector2 direction)
    {
        GameObject bulletObj = new GameObject("Enemy Bullet");
        bulletObj.transform.position = transform.position + (Vector3)(direction * 0.4f);
        bulletObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        SpriteRenderer renderer = bulletObj.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetCircleSprite();
        renderer.color = new Color(1f, 0.35f, 0.2f);
        renderer.sortingOrder = 3;

        Rigidbody2D bulletBody = bulletObj.AddComponent<Rigidbody2D>();
        bulletBody.gravityScale = 0f;
        bulletBody.freezeRotation = true;

        BoxCollider2D collider = bulletObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Projectile bullet = bulletObj.AddComponent<Projectile>();
        bullet.Initialize(direction, bulletSpeed, bulletDamage, bulletLife, Color.white);
        bullet.SetTargetsPlayer();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        StopMovement();
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.7f, 0.2f, 0.8f), 1.2f, 0.3f, 7);
        CombatEffectFactory.CreateCircleEffect(transform.position, Color.white, 0.55f, 0.12f, 7);
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f * Mathf.Deg2Rad + Random.Range(-0.2f, 0.2f);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.55f;
            CombatEffectFactory.CreateCircleEffect(transform.position + offset, new Color(1f, 0.85f, 0.2f, 0.65f), 0.22f, 0.25f, 7);
        }
        if (enemySpawner != null)
        {
            enemySpawner.UnregisterEnemy();
        }

        if (gameManager != null)
        {
            gameManager.AddKill();
            gameManager.AddExperience(expReward);
        }

        Destroy(gameObject);
    }

    private void StopMovement()
    {
        if (body != null)
        {
            body.velocity = Vector2.zero;
        }
    }

    private System.Collections.IEnumerator HitFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(0.08f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }
}
