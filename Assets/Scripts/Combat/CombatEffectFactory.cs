using UnityEngine;

public static class CombatEffectFactory
{
    public static void CreateCircleEffect(Vector3 position, Color color, float size, float lifetime, int sortingOrder)
    {
        CreateEffect(position, GameData.GetCircleSprite(), color, size, lifetime, sortingOrder);
    }

    public static void CreateSpriteEffect(Vector3 position, Sprite sprite, Color fallbackColor, float size, float lifetime, int sortingOrder)
    {
        CreateEffect(position, sprite != null ? sprite : GameData.GetCircleSprite(), sprite != null ? Color.white : fallbackColor, size, lifetime, sortingOrder);
    }

    public static void CreateDamageText(Vector3 worldPosition, string text, Color color)
    {
        GameObject textObject = new GameObject("Damage Text");
        textObject.transform.position = worldPosition;

        float baseScale = 0.0125f;
        textObject.transform.localScale = new Vector3(baseScale, baseScale, baseScale);

        DamageTextEffect effect = textObject.AddComponent<DamageTextEffect>();
        effect.BaseScale = baseScale;
        effect.Initialize(text, color);
    }

    private static void CreateEffect(Vector3 position, Sprite sprite, Color color, float size, float lifetime, int sortingOrder)
    {
        GameObject effectObject = new GameObject("Combat Effect");
        effectObject.transform.position = position;

        SpriteRenderer renderer = effectObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        float spriteSize = sprite != null ? Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) : 1f;
        float scale = spriteSize > 0f ? size / spriteSize : size;
        effectObject.transform.localScale = new Vector3(scale, scale, 1f);

        TemporaryEffect effect = effectObject.AddComponent<TemporaryEffect>();
        effect.Initialize(lifetime);
    }
}
