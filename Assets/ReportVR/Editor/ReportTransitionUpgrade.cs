using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ReportTransitionUpgrade
{
    [MenuItem("Report VR/Particles/Prepare Scene Travel")]
    public static void Assets()
    {
        const string folder="Assets/ReportVR/Resources";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var shader=Shader.Find("ChronoPivot/TransitionFade");if(shader==null || ShaderUtil.ShaderHasError(shader))throw new Exception("Travel fade shader failed to compile");
        var fade=Material(folder+"/TransitionFade.mat",shader);fade.color=new Color(.018f,.04f,.045f,0);EditorUtility.SetDirty(fade);
        var grains=Material(folder+"/TransitionParticles.mat",Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        grains.renderQueue=4100;grains.SetColor("_TintColor",new Color(.5f,.5f,.5f,.5f));grains.SetFloat("_InvFade",30);grains.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ReportVR/Water/Droplet.asset");EditorUtility.SetDirty(grains);
        AssetDatabase.SaveAssets();Debug.Log("SCENE_TRANSITION_ASSETS_OK");
    }
    static Material Material(string path,Shader shader)
    {
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}return material;
    }
    public static void Build(){Assets();SetupReportVR.BuildWindows();Debug.Log("SCENE_TRANSITION_BUILD_OK");}
}
