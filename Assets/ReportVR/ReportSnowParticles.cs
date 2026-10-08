using UnityEngine;

public sealed class ReportSnowParticles : MonoBehaviour
{
    public ReportGrabbable piece;
    public ParticleSystem powder;
    float nextImpact;
    void OnCollisionEnter(Collision collision)
    {
        if(Time.timeScale<=0 || piece.isSelected || Time.time<nextImpact || collision.relativeVelocity.magnitude<1 || collision.contactCount==0)return;
        var other=collision.collider.GetComponentInParent<ReportGrabbable>();if(other!=null && other.isSelected)return;
        var contact=collision.GetContact(0);
        ReportParticleBurst.Emit(powder,contact.point+contact.normal*.01f,contact.normal*.18f,7,.3f);nextImpact=Time.time+.3f;
    }
    public void ResetFeedback(){ReportParticleBurst.Clear(powder);nextImpact=Time.time+.3f;}
    void OnEnable(){ResetFeedback();}
    void OnDisable(){ResetFeedback();}
}
