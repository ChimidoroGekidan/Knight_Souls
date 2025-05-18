
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class AutoSave
{
    public static readonly string manualSaveKey = "autosave@manualSave";

    static double nextTime = 0;
    static bool isChangedHierarchy = false;

    static AutoSave()
    {
        IsManualSave = true;
        // EditorApplication.playmodeStateChanged += () =>
        EditorApplication.playModeStateChanged += (_) =>
        {
            if (IsAutoSave && !EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                IsManualSave = false;
                if (IsSavePrefab)
                    AssetDatabase.SaveAssets();
                if (IsSaveScene)
                {
                    Debug.Log("save scene " + System.DateTime.Now);
                    EditorSceneManager.SaveOpenScenes();
                }
                IsManualSave = true;
            }
            isChangedHierarchy = false;
        };

        nextTime = EditorApplication.timeSinceStartup + Interval;
        EditorApplication.update += () =>
        {
            if (isChangedHierarchy && nextTime < EditorApplication.timeSinceStartup)
            {
                nextTime = EditorApplication.timeSinceStartup + Interval;
                IsManualSave = false;
                if (IsSaveSceneTimer && IsAutoSave && !EditorApplication.isPlaying)
                {
                    if (IsSavePrefab)
                        AssetDatabase.SaveAssets();
                    if (IsSaveScene)
                    {
                        Debug.Log("save scene " + System.DateTime.Now);
                        EditorSceneManager.SaveOpenScenes();
                    }
                }
                isChangedHierarchy = false;
                IsManualSave = true;
            }
        };

        EditorApplication.hierarchyChanged += () =>
        {
            if (!EditorApplication.isPlaying)
                isChangedHierarchy = true;
        };
    }

    public static bool IsManualSave
    {
        get
        {
            return EditorPrefs.GetBool(manualSaveKey);
        }
        private set
        {
            EditorPrefs.SetBool(manualSaveKey, value);
        }
    }


    private static readonly string autoSave = "auto save";
    static bool IsAutoSave
    {
        get
        {
            string value = EditorUserSettings.GetConfigValue(autoSave);
            return !string.IsNullOrEmpty(value) && value.Equals("True");
        }
        set
        {
            EditorUserSettings.SetConfigValue(autoSave, value.ToString());
        }
    }

    private static readonly string autoSavePrefab = "auto save prefab";
    static bool IsSavePrefab
    {
        get
        {
            string value = EditorUserSettings.GetConfigValue(autoSavePrefab);
            return !string.IsNullOrEmpty(value) && value.Equals("True");
        }
        set
        {
            EditorUserSettings.SetConfigValue(autoSavePrefab, value.ToString());
        }
    }

    private static readonly string autoSaveScene = "auto save scene";
    static bool IsSaveScene
    {
        get
        {
            string value = EditorUserSettings.GetConfigValue(autoSaveScene);
            return !string.IsNullOrEmpty(value) && value.Equals("True");
        }
        set
        {
            EditorUserSettings.SetConfigValue(autoSaveScene, value.ToString());
        }
    }

    private static readonly string autoSaveSceneTimer = "auto save scene timer";
    static bool IsSaveSceneTimer
    {
        get
        {
            string value = EditorUserSettings.GetConfigValue(autoSaveSceneTimer);
            return !string.IsNullOrEmpty(value) && value.Equals("True");
        }
        set
        {
            EditorUserSettings.SetConfigValue(autoSaveSceneTimer, value.ToString());
        }
    }

    private static readonly string autoSaveInterval = "save scene interval";
    static int Interval
    {
        get
        {

            string value = EditorUserSettings.GetConfigValue(autoSaveInterval);
            if (value == null)
            {
                value = "60";
            }
            return int.Parse(value);
        }
        set
        {
            if (value < 60)
                value = 60;
            EditorUserSettings.SetConfigValue(autoSaveInterval, value.ToString());
        }
    }


    // [PreferenceItem("Auto Save")]
    [SettingsProvider]
    static SettingsProvider ExampleOnGUI()
    {
        SettingsProvider provider = new SettingsProvider("Preferences/AutoSave", SettingsScope.User)
        {
            label = "[ Auto Save! ]",
            guiHandler = (_) =>
            {
                bool isAutoSave = EditorGUILayout.BeginToggleGroup("auto save", IsAutoSave);
                IsAutoSave = isAutoSave;
                EditorGUILayout.Space();
                IsSavePrefab = EditorGUILayout.ToggleLeft("save prefab", IsSavePrefab);
                IsSaveScene = EditorGUILayout.ToggleLeft("save scene", IsSaveScene);
                IsSaveSceneTimer = EditorGUILayout.BeginToggleGroup("save scene interval", IsSaveSceneTimer);
                Interval = EditorGUILayout.IntField("interval(sec) min60sec", Interval);
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndToggleGroup();
            },
            keywords = new HashSet<string>(new[] { "AutoSave" })
        };
        return provider;
    }

    [MenuItem("File/Backup/Backup")]
    public static void Backup()
    {
        for (int i = 0; (i < EditorSceneManager.sceneCount); ++i)
        {
            string scenePath = EditorSceneManager.GetSceneAt(i).path;
            string exportPath = "Backup/" + scenePath;

            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            if (string.IsNullOrEmpty(scenePath))
                return;
            

            byte[] data = File.ReadAllBytes(scenePath);
            File.WriteAllBytes(exportPath, data);
        }
    }

    [MenuItem("File/Backup/Rollback")]
    public static void RollBack()
    {
        for (int i = 0; (i < EditorSceneManager.sceneCount); ++i)
        {
            string scenePath = EditorSceneManager.GetSceneAt(i).path;
            string exportPath = "Backup/" + scenePath;

            byte[] data = File.ReadAllBytes(exportPath);
            File.WriteAllBytes(scenePath, data);
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
    }

}