using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGHillShrine : BGControllerBase {
	
	private Dictionary<string, Vector2> _key_to_scrollpoint = new Dictionary<string, Vector2>();
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _citybackground;
	[SerializeField] private Image _foreground;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
		
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_citybackground.transform, 1.5f);
		_scroll_registry.add_registry_entry(_foreground.transform, 1.8f);
		
		_key_to_scrollpoint[BGControllerBase.KEY_DEFAULT] = new Vector2(0,0);
		_key_to_scrollpoint["up"] = new Vector2(0,-50);
	}
	
	public override string get_registered_name() { return "bg_hill_shrine"; }
	
	public override void show_background(string name, string key) {
		if (_key_to_scrollpoint.ContainsKey(key)) {
			_target_scroll_pos = _key_to_scrollpoint[key];
		}
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries(game);
		
		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
	
		this.update_showing_mode(_fade_cover);
	}
}
