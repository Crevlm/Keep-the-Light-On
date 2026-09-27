using UnityEngine;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;
using System.Collections.Generic;

public class NarrativeTextController : MonoBehaviour
{
    public LightController lightController;
    public RoomController roomController;
    public DialogueRunner dialogueRunner;

    private int earlyRelightCount = 0;
    private float lightThreshold;

    private int narrativeStage = 0;
    private int[] earlyRelightTriggers = { 5, 15, 25, 35, 45, 55, 65, 80 };

    private Queue<string> dialogueQueue = new Queue<string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        PlayNextDialogue();
    }

    public void LampRelit()
    {
        Debug.Log("LampRelit was called");

        lightThreshold = Mathf.InverseLerp(
            0,
            lightController.maxLampScale,
            roomController.lampScaleChangeThreshold
        );

        if (lightController.lightRatio > lightThreshold)
        {
            earlyRelightCount++;
            Debug.Log("Current Early Relight Count: " + earlyRelightCount);
        }

        
        if (narrativeStage < earlyRelightTriggers.Length)
        {
            if (earlyRelightCount >= earlyRelightTriggers[narrativeStage])
            {
                string nodeName = "EarlyRelight" + (narrativeStage + 1);
                dialogueQueue.Enqueue(nodeName);
                narrativeStage++;
            }
        }
    }

    private void PlayNextDialogue()
    {
        if (!dialogueRunner.IsDialogueRunning && dialogueQueue.Count >0)
        {
            string nextNode = dialogueQueue.Dequeue();
            dialogueRunner.StartDialogue(nextNode);
        }
    }

}