using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Geometry and UV acceptance checks for the existing, authored low-poly assets.
public static class ReportSurfaceCheck
{
    public const string Output="Documentation/SurfaceWorkflow";
    [Serializable] public class Row
    {
        public string path,name;
        public int vertices,triangles,submeshes,uvVertices,uv2Vertices,degenerateFaces,invalidUVFaces;
        public float area;
    }
    [Serializable] public class Audit { public List<Row> meshes=new List<Row>(); }
    public static bool Native(Mesh mesh)
    {
        string path=AssetDatabase.GetAssetPath(mesh);
        return path.StartsWith("Assets/Report") && !path.Contains("/Characters/");
    }
    public static string[] Scenes()=>ReportLevelAudit.Scenes();
    public static Dictionary<Mesh,Material[]> Gather()
    {
        var result=new Dictionary<Mesh,Material[]>();
        foreach(string scene in Scenes())
        {
            EditorSceneManager.OpenScene(scene);
            foreach(var filter in Object.FindObjectsOfType<MeshFilter>(true))
            {
                var renderer=filter.GetComponent<MeshRenderer>();
                if(renderer!=null && filter.sharedMesh!=null && Native(filter.sharedMesh) && !result.ContainsKey(filter.sharedMesh))
                    result.Add(filter.sharedMesh,renderer.sharedMaterials);
            }
        }
        foreach(string guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/ReportGardenLowPoly","Assets/ReportScenesLowPoly"}))
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            foreach(var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer=filter.GetComponent<MeshRenderer>();
                if(renderer!=null && filter.sharedMesh!=null && Native(filter.sharedMesh) && !result.ContainsKey(filter.sharedMesh))
                    result.Add(filter.sharedMesh,renderer.sharedMaterials);
            }
        }
        return result;
    }
    public static Row Inspect(Mesh mesh)
    {
        var row=new Row{path=AssetDatabase.GetAssetPath(mesh),name=mesh.name,vertices=mesh.vertexCount,
            submeshes=mesh.subMeshCount,uvVertices=mesh.uv.Length,uv2Vertices=mesh.uv2.Length};
        var vertices=mesh.vertices;var uv=mesh.uv;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var triangles=mesh.GetTriangles(sub);row.triangles+=triangles.Length/3;
            for(int i=0;i<triangles.Length;i+=3)
            {
                int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
                float area=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]).magnitude*.5f;
                row.area+=area;if(area<1e-9f)row.degenerateFaces++;
                if(uv.Length!=vertices.Length){row.invalidUVFaces++;continue;}
                Vector2 ab=uv[b]-uv[a],ac=uv[c]-uv[a];
                if(Mathf.Abs(ab.x*ac.y-ab.y*ac.x)<1e-12f)row.invalidUVFaces++;
            }
        }
        return row;
    }
    [MenuItem("Chrono Pivot/表面工艺/检查当前网格与UV")]
    public static void AuditCurrent()
    {
        Directory.CreateDirectory(Output);var report=new Audit();
        foreach(var mesh in Gather().Keys.OrderBy(AssetDatabase.GetAssetPath))report.meshes.Add(Inspect(mesh));
        File.WriteAllText(Output+"/audit-before.json",JsonUtility.ToJson(report,true));
        Debug.Log("SURFACE_AUDIT meshes="+report.meshes.Count+" triangles="+report.meshes.Sum(r=>r.triangles)+
            " missingUV="+report.meshes.Count(r=>r.uvVertices!=r.vertices)+" badUVFaces="+report.meshes.Sum(r=>r.invalidUVFaces)+
            " degenerateFaces="+report.meshes.Sum(r=>r.degenerateFaces));
    }
    // Run against the original project first: this must fail when a referenced mesh has no UV.
    public static void RequireReady()
    {
        var meshes=Gather().Keys.ToArray();
        foreach(var mesh in meshes)
        {
            var row=Inspect(mesh);
            if(row.degenerateFaces!=0 || row.invalidUVFaces!=0 || row.uv2Vertices!=row.vertices)
                throw new InvalidOperationException("SURFACE_NOT_READY "+row.path+" degenerate="+row.degenerateFaces+
                    " badUVFaces="+row.invalidUVFaces+" uv2="+row.uv2Vertices+" vertices="+row.vertices);
        }
        Debug.Log("SURFACE_READY meshes="+meshes.Length);
    }
    public static void Baseline()
    {
        AuditCurrent();RequireReady();
    }
}
