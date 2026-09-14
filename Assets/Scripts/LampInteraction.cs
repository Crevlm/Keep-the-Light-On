using UnityEngine;
using UnityEngine.InputSystem;

public class LampInteraction : MonoBehaviour
{
    Collider2D lampCollider; //Grabs the Polygon Collider from the Lamp Object
    public LightController lightController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lampCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Checks to see if the player is clicking on the lamp.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            //Debug.Log("Left Mouse Button was pressed this frame");
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            if (lampCollider.OverlapPoint(worldPosition))
            {
                //Debug.Log("Point overlaps");
                lightController.RestoreLight();
            }
        }

        

    }
}
