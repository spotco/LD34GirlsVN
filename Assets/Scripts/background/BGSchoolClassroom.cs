using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGSchoolClassroom : BGControllerBase {

	private Dictionary<string, Vector2> _key_to_scrollpoint = new Dictionary<string, Vector2>();
	
	[SerializeField] private Image _fade_cover;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _foreground;
	
	[SerializeField] private Transform _scroll_anchor;
	[SerializeField] private Transform _character_root;
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
	
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_foreground.transform, 1.25f);
		
		_key_to_scrollpoint[BGControllerBase.KEY_DEFAULT] = new Vector2(-10,0);
		_key_to_scrollpoint["left"] = new Vector2(20,0);
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
	}
	
	public override Transform get_character_root() {
		return _character_root;
	}
	
	public override string get_registered_name() { return "bg_school_classroom"; }
	
	public override void show_background(string name, string key) {
		if (_key_to_scrollpoint.ContainsKey(key)) {
			_target_scroll_pos = _key_to_scrollpoint[key];
		}
		if (_current_showing_mode == ShowingMode.Hidden) {
			_current_scroll_pos = _target_scroll_pos;
		}
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries();
		
		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
	
		this.update_showing_mode(_fade_cover);
	}
}
