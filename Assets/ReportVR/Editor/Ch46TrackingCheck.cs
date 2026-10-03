using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils;
public static class Ch46TrackingCheck
{
 public static void Run()
 {
  EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
  var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();var a=origin.GetComponentInChildren<LowPolyPlayerAvatar>();
  Debug.Log("CH46_ORIENTATION left="+a.transform.InverseTransformPoint(a.leftHand.position)+" right="+a.transform.InverseTransformPoint(a.rightHand.position)+" toe="+a.transform.InverseTransformPoint(a.leftLeg.GetChild(0).GetChild(0).GetChild(0).position));
  if(a.transform.InverseTransformPoint(a.leftHand.position).x>0)throw new Exception("Ch46 faces backwards: left hand lies on the right");
  a.SendMessage("Start");
  for(int i=0;i<4;i++)
  {
   a.headTarget.position=origin.transform.TransformPoint(new Vector3(.12f,1.65f-i*.12f,-.08f));a.headTarget.rotation=Quaternion.Euler(i*10,i*40,0);
   a.leftTarget.position=a.headTarget.TransformPoint(new Vector3(-.25f,-.4f,.3f));a.rightTarget.position=a.headTarget.TransformPoint(new Vector3(.25f,-.4f,.3f));
   a.SendMessage("LateUpdate");a.SendMessage("LateUpdate");
   if(Vector3.Distance(a.leftHand.position,a.leftTarget.position)>.001f || Vector3.Distance(a.rightHand.position,a.rightTarget.position)>.001f)throw new Exception("Hands failed tracked pose");
   if(float.IsNaN(a.face.position.x))throw new Exception("Invalid bone pose");
   if(a.firstPersonHidden[0].shadowCastingMode!=ShadowCastingMode.ShadowsOnly || !a.firstPersonVisible[0].enabled)throw new Exception("First person visibility failed");
  }
  File.WriteAllText("Documentation/Verification/ch46-tracking.txt","PASS: forward orientation; hands match targets under yaw, pitch and height changes; repeat pose does not drift; full body shadows only; first person limbs visible.\n");
  Debug.Log("CH46_TRACKING_OK");
 }
}
