using System;
using UnityEngine;
using System.Collections;

namespace Tenkoku.Core
{

	[ExecuteInEditMode]
    public class TenkokuCameraTools : MonoBehaviour
	{



	//Public Variables
	public enum tenCamToolType{sky,skybox,particles,none};
	public tenCamToolType cameraType;
	public RenderTexture renderTexDiff;
	//public Material skyMaterial;


	//Private Variables
	private Tenkoku.Core.TenkokuModule tenkokuModuleObject;
	//private Renderer surfaceRenderer;
	private Camera cam;
	private Transform camTrans;
	private Camera copyCam;
	private Transform copyCamTrans;
	private Matrix4x4 camMatrix;
	//private float updateTimer = 0.0f;
	//private int currResolution = 256;
	//private bool doUpdate = false;




	void Start () {

		if (Application.isPlaying){
			tenkokuModuleObject = (Tenkoku.Core.TenkokuModule) FindObjectOfType(typeof(Tenkoku.Core.TenkokuModule));
		
			cam = gameObject.GetComponent<Camera>() as Camera;
			camTrans = gameObject.GetComponent<Transform>() as Transform;
			if (tenkokuModuleObject != null){
				copyCam = tenkokuModuleObject.mainCamera.GetComponent<Camera>();
				copyCamTrans = tenkokuModuleObject.mainCamera.GetComponent<Transform>();
			}
		}

		BuildTexture();
		//RenderSettings.skybox = skyMaterial;
	}




	//void Update () {
	//	if (!Application.isPlaying){
	//		RenderSettings.skybox = skyMaterial;
	//	}
	//}




	void LateUpdate () {
/*
		if (!Application.isPlaying){
			if (skyMaterial != null){
				if (cameraType == tenCamToolType.sky){
					RenderSettings.skybox = skyMaterial;
				}
			}
		}
*/

		if (Application.isPlaying){
/*
			if (skyMaterial != null){
				if (cameraType == tenCamToolType.sky){
					if (RenderSettings.skybox == null){
						RenderSettings.skybox = skyMaterial;
					}
				}
			}
*/

			//update camera tracking when necessary
			if (tenkokuModuleObject.useCameraCam != null){
				copyCam = tenkokuModuleObject.useCameraCam;

				if (tenkokuModuleObject.useCamera != null){
					copyCamTrans = tenkokuModuleObject.useCamera;
				}

				CameraUpdate();
			}
		}

	}




	void CameraUpdate () {

		if (copyCam != null && cam != null){

			//set camera settings
			cam.enabled = true;
			camTrans.position = copyCamTrans.position;
			camTrans.rotation = copyCamTrans.rotation;
			cam.projectionMatrix = copyCam.projectionMatrix;
			cam.fieldOfView = copyCam.fieldOfView;
			cam.renderingPath = copyCam.actualRenderingPath;
			cam.farClipPlane = copyCam.farClipPlane;


			if (renderTexDiff != null){

				

				//pass texture to shader
				if (cameraType == tenCamToolType.sky){

					if (tenkokuModuleObject.atmosphereModelTypeIndex == 0){
						#if UNITY_5_6_OR_NEWER
							cam.allowHDR = false;
						#else
							cam.hdr = false;
						#endif
					}

					if (tenkokuModuleObject.atmosphereModelTypeIndex == 1){
						#if UNITY_5_6_OR_NEWER
							cam.allowHDR = true;
						#else
							cam.hdr = true;
						#endif
					}
					//surfaceRenderer.material.SetTexture("_Tenkoku_SkyTex2",renderTexDiff);
				}



				//pass texture to shader
				if (cameraType == tenCamToolType.skybox){
					//force hdr when using Elek Atmosphere model
					if (tenkokuModuleObject.atmosphereModelTypeIndex == 1){
						#if UNITY_5_6_OR_NEWER
							cam.allowHDR = true;
						#else
							cam.hdr = true;
						#endif
					}
					cam.targetTexture = renderTexDiff;
					Shader.SetGlobalTexture("_Tenkoku_SkyBox",renderTexDiff);
				}


				if (cameraType == tenCamToolType.particles){
					cam.targetTexture = renderTexDiff;
					Shader.SetGlobalTexture("_Tenkoku_ParticleTex",renderTexDiff);
				}

			} else {
				BuildTexture();
			}

		}

	}

	void BuildTexture(){

		if (cameraType == tenCamToolType.sky){
			renderTexDiff = new RenderTexture(128,128,24,RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
		
		} else if (cameraType == tenCamToolType.skybox){
			renderTexDiff = new RenderTexture(128,128,24,RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
		
		}

		renderTexDiff.useMipMap = true;

		#if UNITY_2017_1_OR_NEWER
			renderTexDiff.autoGenerateMips = true;
		#else
			renderTexDiff.generateMips = true;
		#endif

		if (cam != null){
			cam.targetTexture = renderTexDiff;
		}
	}



}
}