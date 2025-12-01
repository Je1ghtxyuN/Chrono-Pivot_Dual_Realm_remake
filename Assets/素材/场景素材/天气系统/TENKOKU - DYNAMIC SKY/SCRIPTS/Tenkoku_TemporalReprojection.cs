// Copyright (c) <2015> <Playdead>
// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE.TXT)
// AUTHOR: Lasse Jon Fuglsang Pedersen <lasse@playdead.com>

// TENKOKU NOTE: This has been modified from PlayDead's original public implementation.
// For more info please see original implementation here: https://github.com/playdeadgames/temporal

using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(Tenkoku_VelocityBuffer))]
[AddComponentMenu ("Image Effects/Tenkoku/Tenkoku Temporal Aliasing")]

public class Tenkoku_TemporalReprojection : Tenkoku_TemporalEffectBase
{
    private static RenderBuffer[] mrt = new RenderBuffer[2];

    public Shader reprojectionShader;
    private Material reprojectionMaterial;
    private Matrix4x4[] reprojectionMatrix;
    private RenderTexture[] reprojectionBuffer;
    private int reprojectionIndex = 0;

    [Range(0f, 1f)] public float feedbackMin = 0.88f;
    [Range(0f, 1f)] public float feedbackMax = 0.9f; //old default 0.97

    private Camera _camera;
    private Tenkoku_VelocityBuffer _velocity;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        _velocity = GetComponent<Tenkoku_VelocityBuffer>();
        reprojectionShader = Shader.Find("Hidden/Tenkoku_TemporalReprojection");
    }

    void Resolve(RenderTexture source, RenderTexture destination)
    {
        EnsureMaterial(ref reprojectionMaterial, reprojectionShader);

        if (_camera.orthographic || _camera.depthTextureMode == DepthTextureMode.None || reprojectionMaterial == null)
        {
            Graphics.Blit(source, destination);
            if (_camera.depthTextureMode == DepthTextureMode.None)
                _camera.depthTextureMode = DepthTextureMode.Depth;
            return;
        }

        if (reprojectionMatrix == null || reprojectionMatrix.Length != 2)
            reprojectionMatrix = new Matrix4x4[2];
        if (reprojectionBuffer == null || reprojectionBuffer.Length != 2)
            reprojectionBuffer = new RenderTexture[2];

        int bufferW = source.width;
        int bufferH = source.height;

        EnsureRenderTarget(ref reprojectionBuffer[0], bufferW, bufferH, RenderTextureFormat.ARGB32, FilterMode.Bilinear);
        EnsureRenderTarget(ref reprojectionBuffer[1], bufferW, bufferH, RenderTextureFormat.ARGB32, FilterMode.Bilinear);


        //Tenkoku - Calculate Projection
        Matrix4x4 cameraP;

        float oExtentY = Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
        float oExtentX = oExtentY * _camera.aspect;

        float cf = _camera.farClipPlane;
        float cn = _camera.nearClipPlane;
        float xm = (0f - oExtentX) * cn;
        float xp = (0f + oExtentX) * cn;
        float ym = (0f - oExtentY) * cn;
        float yp = (0f + oExtentY) * cn;

        //Tenkoku Calculate Matrix
        float x = (2.0f * cn) / (xp - xm);
        float y = (2.0f * cn) / (yp - ym);
        float a = (xp + xm) / (xp - xm);
        float b = (yp + ym) / (yp - ym);
        float c = -(cf + cn) / (cf - cn);
        float d = -(2.0f * cf * cn) / (cf - cn);
        float e = -1.0f;

        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;

        cameraP = m;


        //Set Camera Data
        Matrix4x4 cameraVP = cameraP * _camera.worldToCameraMatrix;

        float oneExtentY = Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
        float oneExtentX = oneExtentY * _camera.aspect;

        if (reprojectionIndex == -1)// bootstrap
        {
            reprojectionIndex = 0;
            reprojectionMatrix[reprojectionIndex] = cameraVP;
            Graphics.Blit(source, reprojectionBuffer[reprojectionIndex]);
        }

        int indexRead = reprojectionIndex;
        int indexWrite = (reprojectionIndex + 1) % 2;

        reprojectionMaterial.SetTexture("_VelocityBuffer", _velocity.velocityBuffer);
        reprojectionMaterial.SetTexture("_VelocityNeighborMax", _velocity.velocityNeighborMax);
        reprojectionMaterial.SetVector("_Corner", new Vector4(oneExtentX, oneExtentY, 0f, 0f));
        reprojectionMaterial.SetMatrix("_PrevVP", reprojectionMatrix[indexRead]);
        reprojectionMaterial.SetTexture("_MainTex", source);
        reprojectionMaterial.SetTexture("_PrevTex", reprojectionBuffer[indexRead]);
        reprojectionMaterial.SetFloat("_FeedbackMin", feedbackMin);
        reprojectionMaterial.SetFloat("_FeedbackMax", feedbackMax);

        // reproject frame n-1 into output + history buffer
        {
            mrt[0] = reprojectionBuffer[indexWrite].colorBuffer;
            mrt[1] = destination.colorBuffer;

            Graphics.SetRenderTarget(mrt, source.depthBuffer);
            reprojectionMaterial.SetPass(0);

            FullScreenQuad();

            reprojectionMatrix[indexWrite] = cameraVP;
            reprojectionIndex = indexWrite;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (destination != null)// resolve without additional blit when not end of chain
        {
            Resolve(source, destination);
        }
        else
        {
            RenderTexture internalDestination = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            {
                Resolve(source, internalDestination);
                Graphics.Blit(internalDestination, destination);
            }
            RenderTexture.ReleaseTemporary(internalDestination);
        }
    }
}
