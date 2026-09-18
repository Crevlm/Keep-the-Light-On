using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{

    public Light2D globalLight; //room light
    public Light2D lampLight; // lamp light
    public Volume globalVolume; // global volume 
    private ColorAdjustments colorAdjustments; 

    public bool isRestoringLight = false; // Bool to check whether the light is restoring light
    
   // One normalized value controls the entire lighting state.
   // 1 = fully lit
   // 0 = fully dark
    public float lightRatio = 1f;
    

    public float lightDimSpeed = 1f;
    public float lightRestoreSpeed = .5f;

    public float maxGlobalLightIntensity = 1f;
    public float maxLampLightIntensity = 1f;
    public float maxLampScale = 6f;
    


    public Color fullLightColor = Color.white;
    public Color darkLightColor;


    public NarrativeTextController narrativeTextController;

    public void RestoreLight()
    {
        Debug.Log("RestoreLight was called");
        isRestoringLight = true;
        narrativeTextController.LampRelit();
       
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);

        //Convert the lamp's starting scale into our normalized 0-1 ratio.
        lightRatio = Mathf.InverseLerp(0f, maxLampScale, lampLight.transform.localScale.x);

    }

    // Update is called once per frame
    void Update()
    {

        if (isRestoringLight)
        {
            //Move the normalized light value toward full light.
            lightRatio = Mathf.MoveTowards(lightRatio, 1f, lightRestoreSpeed * Time.deltaTime);

            if (lightRatio >= 1f)
            {
                lightRatio = 1f;
                isRestoringLight = false;
            }

        }

        else
        {
            // Move the normalized light value toward darkness.
            lightRatio = Mathf.MoveTowards(lightRatio, 0, lightDimSpeed * Time.deltaTime);
        }

        //Convert the same 0-1 reation into each light's actual values

        float roomLightRatio = Mathf.InverseLerp(0.6f, 1f, lightRatio);
        globalLight.intensity = Mathf.Lerp(0f, maxGlobalLightIntensity, roomLightRatio);
        lampLight.intensity = Mathf.Lerp(0f, maxLampLightIntensity, lightRatio);
        float lampScale = Mathf.Lerp(0f, maxLampScale, lightRatio);
        lampLight.transform.localScale = new Vector3(lampScale, lampScale, 1f);





            float darknessAmount = 1f - globalLight.intensity;
        //slowly fades the light of the volume from white to a pale blue to help make the scene more dramatic using a lerp to fade between the two colors.
        colorAdjustments.colorFilter.value = Color.Lerp(fullLightColor, darkLightColor, darknessAmount); 

       

    }
}
