using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class RemainingPreview
{
    static void Call(Component component,string method)
    {component.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(component,null);}
    public static void All()
    {
        int count=0;
        for(int index=4;index<15;index++)
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
            foreach(var handle in Object.FindObjectsOfType<ReportMechanismHandle>())Call(handle,"Awake");
            foreach(var socket in Object.FindObjectsOfType<ReportPlacementSocket>())Call(socket,"Awake");
            foreach(var power in Object.FindObjectsOfType<ReportElectromagnetMeasure>()){Call(power,"Awake");Call(power,"Update");}
            foreach(var hydraulic in Object.FindObjectsOfType<ReportHydraulicMeasure>()){Call(hydraulic,"Awake");Call(hydraulic,"Update");}
            foreach(var pendulum in Object.FindObjectsOfType<ReportPendulumMeasure>())Call(pendulum,"FixedUpdate");
            foreach(var lc in Object.FindObjectsOfType<ReportLCMeasure>())Call(lc,"Update");
            foreach(var dial in Object.FindObjectsOfType<ReportDialMeasure>())Call(dial,"Update");
            foreach(var sight in Object.FindObjectsOfType<ReportSightMeasure>())Call(sight,"Update");
            foreach(var sun in Object.FindObjectsOfType<ReportSunlightMeasure>())Call(sun,"LateUpdate");
            foreach(var heat in Object.FindObjectsOfType<ReportHeatMeasure>()){Call(heat,"Awake");Call(heat,"Update");}
            foreach(var stack in Object.FindObjectsOfType<ReportStackMeasure>())Call(stack,"Update");
            foreach(var wheel in Object.FindObjectsOfType<ReportWheelLiftMeasure>()){Call(wheel,"Awake");Call(wheel,"Update");}
            foreach(var boat in Object.FindObjectsOfType<ReportBuoyancyMeasure>()){Call(boat,"Awake");Call(boat,"FixedUpdate");}
            foreach(var mirror in Object.FindObjectsOfType<ReportMirrorMeasure>()){mirror.mirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");}
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
            var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
            foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}
            foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
            var roots=Object.FindObjectsOfType<ReportTaskFeedback>().OrderBy(f=>f.station.index).Select(f=>f.transform.parent.parent).Distinct().ToArray();
            for(int i=0;i<roots.Length;i++)
            {
                var root=roots[i];var group=root.Find("Later level feedback");
                var panel=group.Cast<Transform>().FirstOrDefault(t=>t.name.EndsWith("_panel"));
                var forward=panel!=null?panel.forward:root.forward;
                var view=root.position-forward*2.7f+Vector3.up*1.8f;
                if(root.GetComponentInChildren<ReportRollMeasure>()!=null)view-=root.right*.65f;
                foreach(var text in Object.FindObjectsOfType<TextMesh>())
                    if(text.GetComponent<ReportTextFacing>()?.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
                material.mainTexture=font.material.mainTexture;
                ReportLevelAudit.Photograph("Documentation/RemainingReview/Level"+(index+1).ToString("D2")+"_"+(i+1)+".png",view,root.position+Vector3.up*1.42f,false,1);count++;
            }
            Object.DestroyImmediate(material);Object.DestroyImmediate(font);
        }
        // Temporary poses/readout fonts are never saved back to the scenes.
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        Debug.Log("REMAINING_PREVIEW_OK images="+count);
    }
    public static void BuildVerified(){ReportRemainingUpgrade.All();RemainingExperienceCheck.Verify();All();SetupReportVR.BuildWindows();Debug.Log("REMAINING_RELEASE_OK");}
}
