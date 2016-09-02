using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGHill : BGControllerBase {
	
	private Dictionary<string, Vector2> _key_to_scrollpoint = new Dictionary<string, Vector2>();
				
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
		
		_key_to_scrollpoint[BGControllerBase.KEY_DEFAULT] = new Vector2(0,65);
		_key_to_scrollpoint["up"] = new Vector2(0,-100);
		
		_current_scroll_pos = _key_to_scrollpoint[BGControllerBase.KEY_DEFAULT];
		_target_scroll_pos = _current_scroll_pos;
	}
	
	public override string get_registered_name() { return "bg_hill"; }
	
	public override void show_background(string name, string key) {
		if (_key_to_scrollpoint.ContainsKey(key)) {
			_target_scroll_pos = _key_to_scrollpoint[key];
		}
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
	
		this.update_showing_mode(_fade_cover);
	}
}
