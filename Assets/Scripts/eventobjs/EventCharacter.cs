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
	
	public void i_initialize() {
		_current_mode = Mode.FadeIn;
		_image.color = new Color(1,1,1,0);
	}
	
	public void i_update(GameMain game, EventModal modal) {
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
