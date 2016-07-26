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
		this.i_initialize_hidden(_fade_cover);
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_back_buildings.transform, 1.5f);
		_scroll_registry.add_registry_entry(_monster.transform, 1.8f);
		_scroll_registry.add_registry_entry(_front_buildings.transform, 2.0f);
		_scroll_registry.add_registry_entry(_hero_mana.transform, 2.5f);
		
		_key_to_scrollpoint[BGControllerBase.KEY_DEFAULT] = new Vector2(20,25);
		_key_to_scrollpoint["default_left"] = new Vector2(20,25);
		_key_to_scrollpoint["default_right"] = new Vector2(-25,25);
		_key_to_scrollpoint["default_center"] = new Vector2(0,25);
		_key_to_scrollpoint["sky_center"] = new Vector2(0,-30);
		
		_scroll_registry.add_registry_behaviour(_monster.transform, HideShowImageRegistryBehaviour.cons(_monster));
		_scroll_registry.add_registry_behaviour(_monster.transform, MovingCharacterRegistryBehaviour.cons(_monster.transform, _monster.transform.localPosition));
		_scroll_registry.add_registry_behaviour(_hero_mana.transform, HideShowImageRegistryBehaviour.cons(_hero_mana));
		_scroll_registry.add_registry_behaviour(_hero_mana.transform, MovingCharacterRegistryBehaviour.cons(_hero_mana.transform, _hero_mana.transform.localPosition));
	}
	
	public override string get_registered_name() { return "bg_city_afternoon"; }
	
	public override void show_background(string name, string key) {
		if (_key_to_scrollpoint.ContainsKey(key)) {
			_target_scroll_pos = _key_to_scrollpoint[key];
		}
		if (_current_showing_mode == ShowingMode.Hidden) {
			_current_scroll_pos = _target_scroll_pos;
		}
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
		if (strparam == "showmonster") {
			_scroll_registry.get_registry_behaviour<HideShowImageRegistryBehaviour>(_monster.transform).set_visible(true);
			
		} else if (strparam == "hidemonster") {
			_scroll_registry.get_registry_behaviour<HideShowImageRegistryBehaviour>(_monster.transform).set_visible(false);
		
		} else if (strparam == "movemonster") {
			_scroll_registry.get_registry_behaviour<MovingCharacterRegistryBehaviour>(_monster.transform).move_to(new Vector2(numparam1,numparam2));
		
		} else if (strparam == "showheromana") {
			_scroll_registry.get_registry_behaviour<HideShowImageRegistryBehaviour>(_hero_mana.transform).set_visible(true);
		
		} else if (strparam == "hideheromana") {
			_scroll_registry.get_registry_behaviour<HideShowImageRegistryBehaviour>(_hero_mana.transform).set_visible(false);
		
		} else if (strparam == "moveheromana") {
			_scroll_registry.get_registry_behaviour<MovingCharacterRegistryBehaviour>(_hero_mana.transform).move_to(new Vector2(numparam1,numparam2));
			
		} else {
			SPUtil.errf("BGCityAfternoon unknown update message (%s)",strparam);
		}
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
