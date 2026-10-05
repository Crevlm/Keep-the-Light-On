using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class RoomController : MonoBehaviour
{
    public int currentRoomState = 0; // Which of the rooms is currently being shown.
    public bool roomChangeTriggered = false; // has the threshold for room changing been hit?
    public Light2D lampLightLevel; // reads the current roomlight in the scene.

    public float lampScaleChangeThreshold = 2f; //stores the light intensity at which a room change should happen.

    public GameObject[] roomStates;

    public LightController lightController;
    public NarrativeTextController narrativeTextController;


    // --------------------
    // AMBIENT AUDIO
    // --------------------

    public AudioClip[] stateAmbience;

    public AudioSource ambienceSourceA;
    public AudioSource ambienceSourceB;

    public float crossfadeDuration = 3f;

    private AudioSource currentAmbienceSource;
    private AudioSource nextAmbienceSource;


    private float previousLampScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.AddComponent<StateDevelopmentNotice>().Initialize(this);
        PlaytestRecorder.BeginSession(currentRoomState);
        previousLampScale = lampLightLevel.transform.localScale.x;

        // Set our two reusable AudioSources
        currentAmbienceSource = ambienceSourceA;
        nextAmbienceSource = ambienceSourceB;

        // Start State 0 ambience
        if (stateAmbience.Length > 0)
        {
            currentAmbienceSource.clip = stateAmbience[0];
            currentAmbienceSource.volume = 1f;
            currentAmbienceSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float currentLampScale = lampLightLevel.transform.localScale.x;

        if (lightController.isRestoringLight == false &&
            previousLampScale > lampScaleChangeThreshold &&
            currentLampScale <= lampScaleChangeThreshold)
        {
            if (currentRoomState < roomStates.Length - 1)
            {
                

                roomStates[currentRoomState].SetActive(false);

                currentRoomState++;

                roomStates[currentRoomState].SetActive(true);
                narrativeTextController.RoomChanged();
                PlaytestRecorder.RoomEntered(currentRoomState);
                if (currentRoomState == 5)
                {
                    var ending = gameObject.AddComponent<EndingSequence>();
                    ending.Begin(lightController, narrativeTextController);
                }

                // Crossfade into the new state's ambience
                if (currentRoomState < stateAmbience.Length)
                {
                    StartCoroutine(CrossfadeAmbience(stateAmbience[currentRoomState]));
                }

                roomChangeTriggered = true;
            }
        }

        previousLampScale = currentLampScale;

        if (lightController.lightRatio >= 1f &&
            roomChangeTriggered == true)
        {
            roomChangeTriggered = false;
        }
    }

    // --------------------
    // AUDIO CROSSFADE
    // --------------------

    IEnumerator CrossfadeAmbience(AudioClip newClip)
    {
        nextAmbienceSource.clip = newClip;
        nextAmbienceSource.volume = 0f;
        nextAmbienceSource.Play();

        float elapsedTime = 0f;

        while (elapsedTime < crossfadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float fadeAmount = elapsedTime / crossfadeDuration;

            currentAmbienceSource.volume = 1f - fadeAmount;
            nextAmbienceSource.volume = fadeAmount;

            yield return null;
        }

        currentAmbienceSource.Stop();
        currentAmbienceSource.volume = 0f;

        nextAmbienceSource.volume = 1f;


        // Swap the AudioSources
        AudioSource tempSource = currentAmbienceSource;
        currentAmbienceSource = nextAmbienceSource;
        nextAmbienceSource = tempSource;

    }
}