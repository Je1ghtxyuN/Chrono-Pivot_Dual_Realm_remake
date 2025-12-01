using System;
using UnityEngine;

namespace Tenkoku.Effects
{

  [ExecuteInEditMode]
  //[ImageEffectAllowedInSceneView]
  [RequireComponent (typeof(Camera))]
  public class TenkokuSkyBlur : MonoBehaviour
  {

    public int downSample = 4;
    public float blurSpread = 0.6f;
    public Shader blurShader = null;
    public Material material = null;

    private int i = 0;
    private int rtW;
    private int rtH;
    private float off;


    void Start(){

        if (material == null){
          material = new Material(blurShader);
          material.hideFlags = HideFlags.DontSave;
        }

        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects){
          enabled = false;
          return;
        }
        // Disable if the shader can't run on the users graphics card
        if (!blurShader || !material.shader.isSupported){
          enabled = false;
          return;
        }
    }


    // Performs one blur iteration.
    void FourTapCone (RenderTexture source, RenderTexture dest, int iteration)
    {
        off = 0.5f + iteration*blurSpread;
        Graphics.BlitMultiTap (source, dest, material,
                               new Vector2(-off, -off),
                               new Vector2(-off,  off),
                               new Vector2( off,  off),
                               new Vector2( off, -off)
            );
    }

    // Downsamples the texture to a quarter resolution.
    void DownSample4x (RenderTexture source, RenderTexture dest)
    {
        Graphics.BlitMultiTap (source, dest, material,
                               new Vector2(-1.0f, -1.0f),
                               new Vector2(-1.0f,  1.0f),
                               new Vector2( 1.0f,  1.0f),
                               new Vector2( 1.0f, -1.0f)
            );
    }

    // Called by the camera to apply the image effect
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        rtW = source.width/downSample;
        rtH = source.height/downSample;
        RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

        // Copy source to the 4x4 smaller texture.
        DownSample4x (source, buffer);

        // Blur the small texture (x3 iterations)
        for(i = 0; i < 3; i++)
        {
            RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            FourTapCone (buffer, buffer2, i);
            RenderTexture.ReleaseTemporary(buffer);
            buffer = buffer2;
        }
        
        Graphics.Blit(buffer, destination);
        RenderTexture.ReleaseTemporary(buffer);
    }



  }
}




