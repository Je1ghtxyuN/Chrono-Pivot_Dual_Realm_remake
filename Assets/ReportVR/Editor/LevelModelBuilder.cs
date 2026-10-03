using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Native flat-shaded geometry. Uses only the palette and models authored for this report.
public sealed class LevelModelBuilder
{
    readonly List<Vector3> vertices=new List<Vector3>();
    readonly Dictionary<Material,List<int>> faces=new Dictionary<Material,List<int>>();
    public static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/ReportScenesLowPoly/Materials/"+name+".mat");
    public void Face(Material material,params Vector3[] points)
    {
        int n=vertices.Count;vertices.AddRange(points);
        if(!faces.ContainsKey(material))faces[material]=new List<int>();
        faces[material].AddRange(new[]{n,n+1,n+2});
        if(points.Length==4)faces[material].AddRange(new[]{n,n+2,n+3});
    }
    public void Box(Vector3 c,Vector3 size,string material,Quaternion? rotation=null)
    {
        Vector3 h=size*.5f;Quaternion q=rotation??Quaternion.identity;
        Vector3[] p={new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
        for(int i=0;i<8;i++)p[i]=c+q*p[i];
        foreach(int[] f in new[]{new[]{3,2,1,0},new[]{4,5,6,7},new[]{0,1,5,4},new[]{7,6,2,3},new[]{4,7,3,0},new[]{1,2,6,5}})Face(Mat(material),f.Select(i=>p[i]).ToArray());
    }
    public void Beam(Vector3 a,Vector3 b,float width,string material)=>Box((a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),material,Quaternion.FromToRotation(Vector3.up,b-a));
    public void Ramp(Vector3 a,Vector3 b,float width,string material)
    {
        var side=Vector3.Cross((b-a).normalized,Vector3.up).normalized*width*.5f;
        var p=new[]{a+side,a-side,b-side,b+side};var mat=Mat(material);
        Face(mat,p.Reverse().ToArray());Face(mat,p.Select(v=>v-Vector3.up*.14f).ToArray());
        for(int i=0;i<4;i++){int j=(i+1)%4;Face(mat,p[i]-Vector3.up*.14f,p[j]-Vector3.up*.14f,p[j],p[i]);}
    }
    public GameObject Save(string key,Transform parent,bool collision=true)
    {
        const string folder="Assets/ReportVR/LevelModels";
        System.IO.Directory.CreateDirectory(folder);
        var mesh=new Mesh{name=key};mesh.SetVertices(vertices);mesh.subMeshCount=faces.Count;
        int i=0;foreach(var indices in faces.Values)mesh.SetTriangles(indices,i++);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string path=folder+"/"+key+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,path);
        var go=new GameObject(key);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=faces.Keys.ToArray();
        if(collision)go.AddComponent<MeshCollider>().sharedMesh=mesh;return go;
    }
}
