using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Popup : MonoBehaviour {

	[SerializeField] private Text _text;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private Image _heart_icon;

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
		
		_heart_icon.gameObject.SetActive(false);
	}
	
	private float _anim_theta;
	public void show_heart() {
		_heart_icon.gameObject.SetActive(true);
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
			this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,1.2f,1/10.0f));
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha - 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha <= 0) {
				_current_mode = Mode.DoRemove;
			}
		}
		
		if (_heart_icon.gameObject.activeSelf) {
			_anim_theta += 0.05f;
			_heart_icon.transform.localEulerAngles = new Vector3(0,0,Mathf.Sin(_anim_theta)*7.5f);
		}
	}
}
