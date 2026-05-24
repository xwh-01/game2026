public enum UpgradeType
{
    AttackDamage,
    SkillDamage,
    MaxHealth,
    MoveSpeed,
    AttackCooldown,
    Heal,
    SkillCooldown,
    FireballSplit,
    ArcaneRainBigger,
    ArcaneRainLonger,
    ArcaneRainFaster,
    SwordWavePierce,
    SwordWaveBigger,
    DashSlashHeal,
    DashSlashCooldown,
    FrostField,
    ManaSurge,
    EarthSplitter,
    BattleFrenzy
}

public class UpgradeOption
{
    public string Title;
    public string Description;
    public UpgradeType Type;

    public UpgradeOption(string title, string description, UpgradeType type)
    {
        Title = title;
        Description = description;
        Type = type;
    }
}
