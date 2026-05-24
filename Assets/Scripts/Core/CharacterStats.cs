using UnityEngine;

[System.Serializable]
public class CharacterStats
{
    public CharacterType Type;
    public string DisplayName;
    public int MaxHealth;
    public float MoveSpeed;

    public string AttackName;
    public int AttackDamage;
    public float AttackCooldown;
    public float BulletSpeed;
    public float BulletLifetime;

    public string SkillName;
    public int SkillDamage;
    public float SkillCooldown;
    public float SkillRadius;
    public float SkillDuration;
    public float RainInterval;
    public float RainHitRadius;
    public float DashDistance;
    public float DashDuration;

    public int AttackProjectileCount = 1;
    public float AttackSpreadAngle = 12f;
    public float ProjectileScaleMultiplier = 1f;
    public bool ProjectilePierce;
    public float SkillRadiusBonus;
    public float SkillDurationBonus;
    public float SkillTickIntervalMultiplier = 1f;
    public int DashHealOnHit;
    public bool DashEndExplosion;
    public float BattleFrenzyDuration;
    public float RainSlowMultiplier = 1f;

    public Color BodyColor;
    public Color BulletColor;

    public CharacterStats Clone()
    {
        return (CharacterStats)MemberwiseClone();
    }
}
