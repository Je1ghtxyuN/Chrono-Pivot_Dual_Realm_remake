using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#if UNITY_EDITOR
	using UnityEditor;
#endif


namespace Tenkoku.Core
{

	[ExecuteInEditMode]
	[System.Serializable]
    public class TenkokuModule : MonoBehaviour
	{

	//PUBLIC VARIABLES
	public string tenkokuVersionNumber = "";

	public bool useAutoFX = true;
	public float systemTime = 1f;
	public bool enableAutoAdvance = true;
	public Transform mainCamera;
	public Transform manualCamera;
	public int cameraTypeIndex = 0;
	public int ambientTypeIndex = 0;

	public int atmosphereModelTypeIndex = 0;
	public int useAtmosphereIndex = -1;

	public Gradient cloudGradient;
	public Gradient cloudAmbGradient;
	public Gradient sunGradient;
	public Gradient horizGradient;
	public Gradient skyGradient;
	public Gradient ambColorGradient;
	public Gradient moonGradient;
	public Gradient ambDayGradient;
	public Gradient ambLightGradient;

	public Gradient temperatureGradient;

	public int lightLayer = 2;
	public LayerMask lightLayerMask;
	[System.NonSerialized] public List<string> tenLayerMasks = new List<string>();

	public Color temperatureColor;
	public Color colorOverlay = Color.white;
	public Color colorSky = new Color(1f,1f,1f,0f);
	public Color colorAmbient = new Color(0f,0f,0f,0.6f);
	private Color useColorAmbient = new Color(0f,0f,0f,0.6f);
	public float ambientDayAmt = 1f;
	public float ambientNightAmt = 1f;

	public Color colorFog = new Color(0f,0.25f,1f,0.25f);
	public Color moonHorizColor = new Color(1f,0.27f,0f,0.5f);

	public bool useSunFlare = true;
	public Flare flareObject;
	public bool useSunRays = true;
	public float sunRayIntensity = 1.0f;
	public float sunRayLength = 0.2f;
	public float moonRayIntensity = 1.0f;
	public bool useAmbient  = true;
	public bool adjustOvercastAmbient = true;
	public float useAmbientIntensity = 1f;

	public bool disableMSA = true;
	public bool enableIBL = false;
	public bool enableFog = true;
	public bool autoFog = true;

	public bool showColors = false;
	public bool showConfig = false;
	public bool showCelSet = false;
	public bool showTimer = true;
	public bool showSounds = false;

	public bool showConfigTime = false;
	public bool showConfigAtmos = false;
	public bool showConfigWeather = false;

	public bool showIBL = false;
	public bool allowMultiLights = true;

	float cloudRotSpeed = 0.2f;
	float cloudOverRot = 0.0f;
	float cloudSc = 1.0f;
	public float skyBrightness = 1.0f;
	public float mieAmount = 1.0f;
	public float mieMnAmount = 1.0f;
	private float useMieAmount = 1.0f;
	private float useMieMnAmount = 1.0f;
	float useSkyBright = 1.0f;
	public float nightBrightness = 0.4f;
	public float fogDistance = 0.0015f;
	public float fogAtmosphere = 0.2f;
	public float fadeDistance = 1.0f;
	public float fogObscurance = 0.0f;

	public float setRotation = 180.0f;
				
	public float setLatitude = 0.0f;
	public float setLongitude = 0.0f;
	public int setTZOffset = 0;
	public bool enableDST = false;

	public float sunSize = 0.02f;
	public float sunBright = 1.0f;
	public float sunSat = 1.0f;
	
	bool moonIsVisible = true;
	public float moonSize = 0.02f;
	public float moonPos = 0.0f;
	public float moonBright = 1.0f;
	public float moonSat = 1.0f;

	bool auroraIsVisible = true;
	public float auroraSize = 1.4f;
	public float auroraLatitude = 1.0f;
	public float auroraIntensity = 1.0f;
	public float auroraSpeed = 0.5f;

	public float planetIntensity = 1.0f;

	public float ambientShift = 0.0f;
	public float moonLightIntensity = 0.25f;
	public float moonDayIntensity = 0.2f;
	public float starIntensity = 1.0f;
	public float galaxyIntensity = 1.0f;

	public float horizonHeight = 0.5f;
	public float horizonScale = 45.0f;

	public float ambHorizonHeight = 1.0f;
	public float ambHorizonScale = 30.0f;

	public float lowHorizonHeight = 0.5f;
	public float lowHorizonScale = 1.0f;

	public Texture2D colorRamp;

	AudioClip[] globalSoundFile;
	Vector4[] globalSoundVolDay;
	Vector4[] globalSoundVolYear;

	public int colorTypeIndex = 0;
	public int weatherTypeIndex = 0;
	public int sunTypeIndex = 0;
	public int moonTypeIndex = 0;
	public int starTypeIndex = 0;
	public int galaxyTypeIndex = 0;
	public int auroraTypeIndex = 0;
	public int galaxyTexIndex = 0;

	public Texture galaxyTex ;
	public Texture galaxyCubeTex;

	public Transform useCamera;
	public Camera useCameraCam;
	private Transform saveCamera;
	private float setSkyUseSize = 1.0f;

	public float moonLightAmt;
	public float moonPhase;
	private Quaternion mres;

	private Mesh mesh;
	private Vector3[] vertices;
	private Color[] colors;
	private Mesh meshAmb;
	private Vector3[] verticesAmb;
	private Color[] colorsAmb;

	private Vector3[] verticesLow;
	private Color[] colorsLow;

	private Color rayCol;

	public Color ambientCol = Color.gray;
	public Color colorSun = Color.white;
	public Color colorMoon = Color.white;
	public Color colorSkyBase = Color.white;
	public Color colorSkyBaseLow = Color.white;
	public Color colorHorizon = Color.white;
	public Color colorHorizonLow = Color.white;
	public Color colorSkyAmbient = Color.white;
	public Color colorClouds = Color.white;
	public Color colorHighlightClouds = Color.white;
	public Color colorSkyboxGround = Color.black;
	public Color colorSkyboxMie = Color.white;

	int stepTime;
	float timeLerp = 0.0f;

	float calcTimeM = 0.0f;
	float setTimeM = 0.0f;
	int stepTimeM = 0;
	float timeLerpM = 0.0f;


	//TIMER VARIABLES
	public string displayTime = "";

	public bool autoTimeSync = false;
	public bool autoDateSync = false;
	public bool autoTime = false;
	public float timeCompression = 100.0f;
	public int currentSecond = 0;
	private int lastSecond = 0;
	public int currentMinute = 45;
	public int currentHour = 5;
	public int currentDay = 22;
	public int currentMonth = 3;
	public int currentYear = 2013;

	public bool useAutoTime = false;
	public float useTimeCompression = 100.0f;
	public bool use24Clock = false;
	public AnimationCurve timeCurves;
	public int setHour = 5;
	public float useSecond = 0f;
	public float useMinute = 0f;
	public float displayHour = 0f;
	public float useHour = 0f;
	public float useDay = 0f;
	public float useMonth = 0f;
	public float useYear = 0f;

	public string hourMode = "";
		
	public int moonSecond = 0;
	public int moonMinute = 0;
	public int moonHour = 0;
	public int moonDay = 0;
	public int moonMonth = 0;
	public int moonYear = 0;

	public int starSecond = 0;
	public int starMinute = 0;
	public int starHour = 0;
	public int starDay = 0;
	public int starMonth = 0;
	public int starYear = 0;

	public bool leapYear = false;
	public int monthFac = 31;

	public Vector3 moonTime;

	public float setDay = 22000.0f;
	public float setStar = 22000.0f;
	public float setMoon = 22000.0f;
	public float setMonth = 22000.0f;
	public float setYear = 22000.0f;

	public float dayValue = 0.5f;
	public float starValue = 0.5f;
	public float moonValue = 0.0f;
	public float monthValue = 0.0f;
	public float yearValue = 0.0f;

	public float countSecond =  0.0f;
	public float countMinute =  0.0f;

	public float countSecondStar =  0.0f;
	public float countMinuteStar =  0.0f;

	public float countSecondMoon =  0.0f;
	public float countMinuteMoon =  0.0f;

	public bool cloudLinkToTime = false;
	public bool useTemporalAliasing = true;
	public bool useFSAliasing = false;
	public bool useLegacyClouds = false;
	public float cloudQuality = 0.75f;
	public float cloudFreq = 1f;
	public float cloudDetail = 1f;
	public float precipQuality = 1f;
	private float cloudStepTime = 0f;
	public float cloudBrightness = 1.0f;

	private Matrix4x4 MV;

	public float starPos = 0.0f;

	public float galaxyPos = 0.0f;



	//Weather Variables
	public int currentWeatherTypeIndex = -1;
	public bool weather_forceUpdate = false;

	public bool weather_setAuto = false;
	public float weather_qualityCloud = 0.7f;
	public float weather_cloudAltAmt = 1.0f;

	public float weather_cloudAltoStratusAmt = 0.0f;
	public float weather_cloudCirrusAmt = 0.0f;

	public float weather_cloudCumulusAmt = 0.2f;
	public float weather_cloudScale = 0.5f;
	public float weather_cloudSpeed = 0.0f;

	public float weather_OvercastDarkeningAmt = 1.0f;
	private float useOvercastDarkening = 0.5f;

	public float weather_OvercastAmt = 0.0f;
	public float weather_RainAmt = 0.0f;
	public float weather_SnowAmt = 0.0f;
	public float weather_WindAmt = 0.0f;
	public float weather_WindDir = 0.0f;
	public float weather_FogAmt = 0.0f;
	public float weather_FogHeight = 0.0f;

	public float weather_autoForecastTime = 5.0f;
	public float weather_PatternTime = 0.0f;
	public float weather_TransitionTime = 1.0f;

	public float weather_temperature = 75.0f;
	public float weather_humidity = 0.25f;
	public float weather_rainbow = 0.0f;
	public float weather_lightning = 0.0f;
	public float weather_lightningDir = 110.0f;
	public float weather_lightningRange = 180.0f;
	public Color lightningColor = Color.white;

	// auto weather variables
	private float w_isCloudy;
	private float w_isOvercast;
	private Vector4 w_cloudAmts;
	private Vector4 w_cloudTgts;
	private float w_cloudAmtCumulus;
	private float w_cloudTgtCumulus;
	private float w_overcastAmt;
	private float w_overcastTgt;
	private float w_windDir;
	private float w_windAmt;
	private float w_windDirTgt;
	private float w_windTgt;
	private float w_rainAmt;
	private float w_rainTgt;
	private float w_snowAmt;
	private float w_snowTgt;
	private float w_humidityAmt;
	private float w_humidityTgt;
	private float w_lightningAmt;
	private float w_lightningTgt;
	private float w_temperatureAmt;
	private float w_temperatureTgt;

	private float cAmt;

	private Tenkoku.Core.TenkokuCalculations calcComponent;
	private Vector2 weather_WindCoords = Vector2.zero;
	private Vector4 weather_CloudCoords = Vector4.zero;
	private Vector2 currCoords = Vector2.zero;

	private float fillFac;
	private float fillIntensity;

	private Material materialObjectCloudSphere;

	//collect for GC
	private float setTime;
	private float calcTime;
	private float currClipDistance = 0.0f;
	private float isLin = 0.0f;
	private float currIsLin = -1.0f;
	private bool doWeatherUpdate = false;
	private float chanceOfRain = -1.0f;

	private float moonApoSize = -1.0f;
	private Color mDCol = Color.black;
	public float ecldiff = -1.0f;
	private Transform tempTrans;
	private Vector3 useVec = new Vector3(0f,0.35f,0.75f);
	private float useMult = -1.0f;
	private float pVisFac = -1.0f;
	private float starAngle = -1.0f;
	private Quaternion starTarget;
	private int aquarialStart = 0;
	private float precRate = -1.0f;
	private float precNudge = -1.0f;
	private float precFactor = -1.0f;
	private float eclipticStarAngle = -1.0f; 
	private Color galaxyColor = Color.black;
	private float galaxyVis = -1.0f;
	private Color ambientCloudCol = Color.black;
	private Color setSkyAmbient = Color.black;
	private Color useAlpha = Color.black;
	private Color setAmbCol = Color.black;
	private Color setAmbientColor = Color.black;
	private Color mixColor = Color.black;
	private Color medCol  = new Color(0.3f,0.3f,0.3f,1.0f);
	private Color useAmbientColor = Color.black;
	private float useOvercast = -1.0f;
	private float fxUseLight = -1.0f;
	private float lDiff = -1.0f;
	private Color rainCol = Color.black;
	private Color splashCol = Color.black;
	private Color rainfogCol = Color.black;
	private float rainfogFac = -1.0f;	
	private Color fogCol = Color.black;
	private Color fogColStart = new Color(.95f,.95f,.95f,.95f);
	private float fogFac = -1.0f;	
	private Color snowCol = Color.black;
	private Color setOverallCol = new Color(0f,0f,0f,0f);
	private Color bgSCol = new Color(0f,0f,0f,0f);

	private float altAdjust = -1.0f;
	public float fogDist = -1.0f;
	private float fogFull = -1.0f;
	private Color skyAmbientCol = Color.black;
	private Color skyHorizonCol = Color.black;
	private Color ambientGI = Color.black;
	private float aurSpan = -1.0f;
	private float aurAmt = -1.0f;
	private float setTimeSpan = -1.0f;
	private float curveVal = 1.0f;
	private string setString = "format";
	private string eon = "";
	private int monthLength = -1;
	private int testMonth = -1;
	private float monthAddition = 0.0f;
	private int aM = 1;
	private float yearDiv = 365.0f;
	private float clampRes = 0.0f;
	private int px = 0;
	private RaycastHit hit;
	private float usePoint = 0.0f;
	private ParticleSystem.Particle[] setParticles;
	private Vector2 dir = Vector2.zero;
	private Vector3 tempAngle = Vector3.zero;
	private Vector2 tempAngleVec2 = Vector2.zero;
	private float texPos = 0f;
	private Color returnColor = Color.black;
	private float eclipticoffset;

	private float sunCalcSize = -1.0f;
	private float moonCalcSize;

	public float atmosphereDensity = 0.55f;
	public float horizonDensity = 2.0f;
	public float horizonDensityHeight = 0.34f;
	private float atmosTime = 1.0f;
	private float setAtmosphereDensity = 0.0f;

	public float fogDensity = 0.55f;

	private bool probeHasUpdated = false;
	public bool enableProbe = true;
	public float reflectionProbeFPS = 1.0f;
	private float useReflectionFPS = 30f;
	private float reflectionTimer = 0.0f;


	//sound variables
	public bool enableSoundFX = true;
	public bool enableTimeAdjust = true;
	public float overallVolume = 1.0f;
	public AudioMixer audioMixer;
	public AudioMixerGroup audioMixerGroup;
	public AudioClip audioWind;
	public AudioClip audioRain;
	public AudioClip audioTurb1;
	public AudioClip audioTurb2;
	public AudioClip audioAmbDay;
	public AudioClip audioAmbNight;
	public float volumeWind = 1.0f;
	public float volumeRain = 1.0f;
	public float volumeThunder = 1.0f;
	public float volumeTurb1 = 1.0f;
	public float volumeTurb2 = 1.0f;
	public float volumeAmbDay = 1.0f;
	public float volumeAmbNight = 1.0f;

	public AnimationCurve curveAmbDay24;
	public AnimationCurve curveAmbDayYR;
	public AnimationCurve curveAmbNight24;
	public AnimationCurve curveAmbNightYR;

	public Tenkoku.Core.Random TenRandom;
	public int randSeed;
	System.DateTime sysTime;


	//Transition variables
	float transitionTime = 0.0f;
	bool doTransition = false;
	public bool isDoingTransition = false;
	string transitionStartTime = "";
	string transitionTargetTime;
	string transitionTargetDate;
	float transitionDuration = 5.0f;
	float transitionDirection = 1.0f;
	AnimationCurve transitionCurve;
	bool transitionCallback = false;
	GameObject transitionCallbackObject;
	private int setTransHour;
	private int setTransMinute;
	private int setTransSecond;
	private int startSecond;
	private int startMinute;
	private int startHour;
	private float timeVal;
	private float setTransVal;
	private int endTime = 0;
	private int startTime = 0;
	private bool endTransition = false;
	private string setSun;
	private string lightVals;

	private Tenkoku.Effects.TenkokuSunShafts sunRayObject;
	private Tenkoku.Effects.TenkokuSkyFog fogObject;

	private int i;
	private string layerName;
	private Vector4 sunPos = Vector4.zero;
	private Color moonFaceCol;
	private Color cloudSpeeds;
	private Color overcastCol;
	private Color WorldCol;


	private string dataString ;
	private string[] dataArray;
	private int pos1;
	private int pos2;
	private int length;
	private string func;
	private string dat;
	private string dataUpdate;
	private string[] data;
	private string setUpdate;
	private string[] values;
	private GameObject callbackObject;


	private float rOC;
	private float mMult;
	private Vector3 delta;
	private Quaternion look;
	private float vertical;
	private Ray mRay;
	private Ray sRay;

	private Vector3 planeObjectScale = new Vector3(0.5f,0.5f,0.1f);
	private Vector3 galaxyScale = new Vector3(0.5f,0.5f,0.5f);
	private Vector3 setScale = new Vector3(1f,1f,1f);
	private Vector3 setScale2 = Vector3.zero;
	private Vector3 reflectionSize = new Vector3(100f,500f,100f);
	private Vector3 windVector = Vector3.one;
	private Vector3 sunBasePosition = new Vector3(-0.51f,0f,0f);
	private Vector3 sunPosition = Vector3.zero;
	private Vector3 sunScale = Vector3.one;
	private Vector3 moonBasePosition = new Vector3(-0.51f,0f,0f);
	private Vector3 moonPosition = Vector3.zero;
	private Vector3 moonScale = Vector3.one;

	private Vector3 plMercuryPosition = Vector3.zero;
	private Vector3 plVenusPosition = Vector3.zero;
	private Vector3 plMarsPosition = Vector3.zero;
	private Vector3 plJupiterPosition = Vector3.zero;
	private Vector3 plSaturnPosition = Vector3.zero;
	private Vector3 plUranusPosition = Vector3.zero;
	private Vector3 plNeptunePosition = Vector3.zero;

	private Vector3 starPosition = Vector3.zero;
	private Vector3 galaxyPosition = Vector3.zero;
	private Vector3 galaxyBasePosition = new Vector3(18.1f,281.87f,338.4f);
	private Vector3 galaxyBaseScale = new Vector3(1f,0.92f,0.98f);

	private Vector3 cloudPosition = Vector3.zero;
	private Vector3 nightSkyPosition = Vector3.zero;
	private Vector3 eclipsePosition = Vector3.zero;
	private Vector3 particleRayPosition = Vector3.zero;
	private Vector3 particlePosition = Vector3.zero;

	private Vector2 cloudTexScale = Vector2.zero;

	private Vector4 windPosition = Vector4.zero;

	private Color baseGAmbColor = new Color(0.38f,0.6f,0.69f,0.7f);
	private Color baseSkyCol = new Color(0.3f,0.4f,0.5f,1.0f);
	private Color baseHorizonCol = new Color(0.7f,0.8f,0.9f,1.0f);
	private Color baseRainCol = new Color(0.65f,0.75f,0.75f,1.0f);
	private Color baseSnowCol = new Color(0.85f,0.85f,0.85f,1f);
	private Color baseHighlightCloudCol = new Color(0.19f,0.19f,0.19f,1.0f);
	private Color ambientBaseCol = Color.black;
	private Color baseRayCol = Color.black;
	private float mC;
	private Color mCol = new Color(0.25f,0.33f,0.36f,1.0f);
	private Color mColMixCol = new Color(0.25f,0.33f,0.36f,1.0f);
	private float sC;
	private Color sCol = Color.black;
	private Color particleCol = Color.black;

	public float minimumHeight = 0f;
	private Vector3 lightClampVector = Vector3.one;

	private float sunsetDegrees;
	private float moonFac;
	private float moonsetFac;
	private float sunFac;
	private float sunsetFac;


	//Variables for Unity 5.3+ only
	#if UNITY_5_4_OR_NEWER
		private Vector3 snowPosition = Vector3.zero;

		private ParticleSystem.MinMaxCurve rainCurve;
		private ParticleSystem.MinMaxCurve rainCurveForceX;
		private ParticleSystem.MinMaxCurve rainCurveForceY;
		private ParticleSystem.MinMaxCurve rainCurveFog;
		private ParticleSystem.MinMaxCurve rainCurveFogForceX;
		private ParticleSystem.MinMaxCurve rainCurveFogForceY;
		private ParticleSystem.MinMaxCurve splashCurve;
		private ParticleSystem.MinMaxCurve fogCurve;
		private ParticleSystem.MinMaxCurve fogCurveForceX;
		private ParticleSystem.MinMaxCurve fogCurveForceY;
		private ParticleSystem.MinMaxCurve snowCurve;
		private ParticleSystem.MinMaxCurve snowCurveForceX;
		private ParticleSystem.MinMaxCurve snowCurveForceY;
	
		private ParticleSystem.EmissionModule rainEmission;
		private ParticleSystem.EmissionModule rainFogEmission;
		private ParticleSystem.EmissionModule splashEmission;
		private ParticleSystem.EmissionModule fogEmission;
		private ParticleSystem.EmissionModule snowEmission;

		private ParticleSystem.ForceOverLifetimeModule rainForces;
		private ParticleSystem.ForceOverLifetimeModule rainFogForces;
		private ParticleSystem.ForceOverLifetimeModule fogForces;
		private ParticleSystem.ForceOverLifetimeModule snowForces;
	#endif



	private Color[] DecodedColors;
	private Color decodeColorSun = Color.black;
	private Color decodeColorAmbientcolor = Color.black;
	private Color decodeColorMoon = Color.black;
	private Color decodeColorSkyambient = Color.black;
	private Color decodeColorSkybase = Color.black;
	private Color decodeColorAmbientcloud = Color.black;
	private Color decodeColorColorhorizon = Color.black;
	private Color decodeColorColorcloud = Color.black;
	private Color decodeColorAmbientGI = Color.black;

	private Color skyAmb = Color.black;			
	private Color gAmb = Color.black;
	private Color sAmb = Color.black;
	private Color eAmb = Color.black;

	private float csSize;
	private Vector3 cSphereSize;

	private float _deltaTime;

	private ColorSpace _linearSpace;
	private Material skyMaterial;


	private float _tempFac;
	private Vector3 _tempVec;

	private Tenkoku_TemporalReprojection temporalFX;
	private Tenkoku_VelocityBuffer temporalBuffer;
	private bool recompileFlag = false;
	private bool saveLinkToTime = false;
	private float eclipseFactor;
	private float totalEclipseFactor;

	private Tenkoku.Core.TenkokuLib libComponent;


	//private float sunAngle;
    private float daylightAngle;
    private float scatterDayFac;
    //private float scatterMorningFac;


	//Oskar-Elek Scatterins Specific
	private RenderTexture _particleDensityLUT = null;
    private RenderTexture _skyboxLUT;
    private RenderTexture _skyboxLUT2;
    private Material _material;
    private float MieG = 0.5f;
    private float MieScatterCoef = 0.5f;
    private float MieExtinctionCoef = 1.0f;
    private float RayleighScatterCoef = 8f;
    private float RayleighExtinctionCoef = 1.25f;
    private float SunIntensity = 0.25f;
    private Color IncomingLight = new Color(4, 4, 4, 4);
    private const float AtmosphereHeight = 80000.0f;
    private const float PlanetRadius = 6371000.0f;
    private readonly Vector4 DensityScale = new Vector4(7994.0f, 1200.0f, 0, 0);
    private readonly Vector4 RayleighSct = new Vector4(5.8f, 13.5f, 33.1f, 0.0f) * 0.000001f;
    private readonly Vector4 MieSct = new Vector4(2.0f, 2.0f, 2.0f, 0.0f) * 0.00001f;
    private Texture nullTexture = null;
    private int cSeconds;

    private bool weatherHasStarted = false;

//Eclipse Accumulation
float eclWarpTime = 0f;
float eclTarget = 0f;
float eclTemp = 0f;
float eclDelay = 0f;
int eclAccumCount = 0;
float eclAccumulation = 0f;
bool totalEclipse = false;




	void Awake(){
		//GET CAMERA OBJECT
		if (Application.isPlaying){
			if (mainCamera == null){
				mainCamera = Camera.main.transform;
			}
		}

		//set custom random generator instance
		randSeed = System.Environment.TickCount;
		TenRandom = new Tenkoku.Core.Random(randSeed);

		// Manually Set Execution Order
		// This ensures that Tenkoku runs after Update() in other functions which
		// prevents unintended timing issues.  This is intended to fix sky/camera
		// stuttering issues when used with some controllers and 3rd party assets.
		#if UNITY_EDITOR
	        string scriptName = typeof(Tenkoku.Core.TenkokuModule).Name;
	        foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts()){
	            if (monoScript.name == scriptName){
	                if (MonoImporter.GetExecutionOrder(monoScript) != 1000){
	                    MonoImporter.SetExecutionOrder(monoScript, 1000);
	                }
	                break;
	            }
	        }
        #endif

        probeHasUpdated = false;

		//get object references
		LoadObjects();

		//get color map
		UpdateColorMap();

		//get decoded colors
		LoadDecodedColors();
	}




	void Start () {

		//disconnect object from prefab connection
		#if UNITY_EDITOR
		#if !UNITY_2018_3_OR_NEWER
			PrefabUtility.DisconnectPrefabInstance(this.gameObject);
		#endif
		#endif


		//Set Project Layers data
		if (Application.isPlaying){
			tenLayerMasks.Clear();
			for (i = 0; i < 32; i++){
				layerName = LayerMask.LayerToName(i);
				tenLayerMasks.Add(layerName);
			}
		}


		//FORCE SHADER INIT (Oskar-Elek)
		Shader shader = Shader.Find("TENKOKU/Tenkoku_sky_elek_Scatter");
        _material = new Material(shader);

	        _material.SetFloat("_AtmosphereHeight", AtmosphereHeight);
	        _material.SetFloat("_PlanetRadius", PlanetRadius);
	        _material.SetVector("_DensityScaleHeight", DensityScale);
	        _material.SetVector("_ScatteringR", RayleighSct * RayleighScatterCoef);
	        _material.SetVector("_ScatteringM", MieSct * MieScatterCoef);
	        _material.SetVector("_ExtinctionR", RayleighSct * RayleighExtinctionCoef);
	        _material.SetVector("_ExtinctionM", MieSct * MieExtinctionCoef);
	        _material.SetColor("_IncomingLight", IncomingLight);
	        _material.SetFloat("_MieG", MieG);
	        _material.SetTexture("_ParticleDensityLUT", _particleDensityLUT);
	        _material.SetFloat("_SunIntensity", SunIntensity);

        //PRECOMPUTE PARTICLE DENSITY
        if (_particleDensityLUT == null)
        {
            _particleDensityLUT = new RenderTexture(1024, 1024, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
            _particleDensityLUT.name = "ParticleDensityLUT";
            _particleDensityLUT.filterMode = FilterMode.Bilinear;
            _particleDensityLUT.Create();
        }

        nullTexture = null;
        Graphics.Blit(nullTexture, _particleDensityLUT, _material, 0);
        _material.SetTexture("_ParticleDensityLUT", _particleDensityLUT);

		useAtmosphereIndex = -1;

		weatherHasStarted = false;

		////TURN OFF ANTI ALIASING in forward mode
	    if (disableMSA) QualitySettings.antiAliasing = 0;

	    //CACHE COLOR SPACE (for performance)
	    _linearSpace = QualitySettings.activeColorSpace;

		//TURN OFF BUILT-IN FOG
		if (enableFog) UnityEngine.RenderSettings.fog = false;

	}







	void LoadObjects() {

		//GET TENKOKU OBJECTS
		calcComponent = (Tenkoku.Core.TenkokuCalculations) FindObjectOfType(typeof(Tenkoku.Core.TenkokuCalculations));
		libComponent = (Tenkoku.Core.TenkokuLib) FindObjectOfType(typeof(Tenkoku.Core.TenkokuLib));

		//SET DEFAULT CURVE PROPERTIES
		#if UNITY_5_4_OR_NEWER
			rainCurve = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			rainCurveForceX = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			rainCurveForceY = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			rainCurveFog = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			rainCurveFogForceX = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			rainCurveFogForceY = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			splashCurve = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			fogCurve = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			fogCurveForceX = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			fogCurveForceY = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			snowCurve = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			snowCurveForceX = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
			snowCurveForceY = new ParticleSystem.MinMaxCurve(0.0f, 1.0f);
		#endif

		//GET CLOUD OBJECT MATERIALS
		#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
			if (Application.isPlaying){
				materialObjectCloudSphere = libComponent.renderObjectCloudSphere.material;
			} else {
				materialObjectCloudSphere = libComponent.renderObjectCloudSphere.sharedMaterial;
			}

		#endif

		//SET DEFAULT CAMERAS
		if (Application.isPlaying){
			useCamera = null;
			useCameraCam = null;
		}

		//SET DEFAULT POSITIONS
		if (libComponent.sunObject != null) libComponent.sunObject.localPosition = sunBasePosition;
		if (libComponent.sunSphereObject != null) libComponent.sunSphereObject.localPosition = Vector3.zero;

		if (libComponent.moonObject != null) libComponent.moonObject.localPosition = moonBasePosition;
		if (libComponent.moonSphereObject != null) libComponent.moonSphereObject.localPosition = Vector3.zero;

		libComponent.planetTransMercury.localPosition = Vector3.zero;
		libComponent.planetTransVenus.localPosition = Vector3.zero;
		libComponent.planetTransMars.localPosition = Vector3.zero;
		libComponent.planetTransJupiter.localPosition = Vector3.zero;
		libComponent.planetTransSaturn.localPosition = Vector3.zero;
		libComponent.planetTransUranus.localPosition = Vector3.zero;
		libComponent.planetTransNeptune.localPosition = Vector3.zero;
	
		//UPDATE SCENE GLOBAL ILLUMINATION
		UnityEngine.DynamicGI.UpdateEnvironment();
	}
	    
	    


	void HandleGlobalSound(){
		if (libComponent.ambientSoundObject != null){

		    //--------------------------------------
		    //---    CALCULATE MASTER VOLUME    ----
		    //--------------------------------------
			overallVolume = Mathf.Clamp01(overallVolume);

		    //---------------------------------------
		    //---    CALCULATE ELEMENT VOLUME    ----
		    //---------------------------------------
			libComponent.ambientSoundObject.volWind = Mathf.Lerp(0.0f,1.5f,weather_WindAmt) * (volumeWind * overallVolume);
			libComponent.ambientSoundObject.volTurb1 = Mathf.Lerp(0.0f,2.0f,weather_WindAmt) * (volumeTurb1 * overallVolume);
			libComponent.ambientSoundObject.volTurb2 = Mathf.Lerp(-1.0f,1.5f,weather_WindAmt) * (volumeTurb2 * overallVolume);
			libComponent.ambientSoundObject.volRain = volumeRain * ((weather_RainAmt*1.5f) * overallVolume);

		    //---------------------------------------------
		    //---    CALCULATE AMBIENT AUDIO CURVES    ----
		    //---------------------------------------------
			//libComponent.ambientSoundObject.volAmbNight = volumeAmbNight * (curveAmbNight24.Evaluate(dayValue) * curveAmbNightYR.Evaluate(yearValue) * overallVolume);
			//libComponent.ambientSoundObject.volAmbDay = volumeAmbDay * (curveAmbDay24.Evaluate(dayValue) * curveAmbDayYR.Evaluate(yearValue) * overallVolume);
			libComponent.ambientSoundObject.volAmbNight = volumeAmbNight * (curveAmbNight24.Evaluate(scatterDayFac * 0.5f) * curveAmbNightYR.Evaluate(yearValue) * overallVolume);
			libComponent.ambientSoundObject.volAmbDay = volumeAmbDay * (curveAmbDay24.Evaluate(scatterDayFac * 0.5f) * curveAmbDayYR.Evaluate(yearValue) * overallVolume);


		    //-----------------------------------------
		    //---    SEND DATA TO SOUND HANDLER    ----
		    //-----------------------------------------
		    libComponent.ambientSoundObject.enableSounds = enableSoundFX;
		    libComponent.ambientSoundObject.enableTimeAdjust = enableTimeAdjust;

		    libComponent.ambientSoundObject.mixer = audioMixer;
		    libComponent.ambientSoundObject.mixerGroup = audioMixerGroup;


			if (audioWind != null){
				libComponent.ambientSoundObject.audioWind = audioWind;
			} else {
				libComponent.ambientSoundObject.audioWind = null;
			}

			if (audioTurb1 != null){
				libComponent.ambientSoundObject.audioTurb1 = audioTurb1;
			} else {
				libComponent.ambientSoundObject.audioTurb1 = null;
			}

			if (audioTurb2 != null){
				libComponent.ambientSoundObject.audioTurb2 = audioTurb2;
			} else {
				libComponent.ambientSoundObject.audioTurb2 = null;
			}

			if (audioRain != null){
				libComponent.ambientSoundObject.audioRain = audioRain;
			} else {
				libComponent.ambientSoundObject.audioRain = null;
			}

			if (audioAmbDay != null){
				libComponent.ambientSoundObject.audioAmbDay = audioAmbDay;
			} else {
				libComponent.ambientSoundObject.audioAmbDay = null;
			}

			if (audioAmbNight != null){
				libComponent.ambientSoundObject.audioAmbNight = audioAmbNight;
			} else {
				libComponent.ambientSoundObject.audioAmbNight = null;
			}

		}
	}





	void UpdatePositions(){

		// -------------------------------
		// --   SET SKY POSITIONING   ---
		// -------------------------------
		if (Application.isPlaying){

			// track positioning
			if (useCamera != null){
				libComponent.skyObject.position = useCamera.position;
			
				//sky sizing based on camera
				if (useCameraCam != null){
				//if (currClipDistance != useCameraCam.farClipPlane){
					currClipDistance = useCameraCam.farClipPlane;

					setSkyUseSize = (currClipDistance/20.0f)*1.9f;
					setScale.x = setSkyUseSize;
					setScale.y = setSkyUseSize;
					setScale.z = setSkyUseSize;

					setScale2.x = setSkyUseSize * 0.76f;
					setScale2.y = setSkyUseSize * 0.76f;
					setScale2.z = setSkyUseSize * 0.76f;

					libComponent.sunSphereObject.localScale = setScale2;
					libComponent.moonSphereObject.localScale = setScale2;
					libComponent.starfieldObject.localScale = setScale;
					libComponent.starRenderSystem.starDistance = currClipDistance;
					libComponent.starRenderSystem.setSize = libComponent.starRenderSystem.baseSize * (currClipDistance/800f);
					libComponent.starGalaxyObject.localScale = galaxyScale;

					Shader.SetGlobalVector("_TenkokuCameraPos", useCamera.position);

				//}
				}
			}
		}
	}






	void LateUpdate () {

	//if (Application.isPlaying){

		//SET VERSION NUMBER
		tenkokuVersionNumber = "1.2.2";


		//UPDATE RANDOM NUMBER SETTING
		if (TenRandom == null){
			TenRandom = new Tenkoku.Core.Random(randSeed);
		}


		////TURN OFF ANTI ALIASING
		#if UNITY_EDITOR
			if (disableMSA) QualitySettings.antiAliasing = 0;
		#endif


		//set project layer masks
		#if UNITY_EDITOR
			if (!Application.isPlaying){
				tenLayerMasks.Clear();
				for (i = 0; i < 32; i++){
					layerName = LayerMask.LayerToName(i);
					tenLayerMasks.Add(layerName);
				}
			}
		#endif


		// SET SKYBOX DATA
		CalculateSkyboxData(RenderSettings.skybox);


		// EDITOR MODE SPECIFIC
		if (!Application.isPlaying){

			//get objects in editor mode
			LoadObjects();
			//Debug.Log("load");

	    	//rotate sun in editor mode
	    	libComponent.sunlightObject.LookAt(libComponent.sunSphereObject);
			libComponent.worldlightObject.localRotation = libComponent.sunlightObject.localRotation;
			libComponent.nightSkyLightObject.localRotation = libComponent.moonObject.localRotation;

			//set layers
			if (libComponent.lightObjectWorld.gameObject.activeInHierarchy){
				libComponent.lightObjectWorld.cullingMask = lightLayer;
				libComponent.lightObjectNight.cullingMask = lightLayer;
				libComponent.lightObjectFill.cullingMask = lightLayer;
			}
		}


		//CACHE SYSTEM VARIABLES (for performance)
		_deltaTime = Time.deltaTime;

		//Set base transition boolean flag
		isDoingTransition = false;


		//UPDATE SYSTEM TIMER
		if (!autoTime) useTimeCompression = 0f;

		//timerCloudMod = 1f;

	    	if (!useAutoTime){
	    		//timerCloudMod = 0.0f;
	    	} else {
	    		cloudStepTime += (_deltaTime * useTimeCompression);
	    		if (cloudStepTime >= 1f) cloudStepTime += _deltaTime;
	    		if (lastSecond != currentSecond) cloudStepTime = 0f;
	    	}




		#if UNITY_EDITOR
		if (EditorApplication.isCompiling){
			recompileFlag = true;
			cloudLinkToTime = false;
		} else {
		#endif

			if (enableAutoAdvance){
				systemTime = ((calcComponent.UT * 3600f));// + timerCloudMod * (0.0031f * weather_cloudSpeed);
			}

			if (cloudLinkToTime){
				Shader.SetGlobalFloat("_tenkokuTimer", systemTime % 60f);
			}

		#if UNITY_EDITOR
		}
		#endif
		


		//UPDATE TENKOKU SYSTEM TIME
		TimeUpdate();


		// UPDATE DECODED COLOR MAP
		// This is only in edit mode and is used for convenience when editing color gradient texture.
		// Keep it on for live editing of color texture.  Turn off for improved in-editor performance.  
		#if UNITY_EDITOR
			//UpdateColorMap();
		#endif

		// UPDATE DECODED COLORS
		// This resamples the gradient texture map based on current time of day.
		LoadDecodedColors();


	    // -----------------------------
		// --   GET CAMERA OBJECTS   ---
		// -----------------------------
		// This is put into the main update loop in order to have updating
		// in edit mode, while the scene is not playing.
		if (cameraTypeIndex == 0){
			if (Camera.main != null){
				mainCamera = Camera.main.transform;
			}
			manualCamera = null;
		}
		if (cameraTypeIndex == 1){
			if (manualCamera != null){
				mainCamera = manualCamera;
			} else {
				if (Camera.main != null){
					mainCamera = Camera.main.transform;
				}
			}
		}

		//if (useCamera != mainCamera){ 
		//	useCamera = mainCamera;
		//}



		if (useCamera != mainCamera && mainCamera != null){
			useCamera = mainCamera;
			useCameraCam = useCamera.GetComponent<Camera>();

			MV = useCameraCam.worldToCameraMatrix.inverse;
			Shader.SetGlobalMatrix("_Tenkoku_CameraMV",MV);
			useCameraCam.clearFlags = CameraClearFlags.Skybox;

			if (useAutoFX && Application.isPlaying){

				fogObject = useCamera.GetComponent<Tenkoku.Effects.TenkokuSkyFog>();
				if (fogObject == null){
					fogObject = useCamera.gameObject.AddComponent<Tenkoku.Effects.TenkokuSkyFog>();
				}

				sunRayObject = useCamera.GetComponent<Tenkoku.Effects.TenkokuSunShafts>();
				if (sunRayObject == null){
					sunRayObject = useCamera.gameObject.AddComponent<Tenkoku.Effects.TenkokuSunShafts>();
				}

				if (useTemporalAliasing){
					temporalFX = useCamera.GetComponent<Tenkoku_TemporalReprojection>();
					temporalBuffer = useCamera.GetComponent<Tenkoku_VelocityBuffer>();
					if (temporalFX == null){
						temporalFX = useCamera.gameObject.AddComponent<Tenkoku_TemporalReprojection>();
						temporalBuffer = useCamera.GetComponent<Tenkoku_VelocityBuffer>();
					}
				}
			}

		}


		if (useAutoFX && useCamera != null && Application.isPlaying){
			if (fogObject == null){
				fogObject = useCamera.GetComponent<Tenkoku.Effects.TenkokuSkyFog>();
				if (fogObject == null){
					fogObject = useCamera.gameObject.AddComponent<Tenkoku.Effects.TenkokuSkyFog>();
				}
			}

			if (sunRayObject == null){
				sunRayObject = useCamera.GetComponent<Tenkoku.Effects.TenkokuSunShafts>();
				if (sunRayObject == null){
					sunRayObject = useCamera.gameObject.AddComponent<Tenkoku.Effects.TenkokuSunShafts>();
				}
			}

			if (useTemporalAliasing){
				if (temporalFX == null){
					temporalFX = useCamera.GetComponent<Tenkoku_TemporalReprojection>();
					temporalBuffer = useCamera.GetComponent<Tenkoku_VelocityBuffer>();
					if (temporalFX == null){
						temporalFX = useCamera.gameObject.AddComponent<Tenkoku_TemporalReprojection>();
						temporalBuffer = useCamera.GetComponent<Tenkoku_VelocityBuffer>();
					}
				}
			}
		}



		if (Application.isPlaying){
			if (useCameraCam == null && mainCamera != null){
				useCameraCam = useCamera.GetComponent<Camera>();
			}
		}



	    // -----------------------------
		// --   TRACK CAMERA   ---
		// -----------------------------
		if (Application.isPlaying){
			UpdatePositions();
		}


		//SYNC TIME TO SYSTEM
		if (Application.isPlaying){
			if (autoTimeSync || autoDateSync){

				sysTime = System.DateTime.Now;
				
				if (autoTimeSync){
					currentHour = Mathf.FloorToInt(sysTime.Hour);
					currentMinute = Mathf.FloorToInt(sysTime.Minute);
					currentSecond = Mathf.FloorToInt(sysTime.Second);
					useTimeCompression = 1.0f;
				}
				if (autoDateSync){
					currentYear = Mathf.FloorToInt(sysTime.Year);
					currentMonth = Mathf.FloorToInt(sysTime.Month);
					currentDay = Mathf.FloorToInt(sysTime.Day);
				}

			} else {
				if (!doTransition){
					useTimeCompression = timeCompression * curveVal;
				}
			}
		}


		//SET LIGHTING LAYERS
		if (Application.isPlaying){
			if (libComponent.lightObjectWorld.gameObject.activeInHierarchy){
				libComponent.lightObjectWorld.cullingMask = lightLayer;
				libComponent.lightObjectNight.cullingMask = lightLayer;
				libComponent.lightObjectFill.cullingMask = lightLayer;
			}
		}






	    // -------------------------------
		// --   SET REFLECTION PROBE   ---
		// -------------------------------
		if (libComponent.tenkokuReflectionObject != null){

			if (enableProbe && Application.isPlaying){

				//enable probe during play mode
				libComponent.tenkokuReflectionObject.enabled = true;
				libComponent.tenkokuReflectionObject.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
				libComponent.tenkokuReflectionObject.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
				//tenkokuReflectionObject.intensity = 1.0f;

				//set size to world size
				if (useCameraCam != null){
					reflectionSize.x = useCameraCam.farClipPlane*2f;
					reflectionSize.y = useCameraCam.farClipPlane;
					reflectionSize.z = useCameraCam.farClipPlane*2f;
					libComponent.tenkokuReflectionObject.size = reflectionSize;
				}

				//handle probe update when set to "scripting"
				if (libComponent.tenkokuReflectionObject.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting){
					reflectionTimer += _deltaTime;

					useReflectionFPS = 20f;
					if (probeHasUpdated) useReflectionFPS = reflectionProbeFPS;

					if (reflectionTimer >= (1.0f/useReflectionFPS)){
						libComponent.tenkokuReflectionObject.RenderProbe(null);
						libComponent.tenkokuReflectionObject.transform.localPosition = libComponent.skyObject.localPosition;
						reflectionTimer = 0.0f;

						if (Time.time > 0.5f){
							useReflectionFPS = 2f;
							probeHasUpdated = true;
						}

						//update GI environment (ambient lighting)
						UnityEngine.DynamicGI.UpdateEnvironment();
					}
				}


			} else {
				//disable probe update during scene-mode
				libComponent.tenkokuReflectionObject.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked;
				libComponent.tenkokuReflectionObject.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
			}
			
		}



	    // --------------------
		// --   SET FLAGS   ---
		// --------------------
		//Set Linear Mode on Shader
		isLin = 0.0f;
		if (_linearSpace == ColorSpace.Linear) isLin = 1.0f;
		if (isLin != currIsLin){
			currIsLin = isLin;
		}
		Shader.SetGlobalFloat("_tenkokuIsLinear",isLin);





	    // --------------------------------
		// --   CALCULATE WEATHER   ---
		// --------------------------------
		
		//RANDOM WEATHER
		if (weatherTypeIndex == 1){
			CalculateWeatherRandom();
		}

		//ADVANCED WEATHER
		if (weatherTypeIndex == 2){
			//
		}

		//Handle Temperature Distortion setting
		Shader.SetGlobalFloat("_Tenkoku_HeatDistortAmt",Mathf.Lerp(0.0f,0.04f,Mathf.Clamp(weather_temperature-90.0f,0.0f,120.0f)/30.0f));

		//Handle Rainbow Rendering
		rOC = 1.0f-Mathf.Clamp(weather_OvercastAmt*2f,0.0f,1.0f);
		Shader.SetGlobalFloat("_tenkoku_rainbowFac1",Mathf.Clamp(Mathf.Lerp(0.0f,1.25f,weather_rainbow),0.0f,rOC));
		Shader.SetGlobalFloat("_tenkoku_rainbowFac2",Mathf.Clamp(Mathf.Lerp(-2.0f,1.0f,weather_rainbow),0.0f,rOC));



	    // --------------------
		// --   SET SOUND   ---
		// --------------------
		HandleGlobalSound();
		


		
		// --------------
		// --   SUN   ---
		// --------------
		//sunSphereObject.gameObject.SetActive(true);
    	//libComponent.sunObject.gameObject.SetActive(true);
	    //libComponent.sunlightObject.gameObject.SetActive(true);
	    
	   	//solar ecliptic movement
		//calcComponent.CalculateNode(1); //sunstatic
		eclipticoffset = 0.0f;

		//sun position and light direction based on Solar Day Calculation
	    calcComponent.CalculateNode(2); //sun
	    sunPosition.x = 0.0f;
	    sunPosition.y = 90f + RoundDigit(calcComponent.azimuth,3) + setRotation;//+0.125f;
	    sunPosition.z = -180.0f + RoundDigit(calcComponent.altitude,3) + 0.6f;
	    libComponent.sunSphereObject.localEulerAngles = sunPosition;
	

	    //sun apparent horizon size
	    //due to optical atmospheric refraction as well as relative viusal proportion artifacts
	    //at the horizon, the sun appears larger at the horizon than it does while overhead.
	    //we're artificially changing the size of the sun to simulate this visual trickery.
	    //sSDiv = Mathf.Abs((sunPosition.z-180.0f)/180.0f);
	    //if (sSDiv == 0.0f) sSDiv = 1.0f;
	    //if (sSDiv < 0.5f && sSDiv > 0.0f) sSDiv = 0.5f + (0.5f - sSDiv);
	    //sunCalcSize = sunSize + Mathf.Lerp(0.0f,(sunSize * Mathf.Lerp(-2.0f,2.0f,sSDiv)),Mathf.Lerp(-1.0f,1.0f,sSDiv));
	    

	    //Calculate moon/horizon atitude
		sunsetDegrees = 20.0f;
		sunFac = libComponent.sunlightObject.eulerAngles.x;
		if (sunFac > 90.0f) sunFac = 0.0f;
		sunsetFac = Mathf.Clamp01(Mathf.Lerp(-0.5f,1.0f,Mathf.Clamp01(sunFac / sunsetDegrees)));

	    //set explicit size of sun in Custom mode
	    sunCalcSize = sunSize*0.5f;
	    Shader.SetGlobalFloat("_Tenkoku_SunSize",sunSize+0.005f);

	    //moon horizon size - turned off for now
	    //due to optical atmospheric refraction as well as relative viusal proportion artifacts
	    //at the horizon, the moon appears larger at the horizon than it does while overhead.
	    //we're artificially changing the size of the moon to simulate this visual trickery.
	    sunCalcSize *=  Mathf.Lerp(1.5f, 1f, sunsetFac);

		//set sun scale
		sunScale.x = sunCalcSize;
		sunScale.y = sunCalcSize;
		sunScale.z = sunCalcSize;
		libComponent.sunObject.localScale = sunScale;

	    //solar parhelion
	    //models the non-circular orbit of the earth.  The sun is not a constant distance
	    //varying over the course of the year, this offsets the perceived sunset and
	    //sunrise times from where they would be given a perfectly circular orbit.
	    //--- no code yet ---

	    //send sun position to shader
		sunPos.x = libComponent.sunObject.localPosition.x/360.0f;
		sunPos.y = libComponent.sunObject.localPosition.y/360.0f;
		sunPos.z = libComponent.sunObject.localPosition.z/360.0f;
		sunPos.w = 0.0f;
		Shader.SetGlobalColor("_Tenkoku_SunPos", sunPos);

	    //set sun color
	    colorSun = decodeColorSun;

	    //sun lightsource, point toward camera
	    if (useCamera != null){
	    	libComponent.sunlightObject.LookAt(useCamera);
	    }









	    // ---------------
		// --   MOON   ---
		// ---------------

		if (moonTypeIndex == 2){
			moonIsVisible = false;
		} else {
			moonIsVisible = true;
		}
		
		if (!moonIsVisible){
		    libComponent.renderObjectMoon.enabled = false;
		} else {
			libComponent.renderObjectMoon.enabled = true;
		}
		

		//Moon Simple Movement
		calcComponent.moonPos = -1;
		if (moonTypeIndex == 1){
	    	calcComponent.moonPos = moonPos;
    	}

    	//Calculate Moon Position
		calcComponent.CalculateNode(3);
		moonPosition.x = 0f;
		moonPosition.y = 90f + RoundDigit(calcComponent.azimuth,3) + setRotation+0.2f;
		moonPosition.z = -180f + RoundDigit(calcComponent.altitude,3);//-0.2f;
		libComponent.moonSphereObject.localEulerAngles = moonPosition;



		//Calculate Moon Light Coverage
		moonLightAmt = Quaternion.Angle(libComponent.sunlightObject.rotation,libComponent.moonlightObject.rotation) / 180.0f;

		// Manual Quaternion Angle Construction
		// We do this instead of Unity's Quaternion.Angle() function because we don't want it automatically clamped.
		// The moon's phase is a float from 0.0 - 1.0, where 0.5 is a full moon and 1.0/0.0 are new moon.
		mres = libComponent.moonlightObject.rotation * Quaternion.Inverse(libComponent.sunlightObject.rotation);
		moonPhase =  (Mathf.Acos(mres.w) * 2.0f * 57.2957795f) / 360f;

	    //moon libration (currently disabled)
	    //as the moon travels across the lunar month it appears to wobbleback and
	    //forth as it transits from one phase to another.  This is called "libration"
	    //var monthAngle = (monthValue);
	    //if (monthAngle == 0.0) monthAngle = 1.0;
	    //if (monthAngle < 0.5 && monthAngle > 0.0) monthAngle = 0.5 + (0.5 - monthAngle);
	    //libComponent.moonObject.localEulerAngles.z = 0.0-Mathf.SmoothStep(336.0,384.0,Mathf.Lerp(-1.0,1.0,moonClipAngle));

		//Calculate moon/horizon atitude
		sunsetDegrees = 20.0f;
		moonFac = libComponent.nightSkyLightObject.eulerAngles.x;
		if (moonFac > 90.0f) moonFac = 0.0f;
		moonsetFac = Mathf.Clamp01(Mathf.Lerp(-0.5f,1.0f,Mathf.Clamp01(moonFac / sunsetDegrees)));
		Shader.SetGlobalFloat("Tenkoku_MoonHFac",moonsetFac);

	    //set explicit size of moon in Custom mode
	    moonCalcSize = moonSize*1.21f;

	    //moon apogee and perigree
	    moonApoSize = Mathf.Lerp(1.2f,0.82f,calcComponent.moonApogee);
	    moonCalcSize *=  moonApoSize;

	    //moon horizon size
	    //due to optical atmospheric refraction as well as relative visual proportion artifacts
	    //at the horizon, the moon appears larger at the horizon than it does while overhead.
	    //we're artificially changing the size of the moon to simulate this visual trickery.
	    moonCalcSize *=  Mathf.Lerp(1.5f, 1f, moonsetFac);

	    //set final moon scale
	    moonScale.x = moonCalcSize;
	    moonScale.y = moonCalcSize;
	    moonScale.z = moonCalcSize;
	    libComponent.moonObject.localScale = moonScale;

	    //moon color
		colorMoon = decodeColorMoon*1.0f;
		colorSkyAmbient = decodeColorSkyambient;

		//moon intensity
	    moonLightIntensity = Mathf.Clamp(moonLightIntensity,0.0f,1.0f);

	    //moon day fade
	    // we apply an extra fade during daylight hours so the moon recedes into the sky.
	    // it is still visible, yet it's visiblilty is superceded by the bright atmosphere.
	    moonDayIntensity = Mathf.Clamp(moonDayIntensity,0.0f,1.0f);
	    mDCol = Color.gray;
	    mDCol.a = Mathf.Lerp(1.0f,moonDayIntensity,colorSkyAmbient.r);

	    if (Application.isPlaying){
	    	libComponent.renderObjectMoon.material.SetColor("_Color", Color.Lerp(mDCol,(ambientCol*colorSun),ambientCol.r)*mDCol);
			libComponent.renderObjectMoon.material.SetColor("_AmbientTint",ambientCol);
		} else {
	    	libComponent.renderObjectMoon.sharedMaterial.SetColor("_Color", Color.Lerp(mDCol,(ambientCol*colorSun),ambientCol.r)*mDCol);
			libComponent.renderObjectMoon.sharedMaterial.SetColor("_AmbientTint",ambientCol);
		}
		//set moon face light color
		moonFaceCol = colorMoon*(1.0f-decodeColorAmbientcolor.r);
		Shader.SetGlobalColor("Tenkoku_MoonLightColor",moonFaceCol);
		Shader.SetGlobalColor("Tenkoku_MoonHorizColor",moonHorizColor);






	    // ----------------------------
		// --   SET DAYLIGHT TIME   ---
		// ----------------------------
		calcTime = Vector3.Angle((libComponent.sunObject.position - libComponent.skyObject.localPosition),libComponent.skyObject.up) / 180.0f;
		calcTime = 1.0f-calcTime;
		setTime = calcTime*1.0f*colorRamp.width;
		stepTime = Mathf.FloorToInt(setTime);
		timeLerp = setTime - stepTime;


	    // ----------------------------
		// --   SET MOONLIGHT TIME   ---
		// ----------------------------
		calcTimeM = Vector3.Angle((libComponent.moonObject.position - libComponent.skyObject.localPosition),libComponent.skyObject.up) / 180.0f;
		calcTimeM = 1.0f-calcTimeM;
		setTimeM = calcTimeM*1.0f*colorRamp.width;
		stepTimeM = Mathf.FloorToInt(setTimeM);
		timeLerpM = setTimeM - stepTimeM;





	    // ------------------
		// --   PLANETS   ---
		// ------------------
		useMult = 1.0f;
		pVisFac = Mathf.Lerp(0.75f,0.1f,colorSkyAmbient.r);
		
		if (Application.isPlaying){

			//enable visibility
			libComponent.planetRendererSaturn.enabled = true;
			libComponent.planetRendererJupiter.enabled = true;
			libComponent.planetRendererNeptune.enabled = true;
			libComponent.planetRendererUranus.enabled = true;
			libComponent.planetRendererMercury.enabled = true; 
			libComponent.planetRendererVenus.enabled = true;
			libComponent.planetRendererMars.enabled = true;
	    

			//set planetary positions
			if (libComponent.planetObjMercury){
			calcComponent.CalculateNode(4); //mercury
			plMercuryPosition.x = 0f;
			plMercuryPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plMercuryPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransMercury.localEulerAngles = plMercuryPosition;
			tempTrans = libComponent.planetTransMercury;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjMercury.planetOffset = tempTrans.localPosition;
		 	libComponent.planetObjMercury.planetVis = pVisFac * planetIntensity;
		 	}

		 	if (libComponent.planetObjVenus){
		 	calcComponent.CalculateNode(5); //venus
			plVenusPosition.x = 0f;
			plVenusPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plVenusPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransVenus.localEulerAngles = plVenusPosition;
			tempTrans = libComponent.planetTransVenus;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjVenus.planetOffset = tempTrans.localPosition;
		 	libComponent.planetObjVenus.planetVis = Mathf.Clamp(pVisFac,0.1f,1.0f) * planetIntensity;
		 	}

		 	if (libComponent.planetObjMars){
		  	calcComponent.CalculateNode(6); //mars
			plMarsPosition.x = 0f;
			plMarsPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plMarsPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransMars.localEulerAngles = plMarsPosition;
			tempTrans = libComponent.planetTransMars;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjMars.planetOffset = tempTrans.localPosition;
		 	libComponent.planetObjMars.planetVis = Mathf.Clamp(pVisFac,0.02f,1.0f) * planetIntensity;
		 	}

		 	if (libComponent.planetObjJupiter){
		  	calcComponent.CalculateNode(7); //jupiter
			plJupiterPosition.x = 0f;
			plJupiterPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plJupiterPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransJupiter.localEulerAngles =  plJupiterPosition;
			tempTrans = libComponent.planetTransJupiter;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjJupiter.planetOffset = tempTrans.localPosition;
		    libComponent.planetObjJupiter.planetVis = Mathf.Clamp(pVisFac,0.05f,1.0f) * planetIntensity;
		    }

		    if (libComponent.planetObjSaturn){
		 	calcComponent.CalculateNode(8); //saturn
			plSaturnPosition.x = 0f;
			plSaturnPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plSaturnPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransSaturn.localEulerAngles = plSaturnPosition;
			tempTrans = libComponent.planetTransSaturn;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjSaturn.planetOffset = tempTrans.localPosition;
		  	libComponent.planetObjSaturn.planetVis = pVisFac * planetIntensity;
		    }

		    if (libComponent.planetObjUranus){
			calcComponent.CalculateNode(9); //uranus
			plUranusPosition.x = 0f;
			plUranusPosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plUranusPosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransUranus.localEulerAngles = plUranusPosition;
			tempTrans = libComponent.planetTransUranus;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjUranus.planetOffset = tempTrans.localPosition; 
			libComponent.planetObjUranus.planetVis = pVisFac * planetIntensity;
			}

			if (libComponent.planetObjNeptune){
			calcComponent.CalculateNode(10); //neptune
			plNeptunePosition.x = 0f;
			plNeptunePosition.y = 90.0f+calcComponent.azimuth+setRotation;
			plNeptunePosition.z = -180.0f+calcComponent.altitude-0.5f;
			libComponent.planetTransNeptune.localEulerAngles = plNeptunePosition;
			tempTrans = libComponent.planetTransNeptune;
			tempTrans.localPosition = Vector3.zero;
			tempTrans.Translate(useVec * (eclipticoffset*useMult), Space.World);
		    libComponent.planetObjNeptune.planetOffset = tempTrans.localPosition; 
		    libComponent.planetObjNeptune.planetVis = pVisFac * planetIntensity;
		    }
		    
	    }


	    // ----------------
		// --   STARS   ---
		// ----------------

	    //SIDEAREAL DAY CALCULATION
		//The sideareal day is more accurate than the solar day, and is
		//the actual determining calculation for the passage of a full year.
		if (starTypeIndex == 0 || starTypeIndex == 2){
		
		    starAngle = -1.0f;
		    starTarget = Quaternion.Euler((setLatitude), 183.0f, -starAngle);
		    libComponent.starfieldObject.rotation = starTarget;
		    starPosition.x = libComponent.starfieldObject.eulerAngles.x;
		    starPosition.y = libComponent.starfieldObject.eulerAngles.y;
		    starPosition.z = -(((calcComponent.UT/23.99972f))*360.0f) + ((setLongitude)-95.0f) + (calcComponent.tzOffset*15f) - (calcComponent.dstOffset*15f) - (360.0f*Mathf.Abs(Mathf.Floor(calcComponent.day/365.25f)-(calcComponent.day/365.25f)));

		    //CALCULATE PRECESSIONAL MOVEMENT
			//precessional movement occurs at a rate of 1 degree every 71.666 years, thus each precessional epoch
			//takes up exactly 32 degrees on the spherical celestial star plane. this is calculated on the
			//assumption that the Aquarial age begins in the year 2600ad.  if you disagree and would like to
			//see other aquarian start calibrations (year 2148 for example), simply change the 'aquarialStart' variable.
			aquarialStart = 2600;
			precRate = 71.6f;
			precNudge = 258.58f; //this value simply helps set the exact position of the star sphere.
			precFactor = ((aquarialStart/precRate)*(aquarialStart))+((aquarialStart-currentYear)/precRate)+precNudge;
			starPosition.z = starPosition.z-precFactor-4.0f;

		} else {
			starPosition.x = libComponent.starfieldObject.eulerAngles.x;
			starPosition.y = libComponent.starfieldObject.eulerAngles.y;
			starPosition.z = starPos;
		}

		//Set final star position
		starPosition.y = setRotation;
		libComponent.starfieldObject.eulerAngles = starPosition;


	    //solar ecliptic movement
	    eclipticStarAngle = (starValue);
	    if (eclipticStarAngle == 0.0) eclipticStarAngle = 1.0f;
	    if (eclipticStarAngle < 0.5f && eclipticStarAngle > 0.0f) eclipticStarAngle = 0.5f + (0.5f - eclipticStarAngle);
	    eclipticStarAngle += 1.35f;

	   
	    //GALAXY / Skybox Rendering
		libComponent.renderObjectGalaxy.enabled = true;
		//useGalaxyIntensity = galaxyIntensity;
	    if (galaxyTypeIndex == 2 && starTypeIndex == 2){
			libComponent.renderObjectGalaxy.enabled = false;
	    } else {
	    	libComponent.renderObjectGalaxy.enabled = true;

	    	// Realistic Galaxy Rotation (match star positions)
			if (galaxyTypeIndex == 0 || galaxyTypeIndex == 2){
				libComponent.starGalaxyObject.localEulerAngles = galaxyBasePosition;
				libComponent.starGalaxyObject.localScale = galaxyBaseScale;
			}

			// Custom Galaxy Rotation
			if (galaxyTypeIndex == 1){
				galaxyPosition.x = starPosition.x;
				galaxyPosition.y = starPosition.y;
				galaxyPosition.z = galaxyPos;
				libComponent.starGalaxyObject.localEulerAngles = galaxyPosition;
			}

	    }
	    galaxyColor = Color.white;
	   	galaxyVis = (1.0f-(colorSkyAmbient.r*1.0f)) * galaxyIntensity;

		if (Application.isPlaying){

		    libComponent.renderObjectGalaxy.material.SetFloat("_GIntensity",galaxyVis);
		    libComponent.renderObjectGalaxy.material.SetColor("_Color",galaxyColor);
			libComponent.renderObjectGalaxy.material.SetFloat("_useGlxy",galaxyTypeIndex*1.0f);
			libComponent.renderObjectGalaxy.material.SetTexture("_GTex",galaxyTex);
			libComponent.renderObjectGalaxy.material.SetFloat("_useCube",galaxyTexIndex*1.0f);
			libComponent.renderObjectGalaxy.material.SetTexture("_CubeTex",galaxyCubeTex);
			libComponent.renderObjectGalaxy.material.SetFloat("_useStar",starTypeIndex*1.0f);
		    libComponent.renderObjectGalaxy.material.SetFloat("_SIntensity",starIntensity);

		} else {
			libComponent.renderObjectGalaxy.sharedMaterial.SetFloat("_GIntensity",galaxyVis);
		    libComponent.renderObjectGalaxy.sharedMaterial.SetColor("_Color",galaxyColor);
			libComponent.renderObjectGalaxy.sharedMaterial.SetFloat("_useGlxy",galaxyTypeIndex*1.0f);
			libComponent.renderObjectGalaxy.sharedMaterial.SetTexture("_GTex",galaxyTex);
			libComponent.renderObjectGalaxy.sharedMaterial.SetFloat("_useCube",galaxyTexIndex*1.0f);
			libComponent.renderObjectGalaxy.sharedMaterial.SetTexture("_CubeTex",galaxyCubeTex);
			libComponent.renderObjectGalaxy.sharedMaterial.SetFloat("_useStar",starTypeIndex*1.0f);
		    libComponent.renderObjectGalaxy.sharedMaterial.SetFloat("_SIntensity",starIntensity);
		}

	    if (libComponent.starRenderSystem != null){
	    	if (starTypeIndex == 2){
				if (libComponent.starParticleSystem.enabled) libComponent.starParticleSystem.enabled = false;
		    } else {
		    	if (!libComponent.starParticleSystem.enabled) libComponent.starParticleSystem.enabled = true;
	    		libComponent.starRenderSystem.starBrightness.a = starIntensity;
		    }
	    }
	    
	    


	    // ---------------------------------
		// --   LIGHTNING HANDLING   ---
		// ---------------------------------

	 	//SET SKY CAMERA POSITIONS
		if (useCamera != null){
			//set global shader for camera forward direction
			Shader.SetGlobalVector("Tenkoku_Vec_CameraFwd", useCamera.forward);
			Shader.SetGlobalVector("Tenkoku_Vec_SunFwd", -libComponent.sunlightObject.forward); 
			Shader.SetGlobalVector("Tenkoku_Vec_MoonFwd", -libComponent.moonlightObject.forward);
		}	




	    // --------------
	    // --   SKY   ---   
	    //---------------                                                                                                                                                                                                                                                  
	    //get ramp colors
		ambientShift = Mathf.Clamp(ambientShift,0.0f,1.0f);
	    colorSkyBase = decodeColorSkybase;
	    ambientCol = decodeColorSkyambient;
	    ambientCloudCol = decodeColorAmbientcloud;           
	                   
		setSkyAmbient = colorSkyAmbient;
		setSkyAmbient.r += ambientShift;
		setSkyAmbient.g += ambientShift;
		setSkyAmbient.b += ambientShift;

		useAlpha.r = 1f;
		useAlpha.g = 1f;
		useAlpha.b = 1f;
		useAlpha.a = (1.0f-setSkyAmbient.r);

		setAmbCol = useAlpha;
		setAmbCol.a = useAlpha.a*0.75f;
		setAmbCol.a = useAlpha.a*1.5f;
		if (setAmbCol.a > 1.0f) setAmbCol.a = 1.0f;

		colorSkyBaseLow = colorSkyBase;
		colorHorizonLow = colorHorizon;
		
		colorSkyBase = (colorSkyBase * 0.8f);
		colorHorizon = Color.Lerp(colorHorizon,(colorSkyBase),0.9f)*1.1f;

		
		


	    // ------------------
	    // --   AMBIENT   ---   
	    //-------------------
		mixColor = colorSun;
		setAmbientColor.r = 1.0f-mixColor.r;
		setAmbientColor.g = 1.0f-mixColor.g;
		setAmbientColor.b = 1.0f-mixColor.b;
		
		useAmbientColor = Color.Lerp((Color.Lerp(colorSkyBase,setAmbientColor,0.7f)),Color.black,0.12f);
		useAmbientColor = Color.Lerp(useAmbientColor,medCol,0.5f) * colorSkyAmbient;
		useAmbientColor *= (sunBright) * skyBrightness;
		
		//HORIZON
	    colorHorizon = decodeColorColorhorizon*1.2f; 

	    //Set Overcast
	    useOvercastDarkening = weather_OvercastDarkeningAmt * weather_OvercastAmt;

	    //Set Ambient
		if (useAmbient){
			useAmbientColor = decodeColorAmbientcolor;
			useColorAmbient = colorAmbient;
			//useColorAmbient.a = colorAmbient.a * Mathf.Lerp(1.0f,0.0f,weather_OvercastAmt*4f);
			useColorAmbient.a = colorAmbient.a * Mathf.Lerp(1.0f,0.0f,useOvercastDarkening*4f);
			

			useAmbientColor = Color.Lerp(useAmbientColor,Color.gray,useOvercastDarkening*3f);
			useAmbientColor = Color.Lerp(useAmbientColor,useAmbientColor*colorAmbient,useColorAmbient.a);

			skyAmb = decodeColorAmbientcolor*0.6f;			
			skyAmb = Color.Lerp(skyAmb, Color.white * Mathf.Lerp(0.25f,1.75f,nightBrightness*setSkyAmbient.r), useOvercastDarkening*2f);

			gAmb = Color.Lerp( (baseGAmbColor*(nightBrightness*0.5f)), useAmbientColor, setSkyAmbient.r);
			gAmb = Color.Lerp(gAmb, gAmb * useColorAmbient, useColorAmbient.a);

			sAmb = skyAmb*(Mathf.Lerp( nightBrightness+Mathf.Lerp(0.25f,1.75f,nightBrightness),1f, setSkyAmbient.r));
			sAmb = Color.Lerp(sAmb, sAmb * useColorAmbient, useColorAmbient.a);

			eAmb = Color.Lerp(gAmb,sAmb,0.25f);
			eAmb = Color.Lerp(eAmb, eAmb * useColorAmbient, useColorAmbient.a);

			float eFac = Mathf.Lerp(0.1f,1.0f, eclipseFactor);
			eFac = RoundDigit(eFac, 3);

			if (ambientTypeIndex == 0){
				UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

				useAmbientIntensity = Mathf.Lerp(ambientNightAmt, ambientDayAmt, libComponent.lightObjectWorld.intensity);

				if (adjustOvercastAmbient){
					useAmbientIntensity *= Mathf.Lerp(1f, 2.5f, weather_OvercastAmt * 2f);
				}

				UnityEngine.RenderSettings.ambientIntensity = useAmbientIntensity;
			
			} else if (ambientTypeIndex == 1){	
				UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				UnityEngine.RenderSettings.ambientSkyColor = sAmb * eFac;
				UnityEngine.RenderSettings.ambientGroundColor = gAmb * eFac;
				UnityEngine.RenderSettings.ambientEquatorColor = eAmb * eFac;

			}else if (ambientTypeIndex == 2){
				UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				UnityEngine.RenderSettings.ambientSkyColor = sAmb * eFac;
			}

		}
		



	 
	    // ----------------------------------
	    // --      LENS FLARE EFFECTS       --
	    // -----------------------------------
		if (useSunFlare && flareObject != null){
			libComponent.lightObjectWorld.flare = flareObject;
		} else {
			libComponent.lightObjectWorld.flare = null;
		}

	    //set default flare
	    //sunFlare.color = Color.white;

	    //fade flare based on overcast
	    //sunFlare.color = Color.Lerp(sunFlare.color,new Color(0f,0f,0f,0f),weather_OvercastAmt*2.0f);
		//sunFlare.brightness = Mathf.Lerp(1.0f,0.0f,weather_OvercastAmt*2.0);

	    //fade flare based on gamma
	    //sunFlare.color *= Mathf.Lerp(0.25f,1.0f,isLin);
		//sunFlare.brightness *= Mathf.Lerp(0.25f,1.0f,isLin);

	    //fade flare based on brightness
	    //sunFlare.color *= Mathf.Lerp(0.0f,1.0f,sunBright*0.6f);
		//sunFlare.brightness *= Mathf.Lerp(0.0f,1.0f,sunBright*0.6f);

		//fade flare based on occlusion
		//(soon)





	    // --------------------------------
	    // --      WEATHER EFFECTS       --
	    // --------------------------------
	    if (Application.isPlaying){

		    //manage particle systems
			if (libComponent.rainSystem != null){

				#if UNITY_5_4_OR_NEWER
					rainEmission = libComponent.rainSystem.emission;
					rainEmission.enabled = true;
					rainCurve.constant = Mathf.Lerp(2,5000*precipQuality,weather_RainAmt);

					#if UNITY_5_5_OR_NEWER
						rainEmission.rateOverTime = rainCurve;
					#else
						rainEmission.rate = rainCurve;
					#endif

					if (libComponent.rainSystem.particleCount <= 10){
						libComponent.renderObjectRain.enabled = false;
					} else {
						libComponent.renderObjectRain.enabled = true;
					}
					//add force
					rainForces = libComponent.rainSystem.forceOverLifetime;
					rainForces.enabled = true;
					rainCurveForceX.constant = -weather_WindCoords.x * (weather_WindAmt*46f);
					rainForces.x = rainCurveForceX;
					rainCurveForceY.constant = weather_WindCoords.y * (weather_WindAmt*46f);
					rainForces.y = rainCurveForceY;
				#else
					libComponent.renderObjectRain.enabled = true;	
					libComponent.rainSystem.enableEmission = true;
					libComponent.rainSystem.emissionRate = Mathf.FloorToInt(Mathf.Lerp(0,5000*precipQuality,weather_RainAmt));
				#endif
			}

			if (libComponent.rainFogSystem != null){

				#if UNITY_5_4_OR_NEWER
					rainFogEmission = libComponent.rainFogSystem.emission;
					rainFogEmission.enabled = true;
					rainCurveFog.constant = Mathf.Lerp(2,800*precipQuality,weather_RainAmt);

					#if UNITY_5_5_OR_NEWER
						rainFogEmission.rateOverTime = rainCurveFog;
					#else
						rainFogEmission.rate = rainCurveFog;
					#endif

					if (libComponent.rainFogSystem.particleCount <= 20){
						libComponent.renderObjectRainFog.enabled = false;
					} else {
						libComponent.renderObjectRainFog.enabled = true;
					}
					//add force
					rainFogForces = libComponent.rainFogSystem.forceOverLifetime;
					rainFogForces.enabled = true;
					rainCurveFogForceX.constant = -weather_WindCoords.x * (weather_WindAmt*4f);
					rainFogForces.x = rainCurveFogForceX;
					rainCurveFogForceY.constant = weather_WindCoords.y * (weather_WindAmt*4f);
					rainFogForces.y = rainCurveFogForceY;
				#else
					libComponent.rainFogSystem.enableEmission = true;
					libComponent.renderObjectRainFog.enabled = true;
					libComponent.rainFogSystem.emissionRate = Mathf.FloorToInt(Mathf.Lerp(0,800*precipQuality,weather_RainAmt));
				#endif
			}


			if (libComponent.rainSplashSystem != null){

				#if UNITY_5_4_OR_NEWER
					splashEmission = libComponent.rainSplashSystem.emission;
					splashEmission.enabled = true;
					splashCurve.constant = Mathf.Lerp(50,800*precipQuality,weather_RainAmt);

					#if UNITY_5_5_OR_NEWER
						splashEmission.rateOverTime = splashCurve;
					#else
						splashEmission.rate = splashCurve;
					#endif

					if (libComponent.rainSplashSystem.particleCount <= 10){
						libComponent.renderObjectRainSplash.enabled = false;
					} else {
						libComponent.renderObjectRainSplash.enabled = true;
					}
				#else
					libComponent.rainSplashSystem.enableEmission = true;
					libComponent.renderObjectRainSplash.enabled = true;
					libComponent.rainSplashSystem.emissionRate = Mathf.FloorToInt(Mathf.Lerp(0,800*precipQuality,weather_RainAmt));
				#endif
			}


			if (libComponent.fogSystem != null){

				#if UNITY_5_4_OR_NEWER
					fogEmission = libComponent.fogSystem.emission;
					fogEmission.enabled = true;
					fogCurve.constant = Mathf.Lerp(2,1200*precipQuality,weather_FogAmt);

					#if UNITY_5_5_OR_NEWER
						fogEmission.rateOverTime = fogCurve;
					#else
						fogEmission.rate = fogCurve;
					#endif

					if (libComponent.fogSystem.particleCount <= 10){
						//renderObjectFog.enabled = false;
					} else {
						//renderObjectFog.enabled = true;
					}
					//add force
					fogForces = libComponent.fogSystem.forceOverLifetime;
					fogForces.enabled = true;
					fogCurveForceX.constant = -weather_WindCoords.x * (weather_WindAmt*6);
					fogForces.x = fogCurveForceX;
					fogCurveForceY.constant = weather_WindCoords.y * (weather_WindAmt*6);
					fogForces.y = fogCurveForceY;
				#else
					libComponent.fogSystem.enableEmission = true;
					//renderObjectFog.enabled = true;
					libComponent.fogSystem.emissionRate = Mathf.FloorToInt(Mathf.Lerp(0,1200*precipQuality,weather_FogAmt));
				#endif
			}
			
			if (libComponent.snowSystem != null){

				#if UNITY_5_4_OR_NEWER
					snowEmission = libComponent.snowSystem.emission;
					snowEmission.enabled = true;
					snowCurve.constant = Mathf.Lerp(2,1500*precipQuality,weather_SnowAmt);

					#if UNITY_5_5_OR_NEWER
						snowEmission.rateOverTime = snowCurve;
					#else
						snowEmission.rate = snowCurve;
					#endif

					if (libComponent.snowSystem.particleCount <= 10){
						libComponent.renderObjectSnow.enabled = false;
					} else {
						libComponent.renderObjectSnow.enabled = true;
					}
					//add force
					snowPosition.x = 0f;
					snowPosition.y = Mathf.Lerp(0.24f,0.14f,weather_WindAmt);
					snowPosition.z = 0f;

					snowForces = libComponent.snowSystem.forceOverLifetime;
					snowForces.enabled = true;
					snowCurveForceX.constant = -weather_WindCoords.x * (weather_WindAmt*2f);
					snowForces.x = snowCurveForceX;
					snowCurveForceY.constant = weather_WindCoords.y * (weather_WindAmt*2f);
					snowForces.y = snowCurveForceY;
				#else
					libComponent.snowSystem.enableEmission = true;
					libComponent.renderObjectSnow.enabled = true;
					libComponent.snowSystem.emissionRate = Mathf.FloorToInt(Mathf.Lerp(0,1500*precipQuality,weather_SnowAmt));
				#endif

			}
		
		}



	    //Overcast Calculations
	    useOvercast = Mathf.Clamp(Mathf.Lerp(-1.0f,1.0f,useOvercastDarkening),0.0f,1.0f);
		colorSkyBase = Color.Lerp(colorSkyBase,baseSkyCol*ambientCol.r,useOvercast);
		colorHorizon = Color.Lerp(colorHorizon,baseHorizonCol*ambientCol.r,useOvercast);



		//apply color and lighting
		//fxUseLight = 0.8f;//Mathf.Clamp(4.0f,0.1f,0.8f);
		lDiff = ((libComponent.lightObjectWorld.intensity - 0.0f)/0.68f);
		lDiff += Mathf.Lerp(0.0f,0.8f,useOvercast);
		fxUseLight = Mathf.Lerp(0.0f,libComponent.lightObjectWorld.intensity,lDiff);

	    rainCol = baseRainCol * fxUseLight;
	    rainCol.a = 0.12f;
		 
	    splashCol = Color.white * fxUseLight;
	    splashCol.a = 0.65f;
	    
	    rainfogCol = Color.white * fxUseLight;
	   	rainfogFac = Mathf.Lerp(0.004f,0.025f,Mathf.Clamp01((weather_RainAmt-0.5f)*2.0f));
	   	rainfogCol.a = rainfogFac;
	    
	    fogCol = fogColStart * fxUseLight;
	    fogFac = Mathf.Lerp(0.0f,0.04f,Mathf.Clamp01(weather_FogAmt));	
	    fogCol.a = fogFac;
	   
	    snowCol = baseSnowCol * fxUseLight;
	    snowCol.a = 0.45f;

		if (Application.isPlaying){
			libComponent.renderObjectRain.material.SetColor("_TintColor", rainCol * Mathf.Lerp(0.35f,1.0f,ambientCol.r));
			libComponent.renderObjectRainSplash.material.SetColor("_TintColor", splashCol);    
			libComponent.renderObjectRainFog.material.SetColor("_TintColor", rainfogCol);
			libComponent.renderObjectFog.material.SetColor("_TintColor",fogCol);
			libComponent.renderObjectSnow.material.SetColor("_TintColor",snowCol);
		} else {
			libComponent.renderObjectRain.sharedMaterial.SetColor("_TintColor", rainCol * Mathf.Lerp(0.35f,1.0f,ambientCol.r));
			libComponent.renderObjectRainSplash.sharedMaterial.SetColor("_TintColor", splashCol);    
			libComponent.renderObjectRainFog.sharedMaterial.SetColor("_TintColor", rainfogCol);
			libComponent.renderObjectFog.sharedMaterial.SetColor("_TintColor",fogCol);
			libComponent.renderObjectSnow.sharedMaterial.SetColor("_TintColor",snowCol);
		}

	    //clamp weather systems
	    if (libComponent.particleObjectRainFog != null){
			//if (libComponent.particleObjectRainFog.particleCount > 0.0f) ClampParticle("rain fog");
		}
		if (libComponent.particleObjectRainSplash != null){
			if (libComponent.particleObjectRainSplash.particleCount > 0.0f) ClampParticle("rain splash");
		}
		if (libComponent.particleObjectFog != null){
			//if (libComponent.particleObjectFog.particleCount > 0.0f) ClampParticle("fog");
		}



	    // --------------
	    // --   FOG   ---   
	    //---------------
		if (enableFog){


			//Calculate Auto Fog (Humidity Settings)
			if (autoFog && useCameraCam != null){


				float camDist = useCameraCam.farClipPlane;

				fogDistance = Mathf.Lerp(camDist, 500f, Mathf.Clamp01(weather_humidity*1f));
				fogAtmosphere = Mathf.Clamp(fogDistance * Mathf.Lerp(0.3f, 0.125f, weather_humidity), 40f, 5000f);
				fogDensity = Mathf.Clamp01(Mathf.Lerp(0.5f,1.25f, weather_humidity));
				fadeDistance = 1.0f - Mathf.Clamp01(weather_humidity * 4.0f);
				fogObscurance = Mathf.Clamp01(Mathf.Lerp(-0.75f, 1.25f, weather_humidity));

			}


			//Set Fog Settings
			Shader.SetGlobalFloat("_Tenkoku_FogStart",fogAtmosphere);
			Shader.SetGlobalFloat("_Tenkoku_FogEnd",fogDistance);
			Shader.SetGlobalFloat("_Tenkoku_FogDensity",fogDensity);

		    if (useCameraCam != null){
				//Shader.SetGlobalFloat("_Tenkoku_shaderDepth",fogDistance/useCameraCam.farClipPlane);
				Shader.SetGlobalColor("_Tenkoku_FogColor", colorFog);
				Shader.SetGlobalFloat("_Tenkoku_shaderDepth", useCameraCam.farClipPlane);
				Shader.SetGlobalFloat("_Tenkoku_FadeDistance", fadeDistance);
				Shader.SetGlobalFloat("_Tenkoku_FogObscurance", fogObscurance);
			}


		} else {
			Shader.SetGlobalFloat("_Tenkoku_FogDensity",0.0f);
		}

	        



	    // -------------------------------------
		// --   CALCULATE WIND COORDINATES   ---
		// -------------------------------------
		weather_WindCoords = TenkokuConvertAngleToVector(weather_WindDir);

		windVector.x = libComponent.tenkokuWindTrans.eulerAngles.x;
		windVector.y = weather_WindDir;
		windVector.z = libComponent.tenkokuWindTrans.eulerAngles.z;

		libComponent.tenkokuWindTrans.eulerAngles = windVector;

		libComponent.tenkokuWindObject.windMain = weather_WindAmt;
		libComponent.tenkokuWindObject.windTurbulence = Mathf.Lerp(0.0f,0.4f,weather_WindAmt);
		libComponent.tenkokuWindObject.windPulseMagnitude = Mathf.Lerp(0.0f,2.0f,weather_WindAmt);
		libComponent.tenkokuWindObject.windPulseFrequency = Mathf.Lerp(0.0f,0.4f,weather_WindAmt);





	    // ------------------------
	    // --   CLOUD SETTINGS   ---   
	    //-------------------------
	    

		//set legacy clouds
		#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
			
		#else
			useLegacyClouds = true;
		#endif


		//GLOBAL CLOUD SETTINGS -----
	    colorClouds = decodeColorColorcloud;  
	    colorClouds = Color.Lerp(colorClouds,colorClouds*colorOverlay,colorOverlay.a);

	    temperatureColor = temperatureGradient.Evaluate( weather_temperature / 120f );
		colorClouds = Color.Lerp(colorClouds,colorClouds*temperatureColor,temperatureColor.a);

		
		

	    

	   	colorHighlightClouds = decodeColorAmbientcloud;  
	    colorHighlightClouds = Color.Lerp(colorHighlightClouds, baseHighlightCloudCol ,useOvercastDarkening*2.0f) * skyAmbientCol;
	    colorHighlightClouds = Color.Lerp(colorHighlightClouds,colorHighlightClouds*colorOverlay,colorOverlay.a);

	    colorHighlightClouds = Color.Lerp(colorHighlightClouds,colorHighlightClouds*temperatureColor,temperatureColor.a);



		Shader.SetGlobalColor("_TenkokuCloudColor",colorClouds);
		Shader.SetGlobalColor("_TenkokuCloudAmbientColor",colorHighlightClouds);

		//Set Temporal FX
		if (Application.platform == RuntimePlatform.OSXEditor){
			useTemporalAliasing = false;
		}
		if (temporalFX != null){

			temporalFX.enabled = useTemporalAliasing;
			
			if (temporalBuffer != null){
				temporalBuffer.enabled = useTemporalAliasing;
			}
		}
		Shader.SetGlobalFloat("fullScreenTemporal", useFSAliasing ? 1.0f : 0.0f);


		//LEGACY CLOUD SETTINGS -----
		if (useLegacyClouds){
			libComponent.renderObjectCloudPlane.enabled = true;
			#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
			libComponent.renderObjectCloudSphere.enabled = false;
			#endif


			if (libComponent.renderObjectCloudPlane != null){



				if (cloudLinkToTime){

					currCoords.x = (systemTime * weather_WindCoords.x * 0.1f);
					currCoords.y = (systemTime * weather_WindCoords.y * 0.1f);

				   	windPosition.x = currCoords.x;
				   	windPosition.y = currCoords.y;
				   	windPosition.z = 0f;
				   	windPosition.w = 0f;
					Shader.SetGlobalColor("windCoords", windPosition);

		    		cloudOverRot += (systemTime * cloudRotSpeed * 2.1f);
					cloudOverRot = Mathf.Clamp(cloudOverRot,0.0f,1.0f);

					cloudSpeeds.r = 0.1f * cloudOverRot; //altostratus
					cloudSpeeds.g = 0.7f * cloudOverRot; //Cirrus
					cloudSpeeds.b = 1.0f * cloudOverRot; //cumulus
					cloudSpeeds.a = 2.0f * cloudOverRot; //overcast

					cloudTexScale.x = cloudSc;
					cloudTexScale.y = cloudSc;

				} else {

					Vector4 _CloudCoords = CloudsConvertAngleToVector(weather_WindDir);
					float cSpd = Mathf.Lerp(0.0f,0.001f, weather_cloudSpeed) * Time.timeScale;

					currCoords.x = currCoords.x + (_CloudCoords.x * cSpd);
					currCoords.y = currCoords.y + (_CloudCoords.z * cSpd);

				   	windPosition.x = currCoords.x;
				   	windPosition.y = currCoords.y;
				   	windPosition.z = 0f;
				   	windPosition.w = 0f;
					Shader.SetGlobalColor("windCoords", windPosition);

					cloudSpeeds.r = 0.1f;// * cloudOverRot; //altostratus
					cloudSpeeds.g = 0.7f;// * cloudOverRot; //Cirrus
					cloudSpeeds.b = 1.0f;// * cloudOverRot; //cumulus
					cloudSpeeds.a = 2.0f;// * cloudOverRot; //overcast

					cloudTexScale.x = cloudSc;
					cloudTexScale.y = cloudSc;
				}



				if (Application.isPlaying){
					libComponent.renderObjectCloudPlane.material.SetColor("_cloudSpd",cloudSpeeds);
			   		libComponent.renderObjectCloudPlane.material.SetTextureScale("_MainTex", cloudTexScale);
			   		libComponent.renderObjectCloudPlane.material.SetTextureScale("_CloudTexB", cloudTexScale);
			   		if (useCameraCam != null && libComponent.renderObjectCloudPlane != null){
			   			libComponent.renderObjectCloudPlane.material.SetFloat("_TenkokuDist",1200f);
			   		}
					libComponent.renderObjectCloudPlane.material.SetFloat("_sizeCloud",weather_cloudCumulusAmt);
					libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudS",weather_cloudAltoStratusAmt);
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudC",weather_cloudCirrusAmt);
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudO",Mathf.Clamp01(weather_OvercastAmt*2.0f));
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_cloudHeight",Mathf.Lerp(10.0f,70.0f,weather_cloudCumulusAmt));
	   			} else {
	   				libComponent.renderObjectCloudPlane.sharedMaterial.SetColor("_cloudSpd",cloudSpeeds);
			   		libComponent.renderObjectCloudPlane.sharedMaterial.SetTextureScale("_MainTex", cloudTexScale);
			   		libComponent.renderObjectCloudPlane.sharedMaterial.SetTextureScale("_CloudTexB", cloudTexScale);
			   		if (useCameraCam != null && libComponent.renderObjectCloudPlane != null){
			   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_TenkokuDist",1200f);
			   		}
					libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_sizeCloud",weather_cloudCumulusAmt);
					libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudS",weather_cloudAltoStratusAmt);
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudC",weather_cloudCirrusAmt);
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudO",Mathf.Clamp01(weather_OvercastAmt*2.0f));
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_cloudHeight",Mathf.Lerp(10.0f,70.0f,weather_cloudCumulusAmt));
	   			}

				//set cloud scale
				planeObjectScale.x = setSkyUseSize * 0.01f;
				planeObjectScale.y = setSkyUseSize * 0.01f;
				planeObjectScale.z = 0.1f;

				libComponent.cloudPlaneObject.localScale = planeObjectScale;
				cloudPosition.x = libComponent.cloudPlaneObject.localPosition.x;
				cloudPosition.y = -1.0f;
				cloudPosition.z = libComponent.cloudPlaneObject.localPosition.z;
				libComponent.cloudPlaneObject.localPosition = cloudPosition;
				cloudSc = weather_cloudScale * 6f;

				if (Application.isPlaying){
					libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudS",weather_cloudAltoStratusAmt);
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudC",weather_cloudCirrusAmt);
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_amtCloudO",Mathf.Clamp01(weather_OvercastAmt*2.0f));
		   			libComponent.renderObjectCloudPlane.material.SetFloat("_cloudHeight",Mathf.Lerp(10.0f,70.0f,weather_cloudCumulusAmt));
	   			} else {
					libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudS",weather_cloudAltoStratusAmt);
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudC",weather_cloudCirrusAmt);
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_amtCloudO",Mathf.Clamp01(weather_OvercastAmt*2.0f));
		   			libComponent.renderObjectCloudPlane.sharedMaterial.SetFloat("_cloudHeight",Mathf.Lerp(10.0f,70.0f,weather_cloudCumulusAmt));
	   			}
			}
		}
		


		//ADVANCED CLOUD SETTINGS (Requires Unity 5.3.4+) -----
		#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
		if (!useLegacyClouds){

			//Force Aurora object to off.
			//libComponent.renderObjectAurora.enabled = false;

			libComponent.renderObjectCloudPlane.enabled = false;
			libComponent.renderObjectCloudSphere.enabled = true;

			if (materialObjectCloudSphere != null){

				//set sphere size based on camera distance
				if (useCameraCam != null){
					csSize = ((useCameraCam.farClipPlane / 20f) * 1.75f);
					cSphereSize.x = csSize;
					cSphereSize.y = csSize;
					cSphereSize.z = csSize;
					libComponent.cloudSphereObject.localScale = cSphereSize;
				}

				//set sphere shader attributes
				cAmt = Mathf.Max(Mathf.Lerp(0.0f,0.75f,weather_cloudCumulusAmt), weather_OvercastAmt*4f);

				//Quality Settings
				materialObjectCloudSphere.SetFloat("_SampleCount0", Mathf.Lerp(32f, 8f, cAmt) * cloudQuality );
				materialObjectCloudSphere.SetFloat("_SampleCount1", Mathf.Lerp(32f, 8f, cAmt) * cloudQuality );
				materialObjectCloudSphere.SetFloat("_SampleCountL", Mathf.Lerp(8f, 4f, cAmt) * cloudQuality );
				materialObjectCloudSphere.SetFloat("_FarDist", Mathf.Lerp(5000f, 20000f, cloudQuality) );
				materialObjectCloudSphere.SetFloat("_Edge", Mathf.Lerp(0.95f, 0.5f, cloudQuality) );
				materialObjectCloudSphere.SetFloat("quality", cloudQuality );
				materialObjectCloudSphere.SetFloat("_brightness", cloudBrightness );
				materialObjectCloudSphere.SetFloat("_humid", weather_humidity );


				//Cloud Height
				materialObjectCloudSphere.SetFloat("_Altitude0",Mathf.Lerp(1400f,900f,cAmt));
				materialObjectCloudSphere.SetFloat("_Altitude1", Mathf.Lerp(1400f,3000f,cAmt));
				materialObjectCloudSphere.SetFloat("_Altitude2", 3500f);
				materialObjectCloudSphere.SetFloat("_Altitude3", 4000f);
				materialObjectCloudSphere.SetFloat("_Altitude4", 6500f);
				materialObjectCloudSphere.SetFloat("_Altitude5", 7000f);

				//Cloud Frequency
				materialObjectCloudSphere.SetFloat("_NoiseFreq1", Mathf.Lerp(1.0f*cloudFreq,5.5f*cloudFreq,cAmt));
				materialObjectCloudSphere.SetFloat("_NoiseFreq2", Mathf.Lerp(30f*cloudDetail,15f*cloudDetail,cAmt));

				//Cloud Scattering
				materialObjectCloudSphere.SetFloat("_Scatter", Mathf.Lerp(0.1f,0.5f,cAmt)); //0.07
				materialObjectCloudSphere.SetFloat("_HGCoeff", 0.0f); //0.3
				materialObjectCloudSphere.SetFloat("_Extinct", Mathf.Lerp(0.005f,0.005f,cAmt)); //0.003

				//Cloud Amount
				materialObjectCloudSphere.SetFloat("_NoiseBias", Mathf.Max(cAmt, weather_OvercastAmt*4f));
				materialObjectCloudSphere.SetFloat("_NoiseBias2", weather_cloudCirrusAmt);
				materialObjectCloudSphere.SetFloat("_NoiseBias3", weather_cloudAltoStratusAmt);

				

				//set direction and scale
				if (cloudLinkToTime){

					#if UNITY_EDITOR
					if (!EditorApplication.isCompiling){
					#endif
						
						Vector4 _CloudCoords = CloudsConvertAngleToVector(weather_WindDir);
						float cSpd = Mathf.Lerp(0.0f,0.05f, weather_cloudSpeed);

						_CloudCoords.x += _CloudCoords.x * (cSpd * systemTime);
						_CloudCoords.z += _CloudCoords.z * (cSpd * systemTime);

						materialObjectCloudSphere.SetVector("_Scroll1", _CloudCoords * 0.5f);
		       			materialObjectCloudSphere.SetVector("_Scroll2", _CloudCoords * 4f);

					#if UNITY_EDITOR
					}
					#endif

				} else {

					Vector4 _CloudCoords = CloudsConvertAngleToVector(weather_WindDir);
					float cSpd = Mathf.Lerp(0.0f,0.001f, weather_cloudSpeed) * Time.timeScale;
					
					weather_CloudCoords.x = weather_CloudCoords.x + (_CloudCoords.x * cSpd);
					weather_CloudCoords.z = weather_CloudCoords.z + (_CloudCoords.z * cSpd);

					materialObjectCloudSphere.SetFloat("_tenkokuTimer", 1f);
					materialObjectCloudSphere.SetVector("_Scroll1", weather_CloudCoords * 0.5f);
	       			materialObjectCloudSphere.SetVector("_Scroll2", weather_CloudCoords * 4f);


				}

				//Set Noise Timer
				// We do this in order to ensure the noise optimizations on the cloud shader do not
				// stop calculating if the Unity.timeScale is set to 0.
				materialObjectCloudSphere.SetFloat("_tenkokuNoiseTimer", Time.realtimeSinceStartup);



				#if UNITY_EDITOR
				if (!EditorApplication.isCompiling){
					if (recompileFlag){
						recompileFlag = false;
						cloudLinkToTime = saveLinkToTime;
					} else {
						saveLinkToTime = cloudLinkToTime;
					}
				}
				#endif


	       		Shader.SetGlobalFloat("_cS",weather_cloudScale/8);


				//Darkness
				materialObjectCloudSphere.SetFloat("_Darkness", Mathf.Clamp(Mathf.Lerp(0.5f,0.5f,cAmt),0f,1f)); //2
			}



		}
		#endif



		// -------------------------------
	    // --   Set Global Tint Color   --
	    // -------------------------------
	    Shader.SetGlobalColor("_TenkokuSkyColor", decodeColorSkybase);
	    Shader.SetGlobalColor("tenkoku_globalTintColor",colorOverlay * Color.Lerp(Color.white, temperatureColor, temperatureColor.a));
	    Shader.SetGlobalColor("tenkoku_globalSkyColor",colorSky);


	 	// -------------------------------
	    // --   Altitude Adjustment   --
	    // -------------------------------
	    // adjust colors and alpha based on altitude height attributes.
	    // can simulate entering and leaving planet atmosphere;
	    altAdjust = 5000.0f;
	    Shader.SetGlobalFloat("tenkoku_altitudeAdjust",altAdjust);
	    
	    

		// -------------------------
	    // --   BRDF Sky Object   --
	    // -------------------------
	    if (useCamera != null){
	    if (useCameraCam != null){
	    	fogDist = (1.0f/useCameraCam.farClipPlane)*(0.9f + fogDistance);
	    	fogFull = useCameraCam.farClipPlane;
	    }
		}
		colorHorizon = Color.Lerp(colorHorizon,colorHorizon*0.5f,useOvercastDarkening);
		skyAmbientCol = ambientCol * Mathf.Lerp(1.0f,0.7f,useOvercast);
		skyHorizonCol = colorHorizon * Mathf.Lerp(1.0f,0.8f,useOvercast);

		Shader.SetGlobalColor("_Tenkoku_SkyHorizonColor",skyHorizonCol);
		Shader.SetGlobalFloat("_tenkokufogFull",floatRound(fogFull));
		Shader.SetGlobalFloat("_tenkokufogStretch",floatRound(fogDist));
		Shader.SetGlobalFloat("_tenkokufogAtmospheric",floatRound(fogAtmosphere));
		Shader.SetGlobalFloat("_Tenkoku_SkyBright",floatRound(useSkyBright));
		Shader.SetGlobalFloat("_Tenkoku_NightBright",floatRound(nightBrightness));
		Shader.SetGlobalColor("_TenkokuAmbientColor",skyAmbientCol);

		//adjust and set atmospheric density based on day factor
		atmosTime = calcTime;
		atmosTime = Mathf.Clamp(Mathf.Lerp(-0.75f,1.75f,(atmosTime)),0.0f,0.5f);

		atmosphereDensity = Mathf.Clamp(atmosphereDensity,0.5f,4.0f);
		setAtmosphereDensity = (horizonDensity * 2.0f * atmosTime);

		//if (atmosphereModelTypeIndex == 0){
			Shader.SetGlobalFloat("_Tenkoku_AtmosphereDensity",floatRound(atmosphereDensity));
			Shader.SetGlobalFloat("_Tenkoku_HorizonDensity",floatRound(setAtmosphereDensity));
			Shader.SetGlobalFloat("_Tenkoku_HorizonHeight",floatRound(horizonDensityHeight));
		//}


		if (enableFog){
			Shader.SetGlobalFloat("_Tenkoku_FogDensity",floatRound(fogDensity));
		}

		//overcast color
		overcastCol = decodeColorSkyambient;//decodeColorAmbientcolor;
		overcastCol.a = useOvercastDarkening;
		Shader.SetGlobalColor("_Tenkoku_overcastColor", overcastCol);
		Shader.SetGlobalFloat("_Tenkoku_overcastAmt",weather_OvercastAmt);


	    // assign colors
	    if (useCamera != null){
		    if (useCameraCam != null){
		    	useCameraCam.backgroundColor = decodeColorSkybase*Color.Lerp(Color.white,Color.black,atmosphereDensity*0.25f);
		    	useCameraCam.backgroundColor = Color.Lerp(Color.black,useCameraCam.backgroundColor,Mathf.Clamp(atmosphereDensity*2f,0.0f,1.0f));
		    	useCameraCam.backgroundColor *= Mathf.Clamp(skyBrightness,0.0f,1.0f);
			}
			nightSkyPosition.x = 60.0f;
			nightSkyPosition.y = useCamera.eulerAngles.y;
			nightSkyPosition.z = useCamera.eulerAngles.z;
		    libComponent.nightSkyLightObject.eulerAngles = nightSkyPosition;
		}

	    if (libComponent.fogCameraCam != null){
	    	libComponent.fogCameraCam.backgroundColor = decodeColorSkybase*Color.Lerp(Color.white,Color.black,atmosphereDensity*0.25f);
	    	libComponent.fogCameraCam.backgroundColor = Color.Lerp(Color.black,libComponent.fogCameraCam.backgroundColor,Mathf.Clamp(atmosphereDensity*2f,0.0f,1.0f));
	    	libComponent.fogCameraCam.backgroundColor *= Mathf.Clamp(skyBrightness,0.0f,1.0f);
		}










	    setOverallCol = colorSun*ambientCol*Color.Lerp(Color.white,colorOverlay,colorOverlay.a);
	    setOverallCol = Color.Lerp(setOverallCol,setOverallCol * temperatureColor,temperatureColor.a);

	    

		Shader.SetGlobalColor("_overallCol",setOverallCol);

		bgSCol = skyHorizonCol;
	    // convert camera color to linear space when appropriate
	    // the scene camera does not render background colors in linear space, so we need to
	    // convert the background color to linear before assigning to in-scene shaders.
	    if (_linearSpace == ColorSpace.Linear){
		    bgSCol.r = Mathf.GammaToLinearSpace(bgSCol.r);
		    bgSCol.g = Mathf.GammaToLinearSpace(bgSCol.g);
		    bgSCol.b = Mathf.GammaToLinearSpace(bgSCol.b);
		}
		Shader.SetGlobalColor("tenkoku_backgroundColor",bgSCol);

		
		//set GI Ambient
		ambientGI = decodeColorAmbientGI;
		

		//handle gamma brightness
		useSkyBright = skyBrightness;
		if (_linearSpace != ColorSpace.Linear){
			useSkyBright *= 0.42f;
		}

		//assign blur settings
	    //if (fogCameraBlur != null){
			libComponent.fogCameraBlur.blurSpread = 1;//0.1f;
		//}

		useSkyBright = useSkyBright * Mathf.Lerp(1.0f,10.0f,ambientGI.r);

		// ------------------------
	    // --   Cloud Objects   --
	    // ------------------------
	    Shader.SetGlobalColor("_TenkokuCloudColor",colorClouds);
	    Shader.SetGlobalColor("_TenkokuCloudHighlightColor",colorHighlightClouds);

		//set cloud effect distance color
		Shader.SetGlobalFloat("_fogStretch",floatRound(fogDist));
		Shader.SetGlobalColor("_Tenkoku_SkyColor",colorSkyBase*ambientCloudCol.r*setOverallCol*useSkyBright);
		Shader.SetGlobalColor("_Tenkoku_HorizonColor",colorHorizon*setOverallCol*useSkyBright);
		Shader.SetGlobalFloat("_Tenkoku_Ambient",ambientCloudCol.r);
		Shader.SetGlobalFloat("_Tenkoku_AmbientGI",ambientGI.r);
		
		//set aurora settings
		auroraIsVisible = false;
		if (auroraTypeIndex == 0){
			auroraIsVisible = true;
		}

		
		if (!auroraIsVisible || auroraLatitude > Mathf.Abs(setLatitude)){
			if (Application.isPlaying){
				libComponent.renderObjectAurora.enabled = false;
			}
		} else {

			//if (useLegacyClouds){
		 		libComponent.renderObjectAurora.enabled = true;
		 	//}

		 	//csSize = ((useCameraCam.farClipPlane / 20f) * 1.75f);
			//cSphereSize.x = csSize;
			//cSphereSize.y = csSize;
			//cSphereSize.z = csSize;
			//libComponent.cloudSphereObject.localScale = cSphereSize;




			Vector3 auroraScale = Vector3.zero;
			Vector3 auroraPos = Vector3.zero;
			//if (!useLegacyClouds){
			//	auroraPos.y = 3f;
			//	auroraScale.x = 0.6f;
			//	auroraScale.y = 0.6f;
			//	auroraScale.z = 0.1f;

			//} else {
			//	auroraPos.y = 3f;
			//	auroraScale.x = 0.6f;
			//	auroraScale.y = 0.6f;
			//	auroraScale.z = 0.1f;
			//}

			if (useCameraCam != null){
				csSize = ((useCameraCam.farClipPlane / 20f) * 1.75f);
				auroraScale.x = csSize + 1.0f;
				auroraScale.y = csSize + 1.0f;
				auroraScale.z = csSize + 1.0f;
			}

		 	libComponent.auroraObjectTransform.localPosition = auroraPos;
		 	libComponent.auroraObjectTransform.localScale = auroraScale;

		}

		aurSpan = 30.0f;
		aurAmt = Mathf.Lerp(0.0f,1.0f,Mathf.Clamp((Mathf.Abs(setLatitude)-auroraLatitude)/aurSpan,0.0f,1.0f));
		Shader.SetGlobalFloat("_Tenkoku_AuroraSpd",auroraSpeed);
		Shader.SetGlobalFloat("_Tenkoku_AuroraAmt",auroraIntensity*aurAmt);	

	    

	    // ----------------------------
	    // --   WORLD LIGHT OBJECT   --
	    // ----------------------------
	 	libComponent.worldlightObject.rotation = libComponent.sunlightObject.rotation;

		//Clamp light height
		if (minimumHeight > 0f){
			if (libComponent.worldlightObject.eulerAngles.x < minimumHeight){
				lightClampVector = libComponent.worldlightObject.eulerAngles;
				lightClampVector.x = minimumHeight;
				libComponent.worldlightObject.eulerAngles = lightClampVector;
			}
		}

	 	ambientBaseCol.r = ambientCol.r;
	 	ambientBaseCol.g = ambientCol.r;
	 	ambientBaseCol.b = ambientCol.r;
	 	ambientBaseCol.a = 1f;
	 	WorldCol = Color.Lerp(decodeColorSun * ambientCol.r, ambientBaseCol*0.7f,Mathf.Clamp01(useOvercastDarkening*2.0f));

		//tint light color with overall color
		WorldCol = WorldCol * Color.Lerp(Color.white,colorOverlay,colorOverlay.a);
		WorldCol = WorldCol * Color.Lerp(Color.white,Color.Lerp(Color.white,temperatureColor,0.5f),temperatureColor.a);


		sC = WorldCol.grayscale;
		sCol.r = sC;
		sCol.g = sC;
		sCol.b = sC;
		sCol.a = WorldCol.a;
		WorldCol = Color.Lerp(sCol, WorldCol, sunSat);

	 	//lightObjectWorld.color = WorldCol;
	    //libComponent.lightObjectWorld.intensity = 2.0f * (sunBright*0.68f) * ambientCol.r;   
	   	//lightObjectWorld.shadowStrength = Mathf.Lerp(1.0f,0.25f,weather_OvercastAmt);

	   	//WorldCol.a = lightObjectWorld.intensity;
	   	libComponent.lightObjectWorld.intensity = WorldCol.a * 2.0f * (sunBright*0.68f) * ambientCol.r;

	   	libComponent.lightObjectWorld.color = WorldCol;
		Shader.SetGlobalColor("_TenkokuSunColor",WorldCol);

//Debug.Log(WorldCol.a);



	    // ------------------------------
	    // --   HANDLE SUN RAY EFFECTS   --
	    // ------------------------------
	    if (useCamera != null){

			rayCol = colorSun;
			baseRayCol.r = rayCol.r;
			baseRayCol.g = rayCol.r;
			baseRayCol.b = rayCol.r;
			baseRayCol.a = rayCol.a;
			rayCol = Color.Lerp(rayCol,baseRayCol, Mathf.Clamp01(weather_OvercastAmt*2f));

			if (sunRayObject != null){
				sunRayObject.enabled = useSunRays;
				sunRayObject.sunTransform = libComponent.sunlightObject;
				sunRayObject.sunColor = rayCol;
				//sunRayObject.tintColor = rayCol;
				sunRayObject.sunShaftIntensity = Mathf.Lerp(0.0f,20.0f,sunRayIntensity) * eclipseFactor;
				sunRayObject.maxRadius = sunRayLength * eclipseFactor;
			}
	    }





	    // ----------------------------
	    // --   NIGHT LIGHT OBJECT   --
	    // ----------------------------
	 	libComponent.nightSkyLightObject.rotation = libComponent.moonlightObject.rotation;

		//Clamp light height
		if (minimumHeight > 0f){
			if (libComponent.nightSkyLightObject.eulerAngles.x < minimumHeight){
				lightClampVector = libComponent.nightSkyLightObject.eulerAngles;
				lightClampVector.x = minimumHeight;
				libComponent.nightSkyLightObject.eulerAngles = lightClampVector;
			}
		}

	    //Set night Color
		mC = mCol.grayscale;
		mCol.r = mC;
		mCol.g = mC;
		mCol.b = mC;

		libComponent.lightObjectNight.color = Color.Lerp(mCol, mColMixCol, moonSat);


	    //Set base night intensity
		libComponent.lightObjectNight.intensity = Mathf.Lerp(0.0f,1.0f,(nightBrightness)*1.2f)+(moonBright) * (1.0f-useOvercastDarkening);
		

		//modulate night intensity based on altitude
		if (libComponent.lightObjectNight != null && libComponent.moonSphereObject != null){

			//find the light factor based on the rotation of the light
			delta = libComponent.moonSphereObject.localPosition - libComponent.moonObject.position;
			look = Quaternion.LookRotation(delta);
			vertical = look.eulerAngles.x;

			if (vertical > 90.0f) vertical = 90f - (vertical-90.0f);
			vertical = Mathf.Clamp01((vertical) / 10.0f); //degrees

			//set the light intensity
			libComponent.lightObjectNight.intensity = libComponent.lightObjectNight.intensity * Mathf.Lerp(0.0f,1.0f,vertical);

		}

		//modulate night intensity based on lunar phase
		//moon phase 0.0 = new moon, 0.5 = full moon, 1.0 = new moon.
		if (libComponent.lightObjectNight != null && libComponent.moonSphereObject != null){
			libComponent.lightObjectNight.intensity = Mathf.Clamp(libComponent.lightObjectNight.intensity * Mathf.Lerp(2.0f, -0.3f, Mathf.Clamp01(Mathf.Sin(moonPhase))), 0f, libComponent.lightObjectNight.intensity);
		}

		//modulate night intensity based on sun intensity
		if (libComponent.lightObjectNight != null && libComponent.lightObjectWorld != null){
			libComponent.lightObjectNight.intensity = libComponent.lightObjectNight.intensity * Mathf.Clamp01(Mathf.Lerp(2.0f,0.0f,libComponent.lightObjectWorld.intensity));
		}




	    // ----------------------------
	    // --   FILL LIGHT OBJECT   --
	    // ----------------------------
		fillFac = libComponent.worldlightObject.eulerAngles.x / 90f;
		if (fillFac > 1f) fillFac = 0f;

		fillIntensity = Mathf.Lerp(0.0f, 3.0f, nightBrightness);

		libComponent.lightObjectFill.intensity = Mathf.Clamp01(Mathf.Lerp(fillIntensity, -2.0f, fillFac));
		if (libComponent.lightObjectFill.intensity > 0f){
			libComponent.lightObjectFill.enabled = true;
		} else {
			libComponent.lightObjectFill.enabled = false;
		}




	    // --------------------------
	    // --   DAY LIGHT OBJECT   --
	    // --------------------------
		//modulate day intensity based on altitude
		if (libComponent.lightObjectWorld != null && libComponent.sunSphereObject != null){

			//find the light factor based on the rotation of the light
			delta = libComponent.sunSphereObject.localPosition - libComponent.sunObject.position;
			look = Quaternion.LookRotation(delta);
			vertical = look.eulerAngles.x;
			
			if (vertical > 90.0f) vertical = 90f - (vertical-90.0f);
			vertical = Mathf.Clamp01(vertical / 2.0f); //degrees

		}






	    // ---------------------------------
		// --   SOLAR ECLIPSE HANDLING   ---
		// ---------------------------------
		//ecldiff = 0.0f;
		eclDelay += Time.deltaTime;
/*
		if (totalEclipse){
			ecldiff += _deltaTime * 0.01f;
			ecldiff = Mathf.Clamp01(ecldiff);
			eclipseFactor = Mathf.Clamp01(Mathf.Lerp(1.4f,0.0f,ecldiff));

			if (eclDelay > 0.5f){
				float ecldiffTest = eclAccumulation / (float)eclAccumCount;
				eclAccumCount = 0;
				eclAccumulation = 0f;
				eclDelay = 0f;

				if (ecldiff > 0.96f) totalEclipse = false;
			}

		}
*/
		if (eclDelay > 0.5f){

			//ecldiff = eclAccumulation / (float)eclAccumCount;
			eclTemp = eclAccumulation / (float)eclAccumCount;
			eclAccumCount = 0;
			eclAccumulation = 0f;
			eclDelay = 0f;

			eclTemp = Mathf.Clamp01(eclTemp);

			//if (!totalEclipse) eclTarget = eclTemp;

			//eclipseFactor = Mathf.Clamp01(Mathf.Lerp(1.4f,0.0f,ecldiff));
			if (totalEclipse && eclTemp < 0.96f) totalEclipse = false;
			if (!totalEclipse && eclTemp > 0.965f) totalEclipse = true;
			

		} else {
			//Accumulate and average eclipse positions
			float eclSampleAccum = (1f - Vector3.Angle(libComponent.moonObject.position, libComponent.sunObject.position));
			//eclSampleAccum = RoundDigit(eclSampleAccum, 1);

			if (eclSampleAccum > 0.90f && eclSampleAccum < 0.93f) eclSampleAccum = 0.90f;
			if (eclSampleAccum > 0.93f && eclSampleAccum < 0.96f) eclSampleAccum = 0.93f;
			if (eclSampleAccum > 0.96f && eclSampleAccum < 0.97f) eclSampleAccum = 0.96f;
			//if (!totalEclipse && (eclSampleAccum - eclTemp) > 0.1f) eclSampleAccum = eclTemp;
			//if (totalEclipse && eclSampleAccum >= 0.96f) eclSampleAccum = 1.5f;

			//if (eclSampleAccum > 0.90f && eclSampleAccum < 0.93f) eclSampleAccum = 0.90f;

			//if (eclSampleAccum >= 0.9f){
			//	float eclFac = 1f-((1f - eclSampleAccum) * 10f);
			//	eclSampleAccum = eclSampleAccum * Mathf.Lerp(1.0f, 10.0f, eclFac);
			//}
			//eclSampleAccum = Mathf.Clamp01(eclSampleAccum);

			eclAccumulation += eclSampleAccum;
			eclAccumCount += 1;
		}


		if (totalEclipse){
			eclTarget = 1.0f;

			if (totalEclipseFactor < 1f) totalEclipseFactor += Time.deltaTime;
			totalEclipseFactor = Mathf.Clamp01(totalEclipseFactor);
		} else {
			eclTarget = eclTemp;
			if (totalEclipseFactor > 0f) totalEclipseFactor -= Time.deltaTime;
			totalEclipseFactor = Mathf.Clamp01(totalEclipseFactor);
		}


		//Warp Eclipse Factor
		if (ecldiff != eclTarget){
			eclWarpTime += Time.deltaTime * 1.0f;
			ecldiff = Mathf.Lerp(ecldiff, eclTarget, eclWarpTime);
		} else {
			eclWarpTime = 0f;
		}
		eclipseFactor = Mathf.Clamp01(Mathf.Lerp(1.4f,0.0f,ecldiff));

		//Warp Mie via Eclipse Factor
		float mieEclFac = 1f;
		mieEclFac = Mathf.Lerp(1.0f, 0.0f, Mathf.Clamp01((1f - eclipseFactor) * 10f));
		mieEclFac = Mathf.Lerp(mieEclFac, 0.0f, totalEclipseFactor);
		useMieMnAmount = mieMnAmount * mieEclFac;
		useMieAmount = mieAmount * mieEclFac;

if (float.IsNaN(eclipseFactor)) eclipseFactor = 1f;

		Shader.SetGlobalFloat("_Tenkoku_EclipseFactor", eclipseFactor);
		Shader.SetGlobalFloat("_Tenkoku_TotalEclipseFactor", 1f-totalEclipseFactor);
		libComponent.lightObjectWorld.intensity *= Mathf.Max(eclipseFactor,0.1f);
		if (libComponent.eclipseObject != null && totalEclipseFactor > 0.0f){
			eclipsePosition.x = libComponent.eclipseObject.eulerAngles.x;
			eclipsePosition.y = libComponent.eclipseObject.eulerAngles.y;
			eclipsePosition.z = libComponent.eclipseObject.eulerAngles.z+_deltaTime*4.0f;
			libComponent.eclipseObject.eulerAngles = eclipsePosition;

			_tempFac = Mathf.Lerp(1f,7f,totalEclipseFactor);
			_tempVec.x = _tempFac;
			_tempVec.y = _tempFac;
			_tempVec.z = _tempFac;
			libComponent.eclipseObject.localScale = _tempVec;
		}





		//handle gamma world lighting brightness
		if (_linearSpace != ColorSpace.Linear){
			libComponent.lightObjectWorld.intensity = libComponent.lightObjectWorld.intensity*0.8f;
		}




		//Handle Multi-Lighting
		if (allowMultiLights){
			libComponent.lightObjectWorld.intensity = Mathf.Clamp(libComponent.lightObjectWorld.intensity,0.001f,8.0f);
			libComponent.lightObjectWorld.renderMode = LightRenderMode.ForcePixel;
			libComponent.lightObjectNight.renderMode = LightRenderMode.ForcePixel;
			//lightObjectNight.shadows = LightShadows.Soft;

			//enable/disable lights
			if (libComponent.lightObjectWorld.intensity < 0.001f){
				libComponent.lightObjectWorld.enabled = false;
			} else {
				libComponent.lightObjectWorld.enabled = true;
			}
			//lightObjectWorld.enabled = true;

			if (libComponent.lightObjectNight.intensity <= 0.05f){
				libComponent.lightObjectNight.enabled = false;
			} else {
				libComponent.lightObjectNight.enabled = true;
			}


		} else {
			
			libComponent.lightObjectWorld.intensity = Mathf.Clamp(libComponent.lightObjectWorld.intensity,0.001f,8.0f);
			libComponent.lightObjectWorld.renderMode = LightRenderMode.ForcePixel;
			libComponent.lightObjectNight.renderMode = LightRenderMode.ForcePixel;
			//libComponent.lightObjectNight.shadows = LightShadows.Soft;

			//enable/disable lights
			if (libComponent.lightObjectWorld.intensity <= 0.1f){
				libComponent.lightObjectWorld.enabled = false;
			} else {
				libComponent.lightObjectWorld.enabled = true;
			}

			if (libComponent.lightObjectWorld.enabled){
				libComponent.lightObjectNight.enabled = false;
			} else {
				libComponent.lightObjectNight.enabled = true;
			}


		}

		Shader.SetGlobalColor("_Tenkoku_Daylight",libComponent.lightObjectWorld.color * (libComponent.lightObjectWorld.intensity*0.5f) * (calcTime));
		Shader.SetGlobalColor("_Tenkoku_Nightlight",libComponent.lightObjectNight.color * (libComponent.lightObjectNight.intensity + nightBrightness) * (1.0f-calcTime) * 2f);



	}






	void CalculateWeatherRandom(){

		//calculate weather pattern timer and initialize update
		doWeatherUpdate = false;
		weather_PatternTime += _deltaTime;
				
		//inherit variables
		if (currentWeatherTypeIndex != weatherTypeIndex ){
			currentWeatherTypeIndex = weatherTypeIndex;
			//doWeatherUpdate = true;
			weatherHasStarted = false;
		}

		//check for weather update
		weather_autoForecastTime = Mathf.Max(weather_autoForecastTime,0.01f);
		weather_TransitionTime = Mathf.Max(weather_TransitionTime, 0.001f);
		if (weather_PatternTime > (weather_autoForecastTime*60.0f) && weather_autoForecastTime >= 0.05f) doWeatherUpdate = true;
		if (weather_forceUpdate) doWeatherUpdate = true;
		
		if (doWeatherUpdate && weatherHasStarted){
			weather_forceUpdate = false;
			weather_PatternTime = 0.0f;
			
			//determine next random weather pattern
			w_isCloudy = Mathf.Clamp(TenRandom.Next(-0.25f,1.5f),0.0f,1.0f);
			
			//record current weather
			w_cloudAmts.y = weather_cloudAltoStratusAmt;
			w_cloudAmts.z = weather_cloudCirrusAmt;
			w_cloudAmtCumulus = weather_cloudCumulusAmt;
			w_overcastAmt = weather_OvercastAmt;
			//w_windCAmt = weather_cloudSpeed;
			w_windAmt = weather_WindAmt;
			w_rainAmt = weather_RainAmt;
			w_humidityAmt = weather_humidity;
			w_temperatureAmt = weather_temperature;
			w_lightningAmt = weather_lightning;

			//set clear weather default
			w_cloudTgts = Vector4.zero;
			w_cloudTgtCumulus = 0.0f;
			w_overcastTgt = 0.0f;
			//w_windCTgt = 0.0f;
			w_windTgt = 0.0f;
			w_rainTgt = 0.0f;
			w_humidityTgt = 0.0f;
			w_lightningTgt = 0.0f;
			w_temperatureTgt = 75.0f;

			//set clouds
			w_cloudTgts.y = Mathf.Clamp(Mathf.Lerp(0.0f,TenRandom.Next(0.0f,0.4f),w_isCloudy),0.0f,0.4f); //AltoCumulus
			w_cloudTgts.z = Mathf.Clamp(Mathf.Lerp(0.0f,TenRandom.Next(0.0f,0.8f),w_isCloudy),0.0f,0.8f); //Cirrus
			w_cloudTgtCumulus = Mathf.Clamp(Mathf.Lerp(0.0f,TenRandom.Next(0.1f,1.25f),w_isCloudy),0.0f,1.0f); // Cumulus
			if (w_cloudTgtCumulus > 0.8f){
				w_overcastTgt = Mathf.Clamp(Mathf.Lerp(0.0f,TenRandom.Next(-1.0f,1.0f),w_isCloudy),0.0f,1.0f); // overcast amount
			}
			
			//set weather
			chanceOfRain = Mathf.Clamp01(TenRandom.Next(-1.0f,1.0f));
			w_humidityTgt = Mathf.Clamp01(TenRandom.Next(0.0f,1.0f));
			w_rainTgt = Mathf.Lerp(0.0f,TenRandom.Next(0.2f,1.4f),w_overcastTgt*chanceOfRain);
			w_lightningTgt = Mathf.Lerp(0.0f,TenRandom.Next(0.0f,0.6f),w_overcastTgt*chanceOfRain);

			//Calculate temperature
			float temperatureAvg = 70.0f;
			//float temperatureMinFlux = 2.0f;
			//float temperatureMaxFlux = 10.0f;
			//float temperatureFlux = Mathf.Lerp(temperatureMinFlux, temperatureMaxFlux, TenRandom.Next(0.0f, 1.0f));
			w_temperatureTgt = temperatureAvg;// + (TenRandom.Next(-1.0f,1.0f) * temperatureFlux);

			//set wind
			//w_windCTgt = TenRandom.Next(0.1f,0.3f)*0.1f;
			w_windTgt = TenRandom.Next(0.1f,0.5f)+Mathf.Lerp(0.0f,0.5f,weather_OvercastAmt);
			
		}


		//Set Default Targets
		if (!weatherHasStarted){
			w_cloudAmts.y = weather_cloudAltoStratusAmt;
			w_cloudAmts.z =  weather_cloudCirrusAmt;
			w_cloudAmtCumulus = weather_cloudCumulusAmt;
			w_overcastAmt = weather_OvercastAmt;
			w_windAmt = weather_WindAmt;
			w_rainAmt = weather_RainAmt;
			w_humidityAmt = weather_humidity;
			w_lightningAmt = weather_lightning;
			w_temperatureAmt = weather_temperature;

			w_cloudTgts.y = weather_cloudAltoStratusAmt;
			w_cloudTgts.z =  weather_cloudCirrusAmt;
			w_cloudTgtCumulus = weather_cloudCumulusAmt;
			w_overcastTgt = weather_OvercastAmt;
			w_windTgt = weather_WindAmt;
			w_rainTgt = weather_RainAmt;
			w_humidityTgt = weather_humidity;
			w_lightningTgt = weather_lightning;
			w_temperatureTgt = weather_temperature;
		}

		//set weather systems
		float patternTime = (weather_PatternTime/(weather_TransitionTime*60.0f));
		weather_cloudAltoStratusAmt = Mathf.SmoothStep(w_cloudAmts.y,w_cloudTgts.y, patternTime);
		weather_cloudCirrusAmt = Mathf.SmoothStep(w_cloudAmts.z,w_cloudTgts.z, patternTime);
		weather_cloudCumulusAmt = Mathf.SmoothStep(w_cloudAmtCumulus,w_cloudTgtCumulus, patternTime);
		weather_OvercastAmt = Mathf.SmoothStep(w_overcastAmt,w_overcastTgt, patternTime);
		//weather_cloudSpeed = Mathf.SmoothStep(w_windCAmt,w_windCTgt, patternTime);
		weather_WindAmt = Mathf.SmoothStep(w_windAmt,w_windTgt, patternTime);
		weather_RainAmt = Mathf.SmoothStep(w_rainAmt,w_rainTgt, patternTime*2.0f);
		weather_lightning = Mathf.SmoothStep(w_lightningAmt,w_lightningTgt, patternTime*2.0f);
		weather_humidity = Mathf.SmoothStep(w_humidityAmt,w_humidityTgt, patternTime*2.0f);

		weather_temperature = Mathf.SmoothStep(w_temperatureAmt,w_temperatureTgt, patternTime);

		//extra modifiers: OVERCAST
		weather_RainAmt *= Mathf.Lerp(-1.0f,1.0f,weather_OvercastAmt);
		weather_lightning *= Mathf.Lerp(-3.0f,0.5f,weather_OvercastAmt);
		volumeAmbDay *= Mathf.Lerp(1.0f,-2.0f,weather_OvercastAmt);

		weatherHasStarted = true;

		//Humidity
		//weather_humidity = Mathf.Max(weather_humidity, weather_RainAmt);

	}






	void TimeUpdate(){

	    //------------------------------
	    //---    CALCULATE TIMER    ----
	    //------------------------------


		//TRANSITION TIME
		if (doTransition){
			Tenkoku_TransitionTime();
		}


		//RECALCULATE DATE and TIME
		RecalculateTime();


		//AUTO INCREASE TIME
		if (useAutoTime && !autoTimeSync && Application.isPlaying){

			//calculate time compression curve
			curveVal = timeCurves.Evaluate(dayValue);
			
			setTimeSpan = (_deltaTime * useTimeCompression);
			countSecond += setTimeSpan;
			countSecondMoon += Mathf.FloorToInt(setTimeSpan*0.92068f);
			countSecondStar += Mathf.FloorToInt(setTimeSpan*0.9333342f);

		}


		if (Mathf.Abs(countSecond) >= 1.0f){
			cSeconds = (int)countSecond;
			currentSecond += cSeconds;
			countSecond -= cSeconds;
		}
		if (Mathf.Abs(countMinute) >= 1.0f){
			currentMinute += Mathf.FloorToInt(countMinute);
			countMinute = 0.0f;
		}
			
		if (Mathf.Abs(countSecondMoon) >= 1.0f){
			moonSecond += Mathf.FloorToInt(countSecondMoon);
			countSecondMoon = 0.0f;
		}
		if (Mathf.Abs(countMinuteMoon) >= 1.0f){
			moonMinute += Mathf.FloorToInt(countMinuteMoon);
			countMinuteMoon = 0.0f;
		}
			
		if (Mathf.Abs(countSecondStar) >= 1.0f){
			starSecond += Mathf.FloorToInt(countSecondStar);
			countSecondStar = 0.0f;
		}
		if (Mathf.Abs(countMinuteStar) >= 1.0f){
			starMinute += Mathf.FloorToInt(countMinuteStar);
			countMinuteStar = 0.0f;
		}
			
			



		

		//SET DISPLAY TIME
		#if UNITY_EDITOR
			displayTime = DisplayTime("[ hh:mm:ss am] [ M/D/Y ad]");
		#endif

	}





	public string DisplayTime( string format ){

		//format string examples:
		// "M/D/Y H:M:S"
		setString = format;
		eon = "ad";
		useHour = setHour;
		
		displayHour = setHour;
		if (use24Clock){
			hourMode = "AM";
			if (useHour > 12){
				displayHour -= 12;
				hourMode = "PM";
			}
		} else {
			hourMode = "";
		}

		if (currentYear < 0) eon = "bc";
		setString = setString.Replace("hh",useHour.ToString("00"));
		setString = setString.Replace("mm",currentMinute.ToString("00"));
		setString = setString.Replace("ss",currentSecond.ToString("00"));
		setString = setString.Replace("Y",Mathf.Abs(currentYear).ToString());
		setString = setString.Replace("M",currentMonth.ToString());
		setString = setString.Replace("D",currentDay.ToString());
		setString = setString.Replace("ad",eon.ToString());
		
		if (use24Clock){
			setString = setString.Replace("am",hourMode.ToString());
		} else {
			setString = setString.Replace("am","");
		}
		
		return setString;
	}





	public int RecalculateLeapYear( int checkMonth, int checkYear){

		//check for leap Year (by div 4 method)
		leapYear = false;
		if ( (checkYear / 4.0f) == Mathf.FloorToInt(checkYear / 4.0f) ) leapYear = true;

		//double check for leap Year (by div 100 + div 400 method)
		if ((checkYear / 100.0f) == Mathf.FloorToInt(checkYear / 100.0f)){
			if ((checkYear / 400.0f) != Mathf.FloorToInt(checkYear / 400.0f)) leapYear = false;
		}
		
		//calculate month length
		monthLength = 31;
		testMonth = Mathf.FloorToInt(checkMonth);
		if (testMonth == 4 || testMonth == 6 || testMonth == 9 || testMonth == 11) monthLength = 30;
		if (testMonth == 2 && !leapYear) monthLength = 28;
		if (testMonth == 2 && leapYear) monthLength = 29;

		return monthLength;
	}




	void RecalculateTime(){
		









		//getLeapYear
		monthFac = RecalculateLeapYear(currentMonth,currentYear);

		//clamp and pass all values
		if (currentSecond > 59 || currentSecond < 0) currentMinute += Mathf.FloorToInt(currentSecond/60.0f);
		if (currentSecond > 59) currentSecond = 0;
		if (currentSecond < 0) currentSecond = 59;
		if (currentMinute > 59 || currentMinute < 0.0) currentHour += Mathf.FloorToInt(currentMinute/60.0f);
		if (currentMinute > 59) currentMinute = 0;
		if (currentMinute < 0) currentMinute = 59;

		if (currentHour > 23 || currentHour < 0) currentDay += Mathf.CeilToInt((currentHour/24.0f));
		if (currentHour > 23) currentHour = 0;
		if (currentHour < 0) currentHour = 23;

		if (currentDay > monthFac || currentDay < 1) currentMonth += Mathf.CeilToInt((currentDay/(monthFac*1.0f))-1.0f);
		if (currentDay > monthFac) currentDay = 1;
		if (currentDay < 1) currentDay = RecalculateLeapYear(currentMonth-1,currentYear);
		if (currentMonth > 12 || currentMonth < 1) currentYear += Mathf.CeilToInt((currentMonth/12.0f)-1f);
		if (currentMonth > 12) currentMonth = 1;
		if (currentMonth < 1) currentMonth = 12;
		if (currentYear == 0) currentYear = 1;
		
		//clamp and pass all moon values
		if (moonSecond > 59 || moonSecond < 0) moonMinute += Mathf.FloorToInt(moonSecond/60.0f);
		if (moonSecond > 59) moonSecond = 0;
		if (moonSecond < 0) moonSecond = 59;
		if (moonMinute > 59 || moonMinute < 0.0) moonHour += Mathf.FloorToInt(moonMinute/60.0f);
		if (moonMinute > 59) moonMinute = 0;
		if (moonMinute < 0) moonMinute = 59;
		if (moonHour > 24 || moonHour < 1) moonDay += Mathf.CeilToInt((moonHour/24.0f)-1f);
		if (moonHour > 24) moonHour = 1;
		if (moonHour < 1) moonHour = 24;
		if (moonDay > monthFac || moonDay < 1) moonMonth += Mathf.CeilToInt((moonDay/(monthFac*1.0f))-1.0f);
		if (moonDay > monthFac) moonDay = 1;
		if (moonDay < 1) moonDay = RecalculateLeapYear(moonMonth-1,currentYear);
		if (moonMonth > 12 || moonMonth < 1) moonYear += Mathf.CeilToInt((moonMonth/12.0f)-1f);
		if (moonMonth > 12) moonMonth = 1;
		if (moonMonth < 1) moonMonth = 12;
		
		//clamp and pass all star values
		if (starSecond > 59 || starSecond < 0) starMinute += Mathf.FloorToInt(starSecond/60.0f);
		if (starSecond > 59) starSecond = 0;
		if (starSecond < 0) starSecond = 59;
		if (starMinute > 59 || starMinute < 0.0) starHour += Mathf.FloorToInt(starMinute/60.0f);
		if (starMinute > 59) starMinute = 0;
		if (starMinute < 0) starMinute = 59;
		if (starHour > 24 || starHour < 1) starDay += Mathf.CeilToInt((starHour/24.0f)-1f);
		if (starHour > 24) starHour = 1;
		if (starHour < 1) starHour = 24;
		if (starDay > monthFac || starDay < 1) starMonth += Mathf.CeilToInt((starDay/(monthFac*1.0f))-1.0f);
		if (starDay > monthFac) starDay = 1;
		if (starDay < 1) starDay = RecalculateLeapYear(starMonth-1,currentYear);
		if (starMonth > 12 || starMonth < 1) starYear += Mathf.CeilToInt((starMonth/12.0f)-1f);
		if (starMonth > 12) starMonth = 1;
		if (starMonth < 1) starMonth = 12;
		
		
		if (!use24Clock && setHour > 12){
			setHour = currentHour + 12;
		} else {
			setHour = currentHour;
		}
		setHour = currentHour;




		//CALCULATE TIMERS
		setDay = ((setHour-1) * 3600.0f) + (currentMinute * 60.0f) + (currentSecond * 1.0f);
		setStar = ((starHour-1) * 3600.0f) + (starMinute * 60.0f) + (starSecond * 1.0f);
		monthAddition = 0.0f;

		for (aM = 1; aM < currentMonth; aM++){
			monthAddition += (RecalculateLeapYear( aM, currentYear) * 1.0f);
		}
		setYear = monthAddition+(currentDay-1f)+((currentSecond + (currentMinute*60f) + (setHour*3600f))/86400.0f);
		setMonth = (Mathf.Floor(moonDay-1)*86400.0f) + (Mathf.Floor(moonHour-1) * 3600.0f) + (Mathf.Floor(moonMinute) * 60.0f) + (Mathf.Floor(moonSecond) * 1.0f);
		setMoon = (Mathf.Floor(moonDay-1)*86400.0f)+(Mathf.Floor(moonHour-1f) * 3600.0f) + (Mathf.Floor(moonMinute) * 60.0f) + (Mathf.Floor(moonSecond) * 1.0f);
		setStar = (Mathf.Floor(starMonth-1)*30.41666f)+Mathf.Floor(starDay-1f)+(Mathf.Floor((starSecond) + (Mathf.Floor(starMinute)*60f) + (Mathf.Floor(starHour-1)*3600f))/86400.0f);


		//CLAMP VALUES
		yearDiv = 365.0f;
		if (leapYear) yearDiv = 366.0f;
		if (setYear > (86400.0f *  yearDiv)) setYear = 0.0f;
		if (setYear < 0.0f) setYear = (86400.0f *  yearDiv);
		

		//CALCULATE VALUES
		dayValue = setDay / 86400.0f;
		monthValue = setMonth / (86400.0f * 29.530589f);
		yearValue = setYear / yearDiv;
		starValue = setDay / 86400.0f;
		starValue -= (setStar / 365.0f);
		moonValue = setDay / 86400.0f;
		moonValue -= setMoon / ((86400.0f) * 29.6666f);
		

		//CALCULATE LIGHT FACTORS
		//sunAngle = 180f - Vector3.Angle(-libComponent.sunlightObject.forward,Vector3.up);
        daylightAngle = 90f - Mathf.Clamp(Vector3.Angle(-libComponent.sunlightObject.forward,Vector3.up), 0f, 90f);
        scatterDayFac = Mathf.Clamp(daylightAngle, 0f, 40f)/40f;
        //scatterMorningFac = Mathf.Clamp(daylightAngle, 0f, 20f)/20f;


		//SEND TIME TO CALCULATIONS COMPONENT
		calcComponent.y = currentYear;
		calcComponent.m = currentMonth;
		calcComponent.D = currentDay;




		calcComponent.UT = (float)((currentHour-1)+(currentMinute/60.0f)+(((currentSecond + cloudStepTime)/60.0f)/60.0f));
		lastSecond = currentSecond;

		calcComponent.local_latitude = setLatitude;
		calcComponent.local_longitude = setLongitude;
		calcComponent.tzOffset = setTZOffset;
		calcComponent.dstOffset = enableDST ? 1 : 0;




	}



	public float floatRound(float inFloat){
		//var retFloat : float = 0.0f;
		//retFloat = Mathf.Round(inFloat*1000.0f)/1000.0f;
		//return retFloat;
		return inFloat;
	}





	void ClampParticle( string px_system ){

		clampRes = 0.0f;
		px = 0;
		usePoint = 0.0f;
		
		//clamp rain fog particles to ground (raycast)
		if (px_system == "rain fog"){
		clampRes = 1.0f;
		setParticles = new ParticleSystem.Particle[libComponent.particleObjectRainFog.particleCount];
		libComponent.particleObjectRainFog.GetParticles(setParticles);
		for (px = 0; px < libComponent.particleObjectRainFog.particleCount; px++){
			
			#if UNITY_2017_1_OR_NEWER
				if (setParticles[px].remainingLifetime >= 2.5f){
			#else
				if (setParticles[px].lifetime >= 2.5f){
			#endif


				if ((px/clampRes) == (Mathf.Floor(px/clampRes))*1.0f){
					particleRayPosition.x = setParticles[px].position.x;
					particleRayPosition.y = 5000.0f;
					particleRayPosition.z = setParticles[px].position.z;
					if (Physics.Raycast(particleRayPosition, -Vector3.up, out hit, 10000.0f)){
						if (!hit.collider.isTrigger){
							usePoint = hit.point.y;
						}
					}	
				}

				particlePosition.x = setParticles[px].position.x;
				particlePosition.y = usePoint;
				particlePosition.z = setParticles[px].position.z;
				setParticles[px].position = particlePosition;
			}
		}
		libComponent.particleObjectRainFog.SetParticles(setParticles,setParticles.Length);
		libComponent.particleObjectRainFog.Play();
		}

		
		
	    
		//clamp rain fog particles to ground (raycast)
		if (px_system == "rain splash"){
		clampRes = 1.0f;
		setParticles = new ParticleSystem.Particle[libComponent.particleObjectRainSplash.particleCount];
		libComponent.particleObjectRainSplash.GetParticles(setParticles);
		for (px = 0; px < libComponent.particleObjectRainSplash.particleCount; px++){
			
			#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
				particleCol.r = setParticles[px].startColor.r;
				particleCol.g = setParticles[px].startColor.g;
				particleCol.b = setParticles[px].startColor.b;
				particleCol.a = 0f;
				setParticles[px].startColor = particleCol;
			#else
				particleCol.r = setParticles[px].color.r;
				particleCol.g = setParticles[px].color.g;
				particleCol.b = setParticles[px].color.b;
				particleCol.a = 0f;
				setParticles[px].color = particleCol;
			#endif

			#if UNITY_2017_1_OR_NEWER
				if (setParticles[px].remainingLifetime >= 0.05f){
			#else
				if (setParticles[px].lifetime >= 0.05f){
			#endif
			
				if ((px/clampRes) == (Mathf.Floor(px/clampRes))*1.0f){
					particleRayPosition.x = setParticles[px].position.x;
					particleRayPosition.y = 5000.0f;
					particleRayPosition.z = setParticles[px].position.z;
					if (Physics.Raycast(particleRayPosition, -Vector3.up, out hit, 10000.0f)){
						if (!hit.collider.isTrigger){
							usePoint = hit.point.y;
						}
					}
				}

				particlePosition.x = setParticles[px].position.x;
				particlePosition.y = usePoint;
				particlePosition.z = setParticles[px].position.z;
				setParticles[px].position = particlePosition;

				#if UNITY_5_4_OR_NEWER || UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_3_7 || UNITY_5_3_8
					particleCol.r = setParticles[px].startColor.r;
					particleCol.g = setParticles[px].startColor.g;
					particleCol.b = setParticles[px].startColor.b;
					particleCol.a = TenRandom.Next(25.0f,150.0f);
					setParticles[px].startColor = particleCol;
				#else
					particleCol.r = setParticles[px].color.r;
					particleCol.g = setParticles[px].color.g;
					particleCol.b = setParticles[px].color.b;
					particleCol.a = TenRandom.Next(25.0f,150.0f);
					setParticles[px].color = particleCol;
				#endif
			}
		}
		libComponent.particleObjectRainSplash.SetParticles(setParticles,setParticles.Length);
		libComponent.particleObjectRainSplash.Play();
		}


		//clamp fog particles to ground (raycast)
		if (px_system == "fog"){
		clampRes = 1.0f;
		setParticles = new ParticleSystem.Particle[libComponent.particleObjectFog.particleCount];
		libComponent.particleObjectFog.GetParticles(setParticles);
		for (px = 0; px < libComponent.particleObjectFog.particleCount; px++){

			#if UNITY_2017_1_OR_NEWER
				if (setParticles[px].remainingLifetime >= 4.8f){
			#else
				if (setParticles[px].lifetime >= 4.8f){
			#endif
			
				if ((px/clampRes) == (Mathf.Floor(px/clampRes))*1.0f){
					particleRayPosition.x = setParticles[px].position.x;
					particleRayPosition.y = 5000.0f;
					particleRayPosition.z = setParticles[px].position.z;
					if (Physics.Raycast(particleRayPosition, -Vector3.up, out hit, 10000.0f)){
						if (!hit.collider.isTrigger){
							usePoint = hit.point.y;
						}
					}	
				}
				if (usePoint > weather_FogHeight) usePoint = weather_FogHeight;

				particlePosition.x = setParticles[px].position.x;
				particlePosition.y = usePoint;
				particlePosition.z = setParticles[px].position.z;
				setParticles[px].position = particlePosition;
			}
		}
		libComponent.particleObjectFog.SetParticles(setParticles,setParticles.Length);
		libComponent.particleObjectFog.Play();
		}
		
		
		
	}



	public Vector2 TenkokuConvertAngleToVector(float convertAngle) {

		dir = Vector3.zero;
		tempAngle = Vector3.zero;
		if (convertAngle <= 180.0f){
			tempAngle = Vector3.Slerp(Vector3.forward,-Vector3.forward,(convertAngle)/180.0f);
			tempAngleVec2.x = tempAngle.x;
			tempAngleVec2.y = -tempAngle.z;
			dir = tempAngleVec2;
		}
		if (convertAngle > 180.0f){
			tempAngle = Vector3.Slerp(-Vector3.forward,Vector3.forward,(convertAngle-180.0f)/180.0f);
			tempAngleVec2.x = -tempAngle.x;
			tempAngleVec2.y = -tempAngle.z;
			dir = tempAngleVec2;
		}
		
		return dir;
	}



	public Vector4 CloudsConvertAngleToVector(float convertAngle) {
		Vector4 dir = Vector4.zero;
		Vector3 tempAngle = Vector3.zero;
		if (convertAngle <= 180.0f){
			tempAngle = Vector3.Slerp(Vector3.forward,-Vector3.forward,(convertAngle)/180.0f);
			dir.x = tempAngle.x;
			dir.z = -tempAngle.z;
		}
		if (convertAngle > 180.0f){
			tempAngle = Vector3.Slerp(-Vector3.forward,Vector3.forward,(convertAngle-180.0f)/180.0f);
			dir.x = -tempAngle.x;
			dir.z = -tempAngle.z;
		}
		dir.y = -0.25f;
		dir.w = 0f;

		return dir;
	}



	public void LoadDecodedColors(){

		decodeColorSun = DecodeColorKey(0);
		//decodeColorSunray = DecodeColorKey(1);
		decodeColorAmbientcolor = DecodeColorKey(2);
		decodeColorMoon = DecodeColorKey(3);
		decodeColorSkyambient = DecodeColorKey(4);
		decodeColorSkybase = DecodeColorKey(5);
		decodeColorAmbientcloud = DecodeColorKey(6);
		decodeColorColorhorizon = DecodeColorKey(7);
		decodeColorColorcloud = DecodeColorKey(8);
		decodeColorAmbientGI = DecodeColorKey(9);
	}



	public void UpdateColorMap(){
		DecodedColors = colorRamp.GetPixels(0);
	}



	public Color DecodeColorKey( int position ) {

		returnColor = Color.black;

		//DECODE TEXTURE
		if (colorTypeIndex == 0){

			//positions
			texPos = 0f;
			/*
			if (position == 0) texPos = 144; //sun
			if (position == 1) texPos = 144; //sunray
			if (position == 2) texPos = 81; //ambientcolor
			if (position == 3) texPos = 59; //moon
			if (position == 4) texPos = 37; //skyambient
			if (position == 5) texPos = 100; //skybase
			if (position == 6) texPos = 186; //ambientcloud
			if (position == 7) texPos = 121; //colorhorizon
			if (position == 8) texPos = 166; //colorcloud
			if (position == 9) texPos = 15; //ambientGI
			*/

			if (position == 0) texPos = 0.685f; //sun
			if (position == 1) texPos = 0.685f; //sunray
			if (position == 2) texPos = 0.385f; //ambientcolor
			if (position == 3) texPos = 0.28f; //moon
			if (position == 4) texPos = 0.176f; //skyambient
			if (position == 5) texPos = 0.476f; //skybase
			if (position == 6) texPos = 0.885f; //ambientcloud
			if (position == 7) texPos = 0.576f; //colorhorizon
			if (position == 8) texPos = 0.79f; //colorcloud
			if (position == 9) texPos = 0.07f; //ambientGI


			//decode texture
			if (colorRamp != null){

				//decode textures
				if (DecodedColors != null){
					returnColor = Color.Lerp(
									DecodedColors[(stepTime + ((int)(texPos * colorRamp.height) * colorRamp.width))],
									DecodedColors[(stepTime + 1 + ((int)(texPos * colorRamp.height) * colorRamp.width))],
									timeLerp);
				}

				//moon
				if (position == 3 && DecodedColors != null){
					returnColor = Color.Lerp(
									DecodedColors[(stepTimeM + ((int)(texPos * colorRamp.height) * colorRamp.width))],
									DecodedColors[(stepTimeM + 1 + ((int)(texPos * colorRamp.height) * colorRamp.width))],
									timeLerpM);
				}

				//sunray
				if (position == 1){
					returnColor.r = Mathf.Pow(returnColor.r,3.2f);
					returnColor.g = Mathf.Pow(returnColor.g,3.2f);
					returnColor.b = Mathf.Pow(returnColor.b,3.2f);
				}


				//linear and gamma conversion
				if (_linearSpace != ColorSpace.Linear){

					//specific color shift
					if (position == 5){ //skybase
						returnColor.r *= 0.4646f;
						returnColor.g *= 0.4646f;
						returnColor.b *= 0.4646f;
					}	
				}

//if (position == 5) returnColor = Color.black;

			}
		}



		//DECODE GRADIENT
		if (colorTypeIndex == 1){

			if (position == 0) returnColor = sunGradient.Evaluate( calcTime ); //sun
			if (position == 1) returnColor = sunGradient.Evaluate( calcTime ); //sunray
			if (position == 2) returnColor = ambColorGradient.Evaluate( calcTime ); //ambientcolor
			if (position == 3) returnColor = moonGradient.Evaluate( calcTimeM ); //moon
			if (position == 4) returnColor = ambDayGradient.Evaluate( calcTime ); //skyDay
			if (position == 5) returnColor = skyGradient.Evaluate( calcTime ); //skybase
			if (position == 6) returnColor = cloudGradient.Evaluate( calcTime ); //ambientcloud
			if (position == 7) returnColor = horizGradient.Evaluate( calcTime ); //colorhorizon
			if (position == 8) returnColor = cloudAmbGradient.Evaluate( calcTime ); //colorcloud
			if (position == 9) returnColor = ambLightGradient.Evaluate( calcTime ); //ambientGI



			//sunray
			if (position == 1){
				returnColor.r = Mathf.Pow(returnColor.r,3.2f);
				returnColor.g = Mathf.Pow(returnColor.g,3.2f);
				returnColor.b = Mathf.Pow(returnColor.b,3.2f);
			}

			//if (position == 9) returnColor = returnColor * 0.5f;

			//if (position == 5 || position == 0){
			//	returnColor.r *= 0.65f;
			//	returnColor.g *= 0.65f;
			//	returnColor.b *= 0.65f;
			//}

			//linear and gamma conversion
			//if (_linearSpace != ColorSpace.Linear){

				//specific color shift
				//if (position == 5){ //skybase
				//	returnColor.r *= 0.4646f;
				//	returnColor.g *= 0.4646f;
				//	returnColor.b *= 0.4646f;
				//}	
			//}

		}



		return returnColor;	    
	}




	// ENCODE SETTINGS TO STRING
	// this is useful to quickly encode
	// Tenkoku settings over a server.
	public string Tenkoku_EncodeData() {

		//run functions
		dataString = currentYear.ToString()+",";
		dataString += currentMonth.ToString()+",";
		dataString += currentDay.ToString()+",";
		dataString += currentHour.ToString()+",";
		dataString += currentMinute.ToString()+",";
		dataString += currentSecond.ToString()+",";

		dataString += setLatitude.ToString()+",";
		dataString += setLongitude.ToString()+",";

		dataString += weather_cloudAltoStratusAmt.ToString()+",";
		dataString += weather_cloudCirrusAmt.ToString()+",";
		dataString += weather_cloudCumulusAmt.ToString()+",";
		dataString += weather_OvercastAmt.ToString()+",";
		dataString += weather_OvercastDarkeningAmt.ToString()+",";
		dataString += weather_cloudScale.ToString()+",";
		dataString += weather_cloudSpeed.ToString()+",";
		dataString += weather_RainAmt.ToString()+",";
		dataString += weather_SnowAmt.ToString()+",";
		dataString += weather_FogAmt.ToString()+",";
		dataString += weather_WindAmt.ToString()+",";
		dataString += weather_WindDir.ToString()+",";

		dataString += weatherTypeIndex.ToString()+",";
		dataString += weather_autoForecastTime.ToString()+",";
		dataString += weather_TransitionTime.ToString()+",";

		dataUpdate = weather_forceUpdate ? "1" : "0";
		dataString += dataUpdate+",";

		dataString += weather_temperature.ToString()+",";
		dataString += weather_humidity.ToString()+",";
		dataString += weather_rainbow.ToString()+",";
		dataString += weather_lightning.ToString()+",";
		dataString += weather_lightningDir.ToString()+",";
		dataString += weather_lightningRange.ToString()+",";

		//save random marker
		dataString += randSeed.ToString()+",";

		//current cloud coordinates
		dataString += currCoords.x.ToString()+",";
		dataString += currCoords.y.ToString();


		return dataString;
	}





	// DECODE SETTINGS FROM STRING
	// this is useful to quickly decode
	// Tenkoku settings over a server.
	public void Tenkoku_DecodeData( string dataString ){

	    data = dataString.Split(","[0]);

	    //set functions
		currentYear = int.Parse(data[0], CultureInfo.InvariantCulture);
		currentMonth = int.Parse(data[1], CultureInfo.InvariantCulture);
		currentDay = int.Parse(data[2], CultureInfo.InvariantCulture);
		currentHour = int.Parse(data[3], CultureInfo.InvariantCulture);
		currentMinute = int.Parse(data[4], CultureInfo.InvariantCulture);
		currentSecond = int.Parse(data[5], CultureInfo.InvariantCulture);

		setLatitude = float.Parse(data[6], CultureInfo.InvariantCulture);
		setLongitude = float.Parse(data[7], CultureInfo.InvariantCulture);

		weather_cloudAltoStratusAmt = float.Parse(data[8], CultureInfo.InvariantCulture);
		weather_cloudCirrusAmt = float.Parse(data[9], CultureInfo.InvariantCulture);
		weather_cloudCumulusAmt = float.Parse(data[10], CultureInfo.InvariantCulture);
		weather_OvercastAmt = float.Parse(data[11], CultureInfo.InvariantCulture);
		weather_OvercastDarkeningAmt = float.Parse(data[12], CultureInfo.InvariantCulture);
		weather_cloudScale = float.Parse(data[13], CultureInfo.InvariantCulture);
		weather_cloudSpeed = float.Parse(data[14], CultureInfo.InvariantCulture);
		weather_RainAmt = float.Parse(data[15], CultureInfo.InvariantCulture);
		weather_SnowAmt = float.Parse(data[16], CultureInfo.InvariantCulture);
		weather_FogAmt = float.Parse(data[17], CultureInfo.InvariantCulture);
		weather_WindAmt = float.Parse(data[18], CultureInfo.InvariantCulture);
		weather_WindDir = float.Parse(data[19], CultureInfo.InvariantCulture);

		weatherTypeIndex = int.Parse(data[20], CultureInfo.InvariantCulture);
		weather_autoForecastTime = float.Parse(data[21], CultureInfo.InvariantCulture);
		weather_TransitionTime = float.Parse(data[22], CultureInfo.InvariantCulture);

		setUpdate = data[23];
		if (setUpdate == "1"){
			weather_forceUpdate = true;
		} else {
			weather_forceUpdate = false;
		}

		weather_temperature = float.Parse(data[24], CultureInfo.InvariantCulture);
		weather_humidity = float.Parse(data[25], CultureInfo.InvariantCulture);

		weather_rainbow = float.Parse(data[26], CultureInfo.InvariantCulture);
		weather_lightning = float.Parse(data[27], CultureInfo.InvariantCulture);
		weather_lightningDir = float.Parse(data[28], CultureInfo.InvariantCulture);
		weather_lightningRange = float.Parse(data[29], CultureInfo.InvariantCulture);

		//decode random marker
		randSeed = int.Parse(data[30], CultureInfo.InvariantCulture);
		TenRandom = new Tenkoku.Core.Random( randSeed );

		//current cloud coordinates
		currCoords.x = float.Parse(data[31], CultureInfo.InvariantCulture);
		currCoords.y = float.Parse(data[32], CultureInfo.InvariantCulture);

	}




	//---------------------------------------------------------------------------------------------------------------


    // -------------------------------------------------------
    // --   CALCULATE SKYBOX MATERIAL AND UPDATE SETTINGS   --
    // -------------------------------------------------------
	void CalculateSkyboxData(Material material){


			//update Sky Model
			if (atmosphereModelTypeIndex != useAtmosphereIndex){

				if (libComponent != null){
					if (atmosphereModelTypeIndex == 0){
						material = libComponent.skyMaterialLegacy;
					}

					if (atmosphereModelTypeIndex == 1){
						material = libComponent.skyMaterialElek;
					}

					UnityEngine.RenderSettings.skybox = material;
					useAtmosphereIndex = atmosphereModelTypeIndex;
				}
			}


			//if (material != null){

				//Update Legacy Sky Settings
				if (atmosphereModelTypeIndex == 0){
					material.SetColor("_GroundColor",colorSkyboxGround);
					material.SetColor("_MieColor",colorSkyboxMie);
					material.SetFloat("_Tenkoku_MieAmt", useMieAmount);
					material.SetFloat("_Tenkoku_MnMieAmt",useMieMnAmount);
					Shader.SetGlobalFloat("_Tenkoku_UseElek", 0.0f);
				}

				//Update Oskar-Elek Sky Settings
				if (atmosphereModelTypeIndex == 1){
			        material.SetFloat("_AtmosphereHeight", Mathf.Lerp(0f,160000f,atmosphereDensity));
			        material.SetFloat("_PlanetRadius", PlanetRadius);
			        material.SetVector("_DensityScaleHeight", DensityScale);
			        material.SetVector("_ScatteringR", RayleighSct * RayleighScatterCoef);
			        material.SetVector("_ScatteringM", MieSct * MieScatterCoef);
			        material.SetVector("_ExtinctionR", RayleighSct * RayleighExtinctionCoef);
			        material.SetVector("_ExtinctionM", MieSct * MieExtinctionCoef);

			        material.SetColor("_IncomingLight", IncomingLight);
			        material.SetFloat("_MieG", float.IsNaN(MieG) ? 0.5f:MieG);

			        material.SetTexture("_ParticleDensityLUT", _particleDensityLUT);

			        material.SetFloat("_SunIntensity", float.IsNaN(SunIntensity) ? 1f:SunIntensity);

			        material.SetColor("_GroundColor",colorSkyboxGround);
					material.SetColor("_MieColor",colorSkyboxMie);
					material.SetFloat("_Tenkoku_MieAmt", float.IsNaN(useMieAmount) ? 0f:useMieAmount);
					material.SetFloat("_Tenkoku_MnMieAmt",float.IsNaN(useMieMnAmount) ? 0.3f:useMieMnAmount);
					material.SetFloat("_Tenkoku_MnIntensity", float.IsNaN(libComponent.lightObjectNight.intensity) ? 0f:libComponent.lightObjectNight.intensity);


			        Shader.SetGlobalFloat("_Tenkoku_UseElek", 1.0f);

				}
			//}


	}




	//---------------------------------------------------------------------------------------------------------------




	// TRANSITION TIME CAPTURE (MULTI INPUT FUNCTIONS)
	public void Tenkoku_SetTransition(string startTime, string targetTime, int duration, float direction){
		Debug.Log("Tenkoku_SetTransition: Note that (string, string, int, float) format will soon be deprecated.  Please change duration value to float instead of int.");
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = null;
		doTransition = true;
	}
	public void Tenkoku_SetTransition(string startTime, string targetTime, float duration, float direction){
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = null;
		doTransition = true;
	}
	public void Tenkoku_SetTransition(string startTime, string targetTime, float duration, float direction, AnimationCurve curve){
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = curve;
		doTransition = true;
	}




	public void Tenkoku_SetTransition(string startTime, string targetTime, int duration, float direction, GameObject callbackObject){
		Debug.Log("Tenkoku_SetTransition: Note that (string, string, int, float, gameObject) format will soon be deprecated.  Please change duration value to float instead of int.");
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = null;
		if (callbackObject != null){
			transitionCallbackObject = callbackObject;
			transitionCallback = true;
		}
		doTransition = true;
	}
	public void Tenkoku_SetTransition(string startTime, string targetTime, float duration, float direction, GameObject callbackObject){
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = null;
		if (callbackObject != null){
			transitionCallbackObject = callbackObject;
			transitionCallback = true;
		}
		doTransition = true;
	}
	public void Tenkoku_SetTransition(string startTime, string targetTime, float duration, float direction, GameObject callbackObject, AnimationCurve curve){
		transitionStartTime = startTime;
		transitionTargetTime = targetTime;
		transitionDuration = duration;
		transitionDirection = direction;
		transitionCurve = curve;
		if (callbackObject != null){
			transitionCallbackObject = callbackObject;
			transitionCallback = true;
		}
		doTransition = true;
	}



	//---------------------------------------------------------------------------------------------------------------




	// DO TIME TRANSITIONS
	void Tenkoku_TransitionTime(){

		//Initialize
		if (transitionTime <= 0.0f){

			//clamp direction
			if (transitionDirection > 0.0f) transitionDirection = 1.0f;
			if (transitionDirection < 0.0f) transitionDirection = -1.0f;

			//calculate ending time
			setTransHour = Mathf.Clamp(System.Int32.Parse(transitionTargetTime.Substring(0,2), CultureInfo.InvariantCulture),0,23);
			setTransMinute = Mathf.Clamp(System.Int32.Parse(transitionTargetTime.Substring(3,2), CultureInfo.InvariantCulture),0,59);
			setTransSecond = Mathf.Clamp(System.Int32.Parse(transitionTargetTime.Substring(6,2), CultureInfo.InvariantCulture),0,59);
			endTime = setTransSecond + (setTransMinute*60) + (setTransHour*3600);

			//calculate starting time
			if (transitionStartTime == null || transitionStartTime == ""){
				startHour = currentHour;
				startMinute = currentMinute;
				startSecond = currentSecond;
			} else {
				startHour = Mathf.Clamp(System.Int32.Parse(transitionStartTime.Substring(0,2), CultureInfo.InvariantCulture),0,23);
				startMinute = Mathf.Clamp(System.Int32.Parse(transitionStartTime.Substring(3,2), CultureInfo.InvariantCulture),0,59);
				startSecond = Mathf.Clamp(System.Int32.Parse(transitionStartTime.Substring(6,2), CultureInfo.InvariantCulture),0,59);
				currentHour = startHour;
				currentMinute = startMinute;
				currentSecond = startSecond;
			}
			startTime = startSecond + (startMinute*60) + (startHour*3600);

			//fudge numbers if they are the same
			if (startTime == endTime){
				if (transitionDirection == 1.0f) startTime = startTime + 1;
				if (transitionDirection == -1.0f) startTime = startTime - 1;
			}

		}

		//check for transition end
		endTransition = false;
		if (transitionTime >= transitionDuration) endTransition = true;

		//END TRANSITION
		if (endTransition){

			//useAutoTime = false;
			doTransition = false;
			transitionTime = 0.0f;

			//Send Callback (optional)
			if (transitionCallback){
				if (transitionCallbackObject != null){
					transitionCallbackObject.SendMessage("CaptureTenkokuCallback",SendMessageOptions.DontRequireReceiver);
				}
			}
			transitionCallbackObject = null;
			transitionCallback = false;
			isDoingTransition = false;

		//DO TRANSITION
		} else {
			//useAutoTime = true;
			transitionTime += _deltaTime;

			float transTime = 0f;
			float transFac = transitionTime / transitionDuration;
			if (transitionCurve != null){
				transFac = transitionCurve.Evaluate(transFac);
			}
			if (transitionDirection == 1.0f && endTime > startTime) transTime = Mathf.SmoothStep(startTime, endTime, transFac);
			if (transitionDirection == -1.0f && endTime < startTime) transTime = Mathf.SmoothStep(startTime, endTime, transFac);
			if (transitionDirection == 1.0f && endTime < startTime){
				transTime = Mathf.SmoothStep(startTime, (endTime + 86400f), transFac) % 86400f;
			}
			if (transitionDirection == -1.0f && endTime > startTime){
				transTime = Mathf.SmoothStep(startTime, (endTime - 86400f), transFac);
				if (transTime < 0) transTime = 86400f - Mathf.Abs(transTime);
			}
			currentHour = (int)(transTime / 3600);
			currentMinute = (int)((transTime - (currentHour * 3600)) / 60);
			currentSecond = (int)((transTime - (currentHour * 3600) - (currentMinute * 60)));

			isDoingTransition = true;
		}

	}


    public static float RoundDigit(float value, int digits){
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }



}
}