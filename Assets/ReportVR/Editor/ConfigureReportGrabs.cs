using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class ConfigureReportGrabs
{
    const string Inputs="Assets/ReportVR/Input/ReportVR.inputactions";
    static InputActionReference Grip(InputActionAsset input,string name,string hand)
    {
        var action=input.FindAction(name);
        if(action==null)
        {
            action=input.actionMaps[0].AddAction(name,InputActionType.Button,expectedControlLayout:"Button");
            action.AddBinding("<XRController>{"+hand+"}/gripPressed");
            action.AddBinding("<XRController>{"+hand+"}/grip",interactions:"press(pressPoint=0.6)");
        }
        string path="Assets/ReportVR/Input/References/"+name+".asset";
        var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
        if(reference==null){reference=InputActionReference.Create(action);AssetDatabase.CreateAsset(reference,path);}
        return reference;
    }
    static void Interactor(Transform source,DesktopPlayer player,XRInteractionManager manager,InputActionReference input,bool desktop,InteractorHandedness hand)
    {
        var grabber=source.GetComponent<ReportGrabInteractor>();
        if(grabber==null)grabber=source.gameObject.AddComponent<ReportGrabInteractor>();
        grabber.player=player;grabber.desktopOnly=desktop;grabber.grabInput=input;grabber.handedness=hand;grabber.interactionManager=manager;
        if(grabber.attachTransform==null)
        {
            var attach=new GameObject("抓取挂点").transform;attach.SetParent(source,false);attach.localPosition=Vector3.forward*(desktop?grabber.desktopHoldDistance:.04f);grabber.attachTransform=attach;
        }
    }
    public static ReportGrabbable Portable(GameObject go,XRInteractionManager manager)
    {
        if(go.GetComponent<ReportGrabbable>()!=null)return go.GetComponent<ReportGrabbable>();
        var collider=go.GetComponent<MeshCollider>();
        if(collider==null || !collider.enabled)throw new Exception("No portable collider on "+go.name);
        collider.convex=true;GameObjectUtility.SetStaticEditorFlags(go,0);
        var body=go.AddComponent<Rigidbody>();body.mass=1;body.useGravity=true;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        var grab=go.AddComponent<ReportGrabbable>();grab.interactionManager=manager;grab.colliders.Clear();grab.colliders.Add(collider);
        grab.selectMode=InteractableSelectMode.Single;grab.movementType=XRBaseInteractable.MovementType.Kinematic;
        grab.useDynamicAttach=false;grab.attachEaseInTime=.12f;grab.throwOnDetach=true;grab.throwVelocityScale=1;grab.throwAngularVelocityScale=1;
        return grab;
    }
    [MenuItem("Report VR/Configure Portable Object Grabs")]
    public static void Apply()
    {
        var input=AssetDatabase.LoadAssetAtPath<InputActionAsset>(Inputs);
        var left=Grip(input,"GrabLeft","LeftHand");var right=Grip(input,"GrabRight","RightHand");
        File.WriteAllText(Inputs,input.ToJson());AssetDatabase.ImportAsset(Inputs,ImportAssetOptions.ForceUpdate);
        Directory.CreateDirectory("Documentation/LevelReview/GrabBackups");int count=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            string backup="Documentation/LevelReview/GrabBackups/"+Path.GetFileName(scene.path);if(!File.Exists(backup))File.Copy(scene.path,backup);
            EditorSceneManager.OpenScene(scene.path);
            var manager=Object.FindObjectOfType<XRInteractionManager>();if(manager==null)manager=new GameObject("Report Grab Interaction Manager").AddComponent<XRInteractionManager>();
            var origin=Object.FindObjectOfType<XROrigin>();var player=origin.GetComponent<DesktopPlayer>();
            Interactor(origin.transform.Find("Camera Floor Offset/Left Controller"),player,manager,left,false,InteractorHandedness.Left);
            Interactor(origin.transform.Find("Camera Floor Offset/Right Controller"),player,manager,right,false,InteractorHandedness.Right);
            Interactor(origin.Camera.transform,player,manager,null,true,InteractorHandedness.None);
            var wrist=origin.transform.Find("Camera Floor Offset/Left Controller/Scene and controller instructions");
            if(wrist!=null)wrist.GetComponent<TextMesh>().text="Left stick: walk | Right stick: turn\nHold either grip: grab | Release: drop\nRight grip on controls: use | Left menu / R: reset\nR / L trigger: next / previous scene";
            foreach(var t in Object.FindObjectsOfType<Transform>(true))
            {
                string parent=t.parent!=null?t.parent.name:"";
                bool portable=parent.StartsWith("HubMaterialCube_") && t.name=="材料样品"
                    || parent=="CanalMaterialWeightBench" && t.name.StartsWith("配重_")
                    || parent=="AcousticMalletAndBronzeIngot" && (t.name=="铜锭" || t.name=="钟槌")
                    || parent=="WorkshopModularPhysicsPartsLibrary" && t.name.StartsWith("模块_")
                    || parent=="SnowballAssetComparisons" && t.name.StartsWith("雪球_");
                if(portable)Portable(t.gameObject,manager);
            }
            var level=Object.FindObjectOfType<DemoLevel>();level.grabbables=Object.FindObjectsOfType<ReportGrabbable>();count+=level.grabbables.Length;
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();GrabSceneCheck.Validate();PolishReportLevels.Validate();
        Debug.Log("REPORT_GRABS_CONFIGURED props="+count);
    }
}
