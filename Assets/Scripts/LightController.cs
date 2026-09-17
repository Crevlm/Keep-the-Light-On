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
    


    //public float globalLightDimSpeed = 0.25f; // how fast the room light dims
    //public float globalLightRestoreSpeed = .5f; // how fast the room light restores
    //public float lampLightShrinkSpeed = .4f; // how fast the light around the lamp shrinks
    //public float lampLightDimSpeed = .1f; // how fast the lamp light dims
    //public float lampLightRestoreSpeed = 3f; // how fast the lamp light restores


    public Color fullLightColor = Color.white;
    public Color darkLightColor;

    public void RestoreLight()
    {
        isRestoringLight = true;

       
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





            //        if (isRestoringLight)
            //        {
            //            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, 1f, (globalLightRestoreSpeed * Time.deltaTime));



            //            lampLight.intensity = Mathf.MoveTowards(lampLight.intensity, 1f, (lampLightRestoreSpeed * Time.deltaTime));

            //            lampLight.transform.localScale = new Vector3
            //                (
            //                Mathf.MoveTowards(lampLight.transform.localScale.x, 6, (lampLightRestoreSpeed * Time.deltaTime)),
            //                Mathf.MoveTowards(lampLight.transform.localScale.y, 6, (lampLightRestoreSpeed * Time.deltaTime)), 1

            //                );
            //            if (globalLight.intensity == 1f && lampLight.intensity == 1f && lampLight.transform.localScale.x == 6 && lampLight.transform.localScale.y == 6)
            //            {
            //                isRestoringLight = false; // reset the bool to not restoring once it hits the max levels. 
            //            }
            //        }


            //else
            //        {
            //            //dims the room light over time but makes sure it doesn't go above 1 or below 0. 
            //            globalLight.intensity = Mathf.Clamp(globalLight.intensity - (globalLightDimSpeed * Time.deltaTime), 0f, 1f);

            //            //dims the intensity of the lamp light over time but makes sure it doesn't go above 1 or below 0. 
            //            lampLight.intensity = Mathf.Clamp(lampLight.intensity - (lampLightDimSpeed * Time.deltaTime), 0f, 1f);

            //            //shrinks the lamp light scale as time passes but makes sure it doesn't drop below 0. 
            //            lampLight.transform.localScale = new Vector3
            //                (
            //                Mathf.Max(lampLight.transform.localScale.x - (lampLightShrinkSpeed * Time.deltaTime), 0),
            //                Mathf.Max(lampLight.transform.localScale.y - (lampLightShrinkSpeed * Time.deltaTime), 0),
            //                1
            //                );

            //        }


            float darknessAmount = 1f - globalLight.intensity;
        //slowly fades the light of the volume from white to a pale blue to help make the scene more dramatic using a lerp to fade between the two colors.
        colorAdjustments.colorFilter.value = Color.Lerp(fullLightColor, darkLightColor, darknessAmount); 

       

    }
}
