using UnityEngine;
using UnityEditor;

public class RoomStatePreviewWindow : EditorWindow
{
    private RoomController roomController;

    [MenuItem("Tools/Room State Previewer")]
    public static void ShowWindow()
    {
        GetWindow<RoomStatePreviewWindow>("Room Preview");
    }

    private void OnEnable()
    {
        FindRoomController();
    }

    private void FindRoomController()
    {
        roomController = FindFirstObjectByType<RoomController>();
    }

    private void OnGUI()
    {
        if (roomController == null)
        {
            FindRoomController();
        }

        GUILayout.Label(
            "KEEP THE LIGHT ON — ROOM PREVIEW",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        if (roomController == null)
        {
            EditorGUILayout.HelpBox(
                "RoomController not found in the current scene.",
                MessageType.Warning
            );

            if (GUILayout.Button("Find Room Controller"))
            {
                FindRoomController();
            }

            return;
        }

        EditorGUILayout.ObjectField(
            "Room Controller",
            roomController,
            typeof(RoomController),
            true
        );

        EditorGUILayout.Space();

        if (roomController.roomStates == null ||
            roomController.roomStates.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "No room states are assigned.",
                MessageType.Warning
            );

            return;
        }

        GUILayout.Label(
            "ROOM STATES",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "Total States",
            roomController.roomStates.Length.ToString()
        );

        EditorGUILayout.LabelField(
            "Gameplay State",
            roomController.currentRoomState.ToString()
        );

        EditorGUILayout.Space();

        for (int i = 0; i < roomController.roomStates.Length; i++)
        {
            GameObject roomState = roomController.roomStates[i];

            if (roomState == null)
            {
                EditorGUILayout.HelpBox(
                    "Room State " + i + " is missing!",
                    MessageType.Error
                );

                continue;
            }

            string buttonName =
                "Preview State " + i + " — " + roomState.name;

            if (GUILayout.Button(buttonName))
            {
                PreviewState(i);
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Restore Gameplay State"))
        {
            PreviewState(roomController.currentRoomState);
        }
    }

    private void PreviewState(int stateToPreview)
    {
        if (roomController == null ||
            roomController.roomStates == null)
        {
            return;
        }

        // Allows the preview change to be undone with Ctrl+Z.
        Undo.RecordObjects(
            roomController.roomStates,
            "Preview Room State"
        );

        for (int i = 0; i < roomController.roomStates.Length; i++)
        {
            if (roomController.roomStates[i] != null)
            {
                roomController.roomStates[i].SetActive(
                    i == stateToPreview
                );
            }
        }

        SceneView.RepaintAll();
    }
}
