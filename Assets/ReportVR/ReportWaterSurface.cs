using UnityEngine;

// Each body keeps its own disturbances; no grab-pass or extra per-eye camera.
[ExecuteAlways,RequireComponent(typeof(Renderer))]
public sealed class ReportWaterSurface : MonoBehaviour
{
    public float waveAmplitude=.008f;
    public Vector2 flowDirection=Vector2.right;
    public float flowSpeed=.6f;
    public bool frozen;
    Renderer surface;
    MaterialPropertyBlock properties;
    readonly Vector4[] ripples=new Vector4[4];
    int nextRipple;
    void Awake(){surface=GetComponent<Renderer>();properties=new MaterialPropertyBlock();Apply();}
    void Update(){if(!Application.isPlaying)Apply();}
    public void Disturb(Vector3 position,float strength=.01f)
    {
        ripples[nextRipple]=new Vector4(position.x,position.z,Time.time,Mathf.Clamp(strength,0,.025f));nextRipple=(nextRipple+1)%ripples.Length;
        Apply();
    }
    void Apply()
    {
        if(surface==null)return;
        surface.GetPropertyBlock(properties);
        properties.SetFloat("_WaveAmplitude",frozen?0:waveAmplitude);
        properties.SetVector("_Flow",new Vector4(flowDirection.x,flowDirection.y,flowSpeed,0));
        for(int i=0;i<ripples.Length;i++)properties.SetVector("_Ripple"+i,ripples[i]);
        surface.SetPropertyBlock(properties);
    }
    void OnWillRenderObject(){if(Camera.current!=null)Camera.current.depthTextureMode|=DepthTextureMode.Depth;}
}
