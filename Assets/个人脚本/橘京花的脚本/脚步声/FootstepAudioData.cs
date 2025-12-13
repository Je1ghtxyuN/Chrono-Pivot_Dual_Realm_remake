//using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SE/Footstep Audio Data")]//创建一个能在创建菜单创建文件的方式
public class FootstepAudioData : ScriptableObject//数据容器类
{
    public List<FootstepAudio> FootstepAudio = new List<FootstepAudio>();
}

[System.Serializable]
public class FootstepAudio
{
    public string Tag;
    public List<AudioClip> AudioClips = new List<AudioClip>();
    public float Delay;
}
