using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGSchoolClassroom : BGControllerBase {

	[SerializeField] private Image _fade_cover;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _foreground;

	[SerializeField] private Transform _during_class_root;
	[SerializeField] private Transform _pre_class_root;

	[SerializeField] private Image _pulse_overlay;
	
	[SerializeField] private Transform _scroll_anchor;
	[SerializeField] private Transform _character_root;
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	private float _current_scale, _target_scale;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);

		_scroll_registry.add_registry_entry(_pulse_overlay.transform, 0);
		_scroll_registry.add_registry_entry(_character_root, -1);
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_foreground.transform, 1.25f);
		_scroll_registry.add_registry_entry(_during_class_root,1);
		_scroll_registry.add_registry_entry(_pre_class_root,1);

		_scroll_registry.add_registry_behaviour(_pulse_overlay.transform, PulseBGRegistryBehaviour.cons(
			new HideShowObjectRegistryBehaviour.Image_ImageTargetAdapter() {
				_img = _pulse_overlay
			}));
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		_current_scale = _target_scale = 1;	
	}
	
	public override Transform get_character_root() {
		return _character_root;
	}
	
	public override string get_registered_name() { return "bg_school_classroom"; }
	
	public override void show_background(string name, string key) {

		if (key.Contains("pulse1")) {
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_running(true,false);
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_count(1);
		} else if (key.Contains("pulse2")) {
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_running(true,false);
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_count(2);
		} else if (key.Contains("pulser")) {
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_running(true,true);
		} else {
			_scroll_registry.get_registry_behaviour<PulseBGRegistryBehaviour>(_pulse_overlay.transform).set_running(false,false);
		}

		if (key == BGControllerBase.KEY_DEFAULT) {
			_during_class_root.gameObject.SetActive(false);
			_pre_class_root.gameObject.SetActive(false);
			_target_scroll_pos = new Vector3(-10,0,0);
			_target_scale = 1;
	
		} else {
			if (key.Contains("during")) {
				_during_class_root.gameObject.SetActive(true);
				_pre_class_root.gameObject.SetActive(false);

			} else if (key.Contains("pre")) {
				_during_class_root.gameObject.SetActive(false);
				_pre_class_root.gameObject.SetActive(true);

			} else {
				_during_class_root.gameObject.SetActive(false);
				_pre_class_root.gameObject.SetActive(false);
			}

			if (key.Contains("normal")) {
				_target_scroll_pos = new Vector3(-10,0,0);
				_target_scale = 1;

			} else if (key.Contains("manafocus")) {
				_target_scroll_pos = new Vector3(63,-48,0);
				_target_scale = 1.15f;

			} else if (key.Contains("up")) {
				_target_scroll_pos = new Vector3(-10,-93,0);
				_target_scale = 1;
			}
		}

	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {		
		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;

		_current_scale = SPUtil.drpt(_current_scale, _target_scale, 1/30.0f);
		_scroll_anchor.transform.localScale = SPUtil.valv(_current_scale);

		//_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries(game);

		this.update_showing_mode(_fade_cover);
	}
}
