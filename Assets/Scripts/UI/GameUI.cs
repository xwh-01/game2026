using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private GameManager gameManager;
    private UpgradeManager upgradeManager;
    private Font font;
    private Canvas gameCanvas;
    private Sprite whiteSprite;
    private GameObject infoPanel;

    private Text characterText;
    private Text levelText;

    private Text hpText;
    private Image hpBarBackground;
    private Image hpBarFill;

    private Text expText;
    private Image expBarBackground;
    private Image expBarFill;

    private Text attackNameText;
    private Text attackCooldownText;
    private Text attackDamageText;
    private Image attackCooldownOverlay;

    private Text skillNameText;
    private Text skillCooldownText;
    private Text skillDamageText;
    private Image skillCooldownOverlay;

    private Text timerText;

    private Text dangerText;

    private Text killsText;
    private GameObject pauseMenu;
    private int lastDangerLevel = -1;
    private float dangerNotifyTimer;
    private float hpFlashTimer;

    private GameObject upgradePanel;
    private GameObject gameOverPanel;
    private GameObject victoryPanel;

    public void Initialize(GameManager manager, UpgradeManager upgrades)
    {
        gameManager = manager;
        upgradeManager = upgrades;
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        EnsureEventSystem();
        BuildUI();
        Refresh();
    }

    private void Update()
    {
        if (gameManager == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (upgradePanel != null) return;
            if (gameOverPanel != null) return;
            if (victoryPanel != null) return;
            HandlePause();
        }

        Refresh();
    }

    public void Refresh()
    {
        if (characterText == null || gameManager == null || gameManager.PlayerHealth == null || gameManager.PlayerController == null)
        {
            return;
        }

        CharacterStats stats = gameManager.PlayerStats;
        if (stats == null)
        {
            return;
        }

        int totalSeconds = Mathf.FloorToInt(gameManager.SurvivalTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int targetSeconds = Mathf.FloorToInt(gameManager.SurvivalTarget);
        int targetMin = targetSeconds / 60;
        int targetSec = targetSeconds % 60;
        timerText.text = string.Format("Time  {0:00}:{1:00} / {2:00}:{3:00}", minutes, seconds, targetMin, targetSec);

        killsText.text = "Kills: " + gameManager.KillCount;

        int diff = gameManager.DifficultyLevel;
        if (diff == 0) dangerText.text = "Danger Lv.0";
        else if (diff == 1) dangerText.text = "Danger Lv.1";
        else if (diff == 2) dangerText.text = "Danger Lv.2";
        else dangerText.text = "Danger Lv.3";
        if (diff == 3) dangerText.color = new Color(1f, 0.25f, 0.2f);
        else if (diff == 2) dangerText.color = new Color(1f, 0.5f, 0.1f);
        else if (diff == 1) dangerText.color = new Color(0.95f, 0.7f, 0.25f);
        else dangerText.color = new Color(0.6f, 0.8f, 0.6f);

        if (diff > lastDangerLevel)
        {
            lastDangerLevel = diff;
            dangerNotifyTimer = 1.5f;
        }
        if (dangerNotifyTimer > 0f)
        {
            dangerNotifyTimer -= Time.unscaledDeltaTime;
            if (dangerNotifyTimer > 0f && Mathf.FloorToInt(dangerNotifyTimer * 10f) % 2 == 0)
            {
                dangerText.color = Color.white;
                dangerText.text = "DANGER LEVEL UP!";
            }
        }

        characterText.text = stats.DisplayName;
        levelText.text = "Lv." + gameManager.Level;

        float hpRatio = Mathf.Clamp01((float)gameManager.PlayerHealth.CurrentHealth / gameManager.PlayerHealth.MaxHealth);
        hpText.text = "HP  " + gameManager.PlayerHealth.CurrentHealth + " / " + gameManager.PlayerHealth.MaxHealth;
        hpBarFill.rectTransform.sizeDelta = new Vector2(380f * hpRatio, 0f);

        if (hpRatio < 0.3f)
        {
            hpFlashTimer += Time.unscaledDeltaTime;
            float flash = Mathf.Sin(hpFlashTimer * 8f) * 0.5f + 0.5f;
            hpBarFill.color = new Color(0.9f, 0.12f, 0.12f + flash * 0.6f, 0.92f);
            if (flash > 0.7f) hpText.color = new Color(1f, 0.5f + flash * 0.5f, 0.5f + flash * 0.5f);
            else hpText.color = Color.white;
        }
        else
        {
            hpFlashTimer = 0f;
            hpBarFill.color = new Color(0.9f, 0.12f, 0.12f, 0.92f);
            hpText.color = Color.white;
        }

        float expRatio = (float)gameManager.CurrentExp / gameManager.NextLevelExp;
        expText.text = "EXP  " + gameManager.CurrentExp + " / " + gameManager.NextLevelExp;
        expBarFill.rectTransform.sizeDelta = new Vector2(380f * expRatio, 0f);

        attackNameText.text = stats.AttackName;
        attackDamageText.text = stats.AttackDamage + " 伤害";
        string atkCdText = gameManager.PlayerController.GetAttackCooldownText();
        attackCooldownText.text = atkCdText;
        attackCooldownText.color = atkCdText == "就绪" ? new Color(0.4f, 1f, 0.4f) : Color.white;
        attackCooldownOverlay.fillAmount = gameManager.PlayerController.GetAttackCooldownRatio();

        skillDamageText.text = stats.SkillDamage + " 伤害";
        skillNameText.text = stats.SkillName;
        string skillCdText = gameManager.PlayerController.GetSkillCooldownText();
        skillCooldownText.text = skillCdText;
        skillCooldownText.color = skillCdText == "就绪" ? new Color(0.4f, 1f, 0.4f) : Color.white;
        skillCooldownOverlay.fillAmount = gameManager.PlayerController.GetSkillCooldownRatio();
    }

    public void ShowUpgradePanel(List<UpgradeOption> options)
    {
        HideUpgradePanel();
        upgradePanel = CreatePanel("Upgrade Panel", new Color(0f, 0f, 0f, 0.82f), new Vector2(820f, 420f));

        Text title = CreateText(upgradePanel.transform, "Level Up!", 38, TextAnchor.MiddleCenter);
        title.color = new Color(1f, 0.85f, 0.2f);
        title.fontStyle = FontStyle.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(400f, 50f));

        Text chooseText = CreateText(upgradePanel.transform, "Choose one upgrade", 18, TextAnchor.MiddleCenter);
        chooseText.color = new Color(0.65f, 0.7f, 0.8f);
        SetRect(chooseText.rectTransform, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), Vector2.zero, new Vector2(360f, 28f));

        for (int i = 0; i < options.Count; i++)
        {
            float xOffset = (i - 1f) * 240f;
            BuildUpgradeCard(upgradePanel.transform, options[i], xOffset);
        }
    }

    private void BuildUpgradeCard(Transform parent, UpgradeOption option, float xOffset)
    {
        UpgradeCategory cat = option.Category;
        Color cardColor, borderColor, labelColor;

        switch (cat)
        {
            case UpgradeCategory.Mage:
                cardColor = new Color(0.06f, 0.05f, 0.22f);
                borderColor = new Color(0.45f, 0.35f, 1f, 0.85f);
                labelColor = new Color(0.55f, 0.45f, 1f);
                break;
            case UpgradeCategory.Warrior:
                cardColor = new Color(0.2f, 0.05f, 0.04f);
                borderColor = new Color(1f, 0.4f, 0.15f, 0.85f);
                labelColor = new Color(1f, 0.45f, 0.15f);
                break;
            case UpgradeCategory.Rare:
                cardColor = new Color(0.12f, 0.08f, 0.02f);
                borderColor = new Color(1f, 0.75f, 0.15f, 0.9f);
                labelColor = new Color(1f, 0.8f, 0.2f);
                break;
            default:
                cardColor = new Color(0.06f, 0.08f, 0.18f);
                borderColor = new Color(0.35f, 0.5f, 0.75f, 0.7f);
                labelColor = new Color(0.45f, 0.65f, 0.9f);
                break;
        }

        string labelText = cat.ToString().ToUpper();

        GameObject cardObj = new GameObject("Upgrade Card");
        cardObj.transform.SetParent(parent, false);

        GameObject borderObj = new GameObject("Card Border");
        borderObj.transform.SetParent(cardObj.transform, false);
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = whiteSprite;
        borderImg.color = borderColor;
        borderImg.raycastTarget = false;
        SetRect(borderImg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject innerObj = new GameObject("Card Inner");
        innerObj.transform.SetParent(cardObj.transform, false);
        Image innerImg = innerObj.AddComponent<Image>();
        innerImg.sprite = whiteSprite;
        innerImg.color = cardColor;
        innerImg.raycastTarget = false;
        SetRect(innerImg.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);

        Button cardButton = cardObj.AddComponent<Button>();
        ColorBlock colors = cardButton.colors;
        colors.normalColor = Color.clear;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
        colors.pressedColor = new Color(0f, 0f, 0f, 0.3f);
        colors.fadeDuration = 0.1f;
        cardButton.colors = colors;

        cardButton.onClick.AddListener(delegate
        {
            if (upgradePanel != null)
            {
                upgradeManager.ApplyUpgrade(option);
            }
        });

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        SetRect(cardRect, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(xOffset, 0f), new Vector2(220f, 280f));

        Text label = CreateText(cardObj.transform, labelText, 14, TextAnchor.MiddleCenter);
        label.color = labelColor;
        label.fontStyle = FontStyle.Bold;
        label.raycastTarget = false;
        SetRect(label.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);

        Text name = CreateText(cardObj.transform, option.Title, 17, TextAnchor.MiddleCenter);
        name.color = Color.white;
        name.fontStyle = FontStyle.Bold;
        name.raycastTarget = false;
        SetRect(name.rectTransform, new Vector2(0.05f, 0.58f), new Vector2(0.95f, 0.79f), Vector2.zero, Vector2.zero);

        Text desc = CreateText(cardObj.transform, option.Description, 13, TextAnchor.MiddleCenter);
        desc.color = new Color(0.65f, 0.68f, 0.75f);
        desc.raycastTarget = false;
        SetRect(desc.rectTransform, new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.55f), Vector2.zero, Vector2.zero);
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            Destroy(upgradePanel);
            upgradePanel = null;
        }
    }

    private void HandlePause()
    {
        if (gameManager == null) return;

        if (gameManager.IsPaused)
        {
            HidePauseMenu();
            gameManager.TogglePause();
        }
        else
        {
            gameManager.TogglePause();
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        if (pauseMenu != null) return;

        pauseMenu = CreatePanel("Pause Menu", new Color(0f, 0f, 0f, 0.8f), new Vector2(420f, 340f));

        Text title = CreateText(pauseMenu.transform, "Paused", 38, TextAnchor.MiddleCenter);
        title.color = new Color(1f, 0.85f, 0.2f);
        title.fontStyle = FontStyle.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(360f, 60f));

        Button resumeBtn = CreateButton(pauseMenu.transform, "Resume", new Color(0.2f, 0.55f, 0.95f));
        SetRect(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(260f, 52f));
        resumeBtn.onClick.AddListener(delegate { HandlePause(); });

        Button restartBtn = CreateButton(pauseMenu.transform, "Restart", new Color(0.25f, 0.35f, 0.55f));
        SetRect(restartBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(260f, 52f));
        restartBtn.onClick.AddListener(delegate
        {
            HidePauseMenu();
            gameManager.TogglePause();
            SceneLoader.LoadGame();
        });

        Button menuBtn = CreateButton(pauseMenu.transform, "Main Menu", new Color(0.3f, 0.15f, 0.15f));
        SetRect(menuBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(260f, 52f));
        menuBtn.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    private void HidePauseMenu()
    {
        if (pauseMenu != null)
        {
            Destroy(pauseMenu);
            pauseMenu = null;
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            return;
        }

        gameOverPanel = CreatePanel("Game Over Panel", new Color(0.08f, 0.02f, 0.02f, 0.9f), new Vector2(560f, 420f));

        BuildResultPanel(gameOverPanel.transform, "\u6e38\u620f\u7ed3\u675f", new Color(1f, 0.25f, 0.2f), new Color(0.2f, 0.42f, 0.72f), new Color(0.5f, 0.16f, 0.18f));
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            return;
        }

        victoryPanel = CreatePanel("Victory Panel", new Color(0.05f, 0.08f, 0.18f, 0.9f), new Vector2(560f, 420f));

        BuildResultPanel(victoryPanel.transform, "Victory!", new Color(1f, 0.85f, 0.2f), new Color(0.2f, 0.55f, 0.25f), new Color(0.5f, 0.16f, 0.18f));
    }

    private void BuildResultPanel(Transform parent, string titleText, Color titleColor, Color retryColor, Color backColor)
    {
        Text title = CreateText(parent, titleText, 44, TextAnchor.MiddleCenter);
        title.color = titleColor;
        title.fontStyle = FontStyle.Bold;
        SetRect(title.rectTransform, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero, new Vector2(420f, 70f));

        int totalSeconds = Mathf.FloorToInt(gameManager.SurvivalTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        int diff = gameManager.DifficultyLevel;

        string line1 = string.Format("Survival:  {0}:{1:00}    Kills:  {2}", minutes, seconds, gameManager.KillCount);
        string line2 = string.Format("Final Level:  Lv.{0}    EXP:  {1}", gameManager.Level, gameManager.CurrentExp);
        string line3 = string.Format("Danger Lv.{0}  (Max)", diff);

        Text stat1 = CreateText(parent, line1, 22, TextAnchor.MiddleCenter);
        stat1.color = new Color(0.85f, 0.9f, 1f);
        SetRect(stat1.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(460f, 36f));

        Text stat2 = CreateText(parent, line2, 22, TextAnchor.MiddleCenter);
        stat2.color = new Color(0.85f, 0.9f, 1f);
        SetRect(stat2.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero, new Vector2(460f, 36f));

        Text stat3 = CreateText(parent, line3, 20, TextAnchor.MiddleCenter);
        stat3.color = diff >= 3 ? new Color(1f, 0.35f, 0.2f) : new Color(1f, 0.7f, 0.2f);
        SetRect(stat3.rectTransform, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(400f, 30f));

        Button retryButton = CreateButton(parent, "\u91cd\u65b0\u5f00\u59cb", retryColor);
        SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(280f, 62f));
        retryButton.onClick.AddListener(delegate { SceneLoader.LoadGame(); });

        Button backButton = CreateButton(parent, "\u8fd4\u56de\u4e3b\u83dc\u5355", backColor);
        SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(280f, 62f));
        backButton.onClick.AddListener(delegate { SceneLoader.LoadMainMenu(); });
    }

    public void HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            Destroy(victoryPanel);
            victoryPanel = null;
        }
    }

    private void BuildUI()
    {
        whiteSprite = CreateWhiteSprite();
        gameCanvas = CreateCanvas("Game Canvas");
        Canvas.ForceUpdateCanvases();

        timerText = CreateText(gameCanvas.transform, "Time 00:00 / 01:00", 22, TextAnchor.MiddleCenter);
        timerText.fontStyle = FontStyle.Bold;
        timerText.color = new Color(0.95f, 0.85f, 0.35f);
        timerText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        timerText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        timerText.rectTransform.pivot = new Vector2(0.5f, 1f);
        timerText.rectTransform.anchoredPosition = new Vector2(0f, -12f);
        timerText.rectTransform.sizeDelta = new Vector2(320f, 34f);

        dangerText = CreateText(gameCanvas.transform, "Danger Lv.0", 16, TextAnchor.MiddleCenter);
        dangerText.fontStyle = FontStyle.Bold;
        dangerText.color = new Color(0.95f, 0.55f, 0.25f);
        dangerText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        dangerText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        dangerText.rectTransform.pivot = new Vector2(0.5f, 1f);
        dangerText.rectTransform.anchoredPosition = new Vector2(0f, -50f);
        dangerText.rectTransform.sizeDelta = new Vector2(200f, 24f);

        killsText = CreateText(gameCanvas.transform, "Kills: 0", 18, TextAnchor.MiddleLeft);
        killsText.fontStyle = FontStyle.Bold;
        killsText.color = new Color(0.9f, 0.85f, 0.5f);
        killsText.rectTransform.anchorMin = new Vector2(0f, 1f);
        killsText.rectTransform.anchorMax = new Vector2(0f, 1f);
        killsText.rectTransform.pivot = new Vector2(0f, 1f);
        killsText.rectTransform.anchoredPosition = new Vector2(20f, -14f);
        killsText.rectTransform.sizeDelta = new Vector2(180f, 28f);

        infoPanel = CreatePanel("Info Panel", new Color(0.05f, 0.05f, 0.12f, 0.82f), new Vector2(760f, 160f));
        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 10f);

        characterText = CreateText(infoPanel.transform, "", 24, TextAnchor.MiddleLeft);
        characterText.fontStyle = FontStyle.Bold;
        characterText.color = new Color(0.95f, 0.85f, 0.4f);
        SetRect(characterText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(140f, 30f));

        levelText = CreateText(infoPanel.transform, "", 18, TextAnchor.MiddleLeft);
        levelText.color = new Color(0.75f, 0.85f, 1f);
        SetRect(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -55f), new Vector2(140f, 24f));

        hpText = CreateText(infoPanel.transform, "", 16, TextAnchor.MiddleCenter);
        hpText.fontStyle = FontStyle.Bold;
        hpText.color = Color.white;
        SetRect(hpText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -14f), new Vector2(380f, 22f));

        GameObject hpBgObject = new GameObject("HP Bar Background");
        hpBgObject.transform.SetParent(infoPanel.transform, false);
        hpBarBackground = hpBgObject.AddComponent<Image>();
        hpBarBackground.sprite = whiteSprite;
        hpBarBackground.color = new Color(0.15f, 0.03f, 0.03f, 0.85f);
        SetRect(hpBarBackground.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -42f), new Vector2(380f, 18f));

        GameObject hpFillObject = new GameObject("HP Bar Fill");
        hpFillObject.transform.SetParent(hpBgObject.transform, false);
        hpBarFill = hpFillObject.AddComponent<Image>();
        hpBarFill.sprite = whiteSprite;
        hpBarFill.color = new Color(0.9f, 0.12f, 0.12f, 0.92f);
        RectTransform hpFillRect = hpBarFill.rectTransform;
        hpFillRect.pivot = new Vector2(0f, 0.5f);
        hpFillRect.anchorMin = new Vector2(0f, 0f);
        hpFillRect.anchorMax = new Vector2(0f, 1f);
        hpFillRect.anchoredPosition = Vector2.zero;
        hpFillRect.sizeDelta = new Vector2(380f, 0f);

        expText = CreateText(infoPanel.transform, "", 14, TextAnchor.MiddleCenter);
        expText.color = new Color(0.6f, 0.75f, 1f);
        SetRect(expText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -82f), new Vector2(380f, 20f));

        GameObject expBgObject = new GameObject("EXP Bar Background");
        expBgObject.transform.SetParent(infoPanel.transform, false);
        expBarBackground = expBgObject.AddComponent<Image>();
        expBarBackground.sprite = whiteSprite;
        expBarBackground.color = new Color(0.03f, 0.05f, 0.18f, 0.85f);
        SetRect(expBarBackground.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -108f), new Vector2(380f, 14f));

        GameObject expFillObject = new GameObject("EXP Bar Fill");
        expFillObject.transform.SetParent(expBgObject.transform, false);
        expBarFill = expFillObject.AddComponent<Image>();
        expBarFill.sprite = whiteSprite;
        expBarFill.color = new Color(0.2f, 0.45f, 0.95f, 0.92f);
        RectTransform expFillRect = expBarFill.rectTransform;
        expFillRect.pivot = new Vector2(0f, 0.5f);
        expFillRect.anchorMin = new Vector2(0f, 0f);
        expFillRect.anchorMax = new Vector2(0f, 1f);
        expFillRect.anchoredPosition = Vector2.zero;
        expFillRect.sizeDelta = new Vector2(380f, 0f);

        CreateAttackSlot();
        CreateSkillSlot();
    }

    private void CreateAttackSlot()
    {
        GameObject slotBg = new GameObject("Attack Slot BG");
        slotBg.transform.SetParent(infoPanel.transform, false);
        Image bgImage = slotBg.AddComponent<Image>();
        bgImage.sprite = whiteSprite;
        bgImage.color = new Color(0.08f, 0.10f, 0.18f, 0.9f);
        RectTransform slotRect = bgImage.rectTransform;
        slotRect.anchorMin = new Vector2(1f, 0.5f);
        slotRect.anchorMax = new Vector2(1f, 0.5f);
        slotRect.pivot = new Vector2(1f, 0.5f);
        slotRect.anchoredPosition = new Vector2(-142f, 0f);
        slotRect.sizeDelta = new Vector2(118f, 118f);

        GameObject overlayObj = new GameObject("Attack Cooldown Overlay");
        overlayObj.transform.SetParent(slotBg.transform, false);
        attackCooldownOverlay = overlayObj.AddComponent<Image>();
        attackCooldownOverlay.sprite = whiteSprite;
        attackCooldownOverlay.color = new Color(0f, 0f, 0f, 0.6f);
        attackCooldownOverlay.type = Image.Type.Filled;
        attackCooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        attackCooldownOverlay.fillOrigin = 2;
        attackCooldownOverlay.fillClockwise = false;
        attackCooldownOverlay.fillAmount = 0f;
        RectTransform overlayRect = attackCooldownOverlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;

        attackNameText = CreateText(slotBg.transform, "", 16, TextAnchor.MiddleCenter);
        attackNameText.fontStyle = FontStyle.Bold;
        attackNameText.color = new Color(1f, 0.85f, 0.5f);
        SetRect(attackNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(105f, 22f));

        Text keyText = CreateText(slotBg.transform, "LMB", 13, TextAnchor.MiddleCenter);
        keyText.color = new Color(0.6f, 0.6f, 0.6f);
        SetRect(keyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(105f, 18f));

        attackDamageText = CreateText(slotBg.transform, "", 14, TextAnchor.MiddleCenter);
        attackDamageText.color = new Color(0.85f, 0.85f, 0.85f);
        SetRect(attackDamageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(105f, 20f));

        attackCooldownText = CreateText(slotBg.transform, "", 15, TextAnchor.MiddleCenter);
        attackCooldownText.fontStyle = FontStyle.Bold;
        attackCooldownText.color = new Color(0.4f, 1f, 0.4f);
        SetRect(attackCooldownText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(105f, 22f));
    }

    private void CreateSkillSlot()
    {
        GameObject slotBg = new GameObject("Skill Slot BG");
        slotBg.transform.SetParent(infoPanel.transform, false);
        Image bgImage = slotBg.AddComponent<Image>();
        bgImage.sprite = whiteSprite;
        bgImage.color = new Color(0.08f, 0.10f, 0.18f, 0.9f);
        RectTransform slotRect = bgImage.rectTransform;
        slotRect.anchorMin = new Vector2(1f, 0.5f);
        slotRect.anchorMax = new Vector2(1f, 0.5f);
        slotRect.pivot = new Vector2(1f, 0.5f);
        slotRect.anchoredPosition = new Vector2(-14f, 0f);
        slotRect.sizeDelta = new Vector2(118f, 118f);

        GameObject overlayObj = new GameObject("Skill Cooldown Overlay");
        overlayObj.transform.SetParent(slotBg.transform, false);
        skillCooldownOverlay = overlayObj.AddComponent<Image>();
        skillCooldownOverlay.sprite = whiteSprite;
        skillCooldownOverlay.color = new Color(0f, 0f, 0f, 0.6f);
        skillCooldownOverlay.type = Image.Type.Filled;
        skillCooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        skillCooldownOverlay.fillOrigin = 2;
        skillCooldownOverlay.fillClockwise = false;
        skillCooldownOverlay.fillAmount = 0f;
        RectTransform overlayRect = skillCooldownOverlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;

        skillNameText = CreateText(slotBg.transform, "", 16, TextAnchor.MiddleCenter);
        skillNameText.fontStyle = FontStyle.Bold;
        skillNameText.color = new Color(1f, 0.85f, 0.5f);
        SetRect(skillNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(105f, 22f));

        Text keyText = CreateText(slotBg.transform, "E", 13, TextAnchor.MiddleCenter);
        keyText.color = new Color(0.6f, 0.6f, 0.6f);
        SetRect(keyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(105f, 18f));

        skillDamageText = CreateText(slotBg.transform, "", 14, TextAnchor.MiddleCenter);
        skillDamageText.color = new Color(0.85f, 0.85f, 0.85f);
        SetRect(skillDamageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(105f, 20f));

        skillCooldownText = CreateText(slotBg.transform, "", 15, TextAnchor.MiddleCenter);
        skillCooldownText.fontStyle = FontStyle.Bold;
        skillCooldownText.color = new Color(0.4f, 1f, 0.4f);
        SetRect(skillCooldownText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(105f, 22f));
    }

    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    private Canvas CreateCanvas(string name)
    {
        GameObject canvasObject = new GameObject(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private GameObject CreatePanel(string name, Color color, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(gameCanvas.transform, false);
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

    private Button CreateButton(Transform parent, string label, Color color)
    {
        GameObject buttonObject = new GameObject("Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = whiteSprite;
        image.color = color;
        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(buttonObject.transform, label, 24, TextAnchor.MiddleCenter);
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
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
