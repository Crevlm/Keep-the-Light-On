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

    public float globalLightDimSpeed = 0.25f; // how fast the room light dims
    public float globalLightRestoreSpeed = .5f; // how fast the room light restores
    public float lampLightShrinkSpeed = .4f; // how fast the light around the lamp shrinks
    public float lampLightDimSpeed = .1f; // how fast the lamp light dims
    public float lampLightRestoreSpeed = 3f; // how fast the lamp light restores


    public Color fullLightColor = Color.white;
    public Color darkLightColor;

    public void RestoreLight()
    {
        isRestoringLight = true;


       //globalLight.intensity = 1f; //restores the room light to full
       //lampLight.intensity = 1f; // restores the lamp light to full
       //lampLight.transform.localScale = new Vector3(6, 6, 1); //resets the localScale of the lamp light back to it's original large size. 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRestoringLight)
        {
            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, 1f, (globalLightRestoreSpeed * Time.deltaTime));

         

            lampLight.intensity = Mathf.MoveTowards(lampLight.intensity, 1f, (lampLightRestoreSpeed * Time.deltaTime));

            lampLight.transform.localScale = new Vector3
                (
                Mathf.MoveTowards(lampLight.transform.localScale.x, 6, (lampLightRestoreSpeed * Time.deltaTime)),
                Mathf.MoveTowards(lampLight.transform.localScale.y, 6, (lampLightRestoreSpeed * Time.deltaTime)), 1

                );
            if (globalLight.intensity == 1f && lampLight.intensity == 1f && lampLight.transform.localScale.x == 6 && lampLight.transform.localScale.y == 6)
            {
                isRestoringLight = false; // reset the bool to not restoring once it hits the max levels. 
            }
        }


else
        {
            //dims the room light over time but makes sure it doesn't go above 1 or below 0. 
            globalLight.intensity = Mathf.Clamp(globalLight.intensity - (globalLightDimSpeed * Time.deltaTime), 0f, 1f);
           
            //dims the intensity of the lamp light over time but makes sure it doesn't go above 1 or below 0. 
            lampLight.intensity = Mathf.Clamp(lampLight.intensity - (lampLightDimSpeed * Time.deltaTime), 0f, 1f);

            //shrinks the lamp light scale as time passes but makes sure it doesn't drop below 0. 
            lampLight.transform.localScale = new Vector3
                (
                Mathf.Max(lampLight.transform.localScale.x - (lampLightShrinkSpeed * Time.deltaTime), 0),
                Mathf.Max(lampLight.transform.localScale.y - (lampLightShrinkSpeed * Time.deltaTime), 0),
                1
                );

        }


        float darknessAmount = 1f - globalLight.intensity;
        //slowly fades the light of the volume from white to a pale blue to help make the scene more dramatic using a lerp to fade between the two colors.
        colorAdjustments.colorFilter.value = Color.Lerp(fullLightColor, darkLightColor, darknessAmount); 

       

    }
}
