using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class AcousticExperienceCheck
{
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static void Call(Component value,string method,params object[] args)=>value.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(value,args);
    public static void Verify()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[2].path);
        var measures=Object.FindObjectsOfType<ReportAcousticMeasure>();
        var strike=measures.Single(m=>m.experiment==ReportAcousticReading.Strike);
        var bank=Resources.Load<ReportSoundBank>("InteractionSoundBank");
        Require(bank!=null && bank.Find(ReportSound.BellStrike)!=null && bank.Find(ReportSound.BellStrike).clip!=null,"Bell sound asset is missing");
        Call(strike,"Start");
        Require(strike.GetComponents<AudioSource>().Length==0,"Bell strike duplicates its ringing with a continuous synthesized tone");
        var resonance=measures.Single(m=>m.experiment==ReportAcousticReading.Resonance);
        Call(resonance.frequency,"Awake");
        resonance.frequency.SetValue(resonance.naturalFrequency);Call(resonance,"Update");
        Require(resonance.Amplitude>.99f,"Matching drive must produce near-maximum resonance");
        resonance.frequency.SetValue(resonance.frequency.valueRange.x);Call(resonance,"Update");
        Require(resonance.Amplitude<.35f,"Far-off drive must visibly suppress resonance");
        var beats=measures.Single(m=>m.experiment==ReportAcousticReading.Beats);
        beats.station=null; // Isolate the PCM check from sequential level activation.
        var fineField=typeof(ReportAcousticMeasure).GetField("fineFrequency");
        Require(fineField!=null,"Beat experiment lacks fine tuning");
        var fine=(ReportMechanismHandle)fineField.GetValue(beats);
        Require(fine!=null,"Fine tuning is not connected in scene");
        Call(beats.frequency,"Awake");Call(fine,"Awake");
        beats.frequency.SetValue(beats.naturalFrequency-1);fine.SetValue(1);Call(beats,"Update");
        Require(beats.Reading<.001f,"Coarse and fine controls must add to the same audible frequency");
        var samples=new float[22050];Call(beats,"OnAudioFilterRead",samples,1);
        Require(samples.Any(v=>Mathf.Abs(v)>.01f),"Matched sources must remain audible rather than cancelling to silence");
        Require(samples.All(v=>!float.IsNaN(v) && Mathf.Abs(v)<=.241f),"Synthesized signal has invalid or excessive samples");
        var envelope=typeof(ReportAcousticMeasure).GetProperty("BeatEnvelope");
        Require(envelope!=null,"Beat visualization lacks actual audio envelope");
        Require((float)envelope.GetValue(beats)>.99f,"Equal sources must have a steady envelope");
        beats.frequency.SetValue(beats.naturalFrequency-1);fine.SetValue(0);Call(beats,"Update");
        Call(beats,"OnAudioFilterRead",samples,1);
        Require((float)envelope.GetValue(beats)<.02f,"A 1Hz beat must reach a trough after half a second");
        Call(beats,"OnAudioFilterRead",samples,1);
        Require((float)envelope.GetValue(beats)>.99f,"A 1Hz beat must return to its crest after one second");
        strike.ResetMeasurements();Require(strike.Strikes==0,"Reset must clear strikes");
        beats.ResetMeasurements();Require((float)envelope.GetValue(beats)>.99f,"Reset must restart beat phase");
        Directory.CreateDirectory("Documentation/AcousticReview");
        File.WriteAllText("Documentation/AcousticReview/physics.txt","PASS: single bell sound path; resonance on/off target; coarse+fine tuning; bounded PCM; actual 1Hz audio envelope trough/crest; reset.\n");
        Debug.Log("ACOUSTIC_PHYSICS_OK");
    }
    public static void VerifyAndPreview()
    {
        Verify();
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[2].path);
        foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
        foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}
        foreach(var measure in Object.FindObjectsOfType<ReportAcousticMeasure>())
        {
            Call(measure,"Start");
            if(measure.frequency!=null){Call(measure.frequency,"Awake");measure.frequency.SetValue(measure.naturalFrequency);}
            Call(measure,"Update");
            material.mainTexture=font.material.mainTexture;
            var root=measure.experiment==ReportAcousticReading.Strike?measure.transform.parent:measure.transform;
            var view=root.position-root.forward*2.3f+Vector3.up*1.75f;
            foreach(var text in Object.FindObjectsOfType<TextMesh>())text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
            ReportLevelAudit.Photograph("Documentation/AcousticReview/"+measure.experiment+".png",view,root.position+Vector3.up*1.35f,false,1);
        }
        Debug.Log("ACOUSTIC_PREVIEW_OK");
    }
    public static void BuildVerified(){ReportAcousticUpgrade.Scene();VerifyAndPreview();SetupReportVR.BuildWindows();Debug.Log("ACOUSTIC_RELEASE_OK");}
}
