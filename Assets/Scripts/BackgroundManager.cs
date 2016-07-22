using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour {
	
	[SerializeField] public bool _disable_move_scrollanchor;
	
	private Dictionary<string, BGControllerBase> _name_to_bgcontroller = new Dictionary<string, BGControllerBase>();
	private List<BGControllerBase> _active_bgcontrollers = new List<BGControllerBase>();
	
	private struct EnqueuedBG {
		public string _name;
		public string _key;
		public BGControllerBase _controller;
	}
	private List<EnqueuedBG> _enqueued_bgcontrollers = new List<EnqueuedBG>();
	
	public void i_initialize(GameMain game) {
		
		_active_bgcontrollers.Clear();
		
		for (int i = 0; i < this.transform.childCount; i++) {
			Transform i_transform = this.transform.GetChild(i);
			if (i_transform.GetComponent<BGControllerBase>()) {
				BGControllerBase i_controller = i_transform.GetComponent<BGControllerBase>();
				i_controller.i_initialize(game);
				_name_to_bgcontroller[i_controller.get_registered_name()] = i_controller;
			}
		}
	}
	
	public void load_background(string name, string key) {
		if (!_name_to_bgcontroller.ContainsKey(name)) {
			SPUtil.errf("No registered background of name(%s)",name);
			return;
		}
		
		bool target_is_active = false;
		BGControllerBase target_bgcontroller = _name_to_bgcontroller[name];
		
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			BGControllerBase i_controller = _active_bgcontrollers[i];
			if (target_bgcontroller == i_controller) {
				target_is_active = true;
				target_bgcontroller.show_background(name,key);
				target_bgcontroller.set_showing(true);
				
			} else if (i_controller != target_bgcontroller) {
				i_controller.set_showing(false);	
			}
		}
		
		if (!target_is_active) {
			_enqueued_bgcontrollers.Clear();
			_enqueued_bgcontrollers.Add(new EnqueuedBG() {
				_name = name,
				_key = key,
				_controller = target_bgcontroller
			});
		}
	}
	
	public BGControllerBase get_latest_active_bgcontroller() {
		for (int i = _enqueued_bgcontrollers.Count-1; i >= 0; i--) {
			return _enqueued_bgcontrollers[i]._controller;
		}
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			return _active_bgcontrollers[i];
		}
		return null;
	}
	
	public void dispatch_update_message_to_active(string strparam, float numparam1, float numparam2) {
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			BGControllerBase i_controller = _active_bgcontrollers[i];
			i_controller.recieve_update_message(strparam, numparam1, numparam2);	
		}
	}
	
	public void i_update(GameMain game) {
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			BGControllerBase i_controller = _active_bgcontrollers[i];
			i_controller.i_update(game);
			if (!i_controller.should_remain_active()) {
				_active_bgcontrollers.RemoveAt(i);
			}
		}
		if (_active_bgcontrollers.Count == 0 && _enqueued_bgcontrollers.Count > 0) {
			for (int i = 0; i < _enqueued_bgcontrollers.Count; i++) {
				EnqueuedBG itr = _enqueued_bgcontrollers[i];
				itr._controller.show_background(itr._name, itr._key);
				itr._controller.set_showing(true);
				_active_bgcontrollers.Add(itr._controller);
			}
		}
	}
	

}
