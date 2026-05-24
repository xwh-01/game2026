using UnityEngine;
using UnityEngine.UI;

public class DamageTextEffect : MonoBehaviour
{
    private float speed = 1.6f;
    private float lifetime = 1.2f;
    private float elapsed;
    private Color textColor;
    private Text textComp;

    public void Initialize(string text, Color color)
    {
        textColor = color;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        canvas.worldCamera = Camera.main;

        RectTransform canvasRect = GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400f, 100f);

        GameObject textChild = new GameObject("Text");
        textChild.transform.SetParent(transform, false);

        textComp = textChild.AddComponent<Text>();

        Font font = Font.CreateDynamicFontFromOSFont("Arial", 36);
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        textComp.font = font;
        textComp.text = text;
        textComp.fontSize = 36;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = color;
        textComp.raycastTarget = false;
        textComp.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComp.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textChild.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform textRect = textChild.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(400f, 100f);

        StartCoroutine(FadeRoutine());
    }

    private System.Collections.IEnumerator FadeRoutine()
    {
        float fadeStart = lifetime * 0.5f;
        yield return new WaitForSeconds(fadeStart);

        while (elapsed < lifetime && textComp != null)
        {
            float ratio = (elapsed - fadeStart) / (lifetime - fadeStart);
            float alpha = Mathf.Lerp(1f, 0f, ratio);
            textComp.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
            yield return null;
        }
    }

    public float BaseScale { get; set; }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += Vector3.up * speed * Time.deltaTime;

        float scale = BaseScale;

        if (elapsed >= 0.08f && elapsed <= 0.28f)
        {
            scale = BaseScale * (1f + Mathf.Sin((elapsed - 0.08f) / 0.2f * Mathf.PI) * 0.3f);
        }

        transform.localScale = new Vector3(scale, scale, scale);

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
