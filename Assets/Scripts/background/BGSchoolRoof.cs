﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGSchoolRoof : BGControllerBase {
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	
	[SerializeField] private Image _sky;
	[SerializeField] private Image _city_skyline;
	[SerializeField] private Image _roof;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
		
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_scroll_registry.add_registry_entry(_sky.transform, 1);
		_scroll_registry.add_registry_entry(_city_skyline.transform, 1.25f);
		_scroll_registry.add_registry_entry(_roof.transform, 1.5f);
	}
	
	public override string get_registered_name() { return "bg_school_roof"; }
	
	public override void show_background(string name, string key) {
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries(game);
	
		this.update_showing_mode(_fade_cover);
	}
}
