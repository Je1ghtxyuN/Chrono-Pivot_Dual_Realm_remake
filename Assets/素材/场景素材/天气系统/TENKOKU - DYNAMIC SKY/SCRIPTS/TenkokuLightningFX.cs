using System;
using UnityEngine;
using System.Collections;

namespace Tenkoku.Core
{
    public class TenkokuLightningFX : MonoBehaviour
	{

	//Public Variables
	public bool startLightning = false;
	public float lightningFrequency = 10.0f;
	public float lightningDistance = 1.0f;
	public float lightningSpeed = 2.0f;
	public float lightningSysTime = 1.0f;
	public float lightningIntensity = 5.0f;
	public float lightningDirection = 180.0f;
	public float lightningRandomRange = 0.0f;
	public bool lightningUseLight = false;

	public Vector3 boltPosStart;
	public float boltLength = 2100.0f;
	public float boltWidth = 100.0f;
	public int boltPoints = 50;

	public AudioClip[] audioThunderDist;
	public AudioClip[] audioThunderMed;
	public AudioClip[] audioThunderNear;
	public GameObject[] thunderObjects;


	//Private Variables
	private Vector3 lightningUseVec;
	private Light lightningLight;
	private Transform lightningTrans;
	private float lightningUseInt = 0.0f;
	private float lightningFreqTime = 0.0f;
	private float useLightningFrequency = 1.0f;
	private float lightningTime = 0.0f;
	private float useLightningSysTime = 0.0f;
	private float boltTime = 0.0f;
	private Tenkoku.Core.Random lightningRand;
	private int srSeed;
	private float useLightningRandPos = 0.0f;
	private Light lightObjectWorld;
	private Light lightObjectNight;
	private LineRenderer lightningLine;
	private Transform lightningLineTrans;
	private AudioSource thunderObject;
	private AudioSource[] thunderAudio;
	private int currAudio = -1;
	private bool startThunder = false;
	private Tenkoku.Core.TenkokuModule tenkokuModule;
	private Tenkoku.Core.TenkokuLib tenkokuLib;
	private float[] thunderVolume;
	private float savefrequency = -1.0f;


	//collect for GC
	private float volEnable;
	private int tX;
	private float freq;
	private int vX;
	private Vector3[] lPositions;
	private Vector3 usePos;
	private float xR;
	private Vector3 sPos;
	private int px;
	private AudioClip useAudio;
	private float thunderDelay = 0.0f;
	private float thunderVol = 1.0f;
	private float thunderPitch = 1.0f;

	private Vector3 thunderPosition = Vector3.zero;
	private Vector3 lightningLightPosition = Vector3.zero;
	
	private float _time;
	private float _deltaTime;



	void Start () {

		//log objects
		//tenkokuModule = GameObject.Find("Tenkoku DynamicSky").GetComponent<Tenkoku.Core.TenkokuModule>() as Tenkoku.Core.TenkokuModule;
		tenkokuModule = (Tenkoku.Core.TenkokuModule) FindObjectOfType(typeof(Tenkoku.Core.TenkokuModule));
		tenkokuLib = (Tenkoku.Core.TenkokuLib) FindObjectOfType(typeof(Tenkoku.Core.TenkokuLib));

		lightningLight = tenkokuLib.lightningLight;
		lightningTrans = tenkokuLib.lightningTrans;
		lightObjectWorld = tenkokuLib.lightObjectWorld;
		lightObjectNight = tenkokuLib.lightObjectNight;
		lightningLine = tenkokuLib.lightningLine;
		lightningLineTrans = tenkokuLib.lightningLineTrans;

		//set random factor
		//lightningRand = new Tenkoku.Core.Random(System.Environment.TickCount);
		srSeed = tenkokuModule.randSeed;
		lightningRand = new Tenkoku.Core.Random(srSeed);


		//initialize thunder audio objects
		thunderVolume = new float[5];//Array(5);
		thunderAudio = new AudioSource[5];//Array(5);
		for (tX = 0; tX < thunderObjects.Length; tX++){
			thunderObjects[tX] = new GameObject();
			thunderObjects[tX].AddComponent<AudioSource>();
			thunderObjects[tX].transform.parent = this.transform;
			thunderObjects[tX].transform.name = "thunderAudio"+tX.ToString();
			thunderAudio[tX] = thunderObjects[tX].GetComponent<AudioSource>() as AudioSource;
			thunderVolume[tX] = 1.0f;
		}

		if (lightningLight != null){
			lightningLight.renderMode = LightRenderMode.ForceVertex;
			lightningLight.shadows = LightShadows.None;
		}

	}



	void LateUpdate () {

		//cache time variables for performance;
		_time = Time.time;
		_deltaTime = Time.deltaTime;

		//use Tenkoku Random
		if (tenkokuModule != null){
			if (srSeed != tenkokuModule.randSeed){
				srSeed = tenkokuModule.randSeed;
				lightningRand = new Tenkoku.Core.Random(srSeed);
			}
		}

		if (lightningRand == null){
			lightningRand = new Tenkoku.Core.Random(srSeed);
		}

		//handle system values
		freq = tenkokuModule.weather_lightning;
		lightningDirection = tenkokuModule.weather_lightningDir;
		lightningRandomRange = tenkokuModule.weather_lightningRange;
		lightningFrequency = Mathf.Lerp(25.0f,1.0f,freq);
		lightningIntensity = Mathf.Clamp(Mathf.Lerp(1.0f,10.0f,freq),0.0f,5.0f);

		//reset based on frequency changes
		if (freq != savefrequency && freq > 0.0f){
			savefrequency = freq;
			useLightningFrequency = lightningFrequency;
		}

		//clamp
		lightningIntensity = Mathf.Clamp(lightningIntensity,0.0f,10.0f);




		//enable lightening
		volEnable = 0.0f;
		if (tenkokuModule.enableSoundFX) volEnable = 1.0f;
		if (freq <= 0.0f){
			startLightning = false;
		}

		//set overall Thunder Volume
		for (vX = 0; vX < thunderAudio.Length; vX++){
			thunderAudio[vX].volume = thunderVolume[vX] * tenkokuModule.volumeThunder * volEnable;
		}

		//Lightning Timing
		if (!startLightning){
			lightningUseInt = 0.0f;
			lightningTime = 0.0f;
			useLightningSysTime = 0.0f;

			lightningFreqTime = lightningFreqTime + (_deltaTime/useLightningFrequency);
			if (lightningFreqTime >= 1.0f) startLightning = true;

		} else {

			//init settings
			if (useLightningSysTime == 0.0f){
				lightningDistance = lightningRand.Next(-0.3f,0.6f);
				lightningDistance = Mathf.Clamp(lightningDistance,0.0f,1.0f);
				lightningSpeed = lightningRand.Next(5.0f,12.0f);
				lightningSysTime = lightningRand.Next(0.0f,1.4f);
				lightningSysTime = Mathf.Clamp(lightningSysTime,0.2f,1.0f);
				lightningUseLight = false;
				if (lightningRand.Next(0.0f,1.5f + freq) >= 1.0f) lightningUseLight = true;
				useLightningRandPos = lightningRand.Next(0.0f-lightningRandomRange,lightningRandomRange);

				boltTime = 1.0f;
				lightningFreqTime = 0.0f;
				useLightningFrequency = lightningRand.Next(lightningFrequency-(lightningFrequency*0.75f),lightningFrequency + (lightningFrequency*0.25f));
			
			}

			//timers
			useLightningSysTime = useLightningSysTime + _deltaTime;
			lightningTime = lightningTime + (_deltaTime * lightningSpeed);
			if (lightningTime <= 1.0f) lightningUseInt = Mathf.SmoothStep(lightningUseInt,lightningIntensity,lightningTime);
			if (lightningTime > 1.0f) lightningUseInt = Mathf.SmoothStep(lightningUseInt,0.2f,lightningTime-1.0f);
			

			//reset
			if (useLightningSysTime >= lightningSysTime && lightningTime > 2.0f){
				startLightning = false;
				startThunder = false;
			}


			if (lightningTime > 2.0f){
				lightningTime = 0.0f;
				lightningSpeed = lightningRand.Next(5.0f,12.0f);
				boltTime = boltTime + _deltaTime*lightningRand.Next(2.0f,10.0f);
				boltPoints = Mathf.FloorToInt(lightningRand.Next(40.0f,80.0f));
			}

			//handle thunder
			if (!startThunder){
				startThunder = true;
				ThunderHandler();
			}
		}





		//position
		lightningUseVec = Quaternion.Euler(Mathf.Lerp(-10.0f,-40.0f,lightningDistance), lightningDirection + useLightningRandPos, 0.0f) * Vector3.forward;


		//set light
		if (lightningLight != null){
			if (lightningUseLight){
				if (!tenkokuModule.allowMultiLights){
					lightObjectWorld.enabled = false;
					lightObjectNight.enabled = false;
				}
				lightningUseInt = lightningUseInt * Mathf.Lerp(0.0f, 2.0f, tenkokuModule.lightningColor.a);
				lightningLight.enabled = true;
				lightningLightPosition.x = Mathf.Clamp(0f-Mathf.Lerp(0f,-60f,lightningDistance),20f,90f);
				lightningLightPosition.y = (lightningDirection+useLightningRandPos)-180f;
				lightningLightPosition.z = 0.0f;
				lightningTrans.eulerAngles = lightningLightPosition;
				lightningLight.intensity = lightningUseInt * lightningRand.Next(0.4f,1.0f);
				lightningLight.color = tenkokuModule.lightningColor;

			} else {
				lightningLight.enabled = false;
				lightningLight.intensity = 0f;
			}

			if (lightningLight.intensity <= 0.05f && !tenkokuModule.allowMultiLights){
				lightObjectWorld.enabled = true;
				lightObjectNight.enabled = true;
				lightningLight.enabled = false;
			}
		}

		//send to shader
		Shader.SetGlobalVector("Tenkoku_Vec_LightningFwd", lightningUseVec); 
		Shader.SetGlobalFloat("Tenkoku_LightningIntensity", 0.2f * (lightningUseInt + ((1f*(1f-lightningDistance))*lightningUseInt))); 
		Shader.SetGlobalFloat("Tenkoku_LightningLightIntensity", 0.2f*lightningLight.intensity); 
		Shader.SetGlobalColor("Tenkoku_LightningColor", tenkokuModule.lightningColor);


		//-------------------------
		//###  Bolt Rendering  ###
		//-------------------------
		if (Camera.main != null){
			lightningLineTrans.position = Camera.main.transform.position;
			//lightningLine.transform.Translate(lightningUseVec * 3000.0f, Space.Self);
			lightningLineTrans.Translate(lightningUseVec * lightningRand.Next(2000f,3000f), Space.Self);
			boltPosStart = lightningLineTrans.position;
			boltPosStart.y = Mathf.Clamp(boltPosStart.y,500.0f,2000f);

			if (lightningRand != null){
			if (boltTime > lightningRand.Next(0.1f,1f) || lightningSysTime == 0f){

				boltTime = 0f;

				if (lightningLine != null){

					//init positional array
					boltPoints = Mathf.FloorToInt(Mathf.Clamp(boltPoints,40,80));
					lPositions = new Vector3[boltPoints];
					usePos = boltPosStart;

					//set start position
					lPositions[0] = boltPosStart;

					//set positions
					xR = 0.0f;
					sPos = usePos;
					#if UNITY_2017_1_OR_NEWER
						lightningLine.positionCount = lPositions.Length;
					#else
						#if UNITY_5_5_OR_NEWER
							lightningLine.numPositions = lPositions.Length;
						#else
							lightningLine.SetVertexCount(lPositions.Length);
						#endif
					#endif
					for (px = 0; px < lPositions.Length; px++){

						//set base vertical position
						sPos.y = usePos.y;
						sPos.y = sPos.y - ((boltLength/(boltPoints-1))*px);
						sPos.x = sPos.x - lightningRand.Next(((-100f/(boltPoints-1f))*px),((100f/(boltPoints-1f))*px));

						if (px > 0 && px < boltPoints-1){

							//set width variance
							xR = ((px*1f)/(boltLength*1f))*_time;
							sPos.x = sPos.x - Mathf.PerlinNoise(xR,xR)*lightningRand.Next(-1f,1f) * boltWidth;
							sPos.z = sPos.z - Mathf.PerlinNoise(xR,xR)*lightningRand.Next(-1f,1f) * boltWidth;
							
							//reset variance toward end
							sPos.x = Mathf.Lerp(sPos.x,usePos.x, ((px*1f)/(boltPoints*1f)) * 0.1f );
							sPos.z = Mathf.Lerp(sPos.z,usePos.z, ((px*1f)/(boltPoints*1f)) * 0.5f );

							//set height variance
							sPos.y = sPos.y - Mathf.PerlinNoise(xR,xR)*lightningRand.Next(-1f,1f)*0.5f;
						}

						lPositions[px] = sPos;
						lightningLine.SetPosition(px,lPositions[px]);
					}
				}
			}
			}

	}
	}





	void ThunderHandler(){


		//---------------------------------
		//###  Thunder Audio Handling  ###
		//---------------------------------

		//select audio object from pool
		currAudio += 1;
		if (currAudio >= thunderObjects.Length) currAudio = 0;
		if (thunderObjects[currAudio] != null) thunderObject = thunderObjects[currAudio].GetComponent<AudioSource>() as AudioSource;


		//Handle Audio effects
		if (thunderObject != null){

			thunderDelay = 0f;
			thunderVol = 1f;
			thunderPitch = 1f;

			//set audio position
			if (Camera.main != null){
				thunderObject.transform.position = Camera.main.transform.position;
				thunderObject.transform.Translate(lightningUseVec * 50.0f, Space.Self);
				thunderPosition.x = thunderObject.transform.position.x;
				thunderPosition.y = Camera.main.transform.position.y;
				thunderPosition.z = thunderObject.transform.position.z;
				thunderObject.transform.position = thunderPosition;
			}


			//distant
			if (!lightningUseLight){
				if (lightningDistance < 0.4f){
					useAudio = audioThunderDist[Mathf.FloorToInt(lightningRand.Next(0f,audioThunderDist.Length*0.99f))];
					thunderDelay = lightningRand.Next(0.7f,2.0f);
					thunderVol = lightningRand.Next(0.2f,0.4f);
					thunderPitch = lightningRand.Next(0.6f,0.8f);
				}
				if (lightningDistance >= 0.4f){
					useAudio = audioThunderMed[Mathf.FloorToInt(lightningRand.Next(0f,audioThunderMed.Length*0.99f))];
					thunderDelay = lightningRand.Next(0.7f,1.5f);
					thunderVol = lightningRand.Next(0.4f,0.6f);
					thunderPitch = lightningRand.Next(0.8f,0.9f);
				}
			}


			if (lightningUseLight){
				if (lightningDistance < 0.4f){
					useAudio = audioThunderMed[Mathf.FloorToInt(lightningRand.Next(0f,audioThunderMed.Length*0.99f))];
					thunderDelay = lightningRand.Next(0.1f,0.4f);
					thunderVol = lightningRand.Next(0.5f,0.8f);
					thunderPitch = lightningRand.Next(0.8f,1.0f);
				}
				if (lightningDistance >= 0.4f){
					useAudio = audioThunderNear[Mathf.FloorToInt(lightningRand.Next(0.0f,audioThunderNear.Length*0.99f))];
					thunderDelay = 0.0f;//lightningRand.Next(0.0,0.0f);
					thunderVol = lightningRand.Next(0.8f,1.0f);
					thunderPitch = lightningRand.Next(0.9f,1.2f);
				}
			}

			//Play Sound
			if (useAudio != null){
				//set overall settings
				thunderObject.spatialBlend = 0.75f;
				thunderObject.minDistance = 50.0f;
				thunderObject.maxDistance = 500.0f;
				thunderObject.spread = 0f;

				//set clip specific settings
				thunderObject.clip = useAudio;
				thunderObject.loop = false;
				thunderVolume[currAudio] = thunderVol*1.4f;
				thunderObject.volume = thunderVol*1.4f;
				thunderObject.pitch = thunderPitch;
				thunderObject.PlayDelayed(thunderDelay);
			}

		}

	}



}
}