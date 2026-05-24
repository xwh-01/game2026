using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class MiniDungeonAutoSetup
{
    private const string AutoSetupKey = "MiniDungeonShooter.AutoSetupDone";
    private const string ScenesFolder = "Assets/Scenes";
    private const string MainMenuScenePath = ScenesFolder + "/MainMenuScene.unity";
    private const string CharacterSelectScenePath = ScenesFolder + "/CharacterSelectScene.unity";
    private const string GameScenePath = ScenesFolder + "/GameScene.unity";

    static MiniDungeonAutoSetup()
    {
        EditorApplication.delayCall += RunOnceAfterProjectOpen;
    }

    [MenuItem("Tools/Mini Dungeon Shooter/Setup Demo Scenes")]
    public static void SetupDemoScenesFromMenu()
    {
        SetupDemoScenes(true);
    }

    [MenuItem("Tools/Mini Dungeon Shooter/Open Main Menu Scene")]
    public static void OpenMainMenuScene()
    {
        SetupDemoScenes(false);
        EditorSceneManager.OpenScene(MainMenuScenePath);
    }

    private static void RunOnceAfterProjectOpen()
    {
        if (SessionState.GetBool(AutoSetupKey, false))
        {
            return;
        }

        SessionState.SetBool(AutoSetupKey, true);
        SetupDemoScenes(false);
    }

    private static void SetupDemoScenes(bool showDialog)
    {
        EnsureScenesFolder();

        CreateOrUpdateScene(MainMenuScenePath, "MainMenuBootstrap", typeof(MainMenuUI));
        CreateOrUpdateScene(CharacterSelectScenePath, "CharacterSelectBootstrap", typeof(CharacterSelectUI));
        CreateOrUpdateScene(GameScenePath, "GameBootstrap", typeof(GameManager));
        EnsureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(MainMenuScenePath);

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Mini Dungeon Shooter",
                "Demo scenes are ready.\n\nOpen Assets/Scenes/MainMenuScene.unity and press Play.",
                "OK");
        }
    }

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    private static void CreateOrUpdateScene(string scenePath, string bootstrapName, System.Type bootstrapType)
    {
        Scene scene;
        if (File.Exists(scenePath))
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        GameObject bootstrap = GameObject.Find(bootstrapName);
        if (bootstrap == null)
        {
            bootstrap = new GameObject(bootstrapName);
        }

        if (bootstrap.GetComponent(bootstrapType) == null)
        {
            bootstrap.AddComponent(bootstrapType);
        }

        RemoveDuplicateBootstraps(bootstrapName, bootstrap);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scenePath);
    }

    private static void RemoveDuplicateBootstraps(string bootstrapName, GameObject keep)
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject item = allObjects[i];
            if (item != keep && item.name == bootstrapName)
            {
                Object.DestroyImmediate(item);
            }
        }
    }

    private static void EnsureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        AddSceneIfMissing(scenes, MainMenuScenePath);
        AddSceneIfMissing(scenes, CharacterSelectScenePath);
        AddSceneIfMissing(scenes, GameScenePath);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == path)
            {
                scenes[i] = new EditorBuildSettingsScene(path, true);
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(path, true));
    }
}
