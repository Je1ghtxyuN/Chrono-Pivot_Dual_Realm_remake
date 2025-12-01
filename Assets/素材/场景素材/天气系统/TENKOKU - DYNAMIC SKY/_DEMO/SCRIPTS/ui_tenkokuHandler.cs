using System;
using UnityEngine;
using System.Collections;

namespace Tenkoku.Demo
{
    public class ui_tenkokuHandler : MonoBehaviour
	{


	public float uiScale = 1.0f;

	//private bool loadFlag = false;
	private Tenkoku.Core.TenkokuModule tenkokuObject;
	private UnityEngine.UI.CanvasScaler uiCanvasScale;

	private UnityEngine.UI.Slider sliderTOY;
	private UnityEngine.UI.Slider sliderTOD;
	private UnityEngine.UI.Slider sliderLat;
	private UnityEngine.UI.Slider sliderTmult;
	private UnityEngine.UI.Slider sliderAtSkyBright;
	private UnityEngine.UI.Slider sliderAtNightBright;
	private UnityEngine.UI.Slider sliderAtFog;
	private UnityEngine.UI.Slider sliderAtDensity;
	private UnityEngine.UI.Slider sliderWeAltoStratus;
	private UnityEngine.UI.Slider sliderWeCirrus;
	private UnityEngine.UI.Slider sliderWeCumulus;
	private UnityEngine.UI.Slider sliderWeOvercast;
	private UnityEngine.UI.Slider sliderWeRain;
	private UnityEngine.UI.Slider sliderWeSnow;
	private UnityEngine.UI.Slider sliderWeWind;
	private UnityEngine.UI.Slider sliderWeWindD;
	private UnityEngine.UI.Slider sliderWeLightning;
	private UnityEngine.UI.Slider sliderWeRainbow;

	private UnityEngine.UI.Text textTOY;
	private UnityEngine.UI.Text textTOD;
	private UnityEngine.UI.Text textLat;
	private UnityEngine.UI.Text textTmult;


	public float currentTODVal = -1.0f;


	void Start () {

		//get main object
		//tenkokuObject = GameObject.Find("Tenkoku DynamicSky").GetComponent<Tenkoku.Core.TenkokuModule>() as Tenkoku.Core.TenkokuModule;
		tenkokuObject = (Tenkoku.Core.TenkokuModule) FindObjectOfType(typeof(Tenkoku.Core.TenkokuModule));
		
		uiCanvasScale = this.transform.GetComponent<UnityEngine.UI.CanvasScaler>() as UnityEngine.UI.CanvasScaler;

		//find UI objects
		sliderTOY = GameObject.Find("Slider_TimeOfYear").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderTOD = GameObject.Find("Slider_TimeOfDay").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderLat = GameObject.Find("Slider_Latitude").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderTmult = GameObject.Find("Slider_TimeMult").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderAtSkyBright = GameObject.Find("Slider_AtSkyBright").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderAtNightBright = GameObject.Find("Slider_AtNightBright").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderAtFog = GameObject.Find("Slider_AtFog").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderAtDensity = GameObject.Find("Slider_AtDensity").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeAltoStratus = GameObject.Find("Slider_WeAltoStratus").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeCirrus = GameObject.Find("Slider_WeCirrus").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeCumulus = GameObject.Find("Slider_WeCumulus").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeOvercast = GameObject.Find("Slider_WeOvercast").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeRain = GameObject.Find("Slider_WeRain").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeSnow = GameObject.Find("Slider_WeSnow").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeWind = GameObject.Find("Slider_WeWind").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeWindD = GameObject.Find("Slider_WeWindD").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeLightning = GameObject.Find("Slider_WeLightning").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;
		sliderWeRainbow = GameObject.Find("Slider_WeRainbow").GetComponent<UnityEngine.UI.Slider>() as UnityEngine.UI.Slider;

		textTOY = GameObject.Find("Text_TimeOfYearText").GetComponent<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
		textTOD = GameObject.Find("Text_TimeOfDayText").GetComponent<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
		textLat = GameObject.Find("Text_LatitudeText").GetComponent<UnityEngine.UI.Text>() as UnityEngine.UI.Text;
		textTmult = GameObject.Find("Text_TimeMultText").GetComponent<UnityEngine.UI.Text>() as UnityEngine.UI.Text;


	}





	void LateUpdate(){

		//CANVAS SCALE
		uiCanvasScale.scaleFactor = uiScale;


		//########################
		// SET TENKOKU DATE/TIME
		//########################

		//set Time of Year
		tenkokuObject.currentMonth = Mathf.FloorToInt(Mathf.Lerp(1f,12.99f,sliderTOY.value));
		float dayVal = Mathf.Lerp(1f,12.99f,sliderTOY.value)-Mathf.FloorToInt(Mathf.Lerp(1f,12.99f,sliderTOY.value));
		tenkokuObject.currentDay = Mathf.FloorToInt(Mathf.Lerp(1f,30f,dayVal));
		textTOY.text = tenkokuObject.currentMonth+"/"+tenkokuObject.currentDay+"/2015";

		//set Time of Day
		if (sliderTmult.value == 0.0f){
			tenkokuObject.currentHour = Mathf.FloorToInt(Mathf.Lerp(0f,23f,sliderTOD.value));
			float minuteVal = Mathf.Lerp(0f,23f,sliderTOD.value)-Mathf.FloorToInt(Mathf.Lerp(0f,23f,sliderTOD.value));
			tenkokuObject.currentMinute = Mathf.FloorToInt(Mathf.Lerp(0f,59f,minuteVal));
		}

		string setPM = "am";
		int setH = tenkokuObject.currentHour;
		string setM = tenkokuObject.currentMinute.ToString("00");
		if (setH > 12){
			setH = setH - 12;
			setPM = "pm";
		}
		textTOD.text = setH+":"+setM+setPM+" ("+tenkokuObject.currentHour+":"+setM+")";

		//Set Time Multiplier
		tenkokuObject.autoTime = true;
		tenkokuObject.timeCompression = Mathf.Lerp(0.0f,2000.0f,sliderTmult.value);
		textTmult.text = Mathf.Lerp(0.0f,2000.0f,sliderTmult.value).ToString("0");
		if (sliderTOD.value >= 1.0f && sliderTmult.value > 0.0f){
			sliderTOD.value = 0.0f;
		//	sliderTOY.value += (1.0/365.25);
		}

		//Set Latitude
		tenkokuObject.setLatitude = Mathf.Floor(Mathf.Lerp(-90.0f,90.0f,sliderLat.value));
		textLat.text = tenkokuObject.setLatitude.ToString();



		//###########################
		// SET TENKOKU ATMOSPHERICS
		//###########################
		tenkokuObject.skyBrightness = Mathf.Lerp(0.0f,5.0f,sliderAtSkyBright.value);
		tenkokuObject.nightBrightness = Mathf.Lerp(0.0f,1.0f,sliderAtNightBright.value);
		tenkokuObject.atmosphereDensity = Mathf.Lerp(0.0f,4.0f,sliderAtDensity.value);



		//###########################
		// SET TENKOKU WEATHER
		//###########################
		tenkokuObject.weather_cloudAltoStratusAmt = Mathf.Lerp(0.0f,1.0f,sliderWeAltoStratus.value);
		tenkokuObject.weather_cloudCirrusAmt = Mathf.Lerp(0.0f,1.0f,sliderWeCirrus.value);
		tenkokuObject.weather_cloudCumulusAmt = Mathf.Lerp(0.0f,1.0f,sliderWeCumulus.value);
		tenkokuObject.weather_OvercastAmt = Mathf.Lerp(0.0f,1.0f,sliderWeOvercast.value);
		tenkokuObject.weather_humidity = Mathf.Lerp(0.0f,1.0f,sliderAtFog.value);
		tenkokuObject.weather_RainAmt = Mathf.Lerp(0.0f,1.0f,sliderWeRain.value);
		tenkokuObject.weather_SnowAmt = Mathf.Lerp(0.0f,1.0f,sliderWeSnow.value);
		tenkokuObject.weather_WindAmt = Mathf.Lerp(0.0f,1.0f,sliderWeWind.value);
		tenkokuObject.weather_cloudSpeed = Mathf.Lerp(0.0f,1.0f,sliderWeWind.value);
		tenkokuObject.weather_WindDir = Mathf.Lerp(0.0f,365.0f,sliderWeWindD.value);
		tenkokuObject.weather_lightning = Mathf.Lerp(0.0f,1.0f,sliderWeLightning.value);
		tenkokuObject.weather_rainbow = Mathf.Lerp(0.0f,1.0f,sliderWeRainbow.value);

		tenkokuObject.weather_lightningRange = 120.0f;
		tenkokuObject.volumeAmbDay = Mathf.Lerp(0.6f,-2.0f,sliderWeOvercast.value);
	}


}
}