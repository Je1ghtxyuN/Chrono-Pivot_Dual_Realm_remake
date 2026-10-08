using UnityEngine;
using UnityEngine.UI;

// A crisp polygonal dial drawn as native UI geometry.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ReportUIDial : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();Vector2 center=rectTransform.rect.center;float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.46f;
        for(int i=0;i<48;i++)
        {
            float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;float inner=radius*(i%4==0?.82f:.94f);
            var vertex=UIVertex.simpleVert;vertex.color=color;
            foreach(var p in new[]{new Vector2(Mathf.Sin(a),Mathf.Cos(a))*inner,new Vector2(Mathf.Sin(a),Mathf.Cos(a))*radius,new Vector2(Mathf.Sin(b),Mathf.Cos(b))*radius,new Vector2(Mathf.Sin(b),Mathf.Cos(b))*inner}){vertex.position=center+p;mesh.AddVert(vertex);}
            int first=i*4;mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
        }
        raycastTarget=false;
    }
}
