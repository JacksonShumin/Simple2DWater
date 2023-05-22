using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    // Time variables
    /// <summary>
    /// Counts frames between updating the FPS UI
    /// </summary>
    private int counter;

    /// <summary>
    /// Frames that need to go by before updating the FPS UI
    /// </summary>
    public int framesToWait;

    // Update is called once per frame
    void Update()
    {
        // only update the UI when enought time has gone by
        if (counter > framesToWait)
        {
            GetComponent<Text>().text = "FPS: " + (int)(1.0f / Time.deltaTime);
            counter = 0;
        }

        // increment the counter
        counter++;
    }
}
