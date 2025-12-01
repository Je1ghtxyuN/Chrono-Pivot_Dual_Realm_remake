using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Tenkoku.Core
{
    public class TenkokuGlobalSound : MonoBehaviour
	{


	//Public Variables
	public AudioMixer mixer;
	public AudioMixerGroup mixerGroup;

	[Space(16)]
	public AudioClip audioWind;
	public AudioClip audioTurb1;
	public AudioClip audioTurb2;
	public AudioClip audioRain;
	public AudioClip audioAmbDay;
	public AudioClip audioAmbNight;

	public float volWind = 1.0f;
	public float volTurb1 = 1.0f;
	public float volTurb2 = 1.0f;
	public float volRain = 1.0f;
	public float volAmbDay = 1.0f;
	public float volAmbNight = 1.0f;

	public bool enableSounds = true;
	public bool enableTimeAdjust = true;

    //Private Variables
	private AudioSource sourceWind;
	private AudioSource sourceTurb1;
	private AudioSource sourceTurb2;
	private AudioSource sourceRain;
	private AudioSource sourceAmbDay;
	private AudioSource sourceAmbNight;

	private float _timescale;
	private AudioMixerGroup useMixGroup;


	void Start () {
		sourceWind = gameObject.AddComponent<AudioSource>();
		sourceTurb1 = gameObject.AddComponent<AudioSource>();
		sourceTurb2 = gameObject.AddComponent<AudioSource>();
		sourceRain = gameObject.AddComponent<AudioSource>();
		sourceAmbDay = gameObject.AddComponent<AudioSource>();
		sourceAmbNight = gameObject.AddComponent<AudioSource>();

		sourceWind.volume = 0.0f;
		sourceTurb1.volume = 0.0f;
		sourceTurb2.volume = 0.0f;
		sourceRain.volume = 0.0f;
		sourceAmbDay.volume = 0.0f;
		sourceAmbNight.volume = 0.0f;

		sourceWind.Stop();
		sourceTurb1.Stop();
		sourceTurb2.Stop();
		sourceRain.Stop();
		sourceAmbDay.Stop();
		sourceAmbNight.Stop();
	}



	void LateUpdate () {

		if (enableSounds){

			//cache time
			_timescale = Time.timeScale;
			
			float sndTimeMod = 1.0f;
			float sndPitchMod = 1.0f;

			if (enableTimeAdjust){
				sndTimeMod = Mathf.Clamp(Mathf.Abs(_timescale),0.2f,1f);
				sndPitchMod = _timescale;
			}

			//float audioLength = setstep.length;
			//float audioFrequency = setstep.frequency;
			//float audioSampleRate = setstep.samples;
			//float magnificationCalc = (((audioSampleRate/audioLength)));

			sourceWind.volume = volWind * sndTimeMod;
			sourceTurb1.volume = volTurb1 * sndTimeMod;
			sourceTurb2.volume = volTurb2 * sndTimeMod;
			sourceRain.volume = volRain * sndTimeMod;
			sourceAmbDay.volume = volAmbDay * sndTimeMod;
			sourceAmbNight.volume = volAmbNight * sndTimeMod;

			sourceWind.pitch = sndPitchMod;
			sourceTurb1.pitch = sndPitchMod;
			sourceTurb2.pitch = sndPitchMod;
			sourceRain.pitch = sndPitchMod;
			sourceAmbDay.pitch = sndPitchMod;
			sourceAmbNight.pitch = sndPitchMod;
			
			//get mixing group
			if (useMixGroup == null && mixer != null && mixerGroup != null){
				useMixGroup = mixer.FindMatchingGroups(mixerGroup.name)[0];
			}

			// Assign audio source properties
			if (audioWind != null && !sourceWind.isPlaying){
				if (useMixGroup != null) sourceWind.outputAudioMixerGroup = useMixGroup;
				sourceWind.clip = audioWind;
				sourceWind.loop = true;
				sourceWind.Play();
			}
			if (audioTurb1 != null && !sourceTurb1.isPlaying){
				if (useMixGroup != null) sourceTurb1.outputAudioMixerGroup = useMixGroup;
				sourceTurb1.clip = audioTurb1;
				sourceTurb1.loop = true;
				sourceTurb1.Play();
			}
			if (audioTurb2 != null && !sourceTurb2.isPlaying){
				if (useMixGroup != null) sourceTurb2.outputAudioMixerGroup = useMixGroup;
				sourceTurb2.clip = audioTurb2;
				sourceTurb2.loop = true;
				sourceTurb2.Play();
			}
			if (audioRain != null && !sourceRain.isPlaying){
				if (useMixGroup != null) sourceRain.outputAudioMixerGroup = useMixGroup;
				sourceRain.clip = audioRain;
				sourceRain.loop = true;
				sourceRain.Play();
			}
			if (audioAmbDay != null && !sourceAmbDay.isPlaying){
				if (useMixGroup != null) sourceAmbDay.outputAudioMixerGroup = useMixGroup;
				sourceAmbDay.clip = audioAmbDay;
				sourceAmbDay.loop = true;
				sourceAmbDay.Play();
			}
			if (audioAmbNight != null && !sourceAmbNight.isPlaying){
				if (useMixGroup != null) sourceAmbNight.outputAudioMixerGroup = useMixGroup;
				sourceAmbNight.clip = audioAmbNight;
				sourceAmbNight.loop = true;
				sourceAmbNight.Play();
			}

		} else {

			sourceWind.Stop();
			sourceTurb1.Stop();
			sourceTurb2.Stop();
			sourceRain.Stop();
			sourceAmbDay.Stop();
			sourceAmbNight.Stop();
		}

	}



}
}