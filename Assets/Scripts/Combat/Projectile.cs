using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    private float lifeTimer;
    private Rigidbody2D body;
    private bool hitsEnemies = true;
    private bool hitsPlayer;
    private bool pierce;
    private HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

    public void Initialize(Vector2 shootDirection, float projectileSpeed, int projectileDamage, float lifetime, Color effectColor)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        lifeTimer = lifetime;

        body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = direction * speed;
        }

        float angle = GameUtility.GetAngleFromVector(direction);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        CreateTrail(effectColor);
    }

    public void SetPierce(bool canPierce)
    {
        pierce = canPierce;
    }

    public void SetTargetsPlayer()
    {
        hitsEnemies = false;
        hitsPlayer = true;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (GameUtility.IsOutsideArena(transform.position, 3f))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitsEnemies)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                if (pierce && hitEnemies.Contains(enemy))
                {
                    return;
                }

                enemy.TakeDamage(damage);
                CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.85f, 0.2f, 0.75f), 0.4f, 0.08f, 5);

                if (pierce)
                {
                    hitEnemies.Add(enemy);
                    return;
                }

                Destroy(gameObject);
                return;
            }
        }

        if (hitsPlayer)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.25f, 0.25f, 0.6f), 0.35f, 0.07f, 5);
                Destroy(gameObject);
                return;
            }
        }

        if (other.GetComponent<Obstacle>() != null)
        {
            CombatEffectFactory.CreateCircleEffect(transform.position, new Color(0.55f, 0.58f, 0.72f, 0.55f), 0.35f, 0.07f, 5);
            Destroy(gameObject);
        }
    }

    private void CreateTrail(Color effectColor)
    {
        TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.18f;
        trail.endWidth = 0f;
        trail.numCornerVertices = 2;
        trail.numCapVertices = 2;
        trail.sortingOrder = 2;

        Color startColor = effectColor;
        startColor.a = 0.75f;
        Color endColor = effectColor;
        endColor.a = 0f;
        trail.startColor = startColor;
        trail.endColor = endColor;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            trail.material = new Material(shader);
        }
    }
}
