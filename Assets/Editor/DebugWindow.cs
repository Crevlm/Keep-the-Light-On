using UnityEngine;
using UnityEditor;

public class KeepTheLightOnDebugWindow : EditorWindow
{
    private LightController lightController;
    private RoomController roomController;
    private NarrativeTextController narrativeController;

    [MenuItem("Tools/Debug")]
    public static void ShowWindow()
    {
        GetWindow<KeepTheLightOnDebugWindow>("Debug Tool");
    }

    private void OnEnable()
    {
        FindControllers();

        // Continuously repaint the window so values update live.
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    private void FindControllers()
    {
        lightController = FindFirstObjectByType<LightController>();
        roomController = FindFirstObjectByType<RoomController>();
        narrativeController = FindFirstObjectByType<NarrativeTextController>();
    }

    private void OnGUI()
    {
        // Automatically reacquire references if entering Play Mode
        // or if one of the objects disappears.
        if (lightController == null ||
            roomController == null ||
            narrativeController == null)
        {
            FindControllers();
        }

        GUILayout.Label(
            "KEEP THE LIGHT ON — DEBUG",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        DrawControllerSection();

        EditorGUILayout.Space();

        DrawLightSection();

        EditorGUILayout.Space();

        DrawRoomSection();

        EditorGUILayout.Space();

        DrawNarrativeSection();

        EditorGUILayout.Space();

        DrawTestingSection();

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh References"))
        {
            FindControllers();
        }
    }

    // --------------------------------------------------
    // CONTROLLERS
    // --------------------------------------------------

    private void DrawControllerSection()
    {
        GUILayout.Label("CONTROLLERS", EditorStyles.boldLabel);

        EditorGUILayout.ObjectField(
            "Light Controller",
            lightController,
            typeof(LightController),
            true
        );

        EditorGUILayout.ObjectField(
            "Room Controller",
            roomController,
            typeof(RoomController),
            true
        );

        EditorGUILayout.ObjectField(
            "Narrative Controller",
            narrativeController,
            typeof(NarrativeTextController),
            true
        );
    }

    // --------------------------------------------------
    // LIGHT
    // --------------------------------------------------

    private void DrawLightSection()
    {
        GUILayout.Label("LIGHT", EditorStyles.boldLabel);

        if (lightController == null)
        {
            EditorGUILayout.HelpBox(
                "LightController not found.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.LabelField(
            "Light Ratio",
            lightController.lightRatio.ToString("F2")
        );

        if (lightController.lampLight != null)
        {
            EditorGUILayout.LabelField(
                "Lamp Scale",
                lightController.lampLight.transform.localScale.x.ToString("F2")
            );

            EditorGUILayout.LabelField(
                "Lamp Intensity",
                lightController.lampLight.intensity.ToString("F2")
            );
        }

        if (lightController.globalLight != null)
        {
            EditorGUILayout.LabelField(
                "Room Light Intensity",
                lightController.globalLight.intensity.ToString("F2")
            );
        }

        EditorGUILayout.LabelField(
            "Restoring Light",
            lightController.isRestoringLight.ToString()
        );

        EditorGUILayout.LabelField(
            "Dim Speed",
            lightController.lightDimSpeed.ToString("F2")
        );

        EditorGUILayout.LabelField(
            "Restore Speed",
            lightController.lightRestoreSpeed.ToString("F2")
        );
    }

    // --------------------------------------------------
    // ROOM
    // --------------------------------------------------

    private void DrawRoomSection()
    {
        GUILayout.Label("ROOM", EditorStyles.boldLabel);

        if (roomController == null)
        {
            EditorGUILayout.HelpBox(
                "RoomController not found.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.LabelField(
            "Current Room State",
            roomController.currentRoomState.ToString()
        );

        EditorGUILayout.LabelField(
            "Change Triggered",
            roomController.roomChangeTriggered.ToString()
        );

        EditorGUILayout.LabelField(
            "Change Threshold",
            roomController.lampScaleChangeThreshold.ToString("F2")
        );

        if (roomController.lampLightLevel != null)
        {
            float currentLampScale =
                roomController.lampLightLevel.transform.localScale.x;

            EditorGUILayout.LabelField(
                "Current Lamp Scale",
                currentLampScale.ToString("F2")
            );

            float distanceFromThreshold =
                currentLampScale -
                roomController.lampScaleChangeThreshold;

            EditorGUILayout.LabelField(
                "Distance From Threshold",
                distanceFromThreshold.ToString("F2")
            );
        }

        if (roomController.roomStates != null)
        {
            EditorGUILayout.LabelField(
                "Total Room States",
                roomController.roomStates.Length.ToString()
            );
        }
    }

    // --------------------------------------------------
    // NARRATIVE
    // --------------------------------------------------

    private void DrawNarrativeSection()
    {
        GUILayout.Label("NARRATIVE", EditorStyles.boldLabel);

        if (narrativeController == null)
        {
            EditorGUILayout.HelpBox(
                "NarrativeTextController not found.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.LabelField(
            "Tired Line Trigger",
            narrativeController.tiredLineTrigger.ToString()
        );

        EditorGUILayout.LabelField(
            "Tired Line Triggered",
            narrativeController.tiredLineTriggered.ToString()
        );

        /*
         * earlyRelightCount is private inside
         * NarrativeTextController, so this tool intentionally
         * does not access it yet.
         *
         * We can expose it safely later with a public
         * read-only property without making the variable itself
         * public.
         */
    }

    // --------------------------------------------------
    // TESTING CONTROLS
    // --------------------------------------------------

    private void DrawTestingSection()
    {
        GUILayout.Label("TESTING", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to use testing controls.",
                MessageType.Info
            );

            return;
        }

        // -------------------------
        // LIGHT CONTROLS
        // -------------------------

        if (lightController != null)
        {
            if (GUILayout.Button("Restore Light"))
            {
                lightController.RestoreLight();
            }

            if (GUILayout.Button("Force Full Light"))
            {
                lightController.lightRatio = 1f;
                lightController.isRestoringLight = false;
            }

            if (GUILayout.Button("Force Half Light"))
            {
                lightController.lightRatio = 0.5f;
                lightController.isRestoringLight = false;
            }

            if (GUILayout.Button("Force Darkness"))
            {
                lightController.lightRatio = 0f;
                lightController.isRestoringLight = false;
            }
        }

        EditorGUILayout.Space();

        // -------------------------
        // NARRATIVE CONTROLS
        // -------------------------

        if (narrativeController != null)
        {
            if (GUILayout.Button("Test Lamp Relit"))
            {
                narrativeController.LampRelit();
            }

            if (GUILayout.Button("Fire EarlyRelight Dialogue"))
            {
                if (narrativeController.dialogueRunner != null)
                {
                    narrativeController.dialogueRunner.StartDialogue(
                        "EarlyRelight"
                    );
                }
            }
        }
    }
}