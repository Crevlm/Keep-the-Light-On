using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// Passive instrumentation: never drives lighting, room changes, or Yarn playback.
public sealed class PlaytestRecorder : MonoBehaviour
{
    [Serializable] public class Settings
    {
        public bool enabled = true;
        public bool recordEditor = false;
        public string endpoint = "";
        public string collectionKey = ""; // Write-only collection identifier, not a secret.
        public string buildLabel = "week-4-playtest-1";
    }

    [Serializable] public class PlaytestEvent
    {
        public string eventId, sessionId, build, type, utc, node, detail;
        public int sequence, room, totalRelights, relightsInRoom, narrativeStage, narrativePath;
        public double elapsedMs;
        public float lightRatio;
        public bool visible, isTest;
    }
    [Serializable] private class Batch { public string collectionKey; public PlaytestEvent[] events; }
    private static PlaytestRecorder instance;
    private Settings settings;
    private string sessionId, sceneName;
    private double began, nextHeartbeat;
    private int sequence, room, totalRelights, relightsInRoom, stage, path = -1;
    private bool active, visible = true;
    private LightController trackedLight;
    public static string CurrentSessionId => instance != null ? instance.sessionId : "";
    public static int RecordedEvents => instance != null ? instance.sequence : 0;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void KtloPlaytestInit(string endpoint, string key);
    [DllImport("__Internal")] private static extern void KtloPlaytestEvent(string json);
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        TextAsset asset = Resources.Load<TextAsset>("PlaytestSettings");
        if (asset == null) return;
        Settings config;
        try { config = JsonUtility.FromJson<Settings>(asset.text); }
        catch (Exception) { Debug.LogWarning("Playtest settings could not be read. Gameplay is unaffected."); return; }
        if (config == null || !config.enabled || (Application.isEditor && !config.recordEditor)) return;
        if (!Uri.TryCreate(config.endpoint, UriKind.Absolute, out Uri uri) ||
            (uri.Scheme != "https" && !(Application.isEditor && uri.IsLoopback)) ||
            string.IsNullOrWhiteSpace(config.collectionKey)) return;
        instance = new GameObject("Playtest Recorder").AddComponent<PlaytestRecorder>();
        instance.settings = config;
        DontDestroyOnLoad(instance.gameObject);
        SceneManager.sceneLoaded += instance.SceneLoaded;
#if UNITY_WEBGL && !UNITY_EDITOR
        KtloPlaytestInit(config.endpoint, config.collectionKey);
#endif
    }

    public static void BeginSession(int initialRoom)
    {
        if (instance == null) return;
        var r = instance;
        if (r.active) r.End("scene_reloaded");
        r.sessionId = Guid.NewGuid().ToString("N");
        r.sceneName = SceneManager.GetActiveScene().name;
        r.began = Time.realtimeSinceStartupAsDouble;
        r.nextHeartbeat = r.began + 10;
        r.sequence = r.totalRelights = r.relightsInRoom = r.stage = 0;
        r.room = initialRoom; r.path = -1; r.active = true;
        r.trackedLight = FindFirstObjectByType<LightController>();
        r.Emit("session_start"); r.Emit("room_enter");
    }

    public static void Relight(int count, int narrativeStage)
    {
        var r = instance; if (r == null || !r.active) return;
        r.totalRelights++; r.relightsInRoom = count; r.stage = narrativeStage;
        r.Emit("relight");
    }
    public static void Milestone(int narrativeStage, string node)
    {
        var r = instance; if (r == null || !r.active) return;
        r.stage = narrativeStage; r.Emit("milestone", node);
    }
    public static void FirstDarkness(int selectedPath, int narrativeStage, int relights, int newRoom)
    {
        var r = instance; if (r == null || !r.active) return;
        r.path = selectedPath; r.stage = narrativeStage; r.relightsInRoom = relights;
        // Keep this choice in the room where the player let go; room_enter follows.
        string node = narrativeStage == 0 ? "DarknessLowRelight" : "DarknessAfterEL" + narrativeStage;
        r.Emit("first_darkness", node, "next_room_" + newRoom);
    }
    public static void RoomEntered(int newRoom)
    {
        var r = instance; if (r == null || !r.active) return;
        r.room = newRoom; r.relightsInRoom = 0; r.Emit("room_enter");
    }
    public static void DialogueQueued(string node) { if (instance != null) instance.Emit("dialogue_queued", node); }
    public static void DialogueStarted(string node) { if (instance != null) instance.Emit("dialogue_started", node); }
    public static void EndSession(string reason) { if (instance != null) instance.End(reason); }
    // Only call from the actual ending when it is implemented. State 5 is not completion.
    public static void CompleteGame() { if (instance != null) instance.End("completed"); }

    private void Update()
    {
        if (active && Time.realtimeSinceStartupAsDouble >= nextHeartbeat)
        { nextHeartbeat = Time.realtimeSinceStartupAsDouble + 10; Emit("heartbeat"); }
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    { if (active && mode == LoadSceneMode.Single && scene.name != sceneName) End("left_gameplay"); }
    private void OnApplicationFocus(bool hasFocus)
    { visible = hasFocus; Emit(hasFocus ? "focus_returned" : "focus_lost"); }
    private void OnApplicationQuit() { End("application_quit"); }
    private void OnDestroy() { SceneManager.sceneLoaded -= SceneLoaded; if (instance == this) instance = null; }
    private void End(string reason) { if (!active) return; Emit("session_end", "", reason); active = false; }
    private void Emit(string type, string node = "", string detail = "")
    {
        if (!active) return;
        var e = new PlaytestEvent {
            eventId = Guid.NewGuid().ToString("N"), sessionId = sessionId, sequence = ++sequence,
            build = settings.buildLabel, type = type, utc = DateTime.UtcNow.ToString("o"),
            elapsedMs = Math.Round((Time.realtimeSinceStartupAsDouble - began) * 1000),
            room = room, totalRelights = totalRelights, relightsInRoom = relightsInRoom,
            narrativeStage = stage, narrativePath = path, node = node, detail = detail,
            lightRatio = trackedLight != null ? trackedLight.lightRatio : -1, visible = visible, isTest = Application.isEditor
        };
#if UNITY_WEBGL && !UNITY_EDITOR
        KtloPlaytestEvent(JsonUtility.ToJson(e));
#else
        // Editor sends individual events only when explicitly enabled in the playtest settings.
        StartCoroutine(SendEditorEvent(e));
#endif
    }
    private IEnumerator SendEditorEvent(PlaytestEvent e)
    {
        string json = JsonUtility.ToJson(new Batch { collectionKey = settings.collectionKey, events = new[] { e } });
        using (var request = new UnityWebRequest(settings.endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "text/plain;charset=UTF-8");
            request.timeout = 10;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogWarning("Playtest upload failed; gameplay continues. " + request.responseCode);
        }
    }
}
