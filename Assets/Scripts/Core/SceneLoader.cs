using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string MainMenuScene = "MainMenuScene";
    public const string CharacterSelectScene = "CharacterSelectScene";
    public const string GameScene = "GameScene";

    public static void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }

    public static void LoadCharacterSelect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(CharacterSelectScene);
    }

    public static void LoadGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameScene);
    }
}
