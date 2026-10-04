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

    // Original EarlyRelight pacing.
    private int[] earlyRelightTriggers = { 5, 8, 12, 15, 18, 21, 24, 30 };

    // Offsets the trigger values after a room change so the player
    // doesn't have to repeat all previous clicks.
    private int relightTriggerOffset = 0;

    // Persistent narrative path chosen on the FIRST darkness.
    //
    // -1 = No path selected yet
    //  0 = Early Let Go
    //  1 = Middle Let Go
    //  2 = Late Let Go
    private int narrativePath = -1;

    // Prevents dialogue from interrupting other dialogue.
    private Queue<string> dialogueQueue = new Queue<string>();


    void Update()
    {
        PlayNextDialogue();
    }


    public void LampRelit()
    {
        relightsSinceDarkness++;

        // Only check for another EarlyRelight if there are
        // still unseen EarlyRelight nodes.
        if (narrativeStage < earlyRelightTriggers.Length)
        {
            int currentTrigger =
                earlyRelightTriggers[narrativeStage] - relightTriggerOffset;

            if (relightsSinceDarkness >= currentTrigger)
            {
                string nodeName = "EarlyRelight" + (narrativeStage + 1);

                dialogueQueue.Enqueue(nodeName);

                // Advance permanently so this EarlyRelight
                // cannot play again.
                narrativeStage++;
            }
        }
    }


    public void RoomChanged()
    {
        Debug.Log("Relights before darkness: " + relightsSinceDarkness);

        // FIRST DARKNESS ONLY
        if (narrativePath == -1)
        {
            // Play the specific first-darkness response.
            if (narrativeStage == 0)
            {
                dialogueQueue.Enqueue("DarknessLowRelight");
            }
            else if (narrativeStage == 1)
            {
                dialogueQueue.Enqueue("DarknessAfterEL1");
            }
            else if (narrativeStage == 2)
            {
                dialogueQueue.Enqueue("DarknessAfterEL2");
            }
            else if (narrativeStage == 3)
            {
                dialogueQueue.Enqueue("DarknessAfterEL3");
            }
            else if (narrativeStage == 4)
            {
                dialogueQueue.Enqueue("DarknessAfterEL4");
            }
            else if (narrativeStage == 5)
            {
                dialogueQueue.Enqueue("DarknessAfterEL5");
            }
            else if (narrativeStage == 6)
            {
                dialogueQueue.Enqueue("DarknessAfterEL6");
            }
            else if (narrativeStage == 7)
            {
                dialogueQueue.Enqueue("DarknessAfterEL7");
            }
            else
            {
                dialogueQueue.Enqueue("DarknessAfterEL8");
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

            dialogueQueue.Enqueue(nodeName);
        }

        // Preserve EarlyRelight spacing.
        if (narrativeStage > 0)
        {
            relightTriggerOffset = earlyRelightTriggers[narrativeStage - 1];
        }

        // Reset clicks for the new room cycle.
        relightsSinceDarkness = 0;
    }


    private void PlayNextDialogue()
    {
        if (!dialogueRunner.IsDialogueRunning &&
            dialogueQueue.Count > 0)
        {
            string nextNode = dialogueQueue.Dequeue();

            dialogueRunner.StartDialogue(nextNode);
        }
    }
}