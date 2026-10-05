using UnityEditor;
using UnityEngine;
using System.IO;

public class PlaytestSettingsWindow : EditorWindow
{
    private PlaytestRecorder.Settings config;
    private const string ConfigPath = "Assets/Resources/PlaytestSettings.json";
    [MenuItem("Tools/Playtesting/Settings")]
    public static void Open() { GetWindow<PlaytestSettingsWindow>("Playtesting"); }
    private void OnEnable()
    { config = File.Exists(ConfigPath) ? JsonUtility.FromJson<PlaytestRecorder.Settings>(File.ReadAllText(ConfigPath)) : new PlaytestRecorder.Settings(); }
    private void OnGUI()
    {
        if (config == null) OnEnable();
        EditorGUILayout.LabelField("Keep the Light On — Playtesting", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Anonymous sessions record relights, narrative paths, rooms, and last activity. Rebuild WebGL after changing these settings. Editor tests are separate from player sessions.", MessageType.Info);
        config.enabled = EditorGUILayout.Toggle("Enable recording", config.enabled);
        config.recordEditor = EditorGUILayout.Toggle("Record editor tests", config.recordEditor);
        config.buildLabel = EditorGUILayout.TextField("Build label", config.buildLabel);
        config.endpoint = EditorGUILayout.TextField("Collection URL", config.endpoint);
        config.collectionKey = EditorGUILayout.TextField("Collection identifier", config.collectionKey);
        if (GUILayout.Button("Save settings"))
        {
            Directory.CreateDirectory("Assets/Resources");
            File.WriteAllText(ConfigPath, JsonUtility.ToJson(config, true));
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Open playtest dashboard"))
        {
            if (System.Uri.TryCreate(config.endpoint, System.UriKind.Absolute, out var uri))
                Application.OpenURL(uri.GetLeftPart(System.UriPartial.Authority));
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current session", PlaytestRecorder.CurrentSessionId ?? "");
        EditorGUILayout.LabelField("Recorded events", PlaytestRecorder.RecordedEvents.ToString());
        EditorGUILayout.HelpBox("State 5 is recorded as reached, not completed. Call PlaytestRecorder.CompleteGame() from the actual ending once it exists.", MessageType.None);
    }
}
