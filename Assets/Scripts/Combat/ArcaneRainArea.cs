using System.Collections.Generic;
using UnityEngine;

public class ArcaneRainArea : MonoBehaviour
{
    private Vector3 center;
    private float radius;
    private float duration;
    private float rainInterval;
    private int damage;
    private float hitRadius;
    private float slowMultiplier = 1f;
    private float elapsed;
    private float nextRainTime;
    private HashSet<EnemyController> slowedEnemies = new HashSet<EnemyController>();

    public void Initialize(Vector3 areaCenter, float areaRadius, float areaDuration, float interval, int rainDamage, float rainHitRadius, float slow = 1f)
    {
        center = areaCenter;
        radius = areaRadius;
        duration = areaDuration;
        rainInterval = interval;
        damage = rainDamage;
        hitRadius = rainHitRadius;
        slowMultiplier = slow;

        transform.position = center;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GameData.GetCircleSprite();
        renderer.color = new Color(0.4f, 0.22f, 1f, 0.32f);
        renderer.sortingOrder = -1;
        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        nextRainTime -= Time.deltaTime;

        float pulse = 1f + Mathf.Sin(elapsed * 4.5f) * 0.05f;
        transform.localScale = new Vector3(radius * 2f * pulse, radius * 2f * pulse, 1f);

        if (nextRainTime <= 0f)
        {
            nextRainTime = rainInterval;
            StrikeRandomPoint();
            DamageEnemiesInArea();
        }

        ApplySlowToEnemiesInArea();

        if (elapsed >= duration)
        {
            ResetAllSlowed();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        ResetAllSlowed();
    }

    private void ResetAllSlowed()
    {
        foreach (EnemyController enemy in slowedEnemies)
        {
            if (enemy != null)
            {
                enemy.SetExternalSpeedMultiplier(1f);
            }
        }
        slowedEnemies.Clear();
    }

    private void ApplySlowToEnemiesInArea()
    {
        if (slowMultiplier >= 1f) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        HashSet<EnemyController> currentInArea = new HashSet<EnemyController>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                currentInArea.Add(enemy);
                enemy.SetExternalSpeedMultiplier(slowMultiplier);
            }
        }

        HashSet<EnemyController> toRestore = new HashSet<EnemyController>();
        foreach (EnemyController enemy in slowedEnemies)
        {
            if (enemy != null && !currentInArea.Contains(enemy))
            {
                toRestore.Add(enemy);
            }
        }

        foreach (EnemyController enemy in toRestore)
        {
            if (enemy != null)
            {
                enemy.SetExternalSpeedMultiplier(1f);
            }
            slowedEnemies.Remove(enemy);
        }

        foreach (EnemyController enemy in currentInArea)
        {
            slowedEnemies.Add(enemy);
        }
    }

    private void StrikeRandomPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector3 strikePosition = center + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Color color = Random.value > 0.5f ? new Color(0.2f, 0.65f, 1f, 0.85f) : new Color(0.65f, 0.25f, 1f, 0.85f);
        CombatEffectFactory.CreateSpriteEffect(strikePosition, GameData.GetEffectSprite("arcane_rain_hit"), color, hitRadius * 2f, 0.15f, 4);
    }

    private void DamageEnemiesInArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyController enemy = hits[i].GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
