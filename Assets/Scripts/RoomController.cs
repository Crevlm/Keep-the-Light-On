using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomController : MonoBehaviour

{
    public int currentRoomState = 0; // Which of the rooms is currently being shown.
    public bool roomChangeTriggered = false; // has the threshold for room changing been hit?
    public Light2D roomLightLevel; // reads the current roomlight in the scene.
    public float roomChangeThreshold = 0.2f; //stores the light intensity at which a room change should happen.


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (roomLightLevel.intensity <= roomChangeThreshold && roomChangeTriggered == false)
        {
            roomChangeTriggered = true;
            currentRoomState++;
        }

        if (roomLightLevel.intensity > roomChangeThreshold && roomChangeTriggered == true)
        {
            roomChangeTriggered = false;

        }

    }
}
