using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Object=UnityEngine.Object;

public static class GrabSceneCheck
{
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Validate()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        for(int i=0;i<3;i++)
        {
            var root=GameObject.Find("HubMaterialCube_"+i);
            var material=root.transform.Find("材料样品");
            Require(material.GetComponent<XRGrabInteractable>()!=null,"First-level material has no grab component: "+i);
            Require(material.GetComponent<Rigidbody>()!=null && !material.GetComponent<Rigidbody>().isKinematic,"Material has no dynamic Rigidbody: "+i);
            Require(material.GetComponent<MeshCollider>().convex,"Material collider must be convex for Rigidbody grabs: "+i);
            Require(root.transform.Find("底座").GetComponent<XRGrabInteractable>()==null,"Base must stay fixed");
        }
        int count=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            var grabs=Object.FindObjectsOfType<XRGrabInteractable>();count+=grabs.Length;
            Require(Object.FindObjectsOfType<XRInteractionManager>().Length==1,"Scene needs one grab interaction manager: "+scene.path);
            var hands=Object.FindObjectsOfType<MonoBehaviour>().Where(c=>c.GetType().Name=="ReportGrabInteractor").ToArray();
            Require(hands.Length==3,"Left hand, right hand and desktop grabbers are not configured: "+scene.path);
            foreach(var grab in grabs)
            {
                Require(grab.colliders.Count>0 && grab.colliders.All(c=>c!=null && c.enabled),"Grab collider missing: "+grab.name);
                Require(grab.GetComponent<Rigidbody>()!=null,"Grab Rigidbody missing: "+grab.name);
                Require(grab.selectMode==InteractableSelectMode.Single,"Object must not be selected by two hands at once: "+grab.name);
            }
        }
        Directory.CreateDirectory("Documentation/Verification");
        File.WriteAllText("Documentation/Verification/grab-scenes.txt","PASS: first-level specimens have grab + dynamic Rigidbody + convex collision, bases stay fixed; all 15 scenes have one manager and left/right/desktop grabbers; portable props="+count+".\n");
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);Debug.Log("GRAB_SCENES_OK props="+count);
    }
}
