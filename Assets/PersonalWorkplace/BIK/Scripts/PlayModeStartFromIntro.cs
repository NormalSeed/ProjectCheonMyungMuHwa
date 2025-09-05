#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeStartFromIntro
{
    private const string IntroScenePath = "Assets/Scenes/IntroScene.unity";
    private const string MenuName = "Tools/Always Start From IntroScene";

    private static bool _enabled;

    static PlayModeStartFromIntro()
    {
        _enabled = EditorPrefs.GetBool(MenuName, true);
        SetPlayModeStartScene(_enabled);

        // 메뉴 아이템 상태 반영
        EditorApplication.delayCall += () => {
            Menu.SetChecked(MenuName, _enabled);
        };
    }

    [MenuItem(MenuName)]
    private static void Toggle()
    {
        _enabled = !_enabled;
        EditorPrefs.SetBool(MenuName, _enabled);
        Menu.SetChecked(MenuName, _enabled);
        SetPlayModeStartScene(_enabled);
    }

    private static void SetPlayModeStartScene(bool enabled)
    {
        if (enabled) {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroScenePath);
            EditorSceneManager.playModeStartScene = sceneAsset;
        }
        else {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
#endif
