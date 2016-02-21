using System.Collections.Generic;
using UnityEngine;

public interface TouchEventDelegate {
	void TouchBeginWithScreenPosition(Vector2 spos);
	void TouchHoldWithScreenPosition(Vector2 spos);
	void TouchEnd();
	int GetID();
}
public interface CancelableTouchEventDelegate : TouchEventDelegate {
	void TouchCancel();
}
public interface Updateable {
	void PUpdate();
}

public class TouchEventDelegateHelper {
	public static void DispatchTouchBegin<T>(List<T> children, 
	                                         Vector2 spos) where T : TouchEventDelegate {
		for (int i = 0; i < children.Count; i++)
			children[i].TouchBeginWithScreenPosition(spos);
	}
	public static void DispatchTouchHold<T>(List<T> children, 
	                                        Vector2 spos) where T : TouchEventDelegate {
		for (int i = 0; i < children.Count; i++)
			children[i].TouchHoldWithScreenPosition(spos);
	}
	public static void DispatchTouchEnd<T>(List<T> children)
	where T : TouchEventDelegate {
		for (int i = 0; i < children.Count; i++)
			children[i].TouchEnd();
	}
	public static void DispatchTouchCancel<T>(List<T> children)
	where T : CancelableTouchEventDelegate {
		for (int i = 0; i < children.Count; i++)
			children[i].TouchCancel();
	}
}

public class TouchEventDelegateDispatcher {
	
	private Dictionary<int,bool> _delegate_id_to_touch_started_on_delegate
		= new Dictionary<int, bool>();
	private Dictionary<int,bool> _delegate_id_to_is_touch_last_frame
		= new Dictionary<int, bool>();
	
	private bool _should_ignore_all_input = false;
	
	public void PInitialize() {}
	
	public void PDispatchTouchWithDelegate(TouchEventDelegate tar, RectTransform bounds) {
		if (_should_ignore_all_input) return;
		int id = tar.GetID();
		if (!_delegate_id_to_is_touch_last_frame.ContainsKey(id))
			_delegate_id_to_is_touch_last_frame[id] = false;
		if (!_delegate_id_to_touch_started_on_delegate.ContainsKey(id))
			_delegate_id_to_touch_started_on_delegate[id] = false;
		
		Vector2 touch_pos;
		bool is_touch = SPUtil.is_touch_and_position(out touch_pos);    
		
		if (is_touch && SPUtil.rect_transform_contains_screen_point(bounds,touch_pos)) {
			if (!_delegate_id_to_is_touch_last_frame[id]) {
				tar.TouchBeginWithScreenPosition(touch_pos);
				_delegate_id_to_touch_started_on_delegate[id] = true;
			} else if (_delegate_id_to_touch_started_on_delegate[id]) {
				tar.TouchHoldWithScreenPosition(touch_pos);
			}
		} else {
			if (_delegate_id_to_touch_started_on_delegate[id]) {
				tar.TouchEnd();
				_delegate_id_to_touch_started_on_delegate[id] = false;
			}
		}
		_delegate_id_to_is_touch_last_frame[id] = is_touch;
	}
	
	public void ResetAll() {
		_delegate_id_to_is_touch_last_frame.Clear();
		_delegate_id_to_touch_started_on_delegate.Clear();
	}
	
	public void SetShouldIgnoreAllInput(bool val) {
		_should_ignore_all_input = val;
	}
	
}