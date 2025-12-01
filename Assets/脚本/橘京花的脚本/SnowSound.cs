using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowSound : MonoBehaviour
{
    public AudioClip snowSound;
    private AudioSource player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<AudioSource>();
        player.clip = snowSound;
        player.loop = true;
        player.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
