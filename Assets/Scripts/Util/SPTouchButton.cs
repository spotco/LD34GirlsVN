using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/**
 * Custom button class for use in manual touch propagation
 */
public class SPTouchButton : CancelableTouchEventDelegate {
	public RectTransform _button_bounds;
	public float _anim_t;
	
	public bool _touch_began_on_button;
	public bool _button_proc;
	
	public void TouchBeginWithScreenPosition(Vector2 spos) {
		_touch_began_on_button = this.BoundsContainsScreenPoint(spos);
	}
	public void TouchHoldWithScreenPosition(Vector2 spos) {
		if (_touch_began_on_button && !this.BoundsContainsScreenPoint(spos)) {
			_touch_began_on_button = false;
		}
	}
	public void TouchEnd() {
		if (_touch_began_on_button) {
			_button_proc = true;
			_touch_began_on_button = false;
		}
	}
	
	/**
   * Get if the button was pressed and then clear state
   */
	public bool GetAndClearButtonProc() {
		bool rtv = _button_proc;
		_button_proc = false;
		return rtv;
	}
	
	public bool GetButtonProc() {
		return _button_proc;
	}
	
	/**
   * Is button currently pressed
   */
	public bool GetIsButtonDown() {
		return _touch_began_on_button;
	}
	
	private bool BoundsContainsScreenPoint(Vector2 screen_touch_pos) {
		Vector2 local_touch_pos = _button_bounds
			.InverseTransformPoint(screen_touch_pos);
		return (_button_bounds.rect.Contains(local_touch_pos));
	}
	
	public void TouchCancel() {
		_touch_began_on_button = false;
	}
	
	public int GetID() {
		return _button_bounds.gameObject.GetInstanceID();
	}
}

public interface SPUITouchButton : Updateable, CancelableTouchEventDelegate {
	void SetVisible(bool val);
	RectTransform GetBounds();
}

public abstract class SPUISingleElementAnimTouchbutton : SPUITouchButton {
	public Color _normal_color;
	public Color _selected_color;
	public System.Action _callback;
	public SPTouchButton _button;
	
	protected bool _visible = true;
	protected float _color_anim_t = 0;
	
	public virtual void PUpdate() {
		if (!_visible) return;
		if (_button.GetIsButtonDown()) {
			_color_anim_t = SPUtil.drpt(
				_color_anim_t, 1, 1 / 4.0f);
		} else {
			_color_anim_t = SPUtil.drpt(
				_color_anim_t, 0, 1 / 10.0f);
		}
		if (_button.GetAndClearButtonProc()) {
			_callback();
		}
	}
	
	public void TouchBeginWithScreenPosition(Vector2 spos) {
		if (!_visible) return;
		_button.TouchBeginWithScreenPosition(spos);
	}
	public void TouchHoldWithScreenPosition(Vector2 spos) {
		if (!_visible) return;
		_button.TouchHoldWithScreenPosition(spos);
	}
	public void TouchEnd() {
		if (!_visible) return;
		_button.TouchEnd();
	}
	public void TouchCancel() {
		if (!_visible) return;
		_button.TouchCancel();
	}
	public int GetID() {
		return _button.GetID();
	}
	public virtual void SetVisible(bool val) {
		_visible = val;
	}
	public RectTransform GetBounds() {
		return _button._button_bounds;
	}
}

public class SPUISingleImageColorTouchButton : SPUISingleElementAnimTouchbutton {
	public Image _image;
	public bool _disabled = false;
	public Color _disabled_color = new Color(0.85f, 0.85f, 0.85f, 0.75f);
	
	public override void PUpdate() {
		if (_disabled) {
			_image.color = _disabled_color;
			_button.TouchCancel();
		} else {
			base.PUpdate();
			_image.color = Color.Lerp(
				_normal_color, _selected_color, _color_anim_t
				);
		}
	}
	public override void SetVisible(bool val) {
		base.SetVisible(val);
		_image.gameObject.SetActive(val);
	}
	
	/**
   * Visible, but new animation state of greyed out and not accepting touch evts
   */
	public void SetButtonDisabled(bool val) {
		_disabled = val;
	}
}

public class SPUISingleTextColorTouchButton : SPUISingleElementAnimTouchbutton {
	public Text _image;
	
	public override void PUpdate() {
		base.PUpdate();
		_image.color = Color.Lerp(
			_normal_color, _selected_color, _color_anim_t
			);
	}
	public override void SetVisible(bool val) {
		base.SetVisible(val);
		_image.gameObject.SetActive(val);
	}
}
