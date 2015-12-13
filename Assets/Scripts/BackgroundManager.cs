using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour {

	[SerializeField] private Image _image;
	
	
	
	private string _currently_loaded_background = "";
	private string _target_loaded_background = "";
	
	private enum Mode {
		FadeOut,
		FadeIn,
		Show
	}
	private Mode _current_mode;
	
	public void i_initialize() {
		_image.sprite = this.cond_load_sprite_of_name("bg_blank");
		_image.color = new Color(0,0,0,1);
		_current_mode = Mode.Show;
	}
	
	private Dictionary<string,Sprite> _cached_background_images = new Dictionary<string, Sprite>();
	private Sprite cond_load_sprite_of_name(string name) {
		if (_cached_background_images.ContainsKey(name)) return _cached_background_images[name];
		Sprite bg_sprite = Resources.Load<Sprite>("img/bg/"+name);
		if (bg_sprite != null) {
			_cached_background_images[name] = bg_sprite;
		}
		return bg_sprite;
	}
	
	public void load_background(string name) {
		_target_loaded_background = name;
		if (_currently_loaded_background != _target_loaded_background) {
			_current_mode = Mode.FadeOut;
		}
	}
	
	public void i_update() {
		if (_current_mode == Mode.FadeOut) {
			float t = _image.color.r;
			if (t <= 0) {
				_current_mode = Mode.FadeIn;
				_image.sprite = this.cond_load_sprite_of_name(_target_loaded_background);
				_currently_loaded_background = _target_loaded_background;
				
			} else {
				t -= 0.1f * SPUtil.dt_scale_get();
				_image.color = new Color(t,t,t,1);
			}
			
		} else if (_current_mode == Mode.FadeIn) {
			float t = _image.color.r;
			if (t < 1) {
				t += 0.1f * SPUtil.dt_scale_get();
				_image.color = new Color(t,t,t,1);
			} else {
				_current_mode = Mode.Show;
			}
		}
	}
	

}
