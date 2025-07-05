using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour 
{
	public static MusicManager Instance;
	public AudioSource audioSrc;
	public AudioClip main_music;
	public AudioClip results_music;
	public AudioClip drowning_music;

	// Use this for initialization
	void Start () 
	{
		Instance = this;
		audioSrc = GetComponent<AudioSource> ();
		audioSrc.clip = main_music;
		audioSrc.Play();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void set_audio(AudioClip audio, bool is_looping=true)
	{
		audioSrc.loop = is_looping;
		audioSrc.Stop ();
		audioSrc.clip = audio;
		audioSrc.PlayDelayed (1f);
	}
}
