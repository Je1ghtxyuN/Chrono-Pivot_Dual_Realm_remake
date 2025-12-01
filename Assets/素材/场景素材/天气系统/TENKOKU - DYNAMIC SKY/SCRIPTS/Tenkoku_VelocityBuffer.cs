// Copyright (c) <2015> <Playdead>
// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE.TXT)
// AUTHOR: Lasse Jon Fuglsang Pedersen <lasse@playdead.com>

// TENKOKU NOTE: This has been modified from PlayDead's original public implementation.
// For more info please see original implementation here: https://github.com/playdeadgames/temporal

using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Tenkoku_VelocityBuffer : Tenkoku_TemporalEffectBase
{
#if UNITY_PS4
    private const RenderTextureFormat velocityFormat = RenderTextureFormat.RGHalf;
#else
    private const RenderTextureFormat velocityFormat = RenderTextureFormat.RGFloat;
#endif

    public Shader velocityShader;
    private Material velocityMaterial;
    private Matrix4x4? velocityViewMatrix;

    [HideInInspector, NonSerialized] public RenderTexture velocityBuffer;
    [HideInInspector, NonSerialized] public RenderTexture velocityNeighborMax;

    private float timeScaleNextFrame;
    public float timeScale { get; private set; }

    public enum NeighborMaxSupport
    {
        TileSize10,
        TileSize20,
        TileSize40,
    };

    public bool neighborMaxGen = false;
    public NeighborMaxSupport neighborMaxSupport = NeighborMaxSupport.TileSize20;

#if UNITY_EDITOR
    [Header("Stats")]
    public int numResident = 0;
    public int numRendered = 0;
    public int numDrawCalls = 0;
#endif

    private Camera _camera;





    void Awake()
    {
        _camera = GetComponent<Camera>();
        velocityShader = Shader.Find("Hidden/Tenkoku_VelocityBuffer");
    }

    void Start()
    {
        timeScaleNextFrame = Time.timeScale;
    }

    void OnPostRender()
    {
        EnsureMaterial(ref velocityMaterial, velocityShader);

        if (_camera.orthographic || _camera.depthTextureMode == DepthTextureMode.None || velocityMaterial == null)
        {
            if (_camera.depthTextureMode == DepthTextureMode.None)
                _camera.depthTextureMode = DepthTextureMode.Depth;
            return;
        }

        timeScale = timeScaleNextFrame;
        timeScaleNextFrame = (Time.timeScale == 0f) ? timeScaleNextFrame : Time.timeScale;

        int bufferW = _camera.pixelWidth;
        int bufferH = _camera.pixelHeight;

        EnsureRenderTarget(ref velocityBuffer, bufferW, bufferH, velocityFormat, FilterMode.Point, 16);

        EnsureKeyword(velocityMaterial, "TILESIZE_10", neighborMaxSupport == NeighborMaxSupport.TileSize10);
        EnsureKeyword(velocityMaterial, "TILESIZE_20", neighborMaxSupport == NeighborMaxSupport.TileSize20);
        EnsureKeyword(velocityMaterial, "TILESIZE_40", neighborMaxSupport == NeighborMaxSupport.TileSize40);

        Matrix4x4 cameraP = _camera.projectionMatrix;
        Matrix4x4 cameraV = _camera.worldToCameraMatrix;
        Matrix4x4 cameraVP = cameraP * cameraV;

        if (velocityViewMatrix == null)
            velocityViewMatrix = cameraV;

        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = velocityBuffer;
        {
            GL.Clear(true, true, Color.black);

            const int kPrepass = 0;
            const int kTileMax = 3;
            const int kNeighborMax = 4;

            //Tenkoku - Calculate corners
            Vector4 corners = Vector4.zero;
            if (_camera != null){
                float oneExtentY = Mathf.Tan(0.5f * Mathf.Deg2Rad * _camera.fieldOfView);
                float oneExtentX = oneExtentY * _camera.aspect;
                corners = new Vector4(oneExtentX, oneExtentY, 0f, 0f);
            }
            velocityMaterial.SetVector("_Corner", corners);


            velocityMaterial.SetMatrix("_CurrV", cameraV);
            velocityMaterial.SetMatrix("_CurrVP", cameraVP);
            velocityMaterial.SetMatrix("_PrevVP", cameraP * velocityViewMatrix.Value);
            velocityMaterial.SetPass(kPrepass);
            FullScreenQuad();


            // 3 + 4: tilemax + neighbormax
            if (neighborMaxGen)
            {
                int tileSize = 1;

                switch (neighborMaxSupport)
                {
                    case NeighborMaxSupport.TileSize10: tileSize = 10; break;
                    case NeighborMaxSupport.TileSize20: tileSize = 20; break;
                    case NeighborMaxSupport.TileSize40: tileSize = 40; break;
                }

                int neighborMaxW = bufferW / tileSize;
                int neighborMaxH = bufferH / tileSize;

                EnsureRenderTarget(ref velocityNeighborMax, neighborMaxW, neighborMaxH, velocityFormat, FilterMode.Bilinear, 0);

                // tilemax
                RenderTexture tileMax = RenderTexture.GetTemporary(neighborMaxW, neighborMaxH, 0, velocityFormat);
                RenderTexture.active = tileMax;
                {
                    velocityMaterial.SetTexture("_VelocityTex", velocityBuffer);
                    velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / bufferW, 1f / bufferH, 0f, 0f));
                    velocityMaterial.SetPass(kTileMax);
                    FullScreenQuad();
                }

                // neighbormax
                RenderTexture.active = velocityNeighborMax;
                {
                    velocityMaterial.SetTexture("_VelocityTex", tileMax);
                    velocityMaterial.SetVector("_VelocityTex_TexelSize", new Vector4(1f / neighborMaxW, 1f / neighborMaxH, 0f, 0f));
                    velocityMaterial.SetPass(kNeighborMax);
                    FullScreenQuad();
                }

                RenderTexture.ReleaseTemporary(tileMax);
            }
            else if (velocityNeighborMax != null)
            {
                RenderTexture.ReleaseTemporary(velocityNeighborMax);
                velocityNeighborMax = null;
            }
        }
        RenderTexture.active = activeRT;

        velocityViewMatrix = cameraV;
    }








}
