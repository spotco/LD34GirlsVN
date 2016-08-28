using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ControlManager {
	
	public enum Control {
		MoveLeft,
		MoveRight,
		MoveUp,
		MoveDown,
		ButtonA,
		ButtonB,
		Debug1,
		Debug2,
		
		TouchClick,
		
		None
	}
	private static bool control_is_pressed(Control test) {
		switch (test) {
		case Control.MoveLeft: {
			return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Keypad4);
		}
		case Control.MoveRight: {
			return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.Keypad6);
		}
		case Control.MoveUp: {
			return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Keypad8);
		}
		case Control.MoveDown: {
			return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.Keypad5);
		}
		case Control.Debug1: {
			return Input.GetKey(KeyCode.Alpha0);
		}
		case Control.Debug2: {
			return Input.GetKey(KeyCode.Alpha1);
		}
		case Control.ButtonB: {
			return Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.LeftShift);
		}
		case Control.ButtonA: {
			return Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space);
		}
		case Control.TouchClick: {
			return SPUtil.is_touch();
		}
		}
		return false;
	}
	
	public static ControlManager cons() {
		return (new ControlManager()).i_cons();
	}
	
	private List<Control> _controls_to_test = new List<Control>();
	private Dictionary<Control,bool> _control_is_down = new Dictionary<Control, bool>();
	private Dictionary<Control,bool> _control_just_released = new Dictionary<Control, bool>();
	private Dictionary<Control,bool> _control_just_pressed = new Dictionary<Control, bool>();
	private bool _press_debug_triggered = false;
	
	public ControlManager i_cons() {
		foreach (Control itr in System.Enum.GetValues(typeof(Control)).Cast<Control>()) {
			_controls_to_test.Add(itr);
			_control_is_down[itr] = false;
			_control_just_released[itr] = false;
			_control_just_pressed[itr] = false;
		}
		_press_debug_triggered = false;
		return this;
	}
	
	public bool get_debug_skip() {
		bool keys_down = this.get_control_down(Control.Debug1) && this.get_control_down(Control.Debug2);
		if (GameMain.DEBUG_CONTROLS && keys_down && !_press_debug_triggered) {
			_press_debug_triggered = true;
			return true;
		}
		return false;
	}
	
	public void i_update() {
		for (int i_test = 0; i_test < _controls_to_test.Count; i_test++) {
			Control itr_test = _controls_to_test[i_test];
			
			_control_just_pressed[itr_test] = false;
			_control_just_released[itr_test] = false;
			
			bool itr_test_pressed = this.control_is_pressed(itr_test);
			if (itr_test_pressed) {
				if (!_control_is_down[itr_test]) {
					_control_just_pressed[itr_test] = true;
					_press_debug_triggered = false;
				}
				_control_is_down[itr_test] = true;
			} else {
				if (_control_is_down[itr_test]) {
					_control_just_released[itr_test] = true;
				}
				_control_is_down[itr_test] = false;
			}
		}
		
		Vector2 frame_touch_pos;
		if (SPUtil.is_touch_and_position(out frame_touch_pos)) {
			_touch_pos = frame_touch_pos;
		} else if (SPUtil.has_mouse_and_position(out frame_touch_pos)) {
			_touch_pos = frame_touch_pos;
		}
	}
	
	private Vector2 _touch_pos;
	public Vector2 get_touch_pos() { return _touch_pos; }
	
	public bool get_control_just_pressed(Control test) {
		return _control_just_pressed[test];
	}
	public bool get_control_just_released(Control test) {
		return _control_just_released[test];
	}
	public bool get_control_down(Control test) {
		return _control_is_down[test];
	}
}
