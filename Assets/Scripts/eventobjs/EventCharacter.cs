using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EventCharacter : MonoBehaviour {

	[SerializeField] public Image _image;
	
	public enum Mode {
		FadeIn,
		Visible,
		FadeOut,
		DoRemove
	}
	public Mode _current_mode;
	public float _talking_ct;
	public float _anim_ct;
	
	public void i_initialize() {
		_current_mode = Mode.FadeIn;
		_image.color = new Color(1,1,1,0);
		_image.transform.localPosition = new Vector2();
	}
	
	public void notify_talking() {
		_talking_ct = 3;
	}
	
	public void i_update(GameMain game, EventModal modal) {
	
		if (_talking_ct > 0) {
			_talking_ct -= SPUtil.dt_scale_get();
			_anim_ct += 0.35f;
			_image.transform.localPosition = new Vector2(0,Mathf.Sin(_anim_ct)*5);
		} else {
			_image.transform.localPosition = new Vector2(0,0);
		}
		
	
		if (_current_mode == Mode.FadeIn) {
			_image.color = new Color(1,1,1,_image.color.a+0.1f*SPUtil.dt_scale_get());
			if (_image.color.a >= 1.0f) {
				_current_mode = Mode.Visible;
			}
			
		} else if (_current_mode == Mode.Visible) {

		
		} else if (_current_mode == Mode.FadeOut) {
			_image.color = new Color(1,1,1,_image.color.a-0.1f*SPUtil.dt_scale_get());
			if (_image.color.a <= 0.0f) {
				_current_mode = Mode.DoRemove;
			}
		}
	
	}
	
}
