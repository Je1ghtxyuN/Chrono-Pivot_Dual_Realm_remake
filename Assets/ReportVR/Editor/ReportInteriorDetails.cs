using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object=UnityEngine.Object;

// Native background storage and workshop fittings. No interaction, colliders or scene saving.
public static class ReportInteriorDetails
{
    public static void Add(int index,DemoLevel level,Transform parent)
    {
        if(!new[]{4,5,6,8,9,13}.Contains(index))return;
        string prefix="Details_Interior_"+index.ToString("D2")+"_";
        foreach(var child in parent.Cast<Transform>().Where(t=>t.name.StartsWith(prefix)).ToArray())Object.DestroyImmediate(child.gameObject);
        float width=index==4?16:index==5 || index==6?18:index==8?21:22;
        float depth=index==4?12:index==5?13:index==6?14:index==8 || index==13?16:17;
        var placed=new List<Vector3>();int count=0;
        for(int group=0;group<3;group++)
        {
            if(!Placement(width,depth,level,parent,placed,group,out var position,out var rotation))
            {Debug.LogWarning("INTERIOR_DETAILS_NO_CLEAR_WALL index="+index+" group="+group);continue;}
            string key=prefix+group;var go=new GameObject(key+"_"+(group==0?"壁面备件架":group==1?"角落器材柜":"实验配套装置"));
            go.transform.SetParent(parent,false);go.transform.SetPositionAndRotation(position,rotation);placed.Add(position);
            if(group==0)WallRack(index,go.transform,key);else if(group==1)Cabinet(index,go.transform,key);else Service(index,go.transform,key);
            count++;
        }
        int triangles=parent.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith(prefix)).Sum(f=>f.sharedMesh.triangles.Length/3);
        if(triangles>=10000)throw new System.InvalidOperationException("Interior detail triangle budget exceeded: "+index+" / "+triangles);
        Debug.Log("INTERIOR_DETAILS_ADDED index="+index+" groups="+count+" triangles="+triangles);
    }
    static bool Placement(float width,float depth,DemoLevel level,Transform parent,List<Vector3> placed,int group,out Vector3 position,out Quaternion rotation)
    {
        // A fitting is 1.6m wide and at most .65m deep, facing from the wall into the room.
        var candidates=new List<(Vector3 position,Quaternion rotation)>();
        foreach(int side in new[]{group==1?-1:1,group==1?1:-1})
            foreach(float z in new[]{-.28f,.02f,.42f,-.42f,.22f,-.14f})
                candidates.Add((new Vector3(side*(width*.5f-.18f),.06f,z*depth),Quaternion.Euler(0,-side*90,0)));
        foreach(float x in new[]{-.30f,.30f,-.18f,.18f})
            candidates.Add((new Vector3(x*width,.06f,depth*.5f-.18f),Quaternion.Euler(0,180,0)));
        var renderers=Object.FindObjectsOfType<MeshRenderer>().Where(r=>r.enabled && r.gameObject.activeInHierarchy
            && r.bounds.size.x<5 && r.bounds.size.z<5).ToArray();
        var texts=Object.FindObjectsOfType<TextMesh>().Select(t=>t.GetComponent<Renderer>()).ToArray();
        foreach(var candidate in candidates)
        {
            Vector3 p=candidate.position;var q=candidate.rotation;
            if(placed.Any(v=>Vector3.ProjectOnPlane(v-p,Vector3.up).magnitude<2.1f))continue;
            Vector3 center=p+q*new Vector3(0,1.24f,.34f);
            if(RouteDistance(level,center)<1.55f)continue;
            if(level.stations.Any(s=>Vector3.ProjectOnPlane(s.viewingPoint-center,Vector3.up).magnitude<1.6f))continue;
            // Test the whole fitting volume, including the space in front of its shelves.
            Vector3 right=q*Vector3.right,forward=q*Vector3.forward;
            Vector3 size=new Vector3(Mathf.Abs(right.x)*1.7f+Mathf.Abs(forward.x)*.68f,2.48f,
                Mathf.Abs(right.z)*1.7f+Mathf.Abs(forward.z)*.68f);
            var box=new Bounds(center,size);
            if(renderers.Any(r=>r.bounds.max.y>p.y+.13f && box.Intersects(r.bounds)))continue;
            if(texts.Any(r=>r.gameObject.activeInHierarchy && box.Intersects(r.bounds)))continue;
            position=p;rotation=q;return true;
        }
        position=Vector3.zero;rotation=Quaternion.identity;return false;
    }
    static float RouteDistance(DemoLevel level,Vector3 p)
    {
        p.y=0;float distance=float.PositiveInfinity;
        for(int i=1;i<level.route.Length;i++)
        {
            Vector3 a=level.route[i-1],b=level.route[i];a.y=0;b.y=0;Vector3 delta=b-a;
            float t=delta.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(p-a,delta)/delta.sqrMagnitude):0;
            distance=Mathf.Min(distance,Vector3.Distance(p,a+delta*t));
        }
        return distance;
    }
    static GameObject Model(LevelModelBuilder mesh,string key,Transform parent,string label)
    {var go=mesh.Save(key,parent,false);go.name=key+"_"+label;return go;}
    static void Box(Transform parent,string key,Vector3 position,Vector3 size,string material,string label)
    {
        var mesh=new LevelModelBuilder();mesh.Box(Vector3.zero,Vector3.one,material);
        var go=Model(mesh,key,parent,label);go.transform.localPosition=position;go.transform.localScale=size;
    }
    static void Panel(LevelModelBuilder mesh,float bottom,float top)
    {
        mesh.Box(new Vector3(0,(bottom+top)*.5f,.025f),new Vector3(1.55f,top-bottom,.045f),"Ivory");
        foreach(float x in new[]{-.80f,.80f})mesh.Box(new Vector3(x,(bottom+top)*.5f,.04f),new Vector3(.04f,top-bottom+.06f,.07f),"Celadon");
        foreach(float y in new[]{bottom,top})mesh.Box(new Vector3(0,y,.04f),new Vector3(1.64f,.04f,.07f),"SatinBrass");
    }
    static void WallRack(int index,Transform root,string key)
    {
        var frame=new LevelModelBuilder();Panel(frame,1.36f,2.22f);
        frame.Box(new Vector3(0,1.34f,.17f),new Vector3(1.65f,.065f,.36f),"Celadon");
        foreach(float x in new[]{-.58f,.58f})frame.Beam(new Vector3(x,1.1f,.07f),new Vector3(x,1.31f,.32f),.035f,"SatinBrass");
        Model(frame,key+"_Backing",root,"浅壁柜背板");
        if(index==4 || index==9)
        {
            var coil=new LevelModelBuilder();
            for(int i=0;i<6;i++)Ring(coil,new Vector3(-.45f+i*.15f,1.83f,.22f),.14f,.019f,"Copper",Quaternion.Euler(0,90,0));
            Tube(coil,new Vector3(-.54f,1.83f,.22f),new Vector3(.40f,1.83f,.22f),.042f,"InkTeal");
            Model(coil,key+"_SpareCoil",root,"线圈备件");
            Box(root,key+"_IronCore",new Vector3(.61f,1.70f,.15f),new Vector3(.09f,.49f,.10f),"DarkGraphite","备用铁芯");
        }
        else if(index==5)
        {
            for(int i=0;i<3;i++)
            {
                var tube=new LevelModelBuilder();float x=(i-1)*.43f;
                Tube(tube,new Vector3(x,1.49f,.18f),new Vector3(x,2.03f,.18f),.043f,"Celadon");
                foreach(float y in new[]{1.52f,1.99f})Tube(tube,new Vector3(x,y-.025f,.18f),new Vector3(x,y+.025f,.18f),.067f,"SatinBrass");
                Model(tube,key+"_Pipe"+i,root,"标准管段");
            }
        }
        else if(index==6)
        {
            for(int i=0;i<3;i++)
            {
                var weight=new LevelModelBuilder();float x=(i-1)*.43f;
                Tube(weight,new Vector3(x,1.56f,.20f),new Vector3(x,1.87f,.20f),.065f+i*.017f,"SatinBrass");
                Ring(weight,new Vector3(x,1.95f,.20f),.041f,.009f,"Copper");
                Model(weight,key+"_Weight"+i,root,"备用摆锤");
            }
        }
        else if(index==8)
        {
            var tools=new LevelModelBuilder();
            foreach(float x in new[]{-.5f,0,.5f})
            {
                tools.Beam(new Vector3(x,1.46f,.17f),new Vector3(x,1.95f,.17f),.045f,"SatinBrass");
                tools.Box(new Vector3(x,2.0f,.17f),new Vector3(.17f,.08f,.07f),"InkTeal");
                tools.Box(new Vector3(x-.062f,2.08f,.17f),new Vector3(.045f,.11f,.07f),"InkTeal");
                tools.Box(new Vector3(x+.062f,2.08f,.17f),new Vector3(.045f,.11f,.07f),"InkTeal");
            }
            Model(tools,key+"_Tools",root,"壁挂装配扳手");
        }
        else
        {
            var specimen=new LevelModelBuilder();
            Ring(specimen,new Vector3(-.33f,1.80f,.23f),.23f,.016f,"SatinBrass");
            Ring(specimen,new Vector3(-.33f,1.80f,.23f),.21f,.014f,"Celadon",Quaternion.Euler(0,55,0));
            specimen.Beam(new Vector3(-.33f,1.52f,.23f),new Vector3(-.33f,2.08f,.23f),.018f,"InkTeal");
            Tube(specimen,new Vector3(.4f,1.52f,.20f),new Vector3(.4f,2.03f,.20f),.065f,"SatinBrass");
            Model(specimen,key+"_Specimens",root,"天文环与测量样本");
        }
    }
    static void Cabinet(int index,Transform root,string key)
    {
        var shell=new LevelModelBuilder();
        shell.Box(new Vector3(0,.08f,.32f),new Vector3(1.47f,.16f,.58f),"Celadon");
        shell.Box(new Vector3(0,.51f,.32f),new Vector3(1.42f,.70f,.54f),"Ivory");
        shell.Box(new Vector3(0,.90f,.32f),new Vector3(1.53f,.08f,.62f),"Celadon");
        foreach(float x in new[]{-.69f,.69f})shell.Box(new Vector3(x,.51f,.62f),new Vector3(.035f,.68f,.035f),"SatinBrass");
        Model(shell,key+"_Cabinet",root,"器材矮柜");
        for(int drawer=0;drawer<3;drawer++)
        {
            float y=.30f+drawer*.22f;
            Box(root,key+"_Drawer"+drawer,new Vector3(0,y,.605f),new Vector3(1.31f,.185f,.035f),"Ivory","可调整抽屉面");
            Box(root,key+"_Handle"+drawer,new Vector3(0,y,.64f),new Vector3(.23f,.026f,.03f),"SatinBrass","抽屉拉手");
        }
        var stock=new LevelModelBuilder();
        if(index==4 || index==6)
        {
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*.34f;
                Tube(stock,new Vector3(x,.96f,.32f),new Vector3(x,1.23f,.32f),.074f,"InkTeal");
                Tube(stock,new Vector3(x,1.22f,.32f),new Vector3(x,1.255f,.32f),.078f,"SatinBrass");
                stock.Box(new Vector3(x,1.29f,.32f),new Vector3(.025f,.06f,.035f),"Copper");
            }
        }
        else if(index==5)
        {
            for(int i=0;i<3;i++)Ring(stock,new Vector3((i-1)*.39f,.965f,.32f),.13f,.025f,i==1?"CoralRed":"InkTeal",Quaternion.Euler(90,0,0));
        }
        else if(index==8)
        {
            for(int i=0;i<4;i++)stock.Box(new Vector3(-.5f+i*.33f,1.08f,.30f),new Vector3(.19f,.28f,.20f),i%2==0?"WarmWood":"Celadon");
            stock.Box(new Vector3(0,1.27f,.20f),new Vector3(1.17f,.07f,.10f),"SatinBrass");
        }
        else if(index==9)
        {
            foreach(float x in new[]{-.35f,.35f})
            {
                stock.Beam(new Vector3(x,.96f,.30f),new Vector3(x,1.18f,.30f),.025f,"SatinBrass");
                Ring(stock,new Vector3(x,1.28f,.30f),.12f,.015f,"SatinBrass");
                stock.Box(new Vector3(x,1.28f,.315f),new Vector3(.15f,.15f,.012f),"GlacierBlue");
            }
        }
        else
        {
            stock.Beam(new Vector3(-.47f,1.26f,.28f),new Vector3(.47f,1.26f,.28f),.025f,"SatinBrass");
            stock.Beam(new Vector3(0,.96f,.28f),new Vector3(0,1.38f,.28f),.035f,"InkTeal");
            foreach(float x in new[]{-.43f,.43f})
            {stock.Beam(new Vector3(x,1.23f,.28f),new Vector3(x,1.01f,.28f),.015f,"Copper");Tube(stock,new Vector3(x,.985f,.28f),new Vector3(x,1.01f,.28f),.15f,"SatinBrass");}
        }
        Model(stock,key+"_Stock",root,"主题备件陈列");
    }
    static void Service(int index,Transform root,string key)
    {
        var panel=new LevelModelBuilder();Panel(panel,1.10f,2.35f);Model(panel,key+"_Panel",root,"配套装置底板");
        var equipment=new LevelModelBuilder();
        if(index==5 || index==9)
        {
            foreach(float x in new[]{-.48f,.48f})Tube(equipment,new Vector3(x,1.22f,.17f),new Vector3(x,2.2f,.17f),.033f,"Celadon");
            Tube(equipment,new Vector3(-.48f,1.50f,.17f),new Vector3(.48f,1.50f,.17f),.033f,"Celadon");
            Ring(equipment,new Vector3(-.48f,1.88f,.23f),.105f,.016f,"CoralRed");
            equipment.Beam(new Vector3(-.56f,1.80f,.23f),new Vector3(-.40f,1.96f,.23f),.022f,"SatinBrass");
            Tube(equipment,new Vector3(.24f,2.04f,.18f),new Vector3(.24f,2.04f,.24f),.135f,"Ivory");
            Ring(equipment,new Vector3(.24f,2.04f,.265f),.133f,.014f,"SatinBrass");
            equipment.Beam(new Vector3(.24f,2.04f,.29f),new Vector3(.30f,2.12f,.29f),.015f,"InkTeal");
        }
        else if(index==4 || index==6)
        {
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*.45f;
                equipment.Box(new Vector3(x,1.93f,.11f),new Vector3(.22f,.28f,.16f),i==1?"GlacierBlue":"InkTeal");
                equipment.Box(new Vector3(x,1.90f,.20f),new Vector3(.08f,.08f,.02f),"Ivory");
                equipment.Beam(new Vector3(x,1.71f,.13f),new Vector3(x,1.37f,.13f),.022f,"Copper");
            }
            equipment.Beam(new Vector3(-.56f,1.36f,.13f),new Vector3(.56f,1.36f,.13f),.025f,"Copper");
            foreach(float x in new[]{-.60f,.60f})equipment.Box(new Vector3(x,1.36f,.13f),new Vector3(.065f,.09f,.035f),"SatinBrass");
        }
        else
        {
            foreach(float y in new[]{1.35f,1.84f})equipment.Box(new Vector3(0,y,.23f),new Vector3(1.53f,.055f,.40f),"Celadon");
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*.43f;
                equipment.Box(new Vector3(x,1.47f,.22f),new Vector3(.19f,.20f,.20f),i==1?"SatinBrass":"WarmWood");
                Tube(equipment,new Vector3(x,1.89f,.22f),new Vector3(x,2.10f,.22f),.054f,"InkTeal");
                Ring(equipment,new Vector3(x,2.19f,.22f),.066f,.012f,"SatinBrass");
            }
        }
        Model(equipment,key+"_Equipment",root,index==5 || index==9?"供水管线与压力表":index==4 || index==6?"整齐线缆与模块盒":"小型样本收纳架");
    }
    static void Tube(LevelModelBuilder mesh,Vector3 a,Vector3 b,float radius,string material,int sides=8)
    {
        Vector3 axis=(b-a).normalized,u=Vector3.Cross(axis,Mathf.Abs(axis.z)<.85f?Vector3.forward:Vector3.right).normalized,v=Vector3.Cross(axis,u);
        var mat=LevelModelBuilder.Mat(material);
        for(int i=0;i<sides;i++)
        {
            float t=i*Mathf.PI*2/sides,next=(i+1)*Mathf.PI*2/sides;
            Vector3 p=(u*Mathf.Cos(t)+v*Mathf.Sin(t))*radius,q=(u*Mathf.Cos(next)+v*Mathf.Sin(next))*radius;
            mesh.Face(mat,a+p,a+q,b+q,b+p);mesh.Face(mat,a,a+q,a+p);mesh.Face(mat,b,b+p,b+q);
        }
    }
    static void Ring(LevelModelBuilder mesh,Vector3 center,float radius,float tube,string material,Quaternion? rotation=null)
    {
        var q=rotation??Quaternion.identity;var mat=LevelModelBuilder.Mat(material);
        Vector3 P(int segment,int side)
        {float a=segment*Mathf.PI/8,b=side*Mathf.PI/3;return center+q*new Vector3(Mathf.Cos(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(a)*(radius+Mathf.Cos(b)*tube),Mathf.Sin(b)*tube);}
        for(int segment=0;segment<16;segment++)for(int side=0;side<6;side++)mesh.Face(mat,P(segment,side),P(segment+1,side),P(segment+1,side+1),P(segment,side+1));
    }
}
