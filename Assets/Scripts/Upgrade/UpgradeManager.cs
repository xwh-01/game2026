using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private GameManager gameManager;
    private GameUI gameUI;
    private int pendingUpgradeCount;
    private bool isShowingUpgrade;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void ShowUpgradeChoices()
    {
        QueueUpgradeChoices(1);
    }

    public void QueueUpgradeChoices(int count)
    {
        pendingUpgradeCount += Mathf.Max(0, count);
        ShowNextUpgradeIfNeeded();
    }

    private void ShowNextUpgradeIfNeeded()
    {
        if (gameUI == null)
        {
            gameUI = GetComponent<GameUI>();
        }

        if (isShowingUpgrade || pendingUpgradeCount <= 0)
        {
            return;
        }

        isShowingUpgrade = true;
        pendingUpgradeCount--;
        Time.timeScale = 0f;
        gameUI.ShowUpgradePanel(GetRandomOptions());
    }

    public void ApplyUpgrade(UpgradeOption option)
    {
        CharacterStats stats = gameManager.PlayerStats;

        if (option.Type == UpgradeType.AttackDamage)
        {
            stats.AttackDamage += 10;
        }
        else if (option.Type == UpgradeType.SkillDamage)
        {
            stats.SkillDamage += 5;
        }
        else if (option.Type == UpgradeType.MaxHealth)
        {
            gameManager.PlayerHealth.IncreaseMaxHealth(20);
            stats.MaxHealth = gameManager.PlayerHealth.MaxHealth;
        }
        else if (option.Type == UpgradeType.MoveSpeed)
        {
            stats.MoveSpeed *= 1.1f;
        }
        else if (option.Type == UpgradeType.AttackCooldown)
        {
            stats.AttackCooldown = Mathf.Max(0.08f, stats.AttackCooldown * 0.9f);
        }
        else if (option.Type == UpgradeType.Heal)
        {
            gameManager.PlayerHealth.Heal(40);
        }
        else if (option.Type == UpgradeType.SkillCooldown)
        {
            stats.SkillCooldown = Mathf.Max(0.5f, stats.SkillCooldown * 0.9f);
        }
        else if (option.Type == UpgradeType.FireballSplit)
        {
            stats.AttackProjectileCount = 3;
        }
        else if (option.Type == UpgradeType.ArcaneRainBigger)
        {
            stats.SkillRadiusBonus += 0.6f;
        }
        else if (option.Type == UpgradeType.ArcaneRainLonger)
        {
            stats.SkillDurationBonus += 1f;
        }
        else if (option.Type == UpgradeType.ArcaneRainFaster)
        {
            stats.SkillTickIntervalMultiplier = Mathf.Max(0.5f, stats.SkillTickIntervalMultiplier - 0.2f);
        }
        else if (option.Type == UpgradeType.SwordWavePierce)
        {
            stats.ProjectilePierce = true;
        }
        else if (option.Type == UpgradeType.SwordWaveBigger)
        {
            stats.ProjectileScaleMultiplier += 0.4f;
        }
        else if (option.Type == UpgradeType.DashSlashHeal)
        {
            stats.DashHealOnHit += 3;
        }
        else if (option.Type == UpgradeType.DashSlashCooldown)
        {
            stats.SkillCooldown = Mathf.Max(1.5f, stats.SkillCooldown * 0.75f);
        }
        else if (option.Type == UpgradeType.FrostField)
        {
            stats.RainSlowMultiplier = Mathf.Max(0.4f, stats.RainSlowMultiplier - 0.15f);
        }
        else if (option.Type == UpgradeType.ManaSurge)
        {
            stats.SkillCooldown = Mathf.Max(0.5f, stats.SkillCooldown * 0.82f);
        }
        else if (option.Type == UpgradeType.EarthSplitter)
        {
            stats.DashEndExplosion = true;
        }
        else if (option.Type == UpgradeType.BattleFrenzy)
        {
            if (stats.BattleFrenzyDuration <= 0f) stats.BattleFrenzyDuration = 2f;
            else stats.BattleFrenzyDuration += 1f;
        }

        if (gameUI == null)
        {
            gameUI = GetComponent<GameUI>();
        }

        gameUI.HideUpgradePanel();
        isShowingUpgrade = false;
        gameManager.RefreshUI();
        if (pendingUpgradeCount > 0)
        {
            ShowNextUpgradeIfNeeded();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private List<UpgradeOption> GetRandomOptions()
    {
        List<UpgradeOption> pool = GameData.CreateUpgradeOptions();
        List<UpgradeOption> result = new List<UpgradeOption>();

        while (result.Count < 3 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
