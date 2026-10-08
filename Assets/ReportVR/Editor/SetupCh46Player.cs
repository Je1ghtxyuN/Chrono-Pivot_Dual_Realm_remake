using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class SetupCh46Player
{
 const string Folder="Assets/ReportVR/Characters/Ch46";
 const string Prefab="Assets/ReportVR/Characters/Ch46Player.prefab";
 static Transform Bone(GameObject go,string name)=>go.GetComponentsInChildren<Transform>().Single(t=>t.name=="mixamorig:"+name);
 static void SaveAsset(Object asset,string path)
 {
  var old=AssetDatabase.LoadMainAssetAtPath(path);
  if(old!=null){EditorUtility.CopySerialized(asset,old);Object.DestroyImmediate(asset);EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(asset,path);
 }
 static Mesh CreateFirstPersonHands(SkinnedMeshRenderer renderer)
 {
  var mesh=Object.Instantiate(renderer.sharedMesh);mesh.name="Ch46 first person hands";
  var weights=mesh.boneWeights;var bones=renderer.bones;
  var visible=new bool[mesh.vertexCount];
  for(int i=0;i<visible.Length;i++)visible[i]=bones[weights[i].boneIndex0].name.Contains("Hand");
  var indices=mesh.triangles;var keep=new List<int>();
  for(int i=0;i<indices.Length;i+=3)if(visible[indices[i]] && visible[indices[i+1]] && visible[indices[i+2]])keep.AddRange(new[]{indices[i],indices[i+1],indices[i+2]});
  mesh.triangles=keep.ToArray();return mesh;
 }
 [MenuItem("Report VR/Update First Person Hands Only")]
 public static void UpdateFirstPersonHands()
 {
  var avatar=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab).GetComponent<LowPolyPlayerAvatar>();
  SaveAsset(CreateFirstPersonHands((SkinnedMeshRenderer)avatar.firstPersonHidden[0]),Folder+"/Ch46FirstPerson.asset");
  AssetDatabase.SaveAssets();Validate();Ch46TrackingCheck.Run();Debug.Log("CH46_HANDS_ONLY_OK");
 }
 [MenuItem("Report VR/Use Ch46 as Main Player")]
 public static void Apply()
 {
  Directory.CreateDirectory(Folder);
  var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/character/Ch46_nonPBR.fbx");
  importer.importAnimation=false;importer.isReadable=true;importer.SaveAndReimport();
  Directory.CreateDirectory("Assets/character/Textures");importer.ExtractTextures("Assets/character/Textures");AssetDatabase.Refresh();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/character/Textures/Ch46_1001_Diffuse.png");
  if(texture==null)throw new Exception("Embedded diffuse texture missing");
  var ti=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));ti.maxTextureSize=2048;ti.mipmapEnabled=true;ti.SaveAndReimport();
  var material=new Material(Shader.Find("Report VR/Ch46 Palette")){name="Ch46 Jade Ivory Ink"};material.mainTexture=texture;SaveAsset(material,Folder+"/Ch46Palette.mat");material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Ch46Palette.mat");
  var root=new GameObject("Ch46 · 青绿游学者");
  var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/character/Ch46_nonPBR.fbx"),root.transform);model.name="Ch46 Model";
  foreach(var a in model.GetComponentsInChildren<Animator>())Object.DestroyImmediate(a);
  var renderer=model.GetComponentInChildren<SkinnedMeshRenderer>();
  float scale=1.8f/renderer.bounds.size.y;model.transform.localScale=Vector3.one*scale;
  model.transform.localPosition=Vector3.up*(-renderer.bounds.min.y);
  var mesh=Object.Instantiate(renderer.sharedMesh);mesh.name="Ch46 styled body";
  var colors=new Color[mesh.vertexCount];var weights=mesh.boneWeights;
  var points=mesh.vertices;var bones=renderer.bones;
  for(int i=0;i<points.Length;i++)
  {
   var p=root.transform.InverseTransformPoint(renderer.transform.TransformPoint(points[i]));
   int index=weights[i].boneIndex0;string name=bones[index].name;
   bool head=name.Contains("Head") || name.Contains("Neck");
   bool hand=name.Contains("Hand");
   Color c=p.y<.32f?new Color(.35f,.20f,.13f):p.y<.57f?new Color(.91f,.86f,.70f):p.y<.95f?new Color(.065f,.12f,.15f):new Color(.10f,.40f,.38f);
   if(!head && !hand && p.y>1.27f && Mathf.Abs(p.x)<.13f)c=new Color(.91f,.86f,.70f);
   c.a=head || hand?0:1;colors[i]=c;
  }
  mesh.colors=colors;SaveAsset(mesh,Folder+"/Ch46Body.asset");renderer.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Ch46Body.asset");renderer.sharedMaterials=new[]{material};renderer.updateWhenOffscreen=true;
  var fp=Object.Instantiate(renderer.gameObject,renderer.transform.parent);fp.name="First person hands";
  var fpRenderer=fp.GetComponent<SkinnedMeshRenderer>();fpRenderer.bones=renderer.bones;fpRenderer.rootBone=renderer.rootBone;
  SaveAsset(CreateFirstPersonHands(renderer),Folder+"/Ch46FirstPerson.asset");fpRenderer.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Ch46FirstPerson.asset");fpRenderer.shadowCastingMode=ShadowCastingMode.Off;fpRenderer.enabled=false;
  var avatar=root.AddComponent<LowPolyPlayerAvatar>();avatar.skinnedRig=true;avatar.torso=Bone(root,"Hips");avatar.face=Bone(root,"Head");
  avatar.leftUpperArm=Bone(root,"LeftArm");avatar.leftForearm=Bone(root,"LeftForeArm");avatar.leftHand=Bone(root,"LeftHand");avatar.leftLeg=Bone(root,"LeftUpLeg");
  avatar.rightUpperArm=Bone(root,"RightArm");avatar.rightForearm=Bone(root,"RightForeArm");avatar.rightHand=Bone(root,"RightHand");avatar.rightLeg=Bone(root,"RightUpLeg");
  avatar.headOffset=new Vector3(0,avatar.face.position.y-1.65f,-.06f);avatar.firstPersonHidden=new Renderer[]{renderer};avatar.firstPersonVisible=new Renderer[]{fpRenderer};
  PrefabUtility.SaveAsPrefabAsset(root,Prefab);Object.DestroyImmediate(root);
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
  foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
  {
   EditorSceneManager.OpenScene(entry.path);var origin=Object.FindObjectOfType<XROrigin>();
   foreach(var old in origin.GetComponentsInChildren<LowPolyPlayerAvatar>())Object.DestroyImmediate(old.gameObject);
   var avatarInstance=((GameObject)PrefabUtility.InstantiatePrefab(prefab,origin.transform)).GetComponent<LowPolyPlayerAvatar>();
   avatarInstance.headTarget=origin.Camera.transform;avatarInstance.leftTarget=origin.transform.Find("Camera Floor Offset/Left Controller");avatarInstance.rightTarget=origin.transform.Find("Camera Floor Offset/Right Controller");avatarInstance.firstPerson=true;
   PrefabUtility.RecordPrefabInstancePropertyModifications(avatarInstance);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
  }
  AssetDatabase.SaveAssets();Validate();Capture();Debug.Log("CH46_SETUP_OK");
 }
 public static void Validate()
 {
  int count=0;
  foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
  {
   EditorSceneManager.OpenScene(entry.path);var origin=Object.FindObjectOfType<XROrigin>();var avatars=origin.GetComponentsInChildren<LowPolyPlayerAvatar>();
   if(avatars.Length!=1 || !avatars[0].skinnedRig)throw new Exception("Ch46 main avatar missing: "+entry.path);
   var a=avatars[0];if(a.headTarget!=origin.Camera.transform || a.leftTarget==null || a.rightTarget==null || !a.firstPerson)throw new Exception("Tracking binding missing");
   if(a.GetComponentsInChildren<Collider>().Length!=0)throw new Exception("Avatar blocks grabs");
   if(a.firstPersonVisible.Length!=1 || a.firstPersonHidden.Length!=1)throw new Exception("First person meshes missing");
   var hands=a.firstPersonVisible[0] as SkinnedMeshRenderer;
   if(hands==null || hands.sharedMesh!=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Ch46FirstPerson.asset"))throw new Exception("Shared first person hand mesh missing: "+entry.path);
   var r=a.firstPersonHidden[0] as SkinnedMeshRenderer;if(r==null || r.sharedMaterial.shader.name!="Report VR/Ch46 Palette")throw new Exception("Style material missing");
   if(a.GetComponentsInChildren<Animator>().Length!=0)throw new Exception("Animation overrides tracked bones");count++;
  }
  Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/ch46-scenes.txt","PASS: "+count+" scenes; Ch46 skinned player; head and controller bindings; jade/ivory/ink matte palette; hands-only first-person mesh; no avatar colliders or Animator conflicts.\n");
 }
 public static void Capture()
 {
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.67f,.68f);
  var light=new GameObject("Studio key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.Euler(30,155,0);
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
  var front=(GameObject)PrefabUtility.InstantiatePrefab(prefab);front.transform.position=new Vector3(-.85f,0,0);front.transform.rotation=Quaternion.Euler(0,-15,0);
  var back=(GameObject)PrefabUtility.InstantiatePrefab(prefab);back.transform.position=new Vector3(.85f,0,0);back.transform.rotation=Quaternion.Euler(0,165,0);
  var holder=new GameObject("Tracked preview figures").transform;
  foreach(var figure in new[]{front,back})
  {
   figure.transform.SetParent(holder,true);var a=figure.GetComponent<LowPolyPlayerAvatar>();
   a.headTarget=new GameObject(figure.name+" head target").transform;a.headTarget.SetParent(holder);a.headTarget.position=figure.transform.TransformPoint(new Vector3(0,1.65f,0));a.headTarget.rotation=figure.transform.rotation;
   a.leftTarget=new GameObject(figure.name+" left target").transform;a.leftTarget.SetParent(holder);a.leftTarget.position=figure.transform.TransformPoint(new Vector3(-.25f,1.22f,.24f));a.leftTarget.rotation=figure.transform.rotation;
   a.rightTarget=new GameObject(figure.name+" right target").transform;a.rightTarget.SetParent(holder);a.rightTarget.position=figure.transform.TransformPoint(new Vector3(.25f,1.22f,.24f));a.rightTarget.rotation=figure.transform.rotation;
   a.SendMessage("Start");a.SendMessage("LateUpdate");
  }
  var camera=new GameObject("Ch46 preview camera").AddComponent<Camera>();camera.transform.position=new Vector3(0,1.1f,4);camera.transform.LookAt(new Vector3(0,.9f,0));camera.orthographic=true;camera.orthographicSize=1.12f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.76f,.84f,.82f);
  var rt=new RenderTexture(1600,1000,24);rt.antiAliasing=4;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var pixels=new Texture2D(1600,1000,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,1000),0,0);pixels.Apply();
  Directory.CreateDirectory("Documentation/CharacterPreviews");File.WriteAllBytes("Documentation/CharacterPreviews/Ch46Player.png",pixels.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(pixels);
  EditorSceneManager.SaveScene(scene,"Assets/ReportVR/Characters/Ch46Preview.unity");
 }
}

