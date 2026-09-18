using UnityEngine;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;

public class NarrativeTextController : MonoBehaviour
{
    public LightController lightController;
    public RoomController roomController;
    public DialogueRunner dialogueRunner;

    private int earlyRelightCount = 0;
    private float lightThreshold;
    public int tiredLineTrigger;


    public bool tiredLineTriggered = false;
   


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void LampRelit()
    {

        Debug.Log("LampRelit was called");
        lightThreshold = Mathf.InverseLerp(0, lightController.maxLampScale, roomController.lampScaleChangeThreshold);

        if(lightController.lightRatio > lightThreshold)
        {
            earlyRelightCount++;
            Debug.Log("Current Early Relight Count: " + earlyRelightCount);
        }

        Debug.Log("Relight Count: " + earlyRelightCount + " | Tired Trigger: " + tiredLineTrigger);
        if (earlyRelightCount >= tiredLineTrigger)
        {
            // Fire the EarlyRelight Yarn node
            if (!tiredLineTriggered)
            {
                dialogueRunner.StartDialogue("EarlyRelight");
                tiredLineTriggered = true;
            }
            
        }


    }

}
