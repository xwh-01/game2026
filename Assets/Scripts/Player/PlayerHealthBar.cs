using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Image fillImage;

    public void Initialize(PlayerHealth health)
    {
        playerHealth = health;

        GameObject canvasObject = new GameObject("Player Health Canvas");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        canvasObject.transform.localScale = new Vector3(0.01f, 0.01f, 1f);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(120f, 14f);

        GameObject bgObject = new GameObject("HP Bar Background");
        bgObject.transform.SetParent(canvasObject.transform, false);
        Image bgImage = bgObject.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        GameObject fillObject = new GameObject("HP Bar Fill");
        fillObject.transform.SetParent(bgObject.transform, false);
        fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.85f, 0.3f, 0.9f);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(120f, 0f);
    }

    private void Update()
    {
        if (playerHealth == null || fillImage == null)
        {
            return;
        }

        float ratio = Mathf.Clamp01((float)playerHealth.CurrentHealth / playerHealth.MaxHealth);
        fillImage.rectTransform.sizeDelta = new Vector2(120f * ratio, 0f);
    }
}
