
using System;
using UnityEngine;

namespace Tenkoku.Core
{
  public class TenkokuLib : MonoBehaviour
  {
  	
  	public Material skyMaterialElek;
  	public Material skyMaterialLegacy;
	public Shader[] shaders;

	[Space(10)]

	//Object Reference Test
	//----------------------------------------------------
	public WindZone tenkokuWindObject;
	public Transform tenkokuWindTrans;
	public ReflectionProbe tenkokuReflectionObject;

	public Transform worldlightObject;
	public Transform sunlightObject;
	public Transform sunSphereObject;
	public Transform sunObject;
	public Transform eclipseObject;
	public Transform moonSphereObject;
	public Transform moonObject;
	public Transform moonlightObject;
	public Transform skyObject;
	public Transform starfieldObject;
	public Transform starGalaxyObject;

	public Tenkoku.Core.ParticleStarfieldHandler starRenderSystem;
	public Renderer starParticleSystem;

	public Tenkoku.Core.ParticlePlanetHandler planetObjMars;
	public Tenkoku.Core.ParticlePlanetHandler planetObjMercury;
	public Tenkoku.Core.ParticlePlanetHandler planetObjVenus;
	public Tenkoku.Core.ParticlePlanetHandler planetObjJupiter;
	public Tenkoku.Core.ParticlePlanetHandler planetObjSaturn;
	public Tenkoku.Core.ParticlePlanetHandler planetObjUranus;
	public Tenkoku.Core.ParticlePlanetHandler planetObjNeptune;
			
	public Renderer planetRendererSaturn;
	public Renderer planetRendererJupiter;
	public Renderer planetRendererNeptune;
	public Renderer planetRendererUranus;
	public Renderer planetRendererMercury;
	public Renderer planetRendererVenus;
	public Renderer planetRendererMars;

	public Transform planetTransSaturn;
	public Transform planetTransJupiter;
	public Transform planetTransNeptune;
	public Transform planetTransUranus;
	public Transform planetTransMercury;
	public Transform planetTransVenus;
	public Transform planetTransMars;

	public Transform nightSkyLightObject;
	public Transform fillLightObject;

	public Tenkoku.Core.TenkokuGlobalSound ambientSoundObject;

	//public LensFlare sunFlare;

	public ParticleSystem rainSystem;
	public ParticleSystem rainFogSystem;
	public ParticleSystem rainSplashSystem;
	public ParticleSystem snowSystem;
	public ParticleSystem fogSystem;
		
	public Transform auroraObjectTransform;
	public Renderer renderObjectAurora;

	public Light lightObjectFill;
	public Light lightObjectNight;
	public Light lightObjectWorld;

	public Renderer renderObjectMoon;
	public Renderer renderObjectGalaxy;

	public Renderer renderObjectRain;
	public Renderer renderObjectRainSplash;
	public Renderer renderObjectRainFog;
	public Renderer renderObjectFog;
	public Renderer renderObjectSnow;

	public ParticleSystem particleObjectRainFog;
	public ParticleSystem particleObjectRainSplash;
	public ParticleSystem particleObjectFog;

	public Transform cloudPlaneObject;
	public Renderer renderObjectCloudPlane;

	public Transform cloudSphereObject;
	public Renderer renderObjectCloudSphere;

	public Light lightningLight;
	public Transform lightningTrans;
	public LineRenderer lightningLine;
	public Transform lightningLineTrans;

	public Camera fogCameraCam;
	public Tenkoku.Effects.TenkokuSkyBlur fogCameraBlur;

  }
}
