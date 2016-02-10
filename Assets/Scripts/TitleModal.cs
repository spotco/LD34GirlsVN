using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Text _text;
	[SerializeField] private CanvasGroup _canvas_group;
	
	public enum Mode {
		Hide,
		FadeIn,
		Hold,
		FadeOut
	}
	public Mode _current_mode;
	
	private bool _end_screen;
	private float _anim_t;
	
	public void i_initialize(GameMain game) {
		this.gameObject.SetActive(false);
		_current_mode = Mode.Hide;
		_end_screen = false;
	}
	public void i_update(GameMain game) {
		if (_current_mode == Mode.Hold && !_end_screen) {
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				game._music.play_sfx("map_yes");
				game._active_modal = game._grid_nav_modal;
				_current_mode = Mode.FadeOut;
			}
		}
	}
	
	public void set_text(string val) {
		_text.text = val;
	}
	
	public void set_end_screen() {
		_end_screen = true;	
	}
	
	public void anim_update(GameMain game) {
		if (_current_mode == Mode.Hide) {
			this.gameObject.SetActive(false);
			
		} else if (_current_mode == Mode.FadeIn) {
			this.gameObject.SetActive(true);
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha + 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha >= 1) {
				_current_mode = Mode.Hold;
			}
			
		} else if (_current_mode == Mode.Hold) {
			_anim_t += SPUtil.dt_scale_get();
			if (!_end_screen) {
				if (_anim_t > 40) {
					_text.gameObject.SetActive(!_text.gameObject.activeSelf);
					_anim_t = 0;
				}
			} else {
				_text.gameObject.SetActive(true);
			}
			
		
		} else if (_current_mode == Mode.FadeOut) {
			this.gameObject.SetActive(true);
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha - 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha <= 0) {
				_current_mode = Mode.Hide;
			}
		}
	}
}
