using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomController : MonoBehaviour
{
    public int currentRoomState = 0; // Which of the rooms is currently being shown.
    public bool roomChangeTriggered = false; // has the threshold for room changing been hit?
    public Light2D lampLightLevel; // reads the current roomlight in the scene.

    public float lampScaleChangeThreshold = 2f; //stores the light intensity at which a room change should happen.

    public GameObject[] roomStates;

    public LightController lightController;


    //private float previousLightRatio;
    //private float roomChangeRatio;

    private float previousLampScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousLampScale = lampLightLevel.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {


        float currentLampScale = lampLightLevel.transform.localScale.x;

        if (lightController.isRestoringLight == false && previousLampScale > lampScaleChangeThreshold && currentLampScale <= lampScaleChangeThreshold)
            
        {
            if (currentRoomState < roomStates.Length - 1)
            {

                Debug.Log("ROOM CHANGED | Lamp Scale: " + currentLampScale
       + " | Threshold: " + lampScaleChangeThreshold
       + " | Light Ratio: " + lightController.lightRatio);

                roomStates[currentRoomState].SetActive(false);

                currentRoomState++;

                roomStates[currentRoomState].SetActive(true);
                roomChangeTriggered = true;
            }
        }

        previousLampScale = currentLampScale;

        if (lightController.lightRatio >= 1f
     && roomChangeTriggered == true)
        {
            roomChangeTriggered = false;
        }
    }
}