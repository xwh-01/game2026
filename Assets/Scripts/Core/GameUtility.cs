using UnityEngine;

public static class GameUtility
{
    public const float MinX = -11.5f;
    public const float MaxX = 11.5f;
    public const float MinY = -6.5f;
    public const float MaxY = 6.5f;

    public static Vector3 GetMouseWorldPosition(Camera camera)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, 0f, Screen.width);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, 0f, Screen.height);

        Vector3 worldPosition = camera.ScreenToWorldPoint(mouseScreenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    public static float GetAngleFromVector(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public static Vector3 ClampToArena(Vector3 position, float padding)
    {
        position.x = Mathf.Clamp(position.x, MinX + padding, MaxX - padding);
        position.y = Mathf.Clamp(position.y, MinY + padding, MaxY - padding);
        return position;
    }

    public static bool IsOutsideArena(Vector3 position, float padding)
    {
        return position.x < MinX - padding || position.x > MaxX + padding || position.y < MinY - padding || position.y > MaxY + padding;
    }
}
