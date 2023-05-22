using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSwapper
{
    /// <summary>
    /// Boolean used to trach which texture is read and write
    /// </summary>
    private bool swap;

    /// <summary>
    /// The first texture in the texture pair 
    /// </summary>
    private RenderTexture Texture1;

    /// <summary>
    /// The second texture in the texture pair 
    /// </summary>
    private RenderTexture Texture2;

    /// <summary>
    /// Create a texture swapper to manage a corrisponding pair of textures. 
    /// When one is used for reading and the other is used for writing they can be swapped easily. 
    /// </summary>
    /// <param name="text1">The first texture to be managed</param>
    /// <param name="text2">The second texture to be managed</param>
    public TextureSwapper(RenderTexture text1, RenderTexture text2)
    {
        Texture1 = text1;
        Texture2 = text2;
        swap = true;
    }

    /// <summary>
    /// Get the texture being used for reading. 
    /// </summary>
    /// <returns>The texture in the pair that can be read. </returns>
    public RenderTexture GetRead()
    {
        return swap ? Texture1 : Texture2;
    }

    /// <summary>
    /// Get the texture that can be written to. 
    /// </summary>
    /// <returns>Texture in the pair that can be written to. </returns>
    public RenderTexture GetWrite()
    {
        return swap ? Texture2 : Texture1;
    }

    /// <summary>
    /// Swaps the read and write textures
    /// </summary>
    public void Swap()
    {
        swap = !swap;
    }
}
