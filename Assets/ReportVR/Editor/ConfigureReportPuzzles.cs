using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class ConfigureReportPuzzles
{
    static DemoLevel level;
    static Transform layout;
    static string key;
    static readonly List<string> guide=new List<string>();
    static GameObject Find(string name)=>Object.FindObjectsOfType<Transform>(true).FirstOrDefault(t=>t.name==name)?.gameObject;
    static ReportGrabbable Item(string root,string child)=>Find(root).transform.Find(child).GetComponent<ReportGrabbable>();
    static ReportPuzzle New(int step,string title,string goal,string clue,string hint)
    {
        var station=level.stations[step];var old=station.transform.Find("Puzzle Controls");if(old!=null)Object.DestroyImmediate(old.gameObject);
        if(station.puzzle!=null)Object.DestroyImmediate(station.puzzle);
        var oldLearning=station.GetComponent<ReportLearningTask>();if(oldLearning!=null)Object.DestroyImmediate(oldLearning);
        var puzzle=station.gameObject.AddComponent<ReportPuzzle>();station.puzzle=puzzle;puzzle.station=station;
        station.instruction=title;station.completionDelay=0;puzzle.goal=goal;puzzle.clue=clue;puzzle.hint=hint;
        puzzle.successMessage=title+"检验通过。";
        return puzzle;
    }
    static ReportPuzzle Choice(int step,string title,string goal,string clue,string[] choices,int answer,string hint,float[] values=null)
    {
        var puzzle=New(step,title,goal,clue,hint);puzzle.mode=ReportPuzzleMode.Choice;puzzle.choices=choices;puzzle.correctChoice=answer;
        puzzle.station.values=values??new[]{0f,0f,0f};Controls(puzzle);
        guide.Add("\n### "+(step+1)+". "+title+"\n\n目标："+goal+"\n\n线索："+clue+"\n\n解法：选择 "+(char)('A'+answer)+"「"+choices[answer]+"」，再按验证。\n");return puzzle;
    }
    static ReportPuzzle Place(int step,string title,string goal,string clue,string hint,ReportGrabbable[] items,string[] labels)
    {
        var puzzle=New(step,title,goal,clue,hint);puzzle.mode=ReportPuzzleMode.Placement;puzzle.station.values=new[]{100f};
        puzzle.placements=new PuzzlePlacement[items.Length];
        for(int i=0;i<items.Length;i++)
        {
            var location=FreeSocketLocation(puzzle.station);
            var model=new LevelModelBuilder();model.Box(Vector3.up*.05f,new Vector3(.74f,.1f,.74f),"SageStone");model.Box(Vector3.up*.43f,new Vector3(.14f,.74f,.14f),"SatinBrass");model.Box(Vector3.up*.85f,new Vector3(.76f,.12f,.76f),"Celadon");
            var stand=model.Save(key+"_PuzzleSocket_"+step+"_"+i,layout);stand.transform.position=location;
            float half=items[i].GetComponent<Renderer>().bounds.extents.y;
            var socket=new GameObject("槽位_"+labels[i]).transform;socket.SetParent(stand.transform,false);socket.localPosition=Vector3.up*(.91f+half+.02f);
            puzzle.placements[i]=new PuzzlePlacement{label=labels[i],item=items[i],socket=socket,tolerance=.4f};
            var seating=socket.gameObject.AddComponent<ReportPlacementSocket>();seating.item=items[i];seating.captureDistance=.4f;
            var plate=new LevelModelBuilder();plate.Box(Vector3.zero,new Vector3(.72f,.24f,.05f),"Ivory");var sign=plate.Save(key+"_SocketLabel_"+step+"_"+i,stand.transform,false);
            sign.transform.position=location+Vector3.up*.66f;sign.transform.rotation=Quaternion.LookRotation(sign.transform.position-puzzle.station.viewingPoint);
            Text(sign.transform,"槽位文字",new Vector3(0,0,-.031f),labels[i],.013f);
            LabelItem(items[i],labels[i],puzzle.station.viewingPoint);
            Physics.SyncTransforms();
        }
        Controls(puzzle);guide.Add("\n### "+(step+1)+". "+title+"\n\n目标："+goal+"\n\n线索："+clue+"\n\n解法：将「"+string.Join("、",labels)+"」分别放到同名槽位，松手后按验证。\n");return puzzle;
    }
    static string Wrap(string value,int count)
    {
        return string.Join("\n",value.Split('\n').Select(line=>{
            for(int i=line.Length/count;i>0;i--)if(i*count<line.Length)line=line.Insert(i*count,"\n");return line;
        }));
    }
    static TextMesh Text(Transform parent,string name,Vector3 position,string text,float size)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var mesh=go.AddComponent<TextMesh>();mesh.text=text;mesh.fontSize=48;mesh.characterSize=size;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=new Color(.025f,.045f,.055f);
        var facing=go.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.WorldFixed;facing.viewer=Object.FindObjectOfType<XROrigin>().Camera.transform;
        go.GetComponent<Renderer>().sharedMaterial=Object.FindObjectOfType<DemoInteractor>().worldText;return mesh;
    }
    static void LabelItem(ReportGrabbable item,string label,Vector3 viewingPoint)
    {
        var old=item.transform.Find("Puzzle Item Label");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var labelRoot=new GameObject("Puzzle Item Label").transform;labelRoot.SetParent(item.transform,false);
        var bounds=item.GetComponent<Renderer>().bounds;labelRoot.position=bounds.center+Vector3.up*(bounds.extents.y+.16f);labelRoot.rotation=Quaternion.LookRotation(labelRoot.position-viewingPoint);
        var model=new LevelModelBuilder();model.Box(Vector3.zero,new Vector3(.65f,.22f,.04f),"Ivory");var board=model.Save(key+"_ItemLabel_"+item.transform.parent.name+"_"+item.name,labelRoot,false);
        var text=Text(board.transform,"物件标记",new Vector3(0,0,-.025f),label,.012f);text.GetComponent<ReportTextFacing>().mode=ReportTextFacingMode.FollowParent;
    }
    static void Controls(ReportPuzzle puzzle)
    {
        var root=new GameObject("Puzzle Controls").transform;root.SetParent(puzzle.station.transform,false);
        var plate=new LevelModelBuilder();plate.Box(new Vector3(0,2.14f,.02f),new Vector3(2.5f,1.0f,.06f),"Ivory");
        plate.Beam(new Vector3(-.6f,1.35f,.04f),new Vector3(-.6f,2.62f,.04f),.045f,"SatinBrass");plate.Beam(new Vector3(.6f,1.35f,.04f),new Vector3(.6f,2.62f,.04f),.045f,"SatinBrass");plate.Save(key+"_PuzzleClue_"+puzzle.station.index,root,false);
        Text(root,"目标与线索",new Vector3(0,2.14f,-.022f),Wrap("目标："+puzzle.goal+"\n线索："+puzzle.clue,22),.015f);
        var buttons=new List<ReportPuzzleButton>();
        for(int i=0;i<puzzle.choices.Length;i++)
        {
            var model=new LevelModelBuilder();model.Box(Vector3.zero,new Vector3(.49f,.29f,.08f),"Celadon");
            var button=model.Save(key+"_PuzzleChoice_"+puzzle.station.index+"_"+i,root);button.transform.localPosition=new Vector3((i-1)*.54f,.92f,-.72f);
            TouchTarget(button);
            var input=button.AddComponent<ReportPuzzleButton>();input.puzzle=puzzle;input.choiceIndex=i;input.indicator=button.GetComponent<Renderer>();buttons.Add(input);
            Text(button.transform,"选项文字",new Vector3(0,0,-.047f),((char)('A'+i))+"\n"+Wrap(puzzle.choices[i],7),.011f);
        }
        var hintModel=new LevelModelBuilder();hintModel.Box(Vector3.zero,new Vector3(.3f,.17f,.08f),"GlacierBlue");
        var hint=hintModel.Save(key+"_PuzzleHint_"+puzzle.station.index,root);hint.transform.localPosition=new Vector3(.56f,1.13f,-.72f);
        TouchTarget(hint);
        var hintInput=hint.AddComponent<ReportPuzzleButton>();hintInput.puzzle=puzzle;hintInput.kind=ReportPuzzleButtonKind.Hint;hintInput.indicator=hint.GetComponent<Renderer>();buttons.Add(hintInput);Text(hint.transform,"提示文字",new Vector3(0,0,-.046f),"提示",.012f);
        puzzle.station.indicator.transform.localPosition=new Vector3(-.2f,1.11f,-.72f);TouchTarget(puzzle.station.indicator.gameObject);Text(root,"验证文字",new Vector3(-.2f,1.12f,-.834f),"验证",.011f);
        var shelf=new LevelModelBuilder();shelf.Box(new Vector3(0,.74f,-.4f),new Vector3(1.7f,.05f,.7f),"InkTeal");shelf.Save(key+"_PuzzleButtonShelf_"+puzzle.station.index,root,false);
        puzzle.buttons=buttons.ToArray();puzzle.ResetPuzzle();
    }
    static void TouchTarget(GameObject go)
    {
        var mesh=go.GetComponent<MeshCollider>();if(mesh!=null)Object.DestroyImmediate(mesh);
        var box=go.GetComponent<BoxCollider>();if(box==null)box=go.AddComponent<BoxCollider>();box.center=go.GetComponent<MeshFilter>().sharedMesh.bounds.center;box.size=go.GetComponent<MeshFilter>().sharedMesh.bounds.size;box.isTrigger=true;
    }
    static float RouteDistance(Vector3 p)
    {
        float result=float.PositiveInfinity;p.y=0;
        for(int i=0;i<level.route.Length-1;i++)
        {
            var a=level.route[i];var b=level.route[i+1];a.y=b.y=0;float t=Mathf.Clamp01(Vector3.Dot(p-a,b-a)/Mathf.Max(.001f,(b-a).sqrMagnitude));result=Mathf.Min(result,Vector3.Distance(p,Vector3.Lerp(a,b,t)));
        }
        return result;
    }
    static Vector3 FreeSocketLocation(DemoStation station,float routeClearance=.82f)
    {
        Physics.SyncTransforms();
        for(float radius=1.8f;radius<=3.01f;radius+=.3f)for(int angle=0;angle<32;angle++)
        {
            var p=station.viewingPoint+Quaternion.Euler(0,angle*11.25f,0)*Vector3.right*radius;
            if(RouteDistance(p)<routeClearance)continue;
            if(!Physics.Raycast(p+Vector3.up*.7f,Vector3.down,out var floor,1.3f,~0,QueryTriggerInteraction.Ignore) || floor.normal.y<.85f || Mathf.Abs(floor.point.y-station.viewingPoint.y)>.25f)continue;
            p.y=floor.point.y;
            if(Physics.OverlapBox(p+Vector3.up*.72f,new Vector3(.5f,.67f,.5f),Quaternion.identity,~0,QueryTriggerInteraction.Ignore).Any(c=>!(c is CharacterController) && c.bounds.max.y>p.y+.09f))continue;
            return p;
        }
        throw new Exception("No reachable clear socket space near "+station.name);
    }
    static void ExitLock()
    {
        if(level.exitDoor!=null)return;
        var exit=Object.FindObjectOfType<DemoExit>();var frameRoot=new GameObject("谜题出口门").transform;frameRoot.SetParent(layout,false);
        frameRoot.position=exit.transform.position;var route=level.route;frameRoot.rotation=Quaternion.LookRotation(route[route.Length-1]-route[route.Length-2]);
        var frame=new LevelModelBuilder();foreach(int side in new[]{-1,1})frame.Box(new Vector3(side*1.24f,1.25f,0),new Vector3(.12f,2.5f,.15f),"InkTeal");frame.Box(Vector3.up*2.51f,new Vector3(2.6f,.12f,.15f),"InkTeal");frame.Save(key+"_PuzzleExitFrame",frameRoot);
        var leaf=new LevelModelBuilder();leaf.Box(Vector3.up*1.19f,new Vector3(2.3f,2.38f,.1f),"SoftGlass");leaf.Box(new Vector3(.89f,1.1f,-.075f),new Vector3(.06f,.25f,.06f),"SatinBrass");
        level.exitDoor=leaf.Save(key+"_PuzzleExitLeaf",frameRoot).transform;level.exitDoorTravel=Vector3.right*2.7f;
    }
    [MenuItem("Report VR/Configure Natural Physics Puzzles")]
    public static void Apply()=>ConfigureNaturalLearning.All();
    public static ReportPuzzle C(int step,string title,string goal,string clue,string[] options,int answer,string hint,float[] values=null)=>Choice(step,title,goal,clue,options,answer,hint,values);
    public static ReportPuzzle P(int step,string title,string goal,string clue,string hint,ReportGrabbable[] items,string[] labels)=>Place(step,title,goal,clue,hint,items,labels);
    public static ReportGrabbable I(string root,string child)=>Item(root,child);
    public static void BeginNatural(DemoLevel target,string sceneKey,Transform parent){level=target;key=sceneKey;layout=parent;}
    public static Vector3 ExperimentLocation(DemoStation station)=>FreeSocketLocation(station,1.12f);
    public static void EnsureExit()=>ExitLock();
    public static ReportPuzzle Natural(int step,string title,string goal,string clue,string hint)
    {
        var puzzle=New(step,title,goal,clue,hint);puzzle.mode=ReportPuzzleMode.Physical;
        puzzle.station.action=DemoAction.Inspect;puzzle.station.values=new[]{0f};
        puzzle.learningTask=puzzle.gameObject.AddComponent<ReportLearningTask>();Controls(puzzle);
        var label=puzzle.station.transform.Find("Puzzle Controls/验证文字").GetComponent<TextMesh>();label.text="状态";
        var collider=puzzle.station.indicator.GetComponent<Collider>();if(collider!=null)Object.DestroyImmediate(collider);
        return puzzle;
    }
    public static ReportGrabbable CreateSeal(int supplyStep)
    {
        var location=FreeSocketLocation(level.stations[supplyStep]);
        var standModel=new LevelModelBuilder();standModel.Box(Vector3.up*.05f,new Vector3(.74f,.1f,.74f),"SageStone");standModel.Box(Vector3.up*.43f,new Vector3(.14f,.74f,.14f),"SatinBrass");standModel.Box(Vector3.up*.85f,new Vector3(.76f,.12f,.76f),"Celadon");
        var stand=standModel.Save(key+"_SealSupply",layout);stand.transform.position=location;
        var ring=new LevelModelBuilder();
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;
            var outerA=new Vector3(Mathf.Cos(a)*.19f,0,Mathf.Sin(a)*.19f);var outerB=new Vector3(Mathf.Cos(b)*.19f,0,Mathf.Sin(b)*.19f);
            var innerA=outerA*(.12f/.19f);var innerB=outerB*(.12f/.19f);var up=Vector3.up*.035f;
            ring.Face(LevelModelBuilder.Mat("CoralRed"),outerA+up,innerA+up,innerB+up,outerB+up);
            ring.Face(LevelModelBuilder.Mat("CoralRed"),outerB-up,innerB-up,innerA-up,outerA-up);
            ring.Face(LevelModelBuilder.Mat("CoralRed"),outerA-up,outerA+up,outerB+up,outerB-up);
            ring.Face(LevelModelBuilder.Mat("CoralRed"),innerB-up,innerB+up,innerA+up,innerA-up);
        }
        var go=ring.Save(key+"_PortableSeal",stand.transform);go.transform.localPosition=Vector3.up*.96f;
        return ConfigureReportGrabs.Portable(go,Object.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>());
    }
}
