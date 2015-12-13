using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Popup : MonoBehaviour {

	[SerializeField] private Text _text;
	[SerializeField] private CanvasGroup _canvas_group;

	public enum Mode {
		FadeIn,
		Hold,
		FadeOut,
		DoRemove
	}
	public Mode _current_mode;

	public void i_initialize(string text) {
		_current_mode = Mode.FadeIn;
		_text.text = text;
		
		this.transform.localScale = SPUtil.valv(1.2f);
		_canvas_group.alpha = 0;
	}
	
	public void i_update() {
		if (_current_mode == Mode.FadeIn) {
			this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,1,1/10.0f));
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha + 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha >= 1) {
				_current_mode = Mode.Hold;
			}
			
		} else if (_current_mode == Mode.Hold) {
		
		} else if (_current_mode == Mode.FadeOut) {
			this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,0,1/10.0f));
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha - 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha <= 0) {
				_current_mode = Mode.DoRemove;
			}
		}
	}
}
