using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

public class NarrativeTextController : MonoBehaviour
{
    public LightController lightController;
    public RoomController roomController;
    public DialogueRunner dialogueRunner;

    // Counts relights during the current room cycle.
    private int relightsSinceDarkness = 0;

    // Tracks which EarlyRelight dialogue comes next.
    private int narrativeStage = 0;

    // Only prompts started by the runner count toward the darkness response.
    private int shownNarrativeStage = 0;

    // Each room has its own finite set of relight responses for the locked path.
    private int pathRelightStage = 0;
    private readonly int[] pathRelightTriggers = { 5, 10, 15 };
    private readonly string[] pathNames = { "Early", "Middle", "Late" };

    // Original EarlyRelight pacing.
    private int[] earlyRelightTriggers = { 6, 12, 18, 24, 30, 36, 42, 48 };

    // Persistent narrative path chosen on the FIRST darkness.
    //
    // -1 = No path selected yet
    //  0 = Early Let Go
    //  1 = Middle Let Go
    //  2 = Late Let Go
    private int narrativePath = -1;

    // Prevents dialogue from interrupting other dialogue.
    private Queue<string> dialogueQueue = new Queue<string>();


    private bool finalState;
    private bool endingQueued;
    private bool wasDialogueRunning;
    private float nextRelightDialogueTime;
    public bool IsDialogueIdle => dialogueQueue.Count == 0 && !dialogueRunner.IsDialogueRunning;

    public void PlayEnding()
    {
        if (endingQueued) return;
        endingQueued = true;
        QueueDialogue("Ending");
    }

    void Update()
    {
        bool running = dialogueRunner.IsDialogueRunning;
        if (wasDialogueRunning && !running)
            nextRelightDialogueTime = Time.unscaledTime + 4f;
        PlayNextDialogue();
        wasDialogueRunning = dialogueRunner.IsDialogueRunning;
    }


    public void LampRelit()
    {
        relightsSinceDarkness++;
        PlaytestRecorder.Relight(relightsSinceDarkness, narrativeStage);

        if (finalState) return;

        if (narrativePath >= 0)
        {
            if (pathRelightStage < pathRelightTriggers.Length &&
                relightsSinceDarkness >= pathRelightTriggers[pathRelightStage])
            {
                string node = pathNames[narrativePath] + "_Relight" +
                    roomController.currentRoomState + "_" + (pathRelightStage + 1);
                QueueDialogue(node);
                pathRelightStage++;
            }
            return;
        }

        // First darkness permanently switches to the selected Let Go path.
        if (narrativePath == -1 && narrativeStage < earlyRelightTriggers.Length)
        {
            int currentTrigger =
                earlyRelightTriggers[narrativeStage];

            if (relightsSinceDarkness >= currentTrigger)
            {
                string nodeName = "EarlyRelight" + (narrativeStage + 1);

                QueueDialogue(nodeName);

                // Advance permanently so this EarlyRelight
                // cannot play again.
                narrativeStage++;
                PlaytestRecorder.Milestone(narrativeStage, nodeName);
            }
        }
    }


    public void RoomChanged()
    {
        Debug.Log("Relights before darkness: " + relightsSinceDarkness);

        // Darkness supersedes unplayed relight prompts from the previous room.
        // Keep room/path responses, which must still be delivered in order.
        var pending = new Queue<string>();
        while (dialogueQueue.Count > 0)
        {
            string node = dialogueQueue.Dequeue();
            if (!node.StartsWith("EarlyRelight") && !node.Contains("_Relight"))
                pending.Enqueue(node);
        }
        while (pending.Count > 0) dialogueQueue.Enqueue(pending.Dequeue());
        narrativeStage = shownNarrativeStage;

        // FIRST DARKNESS ONLY
        if (narrativePath == -1)
        {
            // Play the specific first-darkness response.
            if (narrativeStage == 0)
            {
                QueueDialogue("DarknessLowRelight");
            }
            else if (narrativeStage == 1)
            {
                QueueDialogue("DarknessAfterEL1");
            }
            else if (narrativeStage == 2)
            {
                QueueDialogue("DarknessAfterEL2");
            }
            else if (narrativeStage == 3)
            {
                QueueDialogue("DarknessAfterEL3");
            }
            else if (narrativeStage == 4)
            {
                QueueDialogue("DarknessAfterEL4");
            }
            else if (narrativeStage == 5)
            {
                QueueDialogue("DarknessAfterEL5");
            }
            else if (narrativeStage == 6)
            {
                QueueDialogue("DarknessAfterEL6");
            }
            else if (narrativeStage == 7)
            {
                QueueDialogue("DarknessAfterEL7");
            }
            else
            {
                QueueDialogue("DarknessAfterEL8");
            }

            // Lock in the long-term path.
            if (narrativeStage <= 2)
            {
                narrativePath = 0;
                Debug.Log("Narrative Path: Early Let Go");
            }
            else if (narrativeStage <= 5)
            {
                narrativePath = 1;
                Debug.Log("Narrative Path: Middle Let Go");
            }
            else
            {
                narrativePath = 2;
                Debug.Log("Narrative Path: Late Let Go");
            }
            PlaytestRecorder.FirstDarkness(narrativePath, narrativeStage, relightsSinceDarkness, roomController.currentRoomState);
        }

        // ALL LATER DARKNESS EVENTS
        else
        {
            string nodeName = "";

            if (narrativePath == 0)
            {
                nodeName = "Early_Darkness" + roomController.currentRoomState;
            }
            else if (narrativePath == 1)
            {
                nodeName = "Middle_Darkness" + roomController.currentRoomState;
            }
            else if (narrativePath == 2)
            {
                nodeName = "Late_Darkness" + roomController.currentRoomState;
            }

            QueueDialogue(nodeName);
        }

        // Reset clicks for the new room cycle.
        relightsSinceDarkness = 0;
        pathRelightStage = 0;
        finalState = roomController.currentRoomState == 5;
    }


    private void QueueDialogue(string nodeName)
    {
        dialogueQueue.Enqueue(nodeName);
        PlaytestRecorder.DialogueQueued(nodeName);
    }

    private string TakeNextDialogue()
    {
        string node = dialogueQueue.Dequeue();
        if (node.StartsWith("EarlyRelight") &&
            int.TryParse(node.Substring("EarlyRelight".Length), out int stage))
        {
            shownNarrativeStage = stage;
        }
        return node;
    }

    private void PlayNextDialogue()
    {
        if (!dialogueRunner.IsDialogueRunning &&
            dialogueQueue.Count > 0)
        {
            string pendingNode = dialogueQueue.Peek();
            bool isRelight = pendingNode.StartsWith("EarlyRelight") || pendingNode.Contains("_Relight");
            if (isRelight && Time.unscaledTime < nextRelightDialogueTime) return;
            string nextNode = TakeNextDialogue();

            dialogueRunner.StartDialogue(nextNode);
            PlaytestRecorder.DialogueStarted(nextNode);
        }
    }
}