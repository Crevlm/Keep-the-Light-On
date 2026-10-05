using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class PlaytestIntegrationChecks
{
    [MenuItem("Tools/Playtesting/Verify narrative hooks")]
    public static void Run()
    {
        int[] thresholds = { 6, 12, 18, 24, 30, 36, 42, 48 };
        for (int shown = 0; shown <= 8; shown++)
        {
            var go = new GameObject("Narrative regression check");
            try
            {
                var room = go.AddComponent<RoomController>();
                var narrative = go.AddComponent<NarrativeTextController>();
                narrative.roomController = room;
                var queue = Read<Queue<string>>(narrative, "dialogueQueue");
                // Rapid input earns every prompt; the runner has only started 'shown'.
                for (int clicks = 1; clicks <= 64; clicks++)
                {
                    narrative.LampRelit();
                    Require(queue.Count == thresholds.Count(t => t <= clicks), "Trigger pacing changed");
                }
                for (int stage = 1; stage <= shown; stage++)
                    Require(Take(narrative) == "EarlyRelight" + stage, "Prompt order changed");
                room.currentRoomState = 1;
                narrative.RoomChanged();
                int path = shown <= 2 ? 0 : shown <= 5 ? 1 : 2;
                string response = shown == 0 ? "DarknessLowRelight" : "DarknessAfterEL" + shown;
                Require(Read<int>(narrative, "narrativePath") == path, "Path used queued rather than shown prompts");
                Require(queue.Count == 1 && Take(narrative) == response, "Stale relight prompts delayed darkness");
                Require(Read<int>(narrative, "narrativeStage") == shown, "Unshown prompts counted as reached");
                string prefix = new[] { "Early", "Middle", "Late" }[path];
                for (int state = 1; state <= 4; state++)
                {
                    int[] pathTriggers = { 5, 10, 15 };
                    for (int click = 1; click <= 40; click++)
                    {
                        narrative.LampRelit();
                        Require(queue.Count == pathTriggers.Count(t => t <= click), "Path relight pacing/repetition mismatch");
                    }
                    Require(!queue.Any(n => n.StartsWith("EarlyRelight")), "Original EarlyRelight sequence resumed");
                    for (int prompt = 1; prompt <= 3; prompt++)
                        Require(Take(narrative) == prefix + "_Relight" + state + "_" + prompt, "Wrong path/room relight node");
                    Require(Read<int>(narrative, "narrativePath") == path, "Path changed after relights");
                    if (state < 5)
                    {
                        room.currentRoomState = state + 1;
                        narrative.RoomChanged();
                        Require(queue.Count == 1 && Take(narrative) == prefix + "_Darkness" + (state + 1), "Persistent path response mismatch");
                    }
                }
                for (int click = 0; click < 100; click++) narrative.LampRelit();
                Require(queue.Count == 0, "State 5 queued relight prompts instead of ending");
                narrative.PlayEnding();
                narrative.PlayEnding();
                Require(queue.Count == 1 && Take(narrative) == "Ending", "Ending missing or repeated");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        Debug.Log("PLAYTEST CHECKS PASSED: slower pacing, nine darkness responses, persistent paths, State 5 relight suppression, and one-time ending.");
    }
    private static string Take(object value) => (string)value.GetType().GetMethod("TakeNextDialogue", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(value, null);
    private static T Read<T>(object value, string name) => (T)value.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value);
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
