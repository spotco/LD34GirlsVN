using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGSchoolHallway : BGControllerBase {
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
	}
	
	public override string get_registered_name() { return "bg_school_hallway"; }
	
	public override void show_background(string name, string key) {
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		this.update_showing_mode(_fade_cover);
	}
}
