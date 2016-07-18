using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BGCityAfternoon : BGControllerBase {
	
	private Dictionary<string, Vector2> _key_to_scrollpoint = new Dictionary<string, Vector2>();
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _back_buildings;
	[SerializeField] private Image _monster;
	[SerializeField] private Image _front_buildings;
	[SerializeField] private Image _hero_mana;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	public override void i_initialize(GameMain game) {
		_monster.gameObject.SetActive(false);
		_hero_mana.gameObject.SetActive(false);
		
		_fade_cover.color = new Color(0,0,0,1);
		
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_back_buildings.transform, 1.5f);
		_scroll_registry.add_registry_entry(_monster.transform, 1.6f);
		_scroll_registry.add_registry_entry(_front_buildings.transform, 2.0f);
		_scroll_registry.add_registry_entry(_hero_mana.transform, 2.5f);
		
		_key_to_scrollpoint[BGControllerBase.KEY_DEFAULT] = new Vector2(20,25);
		_key_to_scrollpoint["default_left"] = new Vector2(20,25);
		_key_to_scrollpoint["default_right"] = new Vector2(-25,25);
		_key_to_scrollpoint["default_center"] = new Vector2(0,25);
		_key_to_scrollpoint["sky_center"] = new Vector2(0,-30);
		
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
	}
	
	public override string get_registered_name() { return "bg_city_afternoon"; }
	
	public override void show_background(string name, string key) {
	
		if (_key_to_scrollpoint.ContainsKey(key)) {
			_target_scroll_pos = _key_to_scrollpoint[key];
		}
		if (_current_showing_mode == ShowingMode.Hidden) {
			_current_scroll_pos = _target_scroll_pos;
		}
		
		_current_showing_mode = ShowingMode.TransitionShowing;
	}
	
	public override void i_update(GameMain game) {
		this.update_showing_mode(_fade_cover);
		
		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries();
		
		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
	}
	
	public override void set_showing(bool val) {}
	
}
