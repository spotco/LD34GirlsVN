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
	
	private static float IMG_LOCAL_POS_Y = -140;
	
	public void i_initialize() {
		_current_mode = Mode.FadeIn;
		_image.color = new Color(1,1,1,0);
		_image.transform.localPosition = new Vector2(0,IMG_LOCAL_POS_Y);
		_anim_ct = Mathf.PI;
	}
	
	public void notify_talking() {
		_talking_ct = 3;
	}
	
	public void i_update(GameMain game, EventModal modal) {
	
		if (_talking_ct > 0) {
			_talking_ct -= 1;
			_anim_ct = (_anim_ct + 0.175f * SPUtil.dt_scale_get()) % (Mathf.PI);
			
		} else {
			_anim_ct = Mathf.Clamp(_anim_ct + 0.175f * SPUtil.dt_scale_get(),0,Mathf.PI);
		}
		_image.transform.localPosition = new Vector2(0,IMG_LOCAL_POS_Y+Mathf.Sin(_anim_ct)*5);
	
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
