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
		case Control.ButtonB: {
			return Input.GetKey(KeyCode.X);
		}
		case Control.ButtonA: {
			return Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space);
		}
		}
		return false;
	}
	
	public static ControlManager cons() {
		return (new ControlManager()).i_cons();
	}
	
	private bool _is_move_x, _is_move_y;
	private Vector2 _move_vec;
	private List<Control> _controls_to_test = new List<Control>();
	private Dictionary<Control,bool> _control_is_down = new Dictionary<Control, bool>();
	private Dictionary<Control,bool> _control_just_released = new Dictionary<Control, bool>();
	private Dictionary<Control,bool> _control_just_pressed = new Dictionary<Control, bool>();
	
	public ControlManager i_cons() {
		foreach (Control itr in System.Enum.GetValues(typeof(Control)).Cast<Control>()) {
			_controls_to_test.Add(itr);
			_control_is_down[itr] = false;
			_control_just_released[itr] = false;
			_control_just_pressed[itr] = false;
		}
		return this;
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
				}
				_control_is_down[itr_test] = true;
			} else {
				if (_control_is_down[itr_test]) {
					_control_just_released[itr_test] = true;
				}
				_control_is_down[itr_test] = false;
			}
		}
		
		bool frame_is_move_x = false;
		bool frame_is_move_y = false;
		Vector2 move_mag = Vector2.zero;
		if (this.get_control_down(Control.MoveLeft)) {
			move_mag.x = SPUtil.drpt(_move_vec.x,-1,1/10.0f);
			frame_is_move_x = true;
		} else if (this.get_control_down(Control.MoveRight)) {
			move_mag.x = SPUtil.drpt(_move_vec.x,1,1/10.0f);
			frame_is_move_x = true;
		} else {
			move_mag.x = 0;
		}
		
		if (this.get_control_down(Control.MoveUp)) {
			move_mag.y = SPUtil.drpt(_move_vec.y,1,1/10.0f);
			frame_is_move_y = true;
		} else if (this.get_control_down(Control.MoveDown)) {
			move_mag.y = SPUtil.drpt(_move_vec.y,-1,1/10.0f);
			frame_is_move_y = true;
		} else {
			move_mag.y = 0;
		}
		
		if (move_mag.magnitude > 1) {
			move_mag.Normalize();
		}
		_move_vec = move_mag;
		
		_is_move_x = frame_is_move_x;
		_is_move_y = frame_is_move_y;
	}
	
	public bool get_control_just_pressed(Control test) {
		return _control_just_pressed[test];
	}
	public bool get_control_just_released(Control test) {
		return _control_just_released[test];
	}
	public bool get_control_down(Control test) {
		return _control_is_down[test];
	}
	
	public bool is_move_x() { return _is_move_x; }
	public bool is_move_y() { return _is_move_y; }
	public Vector2 get_move() { return _move_vec; }
}
