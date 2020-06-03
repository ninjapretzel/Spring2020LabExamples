using UnityEngine;

/// <summary> Simple script to make it easier to play from a pool of sound clips, rather than always playing the same clip. </summary>
public class SoundCue : MonoBehaviour {
	/// <summary> Clips to choose from </summary>
	public AudioClip[] clips;
	/// <summary> Pitch modification settings </summary>
	public BMM pitchMod;
	/// <summary> Volume modification settings </summary>
	public BMM volumeMod;
	/// <summary> Connected audio source </summary>
	public AudioSource source;
	/// <summary> Should this sound play on awake? </summary>
	public bool playOnAwake = false;
	/// <summary> Original source pitch setting </summary>
	float originalPitch;
	/// <summary> Original source volume setting </summary>
	float originalVolume;

	void Awake() {
		// FInd source and destroy self if not present. 
		source = GetComponent<AudioSource>();
		if (source == null) {
			Destroy(this);
			return;
		}
		// Grab pitch/volume
		originalPitch = source.pitch;
		originalVolume = source.volume;
		// Play on awake if set.
		if (playOnAwake) {
			Play();
		}
	}

	/// <summary> Method to play from the sound pool </summary>
	public void Play() {
		// source could have been destroyed elsewhere 
		// (this script was used in various games I worked on where we would remove AudioSources from objects for reasons)
		if (source != null) {
			// Pick a clip
			var clip = clips[Random.Range(0, clips.Length)];
			// Randomize the pitch / volume
			float pitch = pitchMod.value;
			float volume = volumeMod.value;

			// Assign pitch and play an additional sound layer at the chosen volume
			source.pitch = originalPitch * pitch;
			source.PlayOneShot(clip, volume * originalVolume);
		}
	}
		
}
