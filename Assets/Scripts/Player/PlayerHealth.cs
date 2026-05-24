using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    public void Initialize(GameManager manager, int maxHealth)
    {
        gameManager = manager;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }
    }

    public void SetSpriteRenderer(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(int damage)
    {
        if (gameManager == null || gameManager.IsGameOver || damage <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        CombatEffectFactory.CreateCircleEffect(transform.position, new Color(1f, 0.2f, 0.2f, 0.55f), 0.8f, 0.1f, 6);
        CombatEffectFactory.CreateDamageText(transform.position + new Vector3(0f, 0.8f, 0f), "-" + damage, new Color(1f, 0.25f, 0.25f, 1f));
        StopCoroutine("HitFlash");
        StartCoroutine("HitFlash");
        gameManager.RefreshUI();

        if (CurrentHealth <= 0)
        {
            gameManager.GameOver();
        }
    }

    public void IncreaseMaxHealth(int amount)
    {
        MaxHealth += amount;
        Heal(amount);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        if (gameManager != null)
        {
            gameManager.RefreshUI();
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
