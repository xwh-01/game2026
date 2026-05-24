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

public enum UpgradeCategory
{
    Common,
    Mage,
    Warrior,
    Rare
}

public class UpgradeOption
{
    public string Title;
    public string Description;
    public UpgradeType Type;
    public UpgradeCategory Category;

    public UpgradeOption(string title, string description, UpgradeType type, UpgradeCategory category = UpgradeCategory.Common)
    {
        Title = title;
        Description = description;
        Type = type;
        Category = category;
    }
}
