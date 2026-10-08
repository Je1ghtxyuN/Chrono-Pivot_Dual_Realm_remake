using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportLevelAudit
{
    public const string Output="Documentation/LevelReview";
    public static string[] Scenes()=>EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
    public static void Baseline(){Audit();DemoTests();}
    public static void Audit()
    {
        Directory.CreateDirectory(Output+"/Before");
        var lines=new List<string>();
        string original=EditorSceneManager.GetActiveScene().path;
        EditorSceneManager.SaveOpenScenes();
        foreach(string path in Scenes())
        {
            EditorSceneManager.OpenScene(path);lines.Add("\nSCENE "+Path.GetFileNameWithoutExtension(path));
            foreach(var f in Object.FindObjectsOfType<MeshFilter>().OrderBy(f=>f.name))
            {
                var r=f.GetComponent<Renderer>();if(r==null)continue;
                lines.Add(f.name+" | pos="+f.transform.position.ToString("F2")+" | min="+r.bounds.min.ToString("F2")+" max="+r.bounds.max.ToString("F2"));
            }
            var origin=GameObject.Find("Report VR Origin");lines.Add("SPAWN "+origin.transform.position);
            Photograph(Output+"/Before/"+Path.GetFileNameWithoutExtension(path)+"_top.png",new Vector3(0,32,0),Vector3.zero,true,22);
            Photograph(Output+"/Before/"+Path.GetFileNameWithoutExtension(path)+"_eye.png",origin.transform.position+Vector3.up*1.65f,origin.transform.position+Vector3.up*1.4f+origin.transform.forward*7,false,22);
        }
        File.WriteAllLines(Output+"/before-bounds.txt",lines);
        if(!string.IsNullOrEmpty(original))EditorSceneManager.OpenScene(original);
    }
    public static void Photograph(string path,Vector3 position,Vector3 look,bool ortho,float size)
    {
        var go=new GameObject("Temporary review camera");var c=go.AddComponent<Camera>();c.enabled=false;c.stereoTargetEye=StereoTargetEyeMask.None;c.transform.position=position;c.transform.LookAt(look,ortho?Vector3.forward:Vector3.up);
        c.orthographic=ortho;c.orthographicSize=size;c.fieldOfView=70;c.farClipPlane=200;c.clearFlags=CameraClearFlags.Skybox;c.depthTextureMode=DepthTextureMode.Depth;
        if(ortho)c.cullingMask&=~(1<<29); // Hide only the ceiling in plan-view review images.
        var rt=new RenderTexture(1600,1000,24);var pix=new Texture2D(1600,1000,TextureFormat.RGB24,false);
        c.targetTexture=rt;c.Render();RenderTexture.active=rt;pix.ReadPixels(new Rect(0,0,1600,1000),0,0);pix.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,pix.EncodeToPNG());
        RenderTexture.active=null;c.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(pix);Object.DestroyImmediate(go);
    }
    public static void DemoTests()
    {
        // Missing ordered steps, reset, or density/frequency gates must fail these behavioral checks.
        var types=AppDomain.CurrentDomain.GetAssemblies().SelectMany(a=>a.GetTypes()).ToArray();
        Type physics=types.FirstOrDefault(t=>t.Name=="DemoPhysics");
        Require(physics!=null,"Missing playable demo physics and objective scripts");
        Func<string,object[],object> call=(name,args)=>physics.GetMethod(name).Invoke(null,args);
        Require(Mathf.Abs((float)call("SubmergedFraction",new object[]{100f,1000f,.2f})-.5f)<.001f,"100kg/0.2m3 must submerge halfway");
        Require((float)call("SubmergedFraction",new object[]{300f,1000f,.2f})==1f,"Overloaded hull must sink, not rise");
        Require(!(bool)call("FrequencyMatches",new object[]{100f,110f}),"Mistuned resonance must stay locked");
        Require((bool)call("FrequencyMatches",new object[]{100f,102f}),"Within tolerance resonance must unlock");
        Require(Mathf.Abs((float)call("PendulumFrequency",new object[]{1f})-.4985f)<.002f,"1m pendulum should have about .5Hz frequency");
        Type level=types.FirstOrDefault(t=>t.Name=="DemoLevel");Type station=types.FirstOrDefault(t=>t.Name=="DemoStation");
        Require(level!=null && station!=null,"Missing objective components");
        var root=new GameObject("Objective behavioral test");var l=root.AddComponent(level);
        var a=new GameObject("First").AddComponent(station);var b=new GameObject("Second").AddComponent(station);
        station.GetField("index").SetValue(a,0);station.GetField("index").SetValue(b,1);
        var array=Array.CreateInstance(station,2);array.SetValue(a,0);array.SetValue(b,1);level.GetField("stations").SetValue(l,array);
        level.GetMethod("Use").Invoke(l,new object[]{b});Require((int)level.GetProperty("Step").GetValue(l)==0,"Out of order use must not advance");
        level.GetMethod("Use").Invoke(l,new object[]{a});Require((int)level.GetProperty("Step").GetValue(l)==1,"First successful task should advance once");
        level.GetMethod("Use").Invoke(l,new object[]{a});Require((int)level.GetProperty("Step").GetValue(l)==1,"Repeated completed task must not skip next objective");
        level.GetMethod("Use").Invoke(l,new object[]{b});Require((bool)level.GetProperty("Completed").GetValue(l),"All ordered tasks should complete");
        level.GetMethod("ResetLevel").Invoke(l,null);Require((int)level.GetProperty("Step").GetValue(l)==0,"Reset must restore first task");
        Require((int)station.GetProperty("Attempts").GetValue(a)==0,"Reset must clear station state");
        Object.DestroyImmediate(a.gameObject);Object.DestroyImmediate(b.gameObject);Object.DestroyImmediate(root);
        Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/demo-behavior.txt","PASS: ordered objectives; repeated/out-of-order protection; complete/reset; overload buoyancy; resonance tolerance; pendulum frequency.\n");
    }
    static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
}
