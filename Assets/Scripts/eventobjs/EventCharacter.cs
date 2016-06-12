using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EventCharacter : MonoBehaviour {

	[SerializeField] public Image _image;
	[SerializeField] public Image _fade_image;
	
	private string _current_image_name;
	private bool _is_transitioning_fade_image;
	private float _rotation_anim_t;
	private float _rotation_anim_target_t;
	
	bool _has_set_facing;
	
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
		
		_current_image_name = null;
		_is_transitioning_fade_image = false;
		_has_set_facing = false;
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
		_fade_image.transform.localPosition = _image.transform.localPosition;
	
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
		
		if (_is_transitioning_fade_image) {
			_fade_image.color = new Color(1,1,1,_fade_image.color.a + 0.25f*SPUtil.dt_scale_get());
			if (_fade_image.color.a >= 1) {
				_is_transitioning_fade_image = false;
				_image.sprite = _fade_image.sprite;
				_fade_image.gameObject.SetActive(false);
			}
		}
		
		_rotation_anim_t = SPUtil.lmovto(_rotation_anim_t, _rotation_anim_target_t, 0.1f * SPUtil.dt_scale_get());
		
		_image.transform.localRotation = SPUtil.set_rotation_quaternion(_image.transform.localRotation, new Vector3(0,
			SPUtil.lerp(0, 180,
			SPUtil.bezier_val_for_t(
				new Vector2(0,0),
				new Vector2(0.5f,0),
				new Vector2(0.5f,1),
				new Vector2(1,1),
				_rotation_anim_t
			).y), 
		0));
		_fade_image.transform.localRotation = _image.transform.localRotation;
		
	}
	
	public void set_image(Sprite image, string image_name) {
		if (_current_image_name == null) {
			_image.sprite = image;
			_fade_image.sprite = image;
			_fade_image.gameObject.SetActive(false);
			_current_image_name = image_name;
		} else {
			_fade_image.gameObject.SetActive(true);
			_fade_image.sprite = image;
			_fade_image.color = new Color(1,1,1,0);
			_is_transitioning_fade_image = true;
		}
		
		_fade_image.SetNativeSize();
		_image.SetNativeSize();
	}
	
	public void set_facing(float xscale) {
		this.transform.localScale = SPUtil.valv(0.75f);
		if (!_has_set_facing) {
			_has_set_facing = true;
			_image.transform.localRotation = SPUtil.set_rotation_quaternion(_image.transform.localRotation, new Vector3(0, xscale > 0 ? 0 : 180, 0));
			_fade_image.transform.localRotation = _image.transform.localRotation;
			_rotation_anim_t = xscale > 0 ? 0 : 1;
			_rotation_anim_target_t = _rotation_anim_t;
			
		} else {
			_rotation_anim_target_t = xscale > 0 ? 0 : 1;
			
		}
	}
	
	public void imm_show() {
		_image.color = new Color(1,1,1,1);
		_current_mode = Mode.Visible;
	}
	public void imm_hide() {
		_image.color = new Color(1,1,1,0);
		_current_mode = Mode.DoRemove;
	}
	
}
