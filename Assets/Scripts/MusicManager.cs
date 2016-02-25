using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {

	private AudioClip _sound;
	[SerializeField] private AudioSource _bg;
	[SerializeField] private AudioSource _sfx;

	private string _currently_loaded_music = "";
	private string _target_loaded_music = "";

	private float _current_volume = 0.0f;

	private enum Mode {
		NotPlaying,
		FadeOut,
		FadeIn,
		Playing
	}
	private Mode _current_mode;

	public void i_initialize() {
		_current_volume = 0.0f;
		_bg.volume = _current_volume;
		_bgm_fade_t = 1;
		_bgm_fade_incr = 0;
		_current_mode = Mode.NotPlaying;
	}

	private Dictionary<string,AudioClip> _cached_background_audio = new Dictionary<string, AudioClip>();
	public AudioClip cond_load_sound_of_name(string name) {
		if (_cached_background_audio.ContainsKey(name)) return _cached_background_audio[name];
		AudioClip audio_clip = Resources.Load<AudioClip>("sound/"+name);
		if (audio_clip != null) {
			_cached_background_audio[name] = audio_clip;
		}
		return audio_clip;
	}

	public void play_sfx(string name) {
		_sfx.PlayOneShot (cond_load_sound_of_name (name), 0.9f);
	}

	public void load_music(string name) {
		if (GameMain.MUTE) return;
		_target_loaded_music = name;
		if (_currently_loaded_music != _target_loaded_music) {
			if (_currently_loaded_music == "") {
				_currently_loaded_music = _target_loaded_music;
				_bg.volume = 0f;
				_bg.clip = cond_load_sound_of_name (_target_loaded_music);
				_bg.loop = true;
				_bg.Play ();
				_current_mode = Mode.FadeIn;
			} else {
				_current_mode = Mode.FadeOut;
			}
		} 
	}
	
	private float _bgm_fade_t;
	private float _bgm_fade_incr;
	
	public void fade_bgm_for_time(float sec) {
		_bgm_fade_t = 0;
		_bgm_fade_incr = SPUtil.sec_to_tick(sec);
	}
	
	private const float BGM_TARGET_VOLUME = 0.5f;
	
	public void i_update() {
		switch (_current_mode) {
			case Mode.FadeIn:{
				if (_current_volume < BGM_TARGET_VOLUME) {
					_current_volume = Mathf.Min(_bg.volume + SPUtil.sec_to_tick(4f) * SPUtil.dt_scale_get(), BGM_TARGET_VOLUME);
				} else {
					_current_mode = Mode.Playing;
				}
			} break;
			case Mode.FadeOut:{
				if (_current_volume > 0) {
					_current_volume = Mathf.Max(_bg.volume - SPUtil.sec_to_tick(4f) * SPUtil.dt_scale_get(), 0);
				} else {
					_currently_loaded_music = _target_loaded_music;
					_bg.clip = cond_load_sound_of_name (_target_loaded_music);
					_bg.Play ();
					_current_mode = Mode.FadeIn;
				}
			} break;
			case Mode.Playing:{	
			} break;
		}
		
		_bgm_fade_t = Mathf.Clamp(_bgm_fade_t+_bgm_fade_incr*SPUtil.dt_scale_get(),0,1);
		float bgm_fade_mult = Mathf.Clamp(SPUtil.bezier_val_for_t(new Vector2(0,1),new Vector2(0,0f),new Vector2(1,0f),new Vector2(1,1),_bgm_fade_t).y,0,1);
		_bg.volume = _current_volume * bgm_fade_mult;
	}
}
