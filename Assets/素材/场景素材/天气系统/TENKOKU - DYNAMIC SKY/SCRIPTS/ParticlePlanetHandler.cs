using System;
using UnityEngine;

namespace Tenkoku.Core
{
    public class ParticlePlanetHandler : MonoBehaviour
	{


	//PUBLIC VARIABLES
	public bool planetReset = false;
	public String planetName = "mars";
	public float planetVis  = 1.0f;
	public Color planetColor = Color.white;
	public float planetSize  = 0.02f;
	public Vector3 planetOffset = new Vector3(0f,0f,0f);

	//PRIVATE VARIABLES
	private int numParticles = 1;
	private bool hasStarted = false;
	private ParticleSystem PlanetSystem;
	private Renderer planetRenderer;
	private ParticleSystem.Particle[] PlanetParticles;
	private Vector4[] planetPOS;
	private String[] planetDataArray;
	private String planetDataString;
	private float currPlanetVis = -1.0f;

	private int px = 0;
	private Color useColor = new Color(0f,0f,0f,0f);
	private Color visColor = new Color(0.5f,0.5f,0.5f,1.0f);

	private Vector3 particlePosition = Vector3.zero;


	void Start () {

		hasStarted = false;
		PlanetSystem = this.GetComponent<ParticleSystem>();
		PlanetSystem.Emit(numParticles);
		planetRenderer = this.GetComponent<Renderer>();
		PlanetParticles = new ParticleSystem.Particle[8];
	}



	void LateUpdate(){

		if (currPlanetVis != planetVis){
			currPlanetVis = planetVis;

			if (planetRenderer != null){
				
				visColor.a = planetVis;
				
				if (Application.isPlaying){
					visColor = planetRenderer.material.GetColor("_TintColor");
					planetRenderer.material.SetColor("_TintColor", visColor);
				} else {
					visColor = planetRenderer.sharedMaterial.GetColor("_TintColor");
					planetRenderer.sharedMaterial.SetColor("_TintColor", visColor);
				}
			}
		}
		
		if (!hasStarted){
			hasStarted = true;
			planetReset = true;
		}

		if (planetReset){
			PlSystemUpdate();
		}
	}




	void PlSystemUpdate () {

		//reset planet system
		if (planetReset){

		planetReset = false;
		PlanetSystem.GetParticles(PlanetParticles);
		
		for (px = 0; px < 8; px++){

				//default position
				//if (px < PlanetSystem.particleCount && PlanetParticles[px] != null){
				if (px < PlanetSystem.particleCount){

					particlePosition.x = -25.0f + planetOffset.x;
					particlePosition.y = planetOffset.y;
					particlePosition.z = planetOffset.z;
					PlanetParticles[px].position = particlePosition;
					
					// set planet size
					#if UNITY_5_3_OR_NEWER
						PlanetParticles[px].startSize = planetSize;
					#else
						PlanetParticles[px].size = planetSize;
					#endif

					// set planet color
					useColor = planetColor;

					#if UNITY_5_3_OR_NEWER
						PlanetParticles[px].startColor = useColor;
					#else
						PlanetParticles[px].color = useColor;
					#endif

				} else {
					break;
				}

				px++;
			}

			PlanetSystem.SetParticles(PlanetParticles,PlanetParticles.Length);
			PlanetSystem.Emit(PlanetParticles.Length);
			PlanetSystem.Play();

		}
	}



}
}


