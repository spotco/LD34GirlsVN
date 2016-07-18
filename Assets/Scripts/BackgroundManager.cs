using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour {
	
	[SerializeField] public bool _disable_move_scrollanchor;
	
	private Dictionary<string, BGControllerBase> _name_to_bgcontroller = new Dictionary<string, BGControllerBase>();
	private List<BGControllerBase> _active_bgcontrollers = new List<BGControllerBase>();
	
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
		
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			BGControllerBase i_controller = _active_bgcontrollers[i];
			i_controller.set_showing(false);	
		}
		
		BGControllerBase target_bgcontroller = _name_to_bgcontroller[name];
		target_bgcontroller.show_background(name, key);
		target_bgcontroller.set_showing(true);
		_active_bgcontrollers.Add(target_bgcontroller);
	}
	
	public void i_update(GameMain game) {
		for (int i = _active_bgcontrollers.Count-1; i >= 0; i--) {
			BGControllerBase i_controller = _active_bgcontrollers[i];
			i_controller.i_update(game);
			if (!i_controller.should_remain_active()) {
				_active_bgcontrollers.RemoveAt(i);
			}
		}
	}
	

}
