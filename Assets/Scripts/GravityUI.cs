using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityUI : MonoBehaviour
{
    /// <summary>
    /// The water simulation we are altering
    /// </summary>
    public WaterSimulationHandle handle;

    // Update is called once per frame
    void Update()
    {
        // check for a click 
        if (Input.GetMouseButton(0))
        {
            // get the mouse loaction in world space
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 toMouse = worldPos - transform.position;
            toMouse.z = 0;

            // check if we clicked the dial
            if (toMouse.magnitude < 3 && toMouse.magnitude > 0)
            {
                // sum of unit components should be one
                toMouse = toMouse / (Mathf.Abs(toMouse.x) + Mathf.Abs(toMouse.y)); 

                // update the gravity 
                handle.gravity.x = toMouse.x;
                handle.gravity.y = toMouse.y;

                // make dial point in correct direction
                transform.up = -toMouse;
            }
        }
    }
}
