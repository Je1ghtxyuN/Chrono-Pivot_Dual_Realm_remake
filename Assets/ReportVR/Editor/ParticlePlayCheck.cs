using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using Object=UnityEngine.Object;

// Real update/physics callbacks, rather than fabricated melting or Collision objects.
public static class ParticlePlayCheck
{
    static int phase,bare,covered;
    static double until,deadline;
    static ReportHeatMeasure heat;
    static ReportRollMeasure roll;
    static Component melting,snow;
    static ParticleSystem drops,powder;
    static readonly HashSet<uint> seeds=new HashSet<uint>();
    static readonly ParticleSystem.Particle[] particles=new ParticleSystem.Particle[64];
    static bool oldXR,oldOptionsEnabled,failed,meltPhoto,snowPhoto;
    static EnterPlayModeOptions oldOptions;
    static string error;
    static float fraction;
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static void Next(int value,double delay){phase=value;until=EditorApplication.timeSinceStartup+delay;seeds.Clear();}
    static void Observe(ParticleSystem system){int n=system.GetParticles(particles);for(int i=0;i<n;i++)seeds.Add(particles[i].randomSeed);}
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXR=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[12].path);phase=0;failed=false;error=null;deadline=EditorApplication.timeSinceStartup+75;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)error=message;}
    static void Tick()
    {
        try
        {
            if(phase==99)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXR;
                EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;AssetDatabase.SaveAssets();EditorApplication.Exit(failed?1:0);return;
            }
            Require(EditorApplication.timeSinceStartup<deadline,"Particle play test timed out, phase "+phase);
            if(!EditorApplication.isPlaying)return;Require(error==null,"Runtime error: "+error);
            if(phase>=1 && phase<=5)Observe(drops);if(phase==7 || phase==8)Observe(powder);
            if(phase==2 && drops.particleCount>0 && !meltPhoto){meltPhoto=true;ReportParticleUpgrade.Photograph("melting",heat.transform.position-heat.transform.forward*1.2f+Vector3.up*1.4f,heat.sample.transform.position);}
            if(phase==7 && powder.particleCount>0 && !snowPhoto){snowPhoto=true;ReportParticleUpgrade.Photograph("snow-impact-test",roll.ball.transform.position+new Vector3(-.4f,.5f,-.6f),roll.ball.transform.position);}
            if(EditorApplication.timeSinceStartup<until)return;
            switch(phase)
            {
                case 0:
                    var ui=Object.FindObjectOfType<ReportGameUI>();if(ui!=null){ui.Resume();ui.enabled=false;}Time.timeScale=1;
                    heat=Object.FindObjectOfType<ReportHeatMeasure>();roll=Object.FindObjectOfType<ReportRollMeasure>();
                    melting=heat.GetComponent("ReportMeltParticles");snow=roll.ball.GetComponent("ReportSnowParticles");
                    Require(melting!=null && snow!=null,"Missing melting or snow particle feedback");
                    drops=melting.GetComponentInChildren<ParticleSystem>();powder=snow.GetComponentInChildren<ParticleSystem>();
                    heat.sample.GetComponent<Rigidbody>().isKinematic=true;heat.insulation.GetComponent<Rigidbody>().isKinematic=true;
                    heat.sample.transform.position=heat.warmZone.position+Vector3.right;heat.insulation.transform.position=heat.coverSeat.position+Vector3.right;
                    Object.FindObjectOfType<DemoLevel>().ResetLevel();heat.sample.transform.position=heat.warmZone.position+Vector3.right;heat.insulation.transform.position=heat.coverSeat.position+Vector3.right;
                    Next(1,.4);break;
                case 1:
                    Require(seeds.Count==0,"Ice outside warm zone must not drip");heat.sample.transform.position=heat.warmZone.position;fraction=heat.MeltFraction;Next(2,.8);break;
                case 2:
                    bare=seeds.Count;Require(heat.MeltFraction>fraction && bare>=3,"Actual melting must emit sparse drops; fraction="+heat.MeltFraction+" drops="+bare+" scale="+Time.timeScale+" sample="+heat.sample.transform.position+" warm="+heat.warmZone.position);
                    Object.FindObjectOfType<DemoLevel>().ResetLevel();Require(drops.particleCount==0,"Reset must clear melting drops");
                    Next(20,.2);break;
                case 20:
                    heat.sample.transform.position=heat.warmZone.position;heat.insulation.transform.position=heat.coverSeat.position;Next(3,.8);break;
                case 3:
                    covered=seeds.Count;Require(heat.MeltFraction>0 && covered<bare*.5f,"Insulation must reduce droplets with the actual melting rate");
                    heat.sample.GetComponent<Rigidbody>().position=heat.warmZone.position+Vector3.right;heat.sample.transform.position=heat.warmZone.position+Vector3.right;Next(30,.15);break;
                case 30:
                    drops.Clear();fraction=heat.MeltFraction;Next(4,.4);break;
                case 4:
                    Require(seeds.Count==0 && heat.MeltFraction==fraction,"Moving ice out of warmth must stop new droplets");heat.sample.transform.position=heat.warmZone.position;Time.timeScale=0;Next(5,.4);break;
                case 5:
                    Require(seeds.Count==0 && heat.MeltFraction==fraction,"Pause must not emit droplets");Time.timeScale=1;
                    melting.gameObject.SetActive(false);Require(drops.particleCount==0,"Disabling must clear drops");
                    var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Particle test floor";floor.transform.position=new Vector3(35,0,0);floor.transform.localScale=new Vector3(4,.2f,4);
                    roll.ball.GetComponent<Rigidbody>().isKinematic=true;roll.ball.transform.position=new Vector3(35,1.6f,0);powder.Clear();Next(6,.35);break;
                case 6:
                    Require(powder.particleCount==0,"Idle snow must not emit powder");var body=roll.ball.GetComponent<Rigidbody>();body.isKinematic=false;body.velocity=Vector3.down;Next(7,.7);break;
                case 7:
                    Require(seeds.Count>0,"Real released snowball collision must emit powder");Require(seeds.Count<=10,"Single impact must have a restrained burst");powder.Clear();Next(8,.5);break;
                case 8:
                    Require(seeds.Count==0,"Resting contact must not emit continuous powder");Object.FindObjectOfType<DemoLevel>().ResetLevel();Require(powder.particleCount==0,"Reset must clear snow powder");
                    Directory.CreateDirectory("Documentation/ParticleReview");File.WriteAllText("Documentation/ParticleReview/play.txt","PASS: real melting, insulation, leaving warmth, idle, pause, reset, disable; real Rigidbody snowball collision and resting contact. Bare drops="+bare+" covered drops="+covered+"\n");
                    Debug.Log("PARTICLE_PLAY_OK");Finish(false);break;
            }
        }
        catch(Exception e){Debug.LogException(e);Directory.CreateDirectory("Documentation/ParticleReview");File.WriteAllText("Documentation/ParticleReview/play.txt","FAIL phase="+phase+" "+e);Finish(true);}
    }
    static void Finish(bool failure){failed=failure;Time.timeScale=1;phase=99;EditorApplication.isPlaying=false;}
}
