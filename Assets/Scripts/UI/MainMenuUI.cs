using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private Font font;
    private Sprite whiteSprite;
    private Canvas canvas;
    private GameObject howToPlayPanel;

    private void Start()
    {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        whiteSprite = CreateWhiteSprite();
        EnsureEventSystem();
        BuildUI();
    }

    private void BuildUI()
    {
        canvas = CreateCanvas("Main Menu Canvas");

        Text title = CreateText(canvas.transform, "Dungeon Survival", 56, TextAnchor.MiddleCenter);
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(1f, 0.85f, 0.2f);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(800f, 80f));

        Text subtitle = CreateText(canvas.transform, "Survive 60 seconds. Build your hero. Defeat the horde.", 22, TextAnchor.MiddleCenter);
        subtitle.color = new Color(0.7f, 0.75f, 0.85f);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(700f, 40f));

        CreateMenuButton(canvas.transform, "Start Game", new Vector2(0.5f, 0.48f), new Color(0.2f, 0.55f, 0.95f),
            delegate { SceneLoader.LoadCharacterSelect(); });

        CreateMenuButton(canvas.transform, "How To Play", new Vector2(0.5f, 0.38f), new Color(0.25f, 0.3f, 0.55f),
            delegate { ShowHowToPlay(); });

        CreateMenuButton(canvas.transform, "Quit", new Vector2(0.5f, 0.28f), new Color(0.3f, 0.15f, 0.15f),
            delegate { Application.Quit(); });
    }

    private void ShowHowToPlay()
    {
        if (howToPlayPanel != null) return;

        howToPlayPanel = CreatePanel("How To Play Panel", new Color(0.02f, 0.04f, 0.1f, 0.94f), new Vector2(600f, 460f));

        Text panelTitle = CreateText(howToPlayPanel.transform, "How To Play", 38, TextAnchor.MiddleCenter);
        panelTitle.color = new Color(1f, 0.85f, 0.2f);
        panelTitle.fontStyle = FontStyle.Bold;
        SetRect(panelTitle.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(480f, 60f));

        string[] instructions =
        {
            "WASD - Move",
            "Mouse Left Click - Attack",
            "E - Skill",
            "Kill enemies to gain EXP",
            "Level up to choose upgrades",
            "Survive 60 seconds to win!"
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            Text line = CreateText(howToPlayPanel.transform, instructions[i], 24, TextAnchor.MiddleCenter);
            line.color = new Color(0.85f, 0.9f, 1f);
            SetRect(line.rectTransform, new Vector2(0.5f, 0.75f - i * 0.09f), new Vector2(0.5f, 0.75f - i * 0.09f), Vector2.zero, new Vector2(500f, 36f));
        }

        CreateMenuButton(howToPlayPanel.transform, "Back", new Vector2(0.5f, 0.10f), new Color(0.3f, 0.35f, 0.5f),
            delegate { HideHowToPlay(); });
    }

    private void HideHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            Destroy(howToPlayPanel);
            howToPlayPanel = null;
        }
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(label + " Button");
        buttonObj.transform.SetParent(parent, false);
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.35f;
        colors.pressedColor = color * 0.7f;
        colors.selectedColor = color;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300f, 56f));
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;

        Text text = CreateText(buttonObj.transform, label, 26, TextAnchor.MiddleCenter);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++) texture.SetPixel(i % 4, i / 4, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas c = canvasObject.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return c;
    }

    private GameObject CreatePanel(string name, Color color, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(canvas.transform, false);
        Image image = panelObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        SetRect(panelObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        return panelObject;
    }

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
