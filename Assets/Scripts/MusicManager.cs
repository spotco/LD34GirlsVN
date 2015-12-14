using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {

	[SerializeField] private AudioClip _sound;
	[SerializeField] private AudioSource _bg;

	private string _currently_loaded_music = "";
	private string _target_loaded_music = "";

	private float fade_seconds = 2.0f;

	private enum Mode {
		FadeOut,
		FadeIn,
		Playing, 
		Pause
	}
	private Mode _current_mode;

	public void i_initialize() {
		_bg.volume = 0.0f;
	}

	private Dictionary<string,AudioClip> _cached_background_audio = new Dictionary<string, AudioClip>();
	private AudioClip cond_load_sound_of_name(string name) {
		if (_cached_background_audio.ContainsKey(name)) return _cached_background_audio[name];
		AudioClip audio_clip = Resources.Load<AudioClip>("sound/"+name);
		if (audio_clip != null) {
			_cached_background_audio[name] = audio_clip;
		}
		return audio_clip;
	}

	public void load_music(string name) {
		_target_loaded_music = name;
		if (_currently_loaded_music != _target_loaded_music) {
			if (_currently_loaded_music == "") {
				_currently_loaded_music = _target_loaded_music;
				_bg.volume = 0f;
				_bg.clip = cond_load_sound_of_name (_target_loaded_music);
				_bg.Play ();
				_current_mode = Mode.FadeIn;
			} else {
				_current_mode = Mode.FadeOut;
			}
		} 
	}

	public void i_update() {
		if (_current_mode == Mode.FadeOut) {
			fadeOut ();
		} else if (_current_mode == Mode.FadeIn) {
			fadeIn ();
		}
	}

	private void fadeOut() {
		float volumeNow = _bg.volume;
		if (volumeNow > 0f) {
			_bg.volume -= Time.deltaTime/fade_seconds;
		} 
		else {
			_currently_loaded_music = _target_loaded_music;
			_bg.clip = cond_load_sound_of_name (_target_loaded_music);
			_bg.Play ();
			_current_mode = Mode.FadeIn;

		}
	}

	private void fadeIn() {
		float volumeNow = _bg.volume;
		if (volumeNow < 8.0f) {
			_bg.volume += Time.deltaTime/fade_seconds;
		} 
		else {
			_current_mode = Mode.Playing;
		}
	}
}
