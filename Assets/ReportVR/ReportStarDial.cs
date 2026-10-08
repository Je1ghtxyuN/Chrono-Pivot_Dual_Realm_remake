using UnityEngine;

// The dial turns the demonstration sky around the elevated north celestial axis.
public sealed class ReportStarDial : MonoBehaviour
{
    public ReportMechanismHandle ring;
    public Transform skyRoot;
    Quaternion initialRotation;
    Material originalSky,sky;
    void Awake()
    {
        if(sky!=null)return;
        initialRotation=skyRoot.rotation;originalSky=RenderSettings.skybox;
        sky=new Material(originalSky);RenderSettings.skybox=sky;
        LateUpdate();
    }
    void LateUpdate()
    {
        var rotation=Quaternion.AngleAxis(ring.Value-ring.startingValue,(Vector3.up+Vector3.forward).normalized);
        skyRoot.rotation=rotation*initialRotation;
        var inverse=Quaternion.Inverse(rotation);sky.SetVector("_SkyRotation",new Vector4(inverse.x,inverse.y,inverse.z,inverse.w));
    }
    void OnDestroy()
    {
        if(sky==null)return;
        if(RenderSettings.skybox==sky)RenderSettings.skybox=originalSky;
        if(Application.isPlaying)Destroy(sky);else DestroyImmediate(sky);
    }
}
