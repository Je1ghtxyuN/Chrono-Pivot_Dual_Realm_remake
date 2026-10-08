using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Editor-only processing: preserve silhouettes, hard normals, material slots and object GUIDs.
public static class ReportSurfaceBake
{
    const string Folder="Assets/ReportVR/BakedSurfaces";
    const string OriginalTag="OriginalSurfaceMaterial";
    sealed class Binding
    {
        public Mesh mesh;public Material[] materials;public string key;public int size;
        public Texture2D albedo,ao,surface;public Material[] baked;
    }
    struct VertexKey : IEquatable<VertexKey>
    {
        public Vector3 position,normal;public Color32 color;
        public bool Equals(VertexKey other)=>position.Equals(other.position) && normal.Equals(other.normal) && color.Equals(other.color);
        public override bool Equals(object value)=>value is VertexKey && Equals((VertexKey)value);
        public override int GetHashCode()=>position.GetHashCode()^normal.GetHashCode()*397^color.GetHashCode();
    }
    static readonly Dictionary<string,Binding> bindings=new Dictionary<string,Binding>();
    static readonly Dictionary<Mesh,float[]> occlusion=new Dictionary<Mesh,float[]>();
    static readonly List<string> texturePaths=new List<string>();
    static Material Original(Material material)
    {
        if(material==null)return null;
        string guid=material.GetTag(OriginalTag,false,"");
        return string.IsNullOrEmpty(guid)?material:AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
    }
    static bool Paint(Material m)=>m!=null && m.shader.name=="Standard" && m.GetFloat("_Mode")==0 && m.mainTexture==null;
    static string Key(Mesh mesh,Material[] materials)=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mesh))+"_"+
        Hash128.Compute(string.Join("|",materials.Select(m=>m==null?"null":AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m))))).ToString();
    static Binding Collect(MeshFilter filter)
    {
        var renderer=filter.GetComponent<MeshRenderer>();
        if(renderer==null || filter.sharedMesh==null || !ReportSurfaceCheck.Native(filter.sharedMesh))return null;
        var materials=renderer.sharedMaterials.Select(Original).ToArray();
        if(!materials.Any(Paint))return null;
        string key=Key(filter.sharedMesh,materials);
        if(!bindings.TryGetValue(key,out var binding))
        {
            if(materials.Length<filter.sharedMesh.subMeshCount)throw new InvalidOperationException("Incomplete palette: "+filter.name);
            binding=new Binding{mesh=filter.sharedMesh,materials=materials,key=key};bindings.Add(key,binding);
        }
        return binding;
    }
    static Vector3 Quantize(Vector3 p,float scale)=>new Vector3(Mathf.Round(p.x*scale)/scale,Mathf.Round(p.y*scale)/scale,Mathf.Round(p.z*scale)/scale);
    static int CleanAndUnwrap(Mesh mesh)
    {
        var source=mesh.vertices;var normals=mesh.normals;var colors=mesh.colors32;
        if(normals.Length!=source.Length)throw new InvalidOperationException("Missing source normals: "+mesh.name);
        var vertices=new List<Vector3>();var cleanNormals=new List<Vector3>();var cleanColors=new List<Color32>();
        var remap=new int[source.Length];var unique=new Dictionary<VertexKey,int>();
        for(int i=0;i<source.Length;i++)
        {
            var color=colors.Length==source.Length?colors[i]:new Color32(255,255,255,255);
            var key=new VertexKey{position=Quantize(source[i],1000000),normal=Quantize(normals[i],100000),color=color};
            if(!unique.TryGetValue(key,out int index))
            {
                index=vertices.Count;unique.Add(key,index);vertices.Add(source[i]);cleanNormals.Add(normals[i]);cleanColors.Add(color);
            }
            remap[i]=index;
        }
        int removed=0;var faces=new List<int[]>();
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var indices=mesh.GetTriangles(sub);var output=new List<int>();var seen=new HashSet<string>();
            for(int i=0;i<indices.Length;i+=3)
            {
                int a=remap[indices[i]],b=remap[indices[i+1]],c=remap[indices[i+2]];
                if(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]).magnitude*.5f<1e-9f){removed++;continue;}
                string signature=a<b && a<c?a+":"+b+":"+c:b<c?b+":"+c+":"+a:c+":"+a+":"+b;
                if(!seen.Add(signature)){removed++;continue;}
                output.AddRange(new[]{a,b,c});
            }
            faces.Add(output.ToArray());
        }
        var used=faces.SelectMany(f=>f).Distinct().OrderBy(i=>i).ToArray();var compact=new Dictionary<int,int>();
        for(int i=0;i<used.Length;i++)compact.Add(used[i],i);
        vertices=used.Select(i=>vertices[i]).ToList();cleanNormals=used.Select(i=>cleanNormals[i]).ToList();cleanColors=used.Select(i=>cleanColors[i]).ToList();
        for(int sub=0;sub<faces.Count;sub++)faces[sub]=faces[sub].Select(i=>compact[i]).ToArray();
        mesh.Clear();mesh.indexFormat=vertices.Count>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);mesh.SetNormals(cleanNormals);if(colors.Length==source.Length)mesh.SetColors(cleanColors);
        mesh.subMeshCount=faces.Count;for(int sub=0;sub<faces.Count;sub++)mesh.SetTriangles(faces[sub],sub);
        mesh.RecalculateBounds();MeshUtility.Optimize(mesh);
        int triangleCount=faces.Sum(f=>f.Length)/3;int size=triangleCount>650?1024:triangleCount>100?512:256;
        UnwrapParam.SetDefaults(out var settings);settings.hardAngle=55;settings.angleError=6;settings.areaError=8;settings.packMargin=6f/size;
        if(!Unwrapping.GenerateSecondaryUVSet(mesh,settings))throw new InvalidOperationException("UV unwrap failed: "+mesh.name);
        mesh.uv=mesh.uv2;RepairCollapsedCharts(mesh,size);mesh.RecalculateTangents();EditorUtility.SetDirty(mesh);
        var check=ReportSurfaceCheck.Inspect(mesh);
        if(check.degenerateFaces!=0 || check.invalidUVFaces!=0)throw new InvalidOperationException("Invalid processed UV: "+mesh.name+" faces="+check.invalidUVFaces);
        return removed;
    }
    static void RepairCollapsedCharts(Mesh mesh,int size)
    {
        var uv=mesh.uv;var bad=new List<Vector2Int>();var submeshes=Enumerable.Range(0,mesh.subMeshCount).Select(mesh.GetTriangles).ToArray();
        for(int sub=0;sub<submeshes.Length;sub++)for(int i=0;i<submeshes[sub].Length;i+=3)
        {
            var t=submeshes[sub];var ab=uv[t[i+1]]-uv[t[i]];var ac=uv[t[i+2]]-uv[t[i]];
            if(Mathf.Abs(ab.x*ac.y-ab.y*ac.x)<1e-12f)bad.Add(new Vector2Int(sub,i));
        }
        if(bad.Count==0)return;
        int columns=Mathf.CeilToInt(Mathf.Sqrt(bad.Count)),rows=Mathf.CeilToInt((float)bad.Count/columns);
        float height=Mathf.Clamp(rows*12f/size,.05f,.3f),padding=2f/size;
        var vertices=mesh.vertices.ToList();var normals=mesh.normals.ToList();var colors=mesh.colors32.ToList();
        bool hasColors=colors.Count==vertices.Count;var coordinates=uv.Select(p=>new Vector2(p.x,p.y*(1-height)+height)).ToList();
        for(int island=0;island<bad.Count;island++)
        {
            var face=bad[island];var t=submeshes[face.x];float x=(float)(island%columns)/columns,y=(float)(island/columns)*height/rows;
            var chart=new[]{new Vector2(x+padding,y+padding),new Vector2(x+1f/columns-padding,y+padding),new Vector2(x+padding,y+height/rows-padding)};
            for(int corner=0;corner<3;corner++)
            {
                int original=t[face.y+corner];t[face.y+corner]=vertices.Count;
                vertices.Add(vertices[original]);normals.Add(normals[original]);if(hasColors)colors.Add(colors[original]);coordinates.Add(chart[corner]);
            }
        }
        mesh.SetVertices(vertices);mesh.SetNormals(normals);if(hasColors)mesh.SetColors(colors);mesh.SetUVs(0,coordinates);mesh.SetUVs(1,coordinates);
        for(int sub=0;sub<submeshes.Length;sub++)mesh.SetTriangles(submeshes[sub],sub);
        Debug.Log("SURFACE_FINE_CHARTS "+mesh.name+" islands="+bad.Count);
    }
    static float[] BakeAO(Mesh mesh,MeshCollider collider,PhysicsScene physics)
    {
        collider.sharedMesh=null;collider.sharedMesh=mesh;Physics.SyncTransforms();
        var vertices=mesh.vertices;var normals=mesh.normals;var result=new float[vertices.Length];
        float distance=Mathf.Clamp(mesh.bounds.size.magnitude*.045f,.04f,.5f),bias=Mathf.Max(.0003f,distance*.008f);
        for(int i=0;i<vertices.Length;i++)
        {
            var n=normals[i].normalized;var tangent=Vector3.Cross(n,Mathf.Abs(n.y)<.9f?Vector3.up:Vector3.right).normalized;
            var bitangent=Vector3.Cross(n,tangent);float blocked=0;
            for(int sample=0;sample<12;sample++)
            {
                float radius=Mathf.Sqrt((sample+.5f)/12),angle=sample*2.39996323f;
                var direction=tangent*(radius*Mathf.Cos(angle))+bitangent*(radius*Mathf.Sin(angle))+n*Mathf.Sqrt(1-radius*radius);
                if(physics.Raycast(vertices[i]+n*bias,direction,out var hit,distance,1<<30,QueryTriggerInteraction.Ignore))
                    blocked+=1-hit.distance/distance;
            }
            result[i]=1-.38f*blocked/12;
        }
        return result;
    }
    static float Grain(string name,Vector3 p)
    {
        string key=name.ToLowerInvariant();
        if(key.Contains("wood") || key.Contains("timber") || key.Contains("walnut"))
            return .985f+.015f*Mathf.Sin(p.y*34+Mathf.Sin(p.x*4+p.z*3)*1.8f);
        if(key.Contains("stone") || key.Contains("ivory") || key.Contains("plaster") || key.Contains("snow"))
            return .99f+.01f*Mathf.Sin(p.x*19+p.y*23)*Mathf.Sin(p.z*17-p.y*11);
        return .995f+.005f*Mathf.Sin(p.x*8+p.y*5+p.z*11);
    }
    static string TexturePath(Binding b,string kind)=>Folder+"/Textures/"+b.key+"_"+kind+".png";
    static void WriteTexture(Binding binding,string kind,Color[] pixels)
    {
        var texture=new Texture2D(binding.size,binding.size,TextureFormat.RGBA32,false,kind!="Albedo");
        texture.SetPixels(pixels);texture.Apply();string path=TexturePath(binding,kind);File.WriteAllBytes(path,texture.EncodeToPNG());
        texturePaths.Add(path);Object.DestroyImmediate(texture);
    }
    static void Raster(Binding binding,float[] ao)
    {
        var mesh=binding.mesh;var positions=mesh.vertices;var uv=mesh.uv;
        int triangles=mesh.triangles.Length/3;binding.size=triangles>650?1024:triangles>100?512:256;
        int size=binding.size;var baseColor=Enumerable.Repeat(Color.white,size*size).ToArray();
        var ambient=Enumerable.Repeat(Color.white,size*size).ToArray();var surface=Enumerable.Repeat(new Color(0,.88f,0,.12f),size*size).ToArray();
        var filled=new bool[size*size];
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var mat=binding.materials[sub];
            float metal=mat!=null && mat.HasProperty("_Metallic")?mat.GetFloat("_Metallic"):0;
            float smooth=mat!=null && mat.HasProperty("_Glossiness")?mat.GetFloat("_Glossiness"):.12f;
            var indices=mesh.GetTriangles(sub);
            for(int t=0;t<indices.Length;t+=3)
            {
                int ia=indices[t],ib=indices[t+1],ic=indices[t+2];var a=uv[ia]*size;var b=uv[ib]*size;var c=uv[ic]*size;
                float denominator=(b.y-c.y)*(a.x-c.x)+(c.x-b.x)*(a.y-c.y);
                int x0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.x,b.x,c.x)),0,size-1),x1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.x,b.x,c.x)),0,size-1);
                int y0=Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(a.y,b.y,c.y)),0,size-1),y1=Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(a.y,b.y,c.y)),0,size-1);
                for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
                {
                    float u=((b.y-c.y)*(x+.5f-c.x)+(c.x-b.x)*(y+.5f-c.y))/denominator;
                    float v=((c.y-a.y)*(x+.5f-c.x)+(a.x-c.x)*(y+.5f-c.y))/denominator,w=1-u-v;
                    if(u<-.00001f || v<-.00001f || w<-.00001f)continue;
                    int pixel=y*size+x;var p=positions[ia]*u+positions[ib]*v+positions[ic]*w;
                    float grain=Grain(mat==null?"":mat.name,p),shade=ao[ia]*u+ao[ib]*v+ao[ic]*w;
                    // Keep the palette in the material; distant mips must not blend face colors.
                    baseColor[pixel]=new Color(grain,grain,grain,1);
                    ambient[pixel]=new Color(shade,shade,shade,1);surface[pixel]=new Color(metal,1-smooth,0,smooth);filled[pixel]=true;
                }
            }
        }
        // Extend the nearest island through the entire gutter, including image borders.
        var queue=new int[filled.Length];int head=0,tail=0;
        for(int pixel=0;pixel<filled.Length;pixel++)if(filled[pixel])queue[tail++]=pixel;
        if(tail==0)throw new InvalidOperationException("No raster coverage: "+mesh.name);
        while(head<tail)
        {
            int pixel=queue[head++],x=pixel%size,y=pixel/size;
            for(int direction=0;direction<4;direction++)
            {
                int next=direction==0?(x>0?pixel-1:-1):direction==1?(x<size-1?pixel+1:-1):direction==2?(y>0?pixel-size:-1):(y<size-1?pixel+size:-1);
                if(next<0 || filled[next])continue;
                baseColor[next]=baseColor[pixel];ambient[next]=ambient[pixel];surface[next]=surface[pixel];filled[next]=true;queue[tail++]=next;
            }
        }
        WriteTexture(binding,"Albedo",baseColor);WriteTexture(binding,"AO",ambient);WriteTexture(binding,"MetalRough",surface);
    }
    static void ImportTextures()
    {
        AssetDatabase.Refresh();AssetDatabase.StartAssetEditing();
        try
        {
            foreach(string path in texturePaths)
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;
                importer.sRGBTexture=path.EndsWith("_Albedo.png");importer.alphaSource=path.EndsWith("_MetalRough.png")?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
                importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=2;
                importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.maxTextureSize=1024;
                AssetDatabase.WriteImportSettingsIfDirty(path);AssetDatabase.ImportAsset(path);
            }
        }
        finally{AssetDatabase.StopAssetEditing();}
        AssetDatabase.StartAssetEditing();
        try { foreach(var b in bindings.Values)
        {
            b.albedo=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(b,"Albedo"));b.ao=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(b,"AO"));
            b.surface=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath(b,"MetalRough"));b.baked=new Material[b.materials.Length];
            for(int slot=0;slot<b.materials.Length;slot++)
            {
                var original=b.materials[slot];if(!Paint(original)){b.baked[slot]=original;continue;}
                string path=Folder+"/Materials/"+b.key+"_"+slot+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null){mat=new Material(Shader.Find("ChronoPivot/Low Poly Baked"));AssetDatabase.CreateAsset(mat,path);}
                mat.name=original.name+" · Baked";mat.color=original.color;mat.SetColor("_BakeTint",original.color);mat.mainTexture=b.albedo;
                mat.SetFloat("_DetailStrength",.35f);
                mat.SetTexture("_OcclusionMap",b.ao);mat.SetFloat("_OcclusionStrength",.65f);mat.SetTexture("_MetallicGlossMap",b.surface);mat.SetFloat("_GlossMapScale",1);
                mat.SetColor("_EmissionColor",original.GetColor("_EmissionColor"));mat.enableInstancing=true;
                mat.SetOverrideTag(OriginalTag,AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(original)));EditorUtility.SetDirty(mat);b.baked[slot]=mat;
            }
        }
        } finally { AssetDatabase.StopAssetEditing(); }
        AssetDatabase.SaveAssets();
    }
    static void ApplyMaterials(GameObject root)
    {
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var b=Collect(filter);if(b!=null)filter.GetComponent<MeshRenderer>().sharedMaterials=b.baked;
        }
    }
    [MenuItem("Chrono Pivot/表面工艺/展开UV并烘焙低多边形材质")]
    public static void Process()=>Process(true);
    [MenuItem("Chrono Pivot/表面工艺/仅重新烘焙材质（保留UV）")]
    public static void RebakeTextures()=>Process(false);
    static void Process(bool unwrap)
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        bindings.Clear();occlusion.Clear();texturePaths.Clear();Directory.CreateDirectory(Folder+"/Textures");Directory.CreateDirectory(Folder+"/Materials");
        var scenes=ReportSurfaceCheck.Scenes();
        foreach(string scene in scenes){EditorSceneManager.OpenScene(scene);foreach(var filter in Object.FindObjectsOfType<MeshFilter>(true))Collect(filter);}
        var prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/ReportGardenLowPoly","Assets/ReportScenesLowPoly"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach(string path in prefabs)foreach(var filter in AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentsInChildren<MeshFilter>(true))Collect(filter);
        var meshes=AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/ReportGardenLowPoly","Assets/ReportScenesLowPoly","Assets/ReportVR"})
            .Select(g=>AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(g))).Where(ReportSurfaceCheck.Native).OrderBy(AssetDatabase.GetAssetPath).ToArray();
        int removed=0;if(unwrap)foreach(var mesh in meshes)removed+=CleanAndUnwrap(mesh);AssetDatabase.SaveAssets();
        Debug.Log("SURFACE_UV_COMPLETE meshes="+meshes.Length+" removedFaces="+removed);
        var physicsScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var go=new GameObject("Local AO probe");go.layer=30;SceneManager.MoveGameObjectToScene(go,physicsScene);var collider=go.AddComponent<MeshCollider>();
        try
        {
            int done=0;foreach(var b in bindings.Values)
            {
                if(!occlusion.TryGetValue(b.mesh,out var ao)){ao=BakeAO(b.mesh,collider,physicsScene.GetPhysicsScene());occlusion.Add(b.mesh,ao);}
                Raster(b,ao);if(++done%50==0)Debug.Log("SURFACE_BAKE_PROGRESS "+done+"/"+bindings.Count);
            }
        }
        finally{Object.DestroyImmediate(go);EditorSceneManager.CloseScene(physicsScene,true);}
        ImportTextures();
        foreach(string path in prefabs)
        {
            var root=PrefabUtility.LoadPrefabContents(path);try{ApplyMaterials(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        foreach(string path in scenes)
        {
            var scene=EditorSceneManager.OpenScene(path);foreach(var root in scene.GetRootGameObjects())ApplyMaterials(root);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();ReportSurfaceCheck.RequireReady();
        var audit=new ReportSurfaceCheck.Audit();foreach(var mesh in meshes)audit.meshes.Add(ReportSurfaceCheck.Inspect(mesh));
        Directory.CreateDirectory(ReportSurfaceCheck.Output);File.WriteAllText(ReportSurfaceCheck.Output+"/audit-after.json",JsonUtility.ToJson(audit,true));
        File.WriteAllText(ReportSurfaceCheck.Output+"/bake-summary.txt","Processed meshes: "+meshes.Length+"\nUV regenerated this run: "+unwrap+"\nFaces removed this run: "+removed+
            "\nBaked palette variants: "+bindings.Count+"\nTexture files: "+texturePaths.Count+"\nAO: 12 cosine-weighted local rays, 0.65 material strength.\nUV0 and UV1: padded unique chart atlas.\nAlbedo: neutral grain multiplied by material palette.\nGutters: full nearest-island extension.\nSee geometry-verification.json for total cleanup versus the original project.\n");
        EditorSceneManager.OpenScene(scenes[0]);Debug.Log("SURFACE_BAKE_OK meshes="+meshes.Length+" variants="+bindings.Count+" textures="+texturePaths.Count);
    }
}
