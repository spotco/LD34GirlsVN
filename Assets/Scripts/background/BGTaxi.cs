using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGTaxi : BGControllerBase {
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	
	[SerializeField] private Image _background;
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
	}
	
	public override string get_registered_name() { return "bg_taxi"; }
	
	public override void show_background(string name, string key) {
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		this.update_showing_mode(_fade_cover);
	}
}
