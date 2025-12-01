using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//this limits the editor to running on object that have type CameraLocationHolder
[CustomEditor(typeof(Tenkoku.Core.TenkokuModule))]
public class TenkokuModuleEditor : Editor {



	SerializedObject objGradColor;
 	SerializedProperty cloudGradColor;
 	SerializedProperty cloudAmbGradient;
 	SerializedProperty sunGradient;
 	SerializedProperty horizGradient;
 	SerializedProperty skyGradient;
 	SerializedProperty ambColorGradient;
 	SerializedProperty moonGradient;
 	SerializedProperty ambDayGradient;
 	SerializedProperty ambLightGradient;
 	SerializedProperty temperatureGradient;


    void OnEnable(){
    	objGradColor = new SerializedObject(target);
	   	cloudGradColor = objGradColor.FindProperty("cloudGradient");
	   	cloudAmbGradient = objGradColor.FindProperty("cloudAmbGradient");
	   	sunGradient = objGradColor.FindProperty("sunGradient");
	   	horizGradient = objGradColor.FindProperty("horizGradient");
	   	skyGradient = objGradColor.FindProperty("skyGradient");
	   	ambColorGradient = objGradColor.FindProperty("ambColorGradient");
	   	moonGradient = objGradColor.FindProperty("moonGradient");
	   	ambDayGradient = objGradColor.FindProperty("ambDayGradient");
	   	ambLightGradient = objGradColor.FindProperty("ambLightGradient");
	   	temperatureGradient = objGradColor.FindProperty("temperatureGradient");
	}


	void ToolTip(float x, float y, string tip){
		Color sCol = GUI.contentColor;
		GUI.contentColor = new Color(1f,0.709f,0f,0.5f);
		GUI.Label (new Rect(x,y,12,15), new GUIContent("*", tip));
		GUI.contentColor = sCol;
	}

	public override void OnInspectorGUI() {
    

		objGradColor.Update();
		
    	Undo.RecordObject(target, "Changed Area Of Effect");

    	Color colorEnabled = new Color(1.0f,1.0f,1.0f,1.0f);
		Color colorDisabled = new Color(1.0f,1.0f,1.0f,0.25f);
		Color colorDisabled2 = new Color(1.0f,1.0f,1.0f,0.5f);
		
    	Texture2D logoTex = Resources.Load("textures/gui_tex_tenkokulogo_i") as Texture2D;
		Texture2D divTex = Resources.Load("textures/gui_tex_tenkokudiv_i") as Texture2D;
		Texture2D divRevTex = Resources.Load("textures/gui_tex_tenkokudivrev_i") as Texture2D;

		if (EditorGUIUtility.isProSkin == true){
			divRevTex = Resources.Load("textures/gui_tex_tenkokudivrev") as Texture2D;
			divTex = Resources.Load("textures/gui_tex_tenkokudiv") as Texture2D;
			logoTex = Resources.Load("textures/gui_tex_tenkokulogo") as Texture2D;
		}
		

		GUIStyle logoTexStyle = new GUIStyle("box");
		logoTexStyle.normal.background = logoTex;

		GUIStyle divTexStyle = new GUIStyle("box");
		divTexStyle.normal.background = divTex;

		GUIStyle divRevTexStyle = new GUIStyle("box");
		divRevTexStyle.normal.background = divRevTex;


    	Tenkoku.Core.TenkokuModule script = (Tenkoku.Core.TenkokuModule) target;

    	EditorGUI.BeginChangeCheck();

/*
		#if UNITY_PRO_LICENSE
			divRevTex = Resources.Load("textures/gui_tex_tenkokudivrev") as Texture;
			divTex = Resources.Load("textures/gui_tex_tenkokudiv") as Texture;
			logoTex = Resources.Load("textures/gui_tex_tenkokulogo") as Texture;
		#else
			divRevTex = Resources.Load("textures/gui_tex_tenkokudivrev_i") as Texture;
			divTex = Resources.Load("textures/gui_tex_tenkokudiv_i") as Texture;
			logoTex = Resources.Load("textures/gui_tex_tenkokulogo_i") as Texture;
		#endif
*/


		//SET SCREEN WIDTH
		//int setWidth = Screen.width-220;
		int setWidth = (int)EditorGUIUtility.currentViewWidth-220;
		if (setWidth < 120) setWidth = 120;
		
		
		//TENKOKU LOGO
		GUIContent buttonText = new GUIContent(""); 
		GUIStyle buttonStyle = GUIStyle.none; 
		Rect rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
		int margin = 15;





		//start menu
		EditorGUI.LabelField(new Rect(rt.x+margin+2, rt.y+35, 50, 18),"Version");
		
		Rect linkVerRect = new Rect(rt.x+margin+51, rt.y+35, 40, 18);
		EditorGUI.LabelField(linkVerRect,script.tenkokuVersionNumber);

	    Rect linkHelpRect = new Rect(rt.x+margin+165, rt.y+35, 28, 18);
	    Rect linkBugRect = new Rect(rt.x+margin+165+42, rt.y+35, 65, 18);
	    Rect linkURLRect = new Rect(rt.x+margin+165+120, rt.y+35, 100, 18);
	    
		if (Event.current.type == EventType.MouseUp && linkHelpRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/forum/");
		if (Event.current.type == EventType.MouseUp && linkBugRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/forum/");
		if (Event.current.type == EventType.MouseUp && linkURLRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/tenkoku/");

		EditorGUI.LabelField(new Rect(rt.x+margin+165+30, rt.y+35, 220, 18),"|");
		EditorGUI.LabelField(new Rect(rt.x+margin+165+110, rt.y+35, 220, 18),"|");
		
		EditorGUI.LabelField(linkHelpRect,"help");
		EditorGUI.LabelField(linkBugRect,"report bug");
		EditorGUI.LabelField(linkURLRect,"tanukidigital.com");
		// end menu





		GUI.Label(new Rect(rt.x+margin,rt.y,387,36), GUIContent.none, logoTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,36),logoTex);
        GUILayout.Space(42.0f);

		
		
		
		
        
        //SKY TIMER
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showTimer = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showTimer, "");
        GUI.Label (new Rect(rt.x+margin+10, rt.y+5, 300, 20), new GUIContent("TIME AND POSITION"));
        
        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showTimer = !script.showTimer;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);

        if (script.showTimer){
        	GUI.Label(new Rect(rt.x+margin,rt.y+309,387,34), GUIContent.none, divRevTexStyle);
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+309,387,34),divRevTex);
        	

        	//float setAmMargin = 0.0f;


        	GUI.contentColor = new Color(0.54f,0.45f,0.15f,1.0f);
        	GUI.Label(new Rect(rt.x+margin+10, rt.y+25, 50, 18), "Time:");
        	GUI.Label(new Rect(rt.x+margin+160, rt.y+25, 50, 18), "Date:");


        	GUI.contentColor = new Color(0.84f,0.75f,0.45f,1.0f);

        	string h = script.displayHour.ToString("00");
        	string m = script.currentMinute.ToString("00");
        	string s = script.currentSecond.ToString("00");
        	GUI.Label(new Rect(rt.x+margin+50, rt.y+25, 100, 18), h+" : "+m+" : "+s);
        	GUI.Label(new Rect(rt.x+margin+120, rt.y+25, 25, 18), script.hourMode);

        	string dy = script.currentDay.ToString("00");
        	string mt = script.currentMonth.ToString("00");
        	string yr = script.currentYear.ToString("0000");
        	GUI.Label(new Rect(rt.x+margin+200, rt.y+25, 100, 18), mt+"/ "+dy+"/ "+yr);

        	GUI.contentColor = colorEnabled;
        	script.use24Clock = EditorGUI.Toggle(new Rect(rt.x+margin+295, rt.y+25, 60, 15), "", script.use24Clock);
        	GUI.Label (new Rect (rt.x+margin+310, rt.y+25, 100, 15), new GUIContent("12H Clock"));
			ToolTip(rt.x+margin+367, rt.y+25,"Shows current time setting according to 12-hour clock format.  For display purposes only, actual hour inputs below will remain on 24 hour cycle.");

			GUILayout.Space(10.0f);

        	if (script.autoDateSync){
        		GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+55, 80, 15), new GUIContent("Year"));
        	script.currentYear = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+55, setWidth+60, 18), "", script.currentYear,-20000,20000);
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+75, 80, 15), new GUIContent("Month"));
        	script.currentMonth = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+75, setWidth+60, 18), "", script.currentMonth,1,12);
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+95, 80, 15), new GUIContent("Day"));
        	script.currentDay = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+95, setWidth+60, 18), "", script.currentDay,1,31);
            GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;    	

        	if (script.autoTimeSync){
        		GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

			GUI.Label (new Rect (rt.x+margin+10, rt.y+125, 80, 15), new GUIContent("Hour"));
        	script.currentHour = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+125, setWidth+60, 18), "", script.currentHour,0,23);
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+145, 80, 15), new GUIContent("Minute"));
        	script.currentMinute = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+145, setWidth+60, 18), "", script.currentMinute,0,59);
            GUI.Label (new Rect (rt.x+margin+10, rt.y+165, 80, 15), new GUIContent("Second"));
        	script.currentSecond = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+165, setWidth+60, 18), "", script.currentSecond,0,59);


        	GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			
			GUI.Label (new Rect (rt.x+margin+10, rt.y+195, 100, 15), new GUIContent ("Latitude"));
			script.setLatitude = EditorGUI.Slider(new Rect(rt.x+margin+100, rt.y+195, setWidth+60, 15), "", script.setLatitude, -90.0f, 90.0f);
			ToolTip(rt.x+margin+57, rt.y+195,"Setting the latitude affects the relative position of celestial objects.  Note for best accuracy you must enter latitude as decimal degrees.");

			GUI.Label (new Rect (rt.x+margin+10, rt.y+215, 100, 15), new GUIContent ("Longitude"));
			script.setLongitude = EditorGUI.Slider(new Rect(rt.x+margin+100, rt.y+215, setWidth+60, 15), "", script.setLongitude, -180.0f, 180.0f);
			ToolTip(rt.x+margin+67, rt.y+215,"Setting Longitude affects the apparent time offset, where longitude 0 is equivalent to GMT+0.  When setting a custom longitude it's highly recommended to also manually adjust the time zone setting below in order to get accurate local time settings. Note for best accuracy you must enter latitude as decimal degrees.");

			GUI.Label (new Rect (rt.x+margin+10, rt.y+235, 100, 15), new GUIContent ("Time Zone"));
			script.setTZOffset = EditorGUI.IntSlider(new Rect(rt.x+margin+100, rt.y+235, setWidth+60, 15), "", script.setTZOffset, -14, 14);
			ToolTip(rt.x+margin+73, rt.y+235,"This is the timezone offset from GMT+0.  This should be used in conjunction with the longitude setting above to get accurate local time.");

     		script.enableDST = EditorGUI.Toggle(new Rect(rt.x+margin+10, rt.y+265, 60, 15), "", script.enableDST);
        	GUI.Label (new Rect (rt.x+margin+30, rt.y+265, 230, 15), new GUIContent("Enable Daylight Savings Time"));
        	ToolTip(rt.x+margin+201, rt.y+265,"Enables a 1-hour daylight savings time offset.  Due to inconsistent daylight savings time rules around the world this should be set manually depending on your in-game location.");

     		script.autoTimeSync = EditorGUI.Toggle(new Rect(rt.x+margin+10, rt.y+295, 60, 15), "", script.autoTimeSync);
        	GUI.Label (new Rect (rt.x+margin+30, rt.y+295, 230, 15), new GUIContent("Sync to System Time"));
        	ToolTip(rt.x+margin+154, rt.y+295,"Will lock the Tenkoku time to the current time on your computer.");

     		script.autoDateSync = EditorGUI.Toggle(new Rect(rt.x+margin+210, rt.y+295, 60, 15), "", script.autoDateSync);
        	GUI.Label (new Rect (rt.x+margin+230, rt.y+295, 230, 15), new GUIContent("Sync to System Date"));
			ToolTip(rt.x+margin+352, rt.y+295,"Will lock the Tenkoku date to the current date on your computer.");

        	if (script.autoTimeSync){
        		GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			script.autoTime = EditorGUI.Toggle(new Rect(rt.x+margin+10, rt.y+315, 60, 15), "", script.autoTime);
        	GUI.Label (new Rect (rt.x+margin+30, rt.y+315, 140, 15), new GUIContent("Advance Time  x"));
        	script.timeCompression = EditorGUI.FloatField(new Rect(rt.x+margin+135, rt.y+315, 50, 15), "", script.timeCompression);
        	ToolTip(rt.x+margin+185, rt.y+315,"Enable Advance Time if you want tenkoku to automatically progress the time.  Use the multiplier box to set a time compression amount, where 1 would be normal real-life time progression, 2 would be twice as fast as normal, etc. A negative multiplier will reverse time.");

        	GUI.Label (new Rect (rt.x+margin+205, rt.y+315, 80, 15), new GUIContent("Speed Curve"));
        	script.timeCurves = EditorGUI.CurveField(new Rect(rt.x+margin+285, rt.y+315, 90, 15), "", script.timeCurves);
        	ToolTip(rt.x+margin+373, rt.y+315,"Using a custom curve allows you to set variable time compression amounts over the course of the day. the Y-axis is the time compression multiplier, and the x-axis is the course of the day.  0 and 1 on the x is midnight, and 0.5 on the x is noon. To keep consistent time progression set the curve to a flat line set at 1 on the y-axis.");

        	GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;

        	GUILayout.Space(310.0f);

        
        }
        GUILayout.Space(10.0f);
        
        
        
        
        //CONFIGURATION
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showConfig = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showConfig, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("CONFIGURATION"));

        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showConfig = !script.showConfig;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);

        if (script.showConfig){
        	GUI.Label(new Rect(rt.x+margin,rt.y+475,387,34), GUIContent.none, divRevTexStyle);
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+475,387,34),divRevTex);
			GUILayout.Space(10.0f);


			EditorGUI.LabelField(new Rect(rt.x+margin+10, rt.y+25, 180, 18),"Camera Mode");
			string[] cameraTypeOptions = new string[]{"Auto Select Camera","Manual Select Camera"};
			script.cameraTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+25, 150, 18),"", script.cameraTypeIndex, cameraTypeOptions);
			ToolTip(rt.x+margin+91, rt.y+25,"Tenkoku must be able to track your scene camera.  When set to 'auto' it will track the camera with the 'MainCamera' tag.  When set to manual it will track the camera designated in the slot below.");


			if (script.cameraTypeIndex == 0){
        		GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
			GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 100, 15), new GUIContent ("Main Camera"));

			script.manualCamera = EditorGUI.ObjectField(new Rect(rt.x+margin+165, rt.y+45, setWidth, 15), script.manualCamera, typeof(Transform), true) as Transform;
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;




			EditorGUI.LabelField(new Rect(rt.x+margin+10, rt.y+75, 180, 18),"Scene Light Layers");
			if (script.gameObject.activeInHierarchy){
				script.lightLayer = EditorGUI.MaskField(new Rect(rt.x+margin+165, rt.y+75, 150, 18),"", script.lightLayer, script.tenLayerMasks.ToArray());
			}
			ToolTip(rt.x+margin+120, rt.y+75,"Sets which game layers accept Tenkoku Day and Night lighting.");


			EditorGUI.LabelField(new Rect(rt.x+margin+10, rt.y+95, 180, 18),"Ambient Source");
			string[] ambientOptions = new string[]{"Skybox","Gradient","Flat Color"};
			script.ambientTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+95, 150, 18),"", script.ambientTypeIndex, ambientOptions);
			ToolTip(rt.x+margin+103, rt.y+95,"Sets the ambient color calculation mode. Note that when set to 'Skybox' ambient data will be updated according to the 'Update FPS' timing below.");




        	GUI.Label (new Rect (rt.x+margin+10, rt.y+115, 140, 15), new GUIContent("Enable Reflection Probe"));
        	script.enableProbe = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+115, 40, 15), "", script.enableProbe);
			if (!script.enableProbe){
        		GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
        	GUI.Label (new Rect (rt.x+margin+200, rt.y+115, 140, 15), new GUIContent("Update FPS"));
        	script.reflectionProbeFPS = EditorGUI.FloatField(new Rect(rt.x+margin+275, rt.y+115, 40, 15), "", script.reflectionProbeFPS);
        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+145, 100, 15), new GUIContent ("Set Orientation"));
			script.setRotation = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+145, setWidth, 15), "", script.setRotation, 0.0f, 359.0f);
			ToolTip(rt.x+margin+98, rt.y+145,"This will completely offset the celestial sphere, which allows you to orient North, South, East, and West in different orientation.  Default is 180.");

			GUI.Label (new Rect (rt.x+margin+10, rt.y+165, 150, 15), new GUIContent ("Minimum Light Altitude"));
			script.minimumHeight = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+165, setWidth, 15), "", script.minimumHeight, 0.0f, 90.0f);
			ToolTip(rt.x+margin+140, rt.y+165,"Sets the minimum angle for sun and moon lighting.  Default is 0.");

			GUI.Label (new Rect (rt.x+margin+10, rt.y+185, 150, 15), new GUIContent ("Allow Multi-Lighting"));
			script.allowMultiLights = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+185, 60, 15), "", script.allowMultiLights);
			ToolTip(rt.x+margin+121, rt.y+185,"When enabled, Tenkok will allow both sun and moon directional lights to illuminate your scene at the same time.  This allows for better day-->night light blending, but may cause additional performance overhead.  If you're having any performance issues it's recommended to turn this option off.");


		    GUI.Label (new Rect (rt.x+margin+10, rt.y+205, 190, 15), new GUIContent("Add Automatic FX"));
			script.useAutoFX = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+205, 30, 18), "", script.useAutoFX);
			ToolTip(rt.x+margin+113, rt.y+205,"When enabled, FX components will be automatically added to the tracked scene camera.  This effects the 'Use Temporal Aliasing, and 'Calculate light rays' and 'enable tenkoku fog' options below.  Disable this option if you want to manually manage FX components.");


		    GUI.Label (new Rect (rt.x+margin+10, rt.y+225, 190, 15), new GUIContent("Disable MSAA"));
			script.disableMSA = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+225, 30, 18), "", script.disableMSA);
			ToolTip(rt.x+margin+90, rt.y+225,"Hardware-based Anti Aliasing is notoriously interfers with complex image buffer compositing, especially in Forward rendering.  If you notice strange artifacts or upside down scene rendering you can ensure that MSAA is forced off by using this option.");

			if (Application.platform == RuntimePlatform.OSXEditor){
				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
		    GUI.Label (new Rect (rt.x+margin+10, rt.y+245, 190, 15), new GUIContent("Use Temporal Aliasing"));
			script.useTemporalAliasing = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+245, 30, 18), "", script.useTemporalAliasing);
			ToolTip(rt.x+margin+139, rt.y+245,"Temporal Aliasing helps blend and smooth out cloud rendering noise and artifacts, allowing better looking clouds at lower quality settings. Note: Currently PC only!  Hopefully a Mac compatible version will be available in the future.");
			
		    GUI.Label (new Rect (rt.x+margin+10, rt.y+265, 190, 15), new GUIContent("Use Full Screen Aliasing"));
			script.useFSAliasing = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+265, 30, 18), "", script.useFSAliasing);
			ToolTip(rt.x+margin+148, rt.y+265,"Control whether Aliasing is applied to the entire screen or only to the sky.  Default is off, and aliasing is only applied to sky.  Mostly helpful to turn on if showing reflections of the sky in your scene.");
			
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;	

		    GUI.Label (new Rect (rt.x+margin+10, rt.y+285, 190, 15), new GUIContent("Link Clouds to System"));
			script.cloudLinkToTime = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+285, 30, 18), "", script.cloudLinkToTime);
			ToolTip(rt.x+margin+140, rt.y+285,"Cloud speed and offset will be multiplied by the 'Advance Time' settings.");

		    GUI.Label (new Rect (rt.x+margin+10, rt.y+305, 190, 15), new GUIContent("Use Legacy Clouds"));
			script.useLegacyClouds = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+305, 30, 18), "", script.useLegacyClouds);
			ToolTip(rt.x+margin+120, rt.y+305,"Switch cloud rendering to the Legacy Cloud system.  Legacy Clouds use less resources at the cost of visual fidelity.");
				

			#if UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_4_OR_NEWER

			#else
				script.useLegacyClouds = true;
				GUI.contentColor = colorDisabled;
    			GUI.backgroundColor = colorDisabled;
				GUI.Label (new Rect (rt.x+margin+210, rt.y+305, 190, 15), new GUIContent("(Unity 5.3.4+ setting only)"));
				GUI.contentColor = colorEnabled;
    			GUI.backgroundColor = colorEnabled;
			#endif

			if (script.useLegacyClouds){
				GUI.contentColor = colorDisabled;
    			GUI.backgroundColor = colorDisabled;
			}
         	GUI.Label (new Rect (rt.x+margin+10, rt.y+325, 180, 15), new GUIContent("Cloud Quality"));
        	script.cloudQuality = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+325, setWidth, 18), "", script.cloudQuality,0.25f,2.0f);
			ToolTip(rt.x+margin+88, rt.y+325,"Strikes a balance between performance and quality.  The higher the quality the fewer noise artifacts will be visible in the clouds.  Higher quality will have a marked improvement in this respect but comes at a noticeable performance hit.  Default is 0.6.");

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+345, 180, 15), new GUIContent("Cloud Frequency"));
        	script.cloudFreq = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+345, setWidth, 18), "", script.cloudFreq,0.1f,3.0f);
			ToolTip(rt.x+margin+107, rt.y+345,"Changes the overall dispercement of clouds from few large clous to frequent small clouds.  Default is 1.");

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+365, 180, 15), new GUIContent("Cloud Detail"));
        	script.cloudDetail = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+365, setWidth, 18), "", script.cloudDetail,0.1f,3.0f);
			ToolTip(rt.x+margin+84, rt.y+365,"Changes the frequency of edge detail, making clouds look straight or fluffy.  Default is 1.");

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+385, 180, 15), new GUIContent("Cloud Brightness"));
        	script.cloudBrightness = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+385, setWidth, 18), "", script.cloudBrightness,0.0f,2.0f);
			ToolTip(rt.x+margin+109, rt.y+385,"Changes the level of cloud brightening.");


        	GUI.contentColor = colorEnabled;
    		GUI.backgroundColor = colorEnabled;
	        	
	        GUI.Label (new Rect (rt.x+margin+10, rt.y+405, 180, 15), new GUIContent("Precipitation Quality"));
	        script.precipQuality = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+405, setWidth, 18), "", script.precipQuality,0.01f,2.0f);
			ToolTip(rt.x+margin+127, rt.y+405,"Controls number of rain and snow particles emitted.  Higher quality emits more particles at the cost of more system resources.  Lower this setting for better performance.  Default is 1.");





			script.enableAutoAdvance = EditorGUI.Toggle(new Rect(rt.x+margin+10, rt.y+435, 20, 18),"", script.enableAutoAdvance);

			if (!script.enableAutoAdvance){
	        	GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}

			EditorGUI.LabelField(new Rect(rt.x+margin+30, rt.y+435, 340, 18),"AUTO-ADVANCE SYSTEM TIMER");
	        script.systemTime = EditorGUI.FloatField(new Rect(rt.x+margin+260, rt.y+435, 120, 18),"",script.systemTime);
			
        	//GUI.contentColor = colorDisabled;
        	//GUI.backgroundColor = colorDisabled;
        	GUI.contentColor = new Color(0.54f,0.55f,0.55f,1.0f);
        	EditorGUI.LabelField(new Rect(rt.x+margin+30, rt.y+455, 340, 18),"the 'systemTime' variable is automatically advanced by");
        	EditorGUI.LabelField(new Rect(rt.x+margin+30, rt.y+467, 340, 18),"default.  This variable can be shared across a network to");
			EditorGUI.LabelField(new Rect(rt.x+margin+30, rt.y+479, 340, 18),"sync cloud positions between client and server computers.");
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;


        	GUILayout.Space(475.0f);
        }
        GUILayout.Space(10.0f);

        







        //COLORS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showColors = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showColors, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("COLOR SETTINGS"));

        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showColors = !script.showColors;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);

        if (script.showColors){
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+425,387,34),divRevTex);
			GUILayout.Space(10.0f);


        	GUI.Label (new Rect (rt.x+margin+10, rt.y+30, 190, 15), new GUIContent("GENERAL COLOR SETTINGS"));

            EditorGUI.LabelField(new Rect(rt.x+margin+10, rt.y+50, 120, 18),"Temperature Tint");
			EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+50, setWidth-18, 15),temperatureGradient,new GUIContent(""),false);


			GUI.Label (new Rect (rt.x+margin+10, rt.y+70, 100, 15), new GUIContent ("Overall Tint"));
			script.colorOverlay = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+70, setWidth, 15), script.colorOverlay);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+90, 100, 15), new GUIContent ("Sky Tint"));
			script.colorSky = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+90, setWidth, 15), script.colorSky);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+110, 100, 15), new GUIContent ("Sun Scattering"));
			script.colorSkyboxMie = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+110, setWidth, 15), script.colorSkyboxMie);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+130, 100, 15), new GUIContent ("Skybox Ground"));
			script.colorSkyboxGround = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+130, setWidth, 15), script.colorSkyboxGround);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+150, 100, 15), new GUIContent ("Fog Tint"));
			script.colorFog = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+150, setWidth, 15), script.colorFog);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+170, 100, 15), new GUIContent ("Lightning Tint"));
			script.lightningColor = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+170, setWidth, 15), script.lightningColor);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+190, 120, 15), new GUIContent ("Moon Horizon Tint"));
			script.moonHorizColor = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+190, setWidth, 15), script.moonHorizColor);



			if (!script.useAmbient || script.ambientTypeIndex == 0){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			GUI.Label (new Rect (rt.x+margin+10, rt.y+210, 100, 15), new GUIContent ("Ambient Tint"));
			script.colorAmbient = EditorGUI.ColorField(new Rect(rt.x+margin+165, rt.y+210, setWidth, 15), script.colorAmbient);
			ToolTip(rt.x+margin+83, rt.y+210,"Works in conjunction the Ambient Probe: Color or Ambient Probe: Gradient settings only.");

			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;

			if (!script.useAmbient || script.ambientTypeIndex >= 1){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			GUI.Label (new Rect (rt.x+margin+10, rt.y+230, 150, 15), new GUIContent ("Day Ambient Amount"));
        	script.ambientDayAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+230, setWidth, 18), "", script.ambientDayAmt,0.0f,5.0f);
			ToolTip(rt.x+margin+135, rt.y+230,"Works in conjunction the Ambient Probe: Skybox setting only.  To adjust ambient brightness when set to color or gradient mode, please adjust the ambient color gradient or texture accordingly.");
			
			GUI.Label (new Rect (rt.x+margin+10, rt.y+250, 150, 15), new GUIContent ("Night Ambient Amount"));
        	script.ambientNightAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+250, setWidth, 18), "", script.ambientNightAmt,0.0f,25.0f);
			ToolTip(rt.x+margin+140, rt.y+250,"Works in conjunction the Ambient Probe: Skybox setting only.  To adjust ambient brightness when set to color or gradient mode, please adjust the ambient color gradient or texture accordingly.");

			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;


			GUI.Label (new Rect (rt.x+margin+10, rt.y+270, 150, 15), new GUIContent ("Set Ambient Colors"));
			script.useAmbient = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+270, 60, 15), "", script.useAmbient);
			ToolTip(rt.x+margin+122, rt.y+270,"Turns off Tenkoku Ambient contribution, allowing you to control ambient colors from elsewhere in your project.");

			GUI.Label (new Rect (rt.x+margin+10, rt.y+290, 150, 15), new GUIContent ("Adjust Overcast Ambient"));
			script.adjustOvercastAmbient = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+290, 60, 15), "", script.adjustOvercastAmbient);
			ToolTip(rt.x+margin+152, rt.y+290,"Adjusts the day and night ambient amounts based on weather Overcast amount.");




        	GUI.Label (new Rect (rt.x+margin+10, rt.y+325, 190, 15), new GUIContent("COLOR GRADIENT SETTINGS"));
			ToolTip(rt.x+margin+182, rt.y+325,"Gradients are sampled based on the height of the sun object, and in general this is analagous to the time of day. The gradient starts at midnight (on the left side) which is the lowest point of the sun and ends at noon (on the right side), which is the highest point of the sun. Once it hits noon it travels back down the gradient until it hits midnight again, so there is no difference between dusk and dawn colors.");

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+345, 190, 15), new GUIContent("Gradient Source"));
        	string[] colorTypeOptions = new string[]{"Texture","Custom Gradient"};
        	script.colorTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+345, setWidth, 18),"",script.colorTypeIndex, colorTypeOptions);

        	if (script.colorTypeIndex == 0){
         		GUI.Label (new Rect (rt.x+margin+20, rt.y+360, 100, 15), new GUIContent ("Color Texture"));
        		script.colorRamp = EditorGUI.ObjectField(new Rect(rt.x+margin+165, rt.y+360, setWidth, 35), script.colorRamp, typeof(Texture2D), true) as Texture2D;
        		
        		GUILayout.Space(380.0f);
        	}

        	if (script.colorTypeIndex == 1){
         		//GUI.Label (new Rect (rt.x+margin+10, rt.y+220, 100, 15), new GUIContent ("Color Texture"));
        		//script.colorRamp = EditorGUI.ObjectField(new Rect(rt.x+margin+165, rt.y+220, setWidth, 35), script.colorRamp, typeof(Texture2D), true) as Texture2D;
        	
                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+365, 120, 18),"Cloud Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+365, setWidth, 18),cloudGradColor,new GUIContent(""),false);
				ToolTip(rt.x+margin+90, rt.y+365,"Affects color tinting of Altostratus and cirrus clouds, as well as directional lighting of cumulus clouds.");

                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+385, 120, 18),"Cloud Ambient");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+385, setWidth, 18),cloudAmbGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+107, rt.y+385,"Affects main color tinting of cumulus clouds.");

                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+405, 120, 18),"Sun Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+405, setWidth, 18),sunGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+80, rt.y+405,"Affects the Tint color of sun lighting.");

                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+425, 120, 18),"Horizon Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+425, setWidth, 18),horizGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+101, rt.y+425," In Elek this is ignored. In the legacy Tenkoku atmospheric model this adds an additional tint on the lower atmosphere/horizon area.");

                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+445, 120, 18),"Sky Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+445, setWidth, 18),skyGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+79, rt.y+445,"In Elek this is ignored. In the legacy Tenkoku atmospheric model this adds an additional sky tint on top of the underlying atmospheric model.");
				
                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+465, 120, 18),"Ambient Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+465, setWidth, 18),ambColorGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+105, rt.y+465,"When 'Ambient Source' is set to Skybox this is ignored. When 'Ambient Source' is set to gradient, or color the scene ambient tint is sourced directly from this gradient.");
				
                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+485, 120, 18),"Moon Color");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+485, setWidth, 18),moonGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+88, rt.y+485,"Currently this is ignored.");
				
                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+505, 120, 18),"Ambient Daylight");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+505, setWidth, 18),ambDayGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+121, rt.y+505,"This affects various settings in subtle ways but the main function can be thought of as the Sun vs Moon lighting switch. This affects sun light intensity and moon light intensity, but also has an affect on cloud light intensity and overall scene ambient intensity.");
				
                EditorGUI.LabelField(new Rect(rt.x+margin+20, rt.y+525, 120, 18),"Ambient Lighting");
				EditorGUI.PropertyField(new Rect(rt.x+margin+165, rt.y+525, setWidth, 18),ambLightGradient,new GUIContent(""),false);
				ToolTip(rt.x+margin+118, rt.y+525,"This mainly effects the intensity / brightness of the skybox, between day and night. It also has some affect on cumulus cloud lighting. This was made separate from the above ambient setting in order to better control how the sky itself responded to incoming and outgoing daylight scattering... which needed different timing compared to normal direct light ambient settings. ");
				

        		GUILayout.Space(525.0f);
        	}


        	objGradColor.ApplyModifiedProperties();
        	
        }
        GUILayout.Space(10.0f);

        





  
        
        //CELESTIAL SETTINGS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showCelSet = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showCelSet, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("CELESTIAL SETTINGS"));

        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showCelSet = !script.showCelSet;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);


        if (script.showCelSet){
        	GUI.Label(new Rect(rt.x+margin,rt.y+629,387,34), GUIContent.none, divRevTexStyle);
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+629,387,34),divRevTex);

        	//sun
        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
			GUI.Label (new Rect (rt.x+margin+10, rt.y+28, 100, 20), new GUIContent ("Sun Rendering"));
			string[] sunTypeOptions = new string[]{"Realistic","Custom","Off"};
			script.sunTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+28, setWidth, 18),"",script.sunTypeIndex, sunTypeOptions);

			if (script.sunTypeIndex == 2){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+45, 100, 20), new GUIContent ("Light Amount"));
			script.sunBright = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+45, setWidth, 15), "", script.sunBright, 0.0f, 5.0f);
	        ToolTip(rt.x+margin+98, rt.y+45,"Sets the brightness level of direct sun lighting.  Default is 1.0");
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+65, 100, 20), new GUIContent ("Light Saturation"));
			script.sunSat = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+65, setWidth, 15), "", script.sunSat, 0.0f, 1.0f);
        	ToolTip(rt.x+margin+111, rt.y+65,"Controls the color saturation of sun lighting, from full color (1.0) to greyscale value (0.0)");
			
        	GUI.Label (new Rect (rt.x+margin+20, rt.y+85, 180, 15), new GUIContent("Sky Amount (Day)"));
        	script.skyBrightness = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+85, setWidth, 18), "", script.skyBrightness,0.0f,5.0f);
			ToolTip(rt.x+margin+128, rt.y+85,"Controls the brightness of the skybox during daytime.  If you're using Tonemapping on your camera it's recommended to increase this setting to 1.0 or higher.");
				
        	GUI.Label (new Rect (rt.x+margin+20, rt.y+105, 180, 15), new GUIContent("Mie Amount"));
        	script.mieAmount = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+105, setWidth, 18), "", script.mieAmount,0.0f,2.0f);
		    ToolTip(rt.x+margin+90, rt.y+105,"Controls glow halo around sun object, drawn directly in skybox.  Can be substituted by use of light rays.");
			
		    GUI.Label (new Rect (rt.x+margin+20, rt.y+125, 100, 20), new GUIContent ("Size"));
			script.sunSize = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+125, setWidth, 15), "", script.sunSize, 0.005f, 0.1f);
			ToolTip(rt.x+margin+46, rt.y+125,"Sets the overall size of the sun disc.");
			
					
			//moon
			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			GUI.Label (new Rect (rt.x+margin+10, rt.y+158, 100, 20), new GUIContent ("Moon Rendering"));
			string[] moonTypeOptions = new string[]{"Realistic","Custom","Off"};
			script.moonTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+158, setWidth, 18),"",script.moonTypeIndex, moonTypeOptions);

			if (script.moonTypeIndex == 2){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+178, 100, 20), new GUIContent ("Light Amount"));
			script.moonBright = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+178, setWidth, 15), "", script.moonBright, 0.0f, 5.0f);
	        ToolTip(rt.x+margin+98, rt.y+178,"Sets the brightness level of direct moon lighting.  Default is 1.0");
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+198, 100, 20), new GUIContent ("Light Saturation"));
			script.moonSat = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+198, setWidth, 15), "", script.moonSat, 0.0f, 1.0f);
        	ToolTip(rt.x+margin+111, rt.y+198,"Controls the color saturation of moon lighting, from full color (1.0) to greyscale value (0.0)");
			
        	GUI.Label (new Rect (rt.x+margin+20, rt.y+218, 180, 15), new GUIContent("Sky Amount (Night)"));
        	script.nightBrightness = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+218, setWidth, 18), "", script.nightBrightness,0.0f,1.0f);
	        ToolTip(rt.x+margin+135, rt.y+218,"Controls the brightness of the skybox during nighttime.  If you're using Tonemapping on your camera it's recommended to increase this setting.");
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+238, 180, 15), new GUIContent("Mie Amount"));
        	script.mieMnAmount = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+238, setWidth, 18), "", script.mieMnAmount,0.0f,2.0f);
	        ToolTip(rt.x+margin+90, rt.y+238,"Controls glow halo around moon object, drawn directly in skybox.");
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+258, 100, 20), new GUIContent ("Size"));
			script.moonSize = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+258, setWidth, 15), "", script.moonSize, 0.005f, 0.1f);
			ToolTip(rt.x+margin+46, rt.y+258,"Sets the overall size of the moon disc.");
			

			if (script.moonTypeIndex == 0){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

			GUI.Label (new Rect (rt.x+margin+20, rt.y+278, 100, 20), new GUIContent ("Custom Offset"));
			script.moonPos = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+278, setWidth, 15), "", script.moonPos, 0.0f, 359.0f);
			//GUI.Label (new Rect (rt.x+margin+20, rt.y+258, 100, 20), new GUIContent ("Phase"));
			//script.moonPhase = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+258, setWidth, 15), "", script.moonPhase, 0.0f, 359.0f);
			


			//stars
			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			GUI.Label (new Rect (rt.x+margin+10, rt.y+313, 120, 20), new GUIContent ("Star Rendering"));
			string[] starTypeOptions = new string[]{"Realistic","Custom","Off"};
			script.starTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+313, setWidth, 18),"",script.starTypeIndex, starTypeOptions);

			if (script.starTypeIndex == 2){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			
        	GUI.Label (new Rect (rt.x+margin+20, rt.y+333, 120, 15), new GUIContent("Brightness"));
        	script.starIntensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+333, setWidth, 18), "", script.starIntensity,0.0f,2.0f);

           	GUI.Label (new Rect (rt.x+margin+20, rt.y+353, 190, 15), new GUIContent("Planet Brightness"));
        	script.planetIntensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+353, setWidth, 18), "", script.planetIntensity,0.0f,2.0f);

			if (script.starTypeIndex == 0){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			
			GUI.Label (new Rect (rt.x+margin+20, rt.y+373, 120, 20), new GUIContent ("Star Offset"));
			script.starPos = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+373, setWidth, 15), "", script.starPos, 0.0f, 359.0f);

			

			//Aurora Borealis
			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			GUI.Label (new Rect (rt.x+margin+10, rt.y+404, 120, 20), new GUIContent ("Aurora Rendering"));
			string[] auroraTypeOptions = new string[]{"On","Off"};
			script.auroraTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+404, setWidth, 18),"",script.auroraTypeIndex, auroraTypeOptions);
			
			if (script.auroraTypeIndex == 1){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			
	        GUI.Label (new Rect (rt.x+margin+20, rt.y+424, 120, 20), new GUIContent ("Visible Latitude"));
			script.auroraLatitude = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+424, setWidth, 15), "", script.auroraLatitude, 0.0f, 90.0f);

			GUI.Label (new Rect (rt.x+margin+20, rt.y+444, 120, 20), new GUIContent("Brightness"));
        	script.auroraIntensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+444, setWidth, 18), "", script.auroraIntensity,0.0f,1.0f);

        	GUI.Label (new Rect (rt.x+margin+20, rt.y+464, 120, 20), new GUIContent("Speed"));
        	script.auroraSpeed = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+464, setWidth, 18), "", script.auroraSpeed,0.0f,2.0f);



			//Galaxy / night skybox
			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			GUI.Label (new Rect (rt.x+margin+10, rt.y+502, 120, 20), new GUIContent ("Galaxy Rendering"));
			string[] galaxyTypeOptions = new string[]{"Realistic","Custom","Off"};
			script.galaxyTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+502, setWidth, 18),"",script.galaxyTypeIndex, galaxyTypeOptions);
			
			if (script.galaxyTypeIndex == 2){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
        	GUI.Label (new Rect (rt.x+margin+20, rt.y+522, 180, 15), new GUIContent("Brightness"));
        	script.galaxyIntensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+522, setWidth, 18), "", script.galaxyIntensity,0.0f,2.0f);

			if (script.galaxyTypeIndex == 0){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			GUI.Label (new Rect (rt.x+margin+20, rt.y+542, 120, 20), new GUIContent ("Galaxy Offset"));
			script.galaxyPos = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+542, setWidth, 15), "", script.galaxyPos, 0.0f, 359.0f);

			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;
			if (script.galaxyTypeIndex == 2){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

         	GUI.Label (new Rect (rt.x+margin+20, rt.y+562, 100, 15), new GUIContent ("Galaxy Texture"));
         	string[] galaxyTexOptions = new string[]{"2D Spheremap","Custom","Cubemap"};
         	script.galaxyTexIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+562, setWidth, 18),"",script.galaxyTexIndex, galaxyTexOptions);
         	if (script.galaxyTexIndex == 0){
        		script.galaxyTex = EditorGUI.ObjectField(new Rect(rt.x+margin+165, rt.y+582, setWidth, 55), script.galaxyTex, typeof(Texture), true) as Texture;
			} else {
				script.galaxyCubeTex = EditorGUI.ObjectField(new Rect(rt.x+margin+165, rt.y+582, setWidth, 55), script.galaxyCubeTex, typeof(Texture), true) as Texture;
			}
        	
        	
        	GUILayout.Space(620.0f);
        }
        GUILayout.Space(10.0f);

        //#########################################################################################
        
        
        
        
        

        
        
               
        //ATMOSPHERICS
        GUI.contentColor = colorEnabled;
        GUI.backgroundColor = colorEnabled;
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showConfigAtmos = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showConfigAtmos, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("ATMOSPHERICS"));
        //float spaceAdjust = 0.0f;

        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showConfigAtmos = !script.showConfigAtmos;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);


        //if (script.useSunRays) spaceAdjust += 40;
			
        if (script.showConfigAtmos){
        	GUI.Label(new Rect(rt.x+margin,rt.y+340,387,34), GUIContent.none, divRevTexStyle);
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+340,387,34),divRevTex);
			GUILayout.Space(10.0f);


			//ATMOSPHERIC MODEL
			EditorGUI.LabelField(new Rect(rt.x+margin+10, rt.y+25, 180, 18),"Atmospheric Model");
			string[] atmosphereTypeOptions = new string[]{"Legacy Tenkoku", "Oskar-Elek 2009"}; //"Nishita (Unity)"
			script.atmosphereModelTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+25, setWidth, 18),"", script.atmosphereModelTypeIndex, atmosphereTypeOptions);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 180, 15), new GUIContent("Atmospheric Density"));
        	script.atmosphereDensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+45, setWidth, 18), "", script.atmosphereDensity,0.5f,4.0f);

        	if (script.atmosphereModelTypeIndex==1){
        		GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
        	}
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 180, 15), new GUIContent("Horizon Density"));
        	script.horizonDensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+65, setWidth, 18), "", script.horizonDensity,0.0f,6.0f);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+85, 180, 15), new GUIContent("Horizon Height"));
        	script.horizonDensityHeight = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+85, setWidth, 18), "", script.horizonDensityHeight,0.0f,1.0f);
        	
        	GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;


        	//FOG
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+125, 150, 15), new GUIContent("Enable Tenkoku Fog"));
        	script.enableFog = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+125, 60, 15), "", script.enableFog);


			if (!script.enableFog){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+145, 150, 15), new GUIContent("Auto Adjust Fog"));
        	script.autoFog = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+145, 60, 15), "", script.autoFog);


			if (script.enableFog && script.autoFog){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+165, 180, 15), new GUIContent("Start Fog"));
        	script.fogAtmosphere = EditorGUI.FloatField(new Rect(rt.x+margin+165, rt.y+165, setWidth, 18), "", script.fogAtmosphere);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+185, 180, 15), new GUIContent("Fog Distance"));
        	script.fogDistance = EditorGUI.FloatField(new Rect(rt.x+margin+165, rt.y+185, setWidth, 18), "", script.fogDistance);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+205, 180, 15), new GUIContent("Fade Distance"));
        	script.fadeDistance = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+205, setWidth, 18), "", script.fadeDistance,0.0f,1.0f);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+225, 180, 15), new GUIContent("Fog Obscurance"));
        	script.fogObscurance = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+225, setWidth, 18), "", script.fogObscurance,0.0f,1.0f);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+245, 180, 15), new GUIContent("Fog Density"));
        	script.fogDensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+245, setWidth, 18), "", script.fogDensity,0.0f,1.0f);

        	if (script.enableFog){
				GUI.contentColor = colorEnabled;
				GUI.backgroundColor = colorEnabled;
			}


        	//GUI.Label (new Rect (rt.x+margin+10, rt.y+265, 180, 15), new GUIContent("Fog Dispersion"));
     		//script.fogDispersion = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+265, setWidth, 18), "", script.fogDispersion,0f,10f);
     		
     		GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;



			//LIGHT RAYS
			GUI.Label (new Rect (rt.x+margin+10, rt.y+275, 150, 15), new GUIContent ("Calculate Light Rays"));
			script.useSunRays = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+275, 60, 15), "", script.useSunRays);
			
			if (!script.useSunRays){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			GUI.Label (new Rect (rt.x+margin+10, rt.y+295, 150, 15), new GUIContent ("Sun Ray Intensity"));
        	script.sunRayIntensity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+295, setWidth, 18), "", script.sunRayIntensity,0.0f,1.0f);

        	GUI.Label (new Rect (rt.x+margin+10, rt.y+315, 150, 15), new GUIContent ("Sun Ray Length"));
        	script.sunRayLength = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+315, setWidth, 18), "", script.sunRayLength,0.0f,1.0f);
     		
     		GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;


			//LIGHT Flare
			GUI.Label (new Rect (rt.x+margin+10, rt.y+345, 150, 15), new GUIContent ("Calculate Light Flare"));
			script.useSunFlare = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+345, 60, 15), "", script.useSunFlare);
			
			if (!script.useSunFlare){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}

			script.flareObject = EditorGUI.ObjectField(new Rect(rt.x+margin+195, rt.y+345, setWidth-30, 16), script.flareObject, typeof(Flare), true) as Flare;

     		GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;



        	GUILayout.Space(340.0f );
        }
        GUILayout.Space(10.0f);



        
        //WEATHER
        GUI.contentColor = colorEnabled;
        GUI.backgroundColor = colorEnabled;
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showConfigWeather = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showConfigWeather, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("WEATHER"));
        
        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showConfigWeather = !script.showConfigWeather;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);





        if (script.showConfigWeather){
        	
			GUILayout.Space(10.0f);

						
        	GUI.Label (new Rect (rt.x+margin+10, rt.y+25, 190, 15), new GUIContent("Weather Setting"));
        	string[] weatherTypeOptions = new string[]{"Manual","Automatic (Random)","Automatic (Advanced)"};
        	script.weatherTypeIndex = EditorGUI.Popup(new Rect(rt.x+margin+165, rt.y+25, setWidth, 18),"",script.weatherTypeIndex, weatherTypeOptions);


		    //GUI.Label (new Rect (rt.x+margin+30, rt.y+45, 190, 15), new GUIContent("Link Clouds to System Timer"));
			//script.cloudLinkToTime = EditorGUI.Toggle(new Rect(rt.x+margin+10, rt.y+45, 30, 18), "", script.cloudLinkToTime);


			//#if UNITY_5_3_4 || UNITY_5_3_5 || UNITY_5_3_6 || UNITY_5_4_OR_NEWER
		    //	GUI.Label (new Rect (rt.x+margin+230, rt.y+45, 190, 15), new GUIContent("Use Legacy Clouds"));
			//	script.useLegacyClouds = EditorGUI.Toggle(new Rect(rt.x+margin+210, rt.y+45, 30, 18), "", script.useLegacyClouds);


			//	if (script.useLegacyClouds){
			//		GUI.contentColor = colorDisabled;
        	//		GUI.backgroundColor = colorDisabled;
			//	}
	        // 	GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 180, 15), new GUIContent("Cloud Quality"));
	        //	script.cloudQuality = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+65, setWidth, 18), "", script.cloudQuality,0.0f,2.0f);

	        //	GUI.contentColor = colorEnabled;
        	//	GUI.backgroundColor = colorEnabled;
	        	
			//#else
			//    GUI.contentColor = colorDisabled;
        	//	GUI.backgroundColor = colorDisabled;
		    //	GUI.Label (new Rect (rt.x+margin+230, rt.y+45, 190, 15), new GUIContent("Use Legacy Clouds"));
			//	script.useLegacyClouds = EditorGUI.Toggle(new Rect(rt.x+margin+210, rt.y+45, 30, 18), "", script.useLegacyClouds);
			//	GUI.contentColor = colorEnabled;
        	//	GUI.backgroundColor = colorEnabled;
			//#endif


	        //GUI.Label (new Rect (rt.x+margin+10, rt.y+85, 180, 15), new GUIContent("Precipitation Quality"));
	        //script.precipQuality = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+85, setWidth, 18), "", script.precipQuality,0.01f,2.0f);


			if (script.weatherTypeIndex == 0){ //custom
				//GUI.Label(new Rect(rt.x+margin,rt.y,387,36), GUIContent.none, logoTexStyle);
				//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+440,387,34),divRevTex);

	         	GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 180, 15), new GUIContent("Clouds AltoStratus"));
	        	script.weather_cloudAltoStratusAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+45, setWidth, 18), "", script.weather_cloudAltoStratusAmt,0.0f,1.0f);
	        	
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 180, 15), new GUIContent("Clouds Cirrus"));
	        	script.weather_cloudCirrusAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+65, setWidth, 18), "", script.weather_cloudCirrusAmt,0.0f,1.0f);

	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+85, 180, 15), new GUIContent("Clouds Cumulus"));
	        	script.weather_cloudCumulusAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+85, setWidth, 18), "", script.weather_cloudCumulusAmt,0.0f,1.0f);
	        	
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+105, 180, 15), new GUIContent("Overcast Amount"));
	        	script.weather_OvercastAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+105, setWidth, 18), "", script.weather_OvercastAmt,0.0f,1.0f);

	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+125, 180, 15), new GUIContent("Overcast Darkening"));
	        	script.weather_OvercastDarkeningAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+125, setWidth, 18), "", script.weather_OvercastDarkeningAmt,0.0f,1.0f);


	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+145, 180, 15), new GUIContent("Cloud Scale"));
	        	script.weather_cloudScale = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+145, setWidth, 18), "", script.weather_cloudScale,0.0f,20.0f);
	        	       
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+165, 180, 15), new GUIContent("Cloud Speed"));
	        	script.weather_cloudSpeed = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+165, setWidth, 18), "", script.weather_cloudSpeed,0.0f,1.0f);
	        	       


	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+195, 180, 15), new GUIContent("Rain Amount"));
	        	script.weather_RainAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+195, setWidth, 18), "", script.weather_RainAmt,0.0f,1.0f);

	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+215, 180, 15), new GUIContent("Snow Amount"));
	        	script.weather_SnowAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+215, setWidth, 18), "", script.weather_SnowAmt,0.0f,1.0f);

	         	GUI.Label (new Rect (rt.x+margin+10, rt.y+235, 180, 15), new GUIContent("Fog Amount"));
	        	script.weather_FogAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+235, setWidth, 18), "", script.weather_FogAmt,0.0f,1.0f);
	       	
	          	GUI.Label (new Rect (rt.x+margin+10, rt.y+255, 180, 15), new GUIContent("Fog Max Height"));
	        	script.weather_FogHeight = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+255, setWidth, 18), "", script.weather_FogHeight,0.0f,10000.0f);
	       	


	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+285, 180, 15), new GUIContent("Wind Amount"));
	        	script.weather_WindAmt = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+285, setWidth, 18), "", script.weather_WindAmt,0.0f,1.0f);
	        	
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+305, 180, 15), new GUIContent("Wind Direction"));
	        	script.weather_WindDir = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+305, setWidth, 18), "", script.weather_WindDir,0.0f,359.0f);
			


				GUI.Label (new Rect (rt.x+margin+10, rt.y+335, 180, 15), new GUIContent("Temperature (f)"));
	        	script.weather_temperature = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+335, setWidth, 18), "", script.weather_temperature,0.0f,120.0f);

				GUI.Label (new Rect (rt.x+margin+10, rt.y+355, 180, 15), new GUIContent("Humidity"));
	        	script.weather_humidity = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+355, setWidth, 18), "", script.weather_humidity,0.0f,1.0f);


				GUI.Label (new Rect (rt.x+margin+10, rt.y+375, 180, 15), new GUIContent("Rainbow"));
	        	script.weather_rainbow = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+375, setWidth, 18), "", script.weather_rainbow,0.0f,1.0f);



				GUI.Label (new Rect (rt.x+margin+10, rt.y+405, 180, 15), new GUIContent("Lightning Amount"));
	        	script.weather_lightning = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+405, setWidth, 18), "", script.weather_lightning,0.0f,1.0f);

				GUI.Label (new Rect (rt.x+margin+10, rt.y+425, 180, 15), new GUIContent("Lightning Direction"));
	        	script.weather_lightningDir = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+425, setWidth, 18), "", script.weather_lightningDir,0.0f,359.0f);

				GUI.Label (new Rect (rt.x+margin+10, rt.y+445, 180, 15), new GUIContent("Lightning Range"));
	        	script.weather_lightningRange = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+445, setWidth, 18), "", script.weather_lightningRange,0.0f,180.0f);


				GUILayout.Space(440.0f);
			}


			if (script.weatherTypeIndex == 1){ //AUTO Simple
				//GUI.Label(new Rect(rt.x+margin,rt.y+540,387,34), GUIContent.none, divRevTexStyle);
				//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+540,387,34),divRevTex);

			    //GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 190, 15), new GUIContent("Link Clouds to Timer"));
        		//script.cloudLinkToTime = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+45, setWidth, 18), "", script.cloudLinkToTime);

	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 180, 15), new GUIContent("Cloud Speed"));
	        	script.weather_cloudSpeed = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+45, setWidth, 18), "", script.weather_cloudSpeed,0.0f,1.0f);
	        	       
				GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 180, 15), new GUIContent ("Next Random Pattern"));
				script.weather_forceUpdate = EditorGUI.Toggle(new Rect(rt.x+margin+165, rt.y+65, 60, 15), "", script.weather_forceUpdate);  
			
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+85, 180, 15), new GUIContent("Weather Pattern Lifetime"));
	        	script.weather_autoForecastTime = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+85, setWidth, 18), "", script.weather_autoForecastTime,0.01f,120.0f);
	        	
	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+105, 180, 15), new GUIContent("Weather Transition Speed"));
	        	script.weather_TransitionTime = EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+105, setWidth, 18), "", script.weather_TransitionTime,0.001f,60.0f);


	        	//Show Weather Variables Grayed Out
		        GUI.contentColor = colorDisabled2;
		        GUI.backgroundColor = colorDisabled2;


		         	GUI.Label (new Rect (rt.x+margin+10, rt.y+145, 180, 15), new GUIContent("Clouds AltoStratus"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+145, setWidth, 18), "", script.weather_cloudAltoStratusAmt,0.0f,1.0f);
		        	
		        	
		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+165, 180, 15), new GUIContent("Clouds Cirrus"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+165, setWidth, 18), "", script.weather_cloudCirrusAmt,0.0f,1.0f);

		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+185, 180, 15), new GUIContent("Clouds Cumulus"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+185, setWidth, 18), "", script.weather_cloudCumulusAmt,0.0f,1.0f);
		        	
		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+205, 180, 15), new GUIContent("Overcast Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+205, setWidth, 18), "", script.weather_OvercastAmt,0.0f,1.0f);

		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+225, 180, 15), new GUIContent("Overcast Darkening"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+225, setWidth, 18), "", script.weather_OvercastDarkeningAmt,0.0f,1.0f);


		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+245, 180, 15), new GUIContent("Cloud Scale"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+245, setWidth, 18), "", script.weather_cloudScale,0.0f,20.0f);
		        	       
		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+265, 180, 15), new GUIContent("Cloud Speed"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+265, setWidth, 18), "", script.weather_cloudSpeed,0.0f,1.0f);
		        	       


		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+295, 180, 15), new GUIContent("Rain Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+295, setWidth, 18), "", script.weather_RainAmt,0.0f,1.0f);

		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+315, 180, 15), new GUIContent("Snow Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+315, setWidth, 18), "", script.weather_SnowAmt,0.0f,1.0f);

		         	GUI.Label (new Rect (rt.x+margin+10, rt.y+335, 180, 15), new GUIContent("Fog Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+335, setWidth, 18), "", script.weather_FogAmt,0.0f,1.0f);
		       	
		          	GUI.Label (new Rect (rt.x+margin+10, rt.y+355, 180, 15), new GUIContent("Fog Max Height"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+355, setWidth, 18), "", script.weather_FogHeight,0.0f,10000.0f);
		       	


		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+385, 180, 15), new GUIContent("Wind Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+385, setWidth, 18), "", script.weather_WindAmt,0.0f,1.0f);
		        	
		        	GUI.Label (new Rect (rt.x+margin+10, rt.y+405, 180, 15), new GUIContent("Wind Direction"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+405, setWidth, 18), "", script.weather_WindDir,0.0f,359.0f);
				


					GUI.Label (new Rect (rt.x+margin+10, rt.y+435, 180, 15), new GUIContent("Temperature (f)"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+435, setWidth, 18), "", script.weather_temperature,0.0f,120.0f);

					GUI.Label (new Rect (rt.x+margin+10, rt.y+455, 180, 15), new GUIContent("Humidity"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+455, setWidth, 18), "", script.weather_humidity,0.0f,1.0f);


					GUI.Label (new Rect (rt.x+margin+10, rt.y+475, 180, 15), new GUIContent("Rainbow"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+475, setWidth, 18), "", script.weather_rainbow,0.0f,1.0f);



					GUI.Label (new Rect (rt.x+margin+10, rt.y+505, 180, 15), new GUIContent("Lightning Amount"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+505, setWidth, 18), "", script.weather_lightning,0.0f,1.0f);

					GUI.Label (new Rect (rt.x+margin+10, rt.y+525, 180, 15), new GUIContent("Lightning Direction"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+525, setWidth, 18), "", script.weather_lightningDir,0.0f,359.0f);

					GUI.Label (new Rect (rt.x+margin+10, rt.y+545, 180, 15), new GUIContent("Lightning Range"));
		        	EditorGUI.Slider(new Rect(rt.x+margin+165, rt.y+545, setWidth, 18), "", script.weather_lightningRange,0.0f,180.0f);




		        GUI.contentColor = colorEnabled;
		        GUI.backgroundColor = colorEnabled;

				GUILayout.Space(540.0f);

				//GUILayout.Space(100.0f);

			}



    		if (script.weatherTypeIndex == 2){ //AUTO Advanced
    			GUI.Label(new Rect(rt.x+margin,rt.y+90,387,34), GUIContent.none, divRevTexStyle);
				//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+90,387,34),divRevTex);

	        	GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 380, 15), new GUIContent("Advanced Weather is currently disabled. Coming Soon."));

				GUILayout.Space(90.0f);
			}    	
        }
        GUILayout.Space(10.0f);
        
    






       //SOUND EFFECTS
        GUI.contentColor = colorEnabled;
        GUI.backgroundColor = colorEnabled;
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        GUI.Label(new Rect(rt.x+margin,rt.y,387,24), GUIContent.none, divTexStyle);
        //EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y,387,24),divTex);
        script.showIBL = EditorGUI.Foldout(new Rect (rt.x+margin+3, rt.y+5, 20, 20), script.showIBL, "");
        GUI.Label (new Rect (rt.x+margin+10, rt.y+5, 300, 20), new GUIContent ("SOUND EFFECTS"));
        
        GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,0.0f);
		if (GUI.Button(new Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) script.showIBL = !script.showIBL;
		GUI.color = new Color(GUI.color.r,GUI.color.g,GUI.color.b,1.0f);

        if (script.showIBL){

        	GUI.Label(new Rect(rt.x+margin,rt.y+230,387,34), GUIContent.none, divRevTexStyle);
        	//EditorGUI.DrawPreviewTexture(new Rect(rt.x+margin,rt.y+230,387,34),divRevTex);
			GUILayout.Space(10.0f);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+25, 190, 15), new GUIContent("Enable Sound FX"));
        	script.enableSoundFX = EditorGUI.Toggle(new Rect(rt.x+margin+135, rt.y+25, 25, 18), "", script.enableSoundFX);

			GUI.Label (new Rect (rt.x+margin+190, rt.y+25, 190, 15), new GUIContent("Adjust via Timescale"));
        	script.enableTimeAdjust = EditorGUI.Toggle(new Rect(rt.x+margin+325, rt.y+25, 25, 18), "", script.enableTimeAdjust);



        	if (!script.enableSoundFX){
        		GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
        	}

			GUI.Label (new Rect (rt.x+margin+10, rt.y+45, 120, 15), new GUIContent ("Master Volume"));
			script.overallVolume = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+45, setWidth+20, 18), "", script.overallVolume,0.0f,1.0f);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+65, 120, 15), new GUIContent ("Use Audio Mixer"));


			int itemWidth = setWidth / 2;
			script.audioMixer = EditorGUI.ObjectField(new Rect(rt.x+margin+135, rt.y+65, Mathf.Max(60,itemWidth-50), 18), script.audioMixer, typeof(AudioMixer), true) as AudioMixer;
			GUI.Label (new Rect (rt.x+margin+itemWidth+110, rt.y+65, 80, 15), new GUIContent ("Group"));
			script.audioMixerGroup = EditorGUI.ObjectField(new Rect(rt.x+margin+itemWidth+156, rt.y+65, itemWidth, 18), script.audioMixerGroup, typeof(AudioMixerGroup), true) as AudioMixerGroup;



			GUI.Label (new Rect (rt.x+margin+10, rt.y+95, 120, 15), new GUIContent ("Wind Volume"));
			script.volumeWind = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+95, setWidth-55, 18), "", script.volumeWind,0.0f,1.0f);
			script.audioWind = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+85), rt.y+95, 70, 18), script.audioWind, typeof(AudioClip), true) as AudioClip;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+115, 120, 15), new GUIContent ("Low Turbulence"));
			script.volumeTurb1 = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+115, setWidth-55, 18), "", script.volumeTurb1,0.0f,1.0f);
			script.audioTurb1 = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+85), rt.y+115, 70, 18), script.audioTurb1, typeof(AudioClip), true) as AudioClip;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+135, 120, 15), new GUIContent ("High Turbulence"));
			script.volumeTurb2 = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+135, setWidth-55, 18), "", script.volumeTurb2,0.0f,1.0f);
			script.audioTurb2 = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+85), rt.y+135, 70, 18), script.audioTurb2, typeof(AudioClip), true) as AudioClip;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+155, 120, 15), new GUIContent ("Rain Volume"));
			script.volumeRain = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+155, setWidth-55, 18), "", script.volumeRain,0.0f,1.0f);
			script.audioRain = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+85), rt.y+155, 70, 18), script.audioRain, typeof(AudioClip), true) as AudioClip;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+175, 120, 15), new GUIContent ("Thunder Volume"));
			script.volumeThunder = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+175, setWidth-55, 18), "", script.volumeThunder,0.0f,1.0f);

			GUI.Label (new Rect (rt.x+margin+10, rt.y+205, 120, 15), new GUIContent ("Day Ambient"));
			script.volumeAmbDay = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+205, setWidth-95, 18), "", script.volumeAmbDay,0.0f,1.0f);
			script.curveAmbDay24 = EditorGUI.CurveField(new Rect(rt.x+margin+(setWidth+45), rt.y+204, 45, 10), "", script.curveAmbDay24);
			script.curveAmbDayYR = EditorGUI.CurveField(new Rect(rt.x+margin+(setWidth+45), rt.y+213, 45, 10), "", script.curveAmbDayYR);
			script.audioAmbDay = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+95), rt.y+205, 60, 18), script.audioAmbDay, typeof(AudioClip), true) as AudioClip;

			GUI.Label (new Rect (rt.x+margin+10, rt.y+225, 120, 15), new GUIContent ("Night Ambient"));
			script.volumeAmbNight = EditorGUI.Slider(new Rect(rt.x+margin+135, rt.y+225, setWidth-95, 18), "", script.volumeAmbNight,0.0f,1.0f);
			script.curveAmbNight24 = EditorGUI.CurveField(new Rect(rt.x+margin+(setWidth+45), rt.y+224, 45, 10), "", script.curveAmbNight24);
			script.curveAmbNightYR = EditorGUI.CurveField(new Rect(rt.x+margin+(setWidth+45), rt.y+233, 45, 10), "", script.curveAmbNightYR);
			script.audioAmbNight = EditorGUI.ObjectField(new Rect(rt.x+margin+(setWidth+95), rt.y+225, 60, 18), script.audioAmbNight, typeof(AudioClip), true) as AudioClip;

	        GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;


        	GUILayout.Space(220.0f);
        }
        
        GUILayout.Space(10.0f);    
        






        GUILayout.Space(10.0f);
        
        
        
        
        
        GUI.contentColor = colorEnabled;
        GUI.backgroundColor = colorEnabled;
        
        

        if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty(target);
		}


    }
    
    


    
}