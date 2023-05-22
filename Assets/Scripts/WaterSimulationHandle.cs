using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterSimulationHandle : MonoBehaviour
{
    /// <summary>
    /// A vector that is used for gravity in the simulation. 
    /// Only the X and Y values are used in this 2D simulation. 
    /// </summary>
    public Vector4 gravity;

    #region Compute Shaders
    /// <summary>
    /// Compute Shader used on the obstical map for initial values. 
    /// </summary>
    [SerializeField] private ComputeShader ObsticleSetup; 

    /// <summary>
    /// Shader used to generate values for the Dispersion texture. 
    /// </summary>
    [SerializeField] private ComputeShader BuildDispersion;

    /// <summary>
    /// Shader that can read the dispersion map and apply the changes to the fluid map. 
    /// </summary>
    [SerializeField] private ComputeShader ApplyDispersion;

    /// <summary>
    /// Fluid injector shader. Doubles as an injector for obsticles as well. 
    /// </summary>
    [SerializeField] private ComputeShader Inject;

    [SerializeField] private Text LengthUI;
    [SerializeField] private float compresson;
    #endregion

    #region Textures
    /// <summary>
    /// Texture that holds the fluid level for every cell. 
    /// </summary>
    private TextureSwapper fluidTextures;

    /// <summary>
    /// Texture that stores outgoing flow for every cell. 
    /// </summary>
    private RenderTexture dispTexture;

    /// <summary>
    /// Texture that stores the location of obsticles that block fluid. 
    /// </summary>
    private RenderTexture obsticleTexture;
    #endregion

    #region Texture Info
    /// <summary>
    /// Width of the simulation textures. 
    /// </summary>
    private int width = 128;

    /// <summary>
    /// Height of the simulation texture. 
    /// </summary>
    private int height = 128;
    #endregion

    #region Injection Info
    /// <summary>
    /// Amount of fluid injected when clicking. 
    /// </summary>
    private float injectionAmount = 1;

    /// <summary>
    /// Radius of the injection area in pixels. 
    /// </summary>
    private float injectionRadius;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        ResetWater();
    }

    /// <summary>
    /// Builds the textures required for the simulation, updates the level of detail UI, and sends textures to materials where necessary 
    /// </summary>
    public void ResetWater()
    {

        #region Make the fluid textures

        // fluid texture used to hold the value of the water 
        RenderTexture fluidTexture1 = new RenderTexture(width, height, 1, RenderTextureFormat.RFloat);
        fluidTexture1.enableRandomWrite = true;
        fluidTexture1.filterMode = FilterMode.Point;
        fluidTexture1.Create();

        // Second fluid texture needed for a swap chain
        // Two reasons for this:
        //     GPU should avoid reading and writing to the same texture in the same pass
        //     Cellular automa cannot work properly when there is a single texture
        RenderTexture fluidTexture2 = new RenderTexture(width, height, 1, RenderTextureFormat.RFloat);
        fluidTexture2.enableRandomWrite = true;
        fluidTexture2.filterMode = FilterMode.Point;
        fluidTexture2.Create();

        // Send the two textures into the texture swapper
        fluidTextures = new TextureSwapper(fluidTexture1, fluidTexture2);

        // fluid texture used to hold the velocity of the water
        dispTexture = new RenderTexture(width, height, 1, RenderTextureFormat.ARGBFloat);
        dispTexture.enableRandomWrite = true;
        dispTexture.filterMode = FilterMode.Point;
        dispTexture.Create();

        // seperate obsticle texture 
        obsticleTexture = new RenderTexture(width, height, 1, RenderTextureFormat.RFloat);
        obsticleTexture.enableRandomWrite = true;
        obsticleTexture.filterMode = FilterMode.Point;
        obsticleTexture.Create();

        #endregion

        // Run the setup compute shader
        // Makes a one pixel border on all the map edges 
        ObsticleSetup.SetTexture(0, "Obsticles", obsticleTexture);
        ObsticleSetup.SetInt("width", obsticleTexture.width);
        ObsticleSetup.SetInt("height", obsticleTexture.height);
        ObsticleSetup.Dispatch(0, width / 8, height / 8, 1);

        // Send the obsticle texture to the display's material
        GetComponent<MeshRenderer>().material.SetTexture("_ObsticleTex", obsticleTexture);

        // Update the Length UI text
        LengthUI.text = "Grid Length: " + width;
    }

    /// <summary>
    /// Doubles the width and height of the simulation. It actually has 4 times more cells. 
    /// </summary>
    public void DoubleDetail()
    {
        // prevent texture from becoming too big
        if (width > 4096)
            return;

        // double relevant values
        injectionRadius *= 2;
        width *= 2;
        height *= 2;

        // remake the required textures
        ResetWater();
    }

    /// <summary>
    /// Halves the width and height of the simulation. It actually has 4 times fewer cells. 
    /// </summary>
    public void HalfDetail()
    {
        // prevent texture from becoming too small
        if (width <= 8)
            return;

        // half relevant values
        injectionRadius /= 2;
        width /= 2;
        height /= 2;

        // remake the water textures
        ResetWater();
    }

    // Update is called once per frame
    void Update()
    {
        // Drawing obsticles and water with mouse
        // Resize drawing with scroll
        MouseDraw();

        // Runs the simulation twice if we just injected 
        for (int i = 0; i < 15 + (Input.GetMouseButton(0) || Input.GetMouseButton(1) ? 1 : 0); i++)
        {
            RunSimulation();
        }
    }

    /// <summary>
    /// Injects fluid with left click, injects obsticles with right click, scroll changes the drawing radius of injection
    /// </summary>
    void MouseDraw()
    {
        // check if a drawing has occured
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            // get the world position of the mouse
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // convert to UV space in the texture
            Vector2 UVPos = new Vector2(worldPos.x, worldPos.y) + Vector2.one * 5;
            UVPos = UVPos / 10;

            // convert into pixel space
            UVPos.x *= width;
            UVPos.y *= height;

            // Encode the position, radius, and amount into a vector
            Vector4 injectionInfo = new Vector4(UVPos.x, UVPos.y, injectionRadius, injectionAmount);

            // Set the shader parameters
            Inject.SetTexture(0, "FluidMapIn", fluidTextures.GetRead());
            Inject.SetTexture(0, "FluidMapOut", fluidTextures.GetWrite());
            Inject.SetTexture(0, "Obsticles", obsticleTexture);
            Inject.SetVector("InjectionInfo", injectionInfo);

            // Set whether this is injecting water or adding obsticles
            Inject.SetInt("type", Input.GetMouseButton(1) ? 1 : 0);

            // Activate the shader
            Inject.Dispatch(0, width / 8, height / 8, 1);

            // swap the fluid map
            fluidTextures.Swap();
        }

        // Change the radius of drawing with scrolling
        injectionRadius += Input.mouseScrollDelta.y * width / 200;
        
        // Clamp the drawing radius so it is never too big or too small
        if (injectionRadius < 1)
            injectionRadius = 1;
        if (injectionRadius > width / 3)
            injectionRadius = width / 3;
    }

    /// <summary>
    /// Runs a single iteration of the cellular automa
    /// </summary>
    void RunSimulation()
    {
        // set textures
        BuildDispersion.SetTexture(0, "Fluid", fluidTextures.GetRead());
        BuildDispersion.SetTexture(0, "Obsticles", obsticleTexture);
        BuildDispersion.SetTexture(0, "Dispersion", dispTexture);

        // pressure information
        BuildDispersion.SetFloat("PressureIncrement", compresson / height);
        BuildDispersion.SetVector("gravity", gravity);

        // dispatch compute shader
        BuildDispersion.Dispatch(0, width / 8, height / 8, 1);

        // Set textures 
        ApplyDispersion.SetTexture(0, "FluidIn", fluidTextures.GetRead());
        ApplyDispersion.SetTexture(0, "Obsticles", obsticleTexture);
        ApplyDispersion.SetTexture(0, "Dispersion", dispTexture);
        ApplyDispersion.SetTexture(0, "FluidOut", fluidTextures.GetWrite());

        // apply the dispersion map
        ApplyDispersion.Dispatch(0, width / 8, height / 8, 1);

        // swap the textures
        fluidTextures.Swap();

        // set the new fluid texure in the renderer
        GetComponent<MeshRenderer>().material.SetTexture("_FluidTex", fluidTextures.GetRead());
    }
}
