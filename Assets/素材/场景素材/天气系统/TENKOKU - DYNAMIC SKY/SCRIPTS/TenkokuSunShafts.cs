using System;
using UnityEngine;

namespace Tenkoku.Effects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("Image Effects/Tenkoku/Tenkoku Sun Shafts")]
    public class TenkokuSunShafts : MonoBehaviour
    {

        //PUBLIC VARIABLES
        public Transform sunTransform;
        public Color blockColor = new Color(0.5f,0.5f,0.5f,1.0f);
        public Color tintColor =  new Color(1f,0.5f,0f,1f);
        [HideInInspector] public Color sunColor = Color.white;
        public int sunShaftDownsample = 1;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.7f;
        public bool  useDepthTexture = true;

        //PRIVATE VARIABLES
        private Material sunShaftsMaterial;
        private Camera camComponent;

        private Vector3 tempVec = new Vector3(1.0f,0.5f,0.0f);
        private Vector4 sunColorVec = Vector4.zero;
        private Vector4 radiusVec = Vector4.zero;
        private Vector4 shaftTintVec = Vector4.zero;
        private Vector4 sunPosVec = Vector4.zero;
        private float ofs;
        private int rtW;
        private int rtH;
        private int it2;



        void Start(){
            sunShaftsMaterial = new Material(Shader.Find("Hidden/TenkokuSunShafts"));
            camComponent = GetComponent<Camera>();
        }




        void OnRenderImage (RenderTexture source, RenderTexture destination) {

            //if (source != null && destination != null){

                camComponent.depthTextureMode |= DepthTextureMode.Depth;

                Vector3 v = Vector3.one * 0.5f;
                if (sunTransform)
                    v = camComponent.WorldToViewportPoint(transform.position + (-sunTransform.forward * 100.0f));
                else
                    v = tempVec;

                //Downsample texture
                rtW = source.width / sunShaftDownsample;
                rtH = source.height / sunShaftDownsample;

                RenderTexture lrColorB;
                RenderTexture lrDepthBuffer = RenderTexture.GetTemporary (rtW, rtH, 0);

                // mask out everything except the skybox
                radiusVec.x = 1f;
                radiusVec.y = 1f;
                radiusVec.z = 0f;
                radiusVec.w = 0f;
                sunShaftsMaterial.SetVector ("_BlurRadius4", radiusVec );

                sunPosVec.x = v.x;
                sunPosVec.y = v.y;
                sunPosVec.z = v.z;
                sunPosVec.w = maxRadius;
                sunShaftsMaterial.SetVector ("_SunPosition", sunPosVec);

                //Blit depth buffer
                Graphics.Blit (source, lrDepthBuffer, sunShaftsMaterial, 2);

                // radial blur:
                //ofs = (1.0f / 468.0f);
                ofs = 0.002136752f;

                radiusVec.x = ofs;
                radiusVec.y = ofs;
                radiusVec.z = 0f;
                radiusVec.w = 0f;
                sunShaftsMaterial.SetVector ("_BlurRadius4", radiusVec);

                sunPosVec.x = v.x;
                sunPosVec.y = v.y;
                sunPosVec.z = v.z;
                sunPosVec.w = maxRadius;
                sunShaftsMaterial.SetVector ("_SunPosition", sunPosVec);


                //set blur iterations explicitly (x4 iterations)
                for (it2 = 0; it2 < 4; it2++ ) {
                    // each iteration takes 2 * 6 samples
                    // we update _BlurRadius each time to cheaply get a very smooth look

                    lrColorB = RenderTexture.GetTemporary (rtW, rtH, 0);
                    Graphics.Blit (lrDepthBuffer, lrColorB, sunShaftsMaterial, 1);
                    RenderTexture.ReleaseTemporary (lrDepthBuffer);

                    lrDepthBuffer = RenderTexture.GetTemporary (rtW, rtH, 0);
                    Graphics.Blit (lrColorB, lrDepthBuffer, sunShaftsMaterial, 1);
                    RenderTexture.ReleaseTemporary (lrColorB);

                    ofs = (((it2 * 24.0f))) / 468.0f;

                    radiusVec.x = ofs;
                    radiusVec.y = ofs;
                    radiusVec.z = 0f;
                    radiusVec.w = 0f;
                    sunShaftsMaterial.SetVector ("_BlurRadius4", radiusVec);
                }

                // put together:
                if (v.z >= 0.0f){
                    sunColorVec.x = sunColor.r;
                    sunColorVec.y = sunColor.g;
                    sunColorVec.z = sunColor.b;
                    sunColorVec.w = sunColor.a;

                    sunShaftsMaterial.SetVector ("_SunColor", sunColorVec * sunShaftIntensity);
                } else {
                    sunShaftsMaterial.SetVector ("_SunColor", Vector4.zero); //no backprojection !
                }


                shaftTintVec.x = Mathf.Lerp(tintColor.r, sunColorVec.x, 0.75f);
                shaftTintVec.y = Mathf.Lerp(tintColor.g, sunColorVec.y, 0.75f);
                shaftTintVec.z = Mathf.Lerp(tintColor.b, sunColorVec.z, 0.75f);
                shaftTintVec.w = tintColor.a;
                sunShaftsMaterial.SetVector ("_TintColor", shaftTintVec);



                sunShaftsMaterial.SetVector ("_ColorBlock", blockColor);
                sunShaftsMaterial.SetTexture ("_ColorBuffer", lrDepthBuffer);

                //force blit into first pass (screen mode)
                Graphics.Blit (source, destination, sunShaftsMaterial, 0);

                //release memory
                RenderTexture.ReleaseTemporary (lrDepthBuffer);


            //}


        }
    }
}
