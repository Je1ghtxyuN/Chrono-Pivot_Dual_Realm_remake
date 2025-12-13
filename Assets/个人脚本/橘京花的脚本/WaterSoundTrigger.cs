using UnityEngine;

public class WaterSoundTrigger : MonoBehaviour
{
    [Header("声音设置")]
    [SerializeField] private AudioClip waterEnterSound;  // 触碰水面的声音
    //[SerializeField] private AudioClip waterExitSound;   // 离开水时的声音（可选）
    [SerializeField] private bool playOnlyOnce = false;  // 是否只播放一次

    private AudioSource audioSource;
    private bool hasPlayedEnter = false;

    private void Awake()
    {
        // 确保玩家身上有 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查是否进入水体（触发器需标记为 "water" 标签）
        if (other.CompareTag("water"))
        {
            if (!playOnlyOnce || !hasPlayedEnter)
            {
                if (waterEnterSound != null)
                {
                    audioSource.PlayOneShot(waterEnterSound);
                    hasPlayedEnter = true;
                    Debug.Log("播放入水声音");
                }
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    // 离开水体时播放（可选）
    //    if (other.CompareTag("water") && waterExitSound != null)
    //    {
    //        audioSource.PlayOneShot(waterExitSound);
    //        Debug.Log("播放出水声音");
    //    }
    //}

    // 重置状态（如关卡重置时调用）
    public void ResetWaterSound()
    {
        hasPlayedEnter = false;
    }
}