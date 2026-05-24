using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    private Font font;
    private Sprite whiteSprite;
    private Canvas canvas;
    private CharacterType selectedType = CharacterType.Mage;
    private GameObject startButtonObject;
    private Image mageHighlightBorder;
    private Image warriorHighlightBorder;
    private bool hasSelection;

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
        canvas = CreateCanvas("Character Select Canvas");

        CreateText(canvas.transform, "Choose Your Hero", 46, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f),
            new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero, new Vector2(600f, 70f));

        BuildCharacterCard(new Vector2(-320f, 20f), CharacterType.Mage);
        BuildCharacterCard(new Vector2(320f, 20f), CharacterType.Warrior);

        Button backButton = CreateMenuButton(canvas.transform, "Back", new Vector2(0.15f, 0.08f), new Color(0.3f, 0.3f, 0.4f), 200f, 44f, 22);
        backButton.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void BuildCharacterCard(Vector2 position, CharacterType type)
    {
        CharacterStats stats = GameData.GetCharacterStats(type);
        bool isMage = type == CharacterType.Mage;

        Sprite portrait = GameData.GetCharacterPortrait(type);

        GameObject cardObj = new GameObject(isMage ? "Mage Card" : "Warrior Card");
        cardObj.transform.SetParent(canvas.transform, false);

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.sprite = whiteSprite;
        cardBg.color = new Color(0.05f, 0.06f, 0.12f, 0.94f);

        Button cardButton = cardObj.AddComponent<Button>();
        ColorBlock colors = cardButton.colors;
        colors.normalColor = new Color(0.05f, 0.06f, 0.12f, 0.94f);
        colors.highlightedColor = new Color(0.1f, 0.12f, 0.22f, 0.94f);
        colors.pressedColor = new Color(0.03f, 0.04f, 0.08f, 0.94f);
        cardButton.colors = colors;
        cardButton.onClick.AddListener(delegate { SelectCharacter(type); });

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        SetRect(cardRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(340f, 520f));

        GameObject highlightObj = new GameObject("Highlight Border");
        highlightObj.transform.SetParent(cardObj.transform, false);
        Image highlightImage = highlightObj.AddComponent<Image>();
        highlightImage.sprite = whiteSprite;
        highlightImage.raycastTarget = false;
        SetRect(highlightImage.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        if (isMage) mageHighlightBorder = highlightImage;
        else warriorHighlightBorder = highlightImage;

        highlightImage.color = new Color(0.85f, 0.68f, 0.28f, 0f);

        GameObject innerObj = new GameObject("Inner Panel");
        innerObj.transform.SetParent(cardObj.transform, false);
        Image innerImage = innerObj.AddComponent<Image>();
        innerImage.sprite = whiteSprite;
        innerImage.color = new Color(0.03f, 0.04f, 0.08f, 1f);
        innerImage.raycastTarget = false;
        SetRect(innerImage.rectTransform, new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f), Vector2.zero, Vector2.zero);

        if (portrait != null)
        {
            GameObject portraitFrameObj = new GameObject("Portrait Frame");
            portraitFrameObj.transform.SetParent(cardObj.transform, false);
            Image frameImage = portraitFrameObj.AddComponent<Image>();
            frameImage.sprite = whiteSprite;
            frameImage.color = new Color(0.08f, 0.1f, 0.18f, 1f);
            frameImage.raycastTarget = false;
            SetRect(frameImage.rectTransform, new Vector2(0.06f, 0.52f), new Vector2(0.94f, 0.94f), Vector2.zero, Vector2.zero);

            GameObject portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(frameImage.transform, false);
            Image portraitImage = portraitObj.AddComponent<Image>();
            portraitImage.sprite = portrait;
            portraitImage.color = Color.white;
            portraitImage.preserveAspect = true;
            portraitImage.raycastTarget = false;
            SetRect(portraitImage.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
        }

        string role = isMage ? "Ranged / Area Damage" : "Melee / Dash Burst";
        string playstyle = isMage ? "Good at clearing groups with magic.\nWeak body, strong spell control." : "Good at close-range burst and survival.\nStronger body, aggressive playstyle.";

        CreateText(cardObj.transform, stats.DisplayName, 28, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f),
            new Vector2(0.5f, 0.43f), new Vector2(0.5f, 0.43f), Vector2.zero, new Vector2(260f, 36f));

        CreateText(cardObj.transform, role, 16, TextAnchor.MiddleCenter, isMage ? new Color(0.45f, 0.55f, 1f) : new Color(1f, 0.45f, 0.2f),
            new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.36f), Vector2.zero, new Vector2(260f, 24f));

        string statsLine1 = string.Format("HP: {0}   Speed: {1}", stats.MaxHealth, stats.MoveSpeed.ToString("0.0"));
        string statsLine2 = string.Format("{0}: {1} dmg", stats.AttackName, stats.AttackDamage);
        string statsLine3 = string.Format("{0}: {1} dmg / CD {2}s", stats.SkillName, stats.SkillDamage, stats.SkillCooldown.ToString("0.0"));

        CreateText(cardObj.transform, statsLine1, 16, TextAnchor.MiddleCenter, new Color(0.8f, 0.85f, 0.9f),
            new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f), Vector2.zero, new Vector2(280f, 22f));

        CreateText(cardObj.transform, statsLine2, 15, TextAnchor.MiddleCenter, new Color(0.7f, 0.75f, 0.8f),
            new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(280f, 20f));

        CreateText(cardObj.transform, statsLine3, 15, TextAnchor.MiddleCenter, new Color(0.7f, 0.75f, 0.8f),
            new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), Vector2.zero, new Vector2(280f, 20f));

        CreateText(cardObj.transform, playstyle, 13, TextAnchor.MiddleCenter, new Color(0.55f, 0.6f, 0.7f),
            new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), Vector2.zero, new Vector2(290f, 36f));
    }

    private void SelectCharacter(CharacterType type)
    {
        selectedType = type;
        hasSelection = true;

        mageHighlightBorder.color = new Color(0.85f, 0.68f, 0.28f, type == CharacterType.Mage ? 0.95f : 0f);
        warriorHighlightBorder.color = new Color(0.85f, 0.68f, 0.28f, type == CharacterType.Warrior ? 0.95f : 0f);

        if (startButtonObject != null)
        {
            Destroy(startButtonObject);
        }

        GameObject buttonObj = new GameObject("Start Button Container");
        buttonObj.transform.SetParent(canvas.transform, false);
        startButtonObject = buttonObj;

        Button startBtn = CreateMenuButton(buttonObj.transform, "Start Game", new Vector2(0.5f, 0.08f), new Color(0.2f, 0.55f, 0.25f), 260f, 52f, 26);
        startBtn.onClick.AddListener(delegate
        {
            GameData.SelectedCharacter = selectedType;
            SceneLoader.LoadGame();
        });
    }

    private Button CreateMenuButton(Transform parent, string label, Vector2 anchor, Color color, float width, float height, int fontSize)
    {
        GameObject buttonObj = new GameObject(label + " Button");
        buttonObj.transform.SetParent(parent, false);
        Image image = buttonObj.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObj.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.35f;
        colors.pressedColor = color * 0.7f;
        colors.selectedColor = color;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;

        Text text = CreateText(buttonObj.transform, label, fontSize, TextAnchor.MiddleCenter, Color.white);
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
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

    private Text CreateText(Transform parent, string value, int size, TextAnchor alignment, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        SetRect(text.rectTransform, anchorMin, anchorMax, anchoredPosition, sizeDelta);
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
