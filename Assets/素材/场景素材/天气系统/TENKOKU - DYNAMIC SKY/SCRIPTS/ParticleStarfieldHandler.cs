using System;
using System.IO;
using UnityEngine;

namespace Tenkoku.Core
{
    public class ParticleStarfieldHandler : MonoBehaviour
	{


	//PUBLIC VARIABLES
	public bool starReset = false;
	public int numParticles = 9110;
	public float baseSize = 0.02f;
	public float setSize = 0.02f;
	public float constellationSize = 0.025f;
	public Color starBrightness = Color.white;
	public Vector3 offset = new Vector3(0f,0f,0f);
	public TenkokuStarData starData;
	public float starDistance = 1300f;

	//PRIVATE VARIABLES
	private bool hasStarted = false;
	private ParticleSystem StarSystem;
	private ParticleSystem.Particle[] StarParticles;
	private Vector4[] starPOS;
	private String[] starDataArray;
	private String starDataString;
	private Vector3 offsetC = new Vector3(0f,0f,0f);
	private float baseSizeC = 0.02f;
	private float constellationSizeC = 0.025f;
	private Color currStarBrightness = new Color(1f,1f,1f,1f);
	private Renderer rendererComponent;

	//Collect for GC
	private float starDeclination = 0.0f;
	private float starAscension = 0.0f;
	private float h;
	private float m;
	private float s;
	private Color starColO = new Color(0.41f,0.66f,1.0f,0.5f);
	private Color starColB = new Color(0.76f,0.86f,1.0f,0.5f);
	private Color starColA = new Color(1.0f,1.0f,1.0f,0.5f);
	private Color starColF = new Color(0.99f,1.0f,0.94f,0.5f);
	private Color starColG = new Color(1.0f,0.99f,0.55f,0.5f);
	private Color starColK = new Color(1.0f,0.72f,0.36f,0.5f);
	private Color starColM = new Color(1.0f,0.07f,0.07f,0.5f);		
	private Color setColor = new Color(1.0f,1.0f,1.0f,1.0f);
	private float starFactor = 0.0f;
	private float starMagnitude = 0.0f;
	private bool ConstellationStar = false;

	private int px = 0;
	private int sx = 0;

	private Vector3 setpos;
	private float useSize;

	private Vector3 particlePosition = Vector3.zero;
	private Color baseLerpColor = new Color(0.5f,0.6f,1.0f,1.0f);


	void Start () {

		hasStarted = false;
		StarSystem = this.GetComponent<ParticleSystem>();
		rendererComponent = this.GetComponent<Renderer>();
		numParticles = 9110;
		StarSystem.Emit(numParticles);

	}




	void LateUpdate(){

		if (offset != offsetC){
			offsetC = offset;
			starReset = true;
		}

		if (setSize != baseSizeC){
			baseSizeC = setSize;
			starReset = true;
		}

		if (constellationSize != constellationSizeC){
			constellationSizeC = constellationSize;
			starReset = true;
		}
		
		//set overall material color
		if (currStarBrightness != starBrightness){
			currStarBrightness = starBrightness;
			if (Application.isPlaying){
				rendererComponent.material.SetColor("_TintColor", starBrightness);
			} else {
				rendererComponent.sharedMaterial.SetColor("_TintColor", starBrightness);
			}
		}

		if (!hasStarted){
			hasStarted = true;
			starReset = true;
		}

		if (starReset){
			StSystemUpdate();
		}

	}







	void StSystemUpdate () {

		//reset star system
		starReset = false;

		//get Star Data and parse
		StarParticles = new ParticleSystem.Particle[starData.starElements.Length];
		StarSystem.GetParticles(StarParticles);
		
		


			for (px = 0; px < StarParticles.Length; px++){

				for (sx = 0; sx < (starData.starElements.Length); sx++){
				
					//calculate Right Ascension
					h = starData.starElements[sx].ascH;
					m = starData.starElements[sx].ascM;
					s = starData.starElements[sx].ascS;
					starAscension = ((s/60.0f)*0.1f)+(m/60.0f)+(h);
					
					//calculate Declination
					h = Mathf.Abs(starData.starElements[sx].decH);
					m = starData.starElements[sx].decM;
					s = starData.starElements[sx].decS;
					starDeclination = ((s/60.0f)*0.1f)+(m/60.0f)+(h);
					if (starData.starElements[sx].decH < 0.0f) starDeclination = 0.0f-starDeclination;
					
					//set particle positions
					particlePosition.x = 0f;
					particlePosition.y = 0f;
					particlePosition.z = starDistance;
					setpos = particlePosition;
					
					setpos = Quaternion.AngleAxis((90.0f-starDeclination),-Vector3.left) * setpos;
					StarParticles[px].position = setpos;

					setpos = Quaternion.AngleAxis(((starAscension*15.0f)),Vector3.forward) * setpos;
					setpos.x = -setpos.x;
					setpos.y = -setpos.y;
					setpos.z = -setpos.z + 4.5f;
					StarParticles[px].position = setpos;

					//set star colors
					if (starData.starElements[sx].color == 0) setColor = Color.Lerp(starColO,starColB,starFactor);
					if (starData.starElements[sx].color == 1) setColor = Color.Lerp(starColB,starColA,starFactor);
					if (starData.starElements[sx].color == 2) setColor = Color.Lerp(starColA,starColF,starFactor);
					if (starData.starElements[sx].color == 3) setColor = Color.Lerp(starColF,starColG,starFactor);
					if (starData.starElements[sx].color == 4) setColor = Color.Lerp(starColG,starColK,starFactor);
					if (starData.starElements[sx].color == 5) setColor = Color.Lerp(starColK,starColM,starFactor);
					if (starData.starElements[sx].color == 6) setColor = Color.Lerp(starColM,starColM,starFactor);

					//set star factor
					starFactor = starData.starElements[sx].factor;
					if (starFactor > 0.0f) starFactor = starFactor/9.0f;


					setColor = Color.Lerp(setColor,baseLerpColor,0.5f);

					//calculate magnitude
					starMagnitude = starData.starElements[sx].magnitude;
					
					setColor.a = 1.0f;
					if (starMagnitude >= 2.0f) setColor.a = Mathf.Lerp(1.0f,0.0f,(starMagnitude/9.0f));
					if (starMagnitude < 2.0f) setColor *= 2.0f;
					setColor.a = Mathf.Lerp(1.0f,0.0f,(starMagnitude/8.0f));

					//check for constellations
					ConstellationStar = false;
					ConstellationStar = CheckConstellationStarData(starData.starElements[sx].starIndex);
					
					
					//temp
					setColor.a = Mathf.Lerp(1.0f,0.075f,(starMagnitude/8.0f));
					if (setColor.a < 0.6f) setColor.a *= 0.1f;

					//set star sizes
					useSize = Mathf.Lerp(setSize*1.4f,setSize,(starMagnitude/8.0f));
					if (ConstellationStar && useSize < constellationSize) useSize = constellationSize; 
					

					if (starData.starElements[sx].brightFactor == 4){
						setColor.a *= 1.2f;
					} else if (starData.starElements[sx].brightFactor == 5){
						setColor.a *= 1.0f;
					} else if (starData.starElements[sx].brightFactor == 3){
						setColor.a *= 1.4f;
					} else if (starData.starElements[sx].brightFactor == 2){
						useSize = useSize*1.1f;
						setColor.a *= 1.6f;
					} else if (starData.starElements[sx].brightFactor == 1){
						useSize = useSize*1.2f;
						setColor.a *= 4.0f;
					}

					//polaris star
					if (starData.starElements[sx].starIndex == 8890) useSize *= 1.25f;



				
					//set particle size
					#if UNITY_5_3_OR_NEWER
						StarParticles[px].startSize = useSize;
					#else
						StarParticles[px].size = useSize;
					#endif

					//set particle color
					#if UNITY_5_3_OR_NEWER
						StarParticles[px].startColor = setColor;
					#else
						StarParticles[px].color = setColor;
					#endif


					px++;
				}
				
			}



		StarSystem.SetParticles(StarParticles,StarParticles.Length);
		StarSystem.Emit(StarParticles.Length);
		StarSystem.Play();
	}








	public bool CheckConstellationStarData(int starIndex){

		bool isConstellationStar = false;

		//little dipper
		if (starIndex == 8890) isConstellationStar = true;  //polaris
		if (starIndex == 153751) isConstellationStar = true;  //urodelus
		if (starIndex == 131873) isConstellationStar = true;  //kochab
		if (starIndex == 137422) isConstellationStar = true;  //pherkad
		if (starIndex == 166205) isConstellationStar = true;  //yildun
		if (starIndex == 142105) isConstellationStar = true;  //zeta ursae minoris
		if (starIndex == 148048) isConstellationStar = true;  //eta ursae minoris
		//big dipper
		if (starIndex == 103287) isConstellationStar = true;  //phekda
		if (starIndex == 120315) isConstellationStar = true;  //elkeid
		if (starIndex == 116842) isConstellationStar = true;  //alcor
		if (starIndex == 106591) isConstellationStar = true;  //megrez
		if (starIndex == 95418) isConstellationStar = true;  //merak
		if (starIndex == 95689) isConstellationStar = true;  //dubhe
		if (starIndex == 112185) isConstellationStar = true;  //alioth
		// orion
		if (starIndex == 39801) isConstellationStar = true;  //betelgeuse
		if (starIndex == 36861) isConstellationStar = true;  //meissa
		if (starIndex == 35468) isConstellationStar = true;  //bellatrix
		if (starIndex == 34085) isConstellationStar = true;  //rigel
		if (starIndex == 38771) isConstellationStar = true;  //saiph
		if (starIndex == 36486) isConstellationStar = true;  //mintaka
		if (starIndex == 37742) isConstellationStar = true;  //alnitak
		if (starIndex == 37128) isConstellationStar = true;  //alnilam
		//taurus
		if (starIndex == 35497) isConstellationStar = true;  //elnath
		if (starIndex == 29139) isConstellationStar = true;  //aldebaran
		if (starIndex == 28305) isConstellationStar = true;  //e taur
		if (starIndex == 28319) isConstellationStar = true;  //
		if (starIndex == 27371) isConstellationStar = true;  //
		if (starIndex == 25204) isConstellationStar = true;  //
		if (starIndex == 21120) isConstellationStar = true;  //
		if (starIndex == 37202) isConstellationStar = true;  //
		//scorpius
		if (starIndex == 148478) isConstellationStar = true;  //
		if (starIndex == 158926) isConstellationStar = true;  //
		if (starIndex == 159532) isConstellationStar = true;  //
		if (starIndex == 143275) isConstellationStar = true;  //
		if (starIndex == 151680) isConstellationStar = true;  //
		if (starIndex == 160578) isConstellationStar = true;  //
		if (starIndex == 144217) isConstellationStar = true;  //
		if (starIndex == 158408) isConstellationStar = true;  //
		if (starIndex == 149438) isConstellationStar = true;  //
		if (starIndex == 143018) isConstellationStar = true;  //
		if (starIndex == 147165) isConstellationStar = true;  //
		if (starIndex == 161471) isConstellationStar = true;  //
		if (starIndex == 151890) isConstellationStar = true;  //
		if (starIndex == 161892) isConstellationStar = true;  //
		if (starIndex == 155203) isConstellationStar = true;  //
		if (starIndex == 151985) isConstellationStar = true;  //
		if (starIndex == 152334) isConstellationStar = true;  //
		//pegasus
		if (starIndex == 206778) isConstellationStar = true;  //
		if (starIndex == 217906) isConstellationStar = true;  //
		if (starIndex == 218045) isConstellationStar = true;  //
		if (starIndex == 886) isConstellationStar = true;  //
		if (starIndex == 215182) isConstellationStar = true;  //
		if (starIndex == 214923) isConstellationStar = true;  //
		if (starIndex == 216131) isConstellationStar = true;  //
		if (starIndex == 210418) isConstellationStar = true;  //
		if (starIndex == 210027) isConstellationStar = true;  //
		if (starIndex == 215665) isConstellationStar = true;  //
		if (starIndex == 206901) isConstellationStar = true;  //
		if (starIndex == 215648) isConstellationStar = true;  //
		if (starIndex == 210459) isConstellationStar = true;  //
		if (starIndex == 224427) isConstellationStar = true;  //
		//cassiopeia
		if (starIndex == 5394) isConstellationStar = true;  //
		if (starIndex == 3712) isConstellationStar = true;  //
		if (starIndex == 432) isConstellationStar = true;  //
		if (starIndex == 8538) isConstellationStar = true;  //
		if (starIndex == 11415) isConstellationStar = true;  //
		if (starIndex == 4514) isConstellationStar = true;  //
		if (starIndex == 4614) isConstellationStar = true;  //
		if (starIndex == 3360) isConstellationStar = true;  //
		//pisces
		if (starIndex == 219615) isConstellationStar = true;  //
		if (starIndex == 220954) isConstellationStar = true;  //
		if (starIndex == 9270) isConstellationStar = true;  //
		if (starIndex == 224617) isConstellationStar = true;  //
		if (starIndex == 222368) isConstellationStar = true;  //
		if (starIndex == 10761) isConstellationStar = true;  //
		if (starIndex == 12446) isConstellationStar = true;  //
		if (starIndex == 6186) isConstellationStar = true;  //
		if (starIndex == 220954) isConstellationStar = true;  //
		if (starIndex == 224935) isConstellationStar = true;  //
		if (starIndex == 4656) isConstellationStar = true;  //
		if (starIndex == 10380) isConstellationStar = true;  //
		if (starIndex == 217891) isConstellationStar = true;  //
		if (starIndex == 222603) isConstellationStar = true;  //
		if (starIndex == 7106) isConstellationStar = true;  //
		if (starIndex == 28) isConstellationStar = true;  //
		if (starIndex == 11559) isConstellationStar = true;  //
		if (starIndex == 7087) isConstellationStar = true;  //
		if (starIndex == 7318) isConstellationStar = true;  //
		if (starIndex == 7964) isConstellationStar = true;  //
		if (starIndex == 9138) isConstellationStar = true;  //
		if (starIndex == 224533) isConstellationStar = true;  //
		//aquarius
		if (starIndex == 204867) isConstellationStar = true;  //
		if (starIndex == 209750) isConstellationStar = true;  //
		if (starIndex == 216627) isConstellationStar = true;  //
		if (starIndex == 213051) isConstellationStar = true;  //
		if (starIndex == 218594) isConstellationStar = true;  //
		if (starIndex == 216386) isConstellationStar = true;  //
		if (starIndex == 198001) isConstellationStar = true;  //
		if (starIndex == 212061) isConstellationStar = true;  //
		if (starIndex == 220321) isConstellationStar = true;  //
		if (starIndex == 213998) isConstellationStar = true;  //
		if (starIndex == 216032) isConstellationStar = true;  //
		if (starIndex == 211391) isConstellationStar = true;  //
		if (starIndex == 219215) isConstellationStar = true;  //
		if (starIndex == 219449) isConstellationStar = true;  //
		if (starIndex == 209819) isConstellationStar = true;  //
		if (starIndex == 219688) isConstellationStar = true;  //
		//capricornus
		if (starIndex == 192876) isConstellationStar = true;  //
		if (starIndex == 192947) isConstellationStar = true;  //
		if (starIndex == 193495) isConstellationStar = true;  //
		if (starIndex == 194943) isConstellationStar = true;  //
		if (starIndex == 197692) isConstellationStar = true;  //
		if (starIndex == 198542) isConstellationStar = true;  //
		if (starIndex == 204075) isConstellationStar = true;  //
		if (starIndex == 205637) isConstellationStar = true;  //
		if (starIndex == 206453) isConstellationStar = true;  //
		if (starIndex == 207098) isConstellationStar = true;  //
		if (starIndex == 206088) isConstellationStar = true;  //
		if (starIndex == 203387) isConstellationStar = true;  //
		if (starIndex == 200761) isConstellationStar = true;  //
		if (starIndex == 196662) isConstellationStar = true;  //
		if (starIndex == 195094) isConstellationStar = true;  //
		//sagittarius
		if (starIndex == 169022) isConstellationStar = true;  //
		if (starIndex == 175191) isConstellationStar = true;  //
		if (starIndex == 176687) isConstellationStar = true;  //
		if (starIndex == 168454) isConstellationStar = true;  //
		if (starIndex == 169916) isConstellationStar = true;  //
		if (starIndex == 178524) isConstellationStar = true;  //
		if (starIndex == 165135) isConstellationStar = true;  //
		if (starIndex == 167618) isConstellationStar = true;  //
		if (starIndex == 173300) isConstellationStar = true;  //
		if (starIndex == 177716) isConstellationStar = true;  //
		if (starIndex == 175775) isConstellationStar = true;  //
		if (starIndex == 177241) isConstellationStar = true;  //
		if (starIndex == 166937) isConstellationStar = true;  //
		if (starIndex == 181577) isConstellationStar = true;  //
		if (starIndex == 181454) isConstellationStar = true;  //
		if (starIndex == 181869) isConstellationStar = true;  //
		if (starIndex == 188114) isConstellationStar = true;  //
		if (starIndex == 181623) isConstellationStar = true;  //
		if (starIndex == 189103) isConstellationStar = true;  //
		if (starIndex == 189763) isConstellationStar = true;  //
		if (starIndex == 181615) isConstellationStar = true;  //
		if (starIndex == 161592) isConstellationStar = true;  //
		//libra
		if (starIndex == 135742) isConstellationStar = true;  //
		if (starIndex == 130841) isConstellationStar = true;  //
		if (starIndex == 133216) isConstellationStar = true;  //
		if (starIndex == 139063) isConstellationStar = true;  //
		if (starIndex == 139365) isConstellationStar = true;  //
		if (starIndex == 138905) isConstellationStar = true;  //
		//virgo
		if (starIndex == 116658) isConstellationStar = true;  //
		if (starIndex == 114330) isConstellationStar = true;  //
		if (starIndex == 110379) isConstellationStar = true;  //
		if (starIndex == 107259) isConstellationStar = true;  //
		if (starIndex == 102870) isConstellationStar = true;  //
		if (starIndex == 102212) isConstellationStar = true;  //
		if (starIndex == 104979) isConstellationStar = true;  //
		if (starIndex == 112300) isConstellationStar = true;  //
		if (starIndex == 113226) isConstellationStar = true;  //
		if (starIndex == 118098) isConstellationStar = true;  //
		if (starIndex == 122408) isConstellationStar = true;  //
		if (starIndex == 124850) isConstellationStar = true;  //
		if (starIndex == 129502) isConstellationStar = true;  //
		//leo
		if (starIndex == 87901) isConstellationStar = true;  //regulus
		if (starIndex == 87737) isConstellationStar = true;  //
		if (starIndex == 89484) isConstellationStar = true;  //algieba
		if (starIndex == 89025) isConstellationStar = true;  //
		if (starIndex == 85503) isConstellationStar = true;  //
		if (starIndex == 84441) isConstellationStar = true;  //
		if (starIndex == 97603) isConstellationStar = true;  //
		if (starIndex == 102647) isConstellationStar = true;  //
		if (starIndex == 97633) isConstellationStar = true;  //
		//leo minor
		if (starIndex == 94264) isConstellationStar = true;  //
		if (starIndex == 90537) isConstellationStar = true;  //
		if (starIndex == 87696) isConstellationStar = true;  //
		if (starIndex == 82635) isConstellationStar = true;  //
		if (starIndex == 92125) isConstellationStar = true;  //
		if (starIndex == 90277) isConstellationStar = true;  //
		//cancer
		if (starIndex == 69267) isConstellationStar = true;  //
		if (starIndex == 42911) isConstellationStar = true;  //
		if (starIndex == 74198) isConstellationStar = true;  //
		if (starIndex == 74739) isConstellationStar = true;  //
		if (starIndex == 76756) isConstellationStar = true;  //
		//gemini
		if (starIndex == 62509) isConstellationStar = true;  //
		if (starIndex == 60179) isConstellationStar = true;  //
		if (starIndex == 62345) isConstellationStar = true;  //
		if (starIndex == 45542) isConstellationStar = true;  //
		if (starIndex == 58207) isConstellationStar = true;  //
		if (starIndex == 56986) isConstellationStar = true;  //
		if (starIndex == 56537) isConstellationStar = true;  //
		if (starIndex == 52973) isConstellationStar = true;  //
		if (starIndex == 31681) isConstellationStar = true;  //
		if (starIndex == 48737) isConstellationStar = true;  //
		if (starIndex == 54719) isConstellationStar = true;  //
		if (starIndex == 50019) isConstellationStar = true;  //
		if (starIndex == 48329) isConstellationStar = true;  //
		if (starIndex == 257937) isConstellationStar = true;  //
		if (starIndex == 44478) isConstellationStar = true;  //
		//draco
		if (starIndex == 164058) isConstellationStar = true;  //
		if (starIndex == 159181) isConstellationStar = true;  //
		if (starIndex == 163588) isConstellationStar = true;  //
		if (starIndex == 182564) isConstellationStar = true;  //
		if (starIndex == 188119) isConstellationStar = true;  //
		if (starIndex == 175306) isConstellationStar = true;  //
		if (starIndex == 170153) isConstellationStar = true;  //
		if (starIndex == 160922) isConstellationStar = true;  //
		if (starIndex == 155763) isConstellationStar = true;  //
		if (starIndex == 148387) isConstellationStar = true;  //
		if (starIndex == 144284) isConstellationStar = true;  //
		if (starIndex == 137759) isConstellationStar = true;  //
		if (starIndex == 123299) isConstellationStar = true;  //
		if (starIndex == 109387) isConstellationStar = true;  //
		if (starIndex == 100029) isConstellationStar = true;  //
		if (starIndex == 85819) isConstellationStar = true;  //
		
		//----
		if (starIndex ==  4128) isConstellationStar = true;  //deneb kaitos
		if (starIndex ==  14386) isConstellationStar = true;  //mira
		if (starIndex ==  18884) isConstellationStar = true;  //menkar
		if (starIndex ==  12929) isConstellationStar = true;  //hamal
				
				

	return isConstellationStar;

	}







}
}