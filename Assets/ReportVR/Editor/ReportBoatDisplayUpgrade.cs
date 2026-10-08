using System.Linq;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportBoatDisplayUpgrade
{
    static Transform Group(Transform parent,string name)
    {
        var old=parent.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);
        var group=new GameObject(name).transform;group.SetParent(parent,false);return group;
    }
    public static void Configure(ReportBuoyancyMeasure boat)
    {
        var ship=GameObject.Find("HandcraftedCargoPavilionBoat").transform;
        boat.transform.position=new Vector3(-7.2f,.03f,4.6f);
        boat.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(ship.position-boat.transform.position,Vector3.up));
        foreach(var renderer in boat.hull.Find("Visible draft marks").GetComponentsInChildren<Renderer>())renderer.enabled=false;
        ClassicHull(boat);
        foreach(string name in new[]{"石配重","铜配重"})ship.Find(name).gameObject.SetActive(false);
        var loads=Group(ship,"Linked cargo");var cargo=new GameObject[3];
        string[] materials={"WarmWood","SageStone","Copper"};float[] sizes={.44f,.34f,.30f};
        for(int i=0;i<cargo.Length;i++)
        {
            var block=new LevelModelBuilder();block.Box(new Vector3(.35f+i*.6f,.67f,(i-1)*.25f),Vector3.one*sizes[i],materials[i]);
            cargo[i]=block.Save("Fig_2_02_Cargo_DisplayLoad_"+i,loads,false);cargo[i].SetActive(false);
        }
        var display=boat.GetComponent<ReportBoatDisplay>();if(display==null)display=boat.gameObject.AddComponent<ReportBoatDisplay>();
        display.experiment=boat;display.hull=ship;display.gate=GameObject.Find("CanalSluiceAndCrank").GetComponentsInChildren<Transform>(true).First(t=>t.name=="活动闸板");display.cargo=cargo;
    }
    // Curved pointed ends, thin inner/outer sides, an open floor and transverse seats.
    public static void ClassicHull(ReportBuoyancyMeasure boat)
    {
        var group=Group(boat.hull,"Recognizable boat");var model=new LevelModelBuilder();var wood=LevelModelBuilder.Mat("WarmWood");
        const int segments=12;
        foreach(float side in new[]{-1f,1f})
        {
            var low=new Vector3[segments+1];var middle=new Vector3[segments+1];var rim=new Vector3[segments+1];var inside=new Vector3[segments+1];var innerRim=new Vector3[segments+1];
            for(int i=0;i<=segments;i++)
            {
                float s=i*2f/segments-1,curve=Mathf.Pow(Mathf.Max(0,1-s*s),.65f),lift=Mathf.Pow(s,4);
                low[i]=new Vector3(.50f*s,-.20f+.26f*lift,side*.105f*curve);
                middle[i]=new Vector3(.57f*s,-.06f+.30f*lift,side*.205f*curve);
                rim[i]=new Vector3(.62f*s,.20f+.09f*lift,side*.265f*curve);
                inside[i]=new Vector3(.51f*s,.055f+.03f*lift,side*.15f*curve);
                innerRim[i]=new Vector3(.60f*s,.195f+.09f*lift,side*.242f*curve);
            }
            for(int i=0;i<segments;i++)
            {
                int next=i+1;
                if(side>0)
                {
                    model.Face(wood,low[i],new Vector3(low[i].x,low[i].y,0),new Vector3(low[next].x,low[next].y,0),low[next]);
                    model.Face(wood,middle[i],low[i],low[next],middle[next]);model.Face(wood,rim[i],middle[i],middle[next],rim[next]);
                    model.Face(wood,inside[i],innerRim[i],innerRim[next],inside[next]);
                }
                else
                {
                    model.Face(wood,low[next],new Vector3(low[next].x,low[next].y,0),new Vector3(low[i].x,low[i].y,0),low[i]);
                    model.Face(wood,middle[next],low[next],low[i],middle[i]);model.Face(wood,rim[next],middle[next],middle[i],rim[i]);
                    model.Face(wood,inside[next],innerRim[next],innerRim[i],inside[i]);
                }
                if(side>0)model.Face(wood,rim[i],rim[next],innerRim[next],innerRim[i]);else model.Face(wood,innerRim[i],innerRim[next],rim[next],rim[i]);model.Beam(rim[i],rim[next],.024f,"WarmWood");
                model.Beam(middle[i]+Vector3.forward*side*.003f,middle[next]+Vector3.forward*side*.003f,.003f,"DarkGraphite");
                if(side>0)model.Face(wood,new Vector3(0,.055f,0),inside[i],inside[next]);else model.Face(wood,new Vector3(0,.055f,0),inside[next],inside[i]);
            }
        }
        foreach(float x in new[]{-.26f,0,.26f})model.Box(new Vector3(x,.205f,0),new Vector3(.095f,.025f,x==0?.47f:.43f),"WarmWood");
        foreach(float side in new[]{-1f,1f})
            foreach(var mark in new[]{new Vector3(-.10f,.18f,0),new Vector3(0,.22f,1),new Vector3(.20f,.265f,2)})
                for(int i=0;i<6;i++)
                {
                    float a=-.55f+i*.1833f,b=a+.15f;
                    Vector3 first=new Vector3(a*.57f,mark.x,side*(mark.y*Mathf.Pow(1-a*a,.65f)+.007f));
                    Vector3 second=new Vector3(b*.57f,mark.x,side*(mark.y*Mathf.Pow(1-b*b,.65f)+.007f));
                    model.Beam(first,second,.008f,mark.z==0?"Ivory":mark.z==1?"SatinBrass":"CoralRed");
                }
        model.Save("Fig_2_02_Cargo_WoodenBoat",group,false);
        var guide=boat.hull.Find("Cargo loading area");if(guide!=null)guide.gameObject.SetActive(false);
    }
}
