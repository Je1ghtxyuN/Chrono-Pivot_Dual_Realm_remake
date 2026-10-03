using UnityEngine;

// Educational approximations in SI units, not a coupled physics solver.
public static class DemoPhysics
{
    public static float SubmergedFraction(float mass,float fluidDensity,float hullVolume)=>Mathf.Clamp01(mass/(fluidDensity*hullVolume));
    public static bool FrequencyMatches(float actual,float target)=>Mathf.Abs(actual-target)<=Mathf.Abs(target)*.03f;
    public static float PendulumFrequency(float length)=>Mathf.Sqrt(9.81f/length)/(2*Mathf.PI);
    public static float CoilField(float current,int turns,float length)=>4e-7f*Mathf.PI*turns*current/length;
}
