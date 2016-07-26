using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGSchoolHallway : BGControllerBase {
	
	[SerializeField] private Image _fade_cover;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _frontperson_left;
	[SerializeField] private Image _frontperson_right;
	[SerializeField] private Image _midperson_left;
	[SerializeField] private Image _midperson_right;
	[SerializeField] private Image _lockercrouchperson;
	[SerializeField] private Image _backperson_left;
	[SerializeField] private Image _backperson_right;
	
	[SerializeField] private Transform _scroll_anchor;
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	private List<Transform> _peoples = new List<Transform>();
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_backperson_left.transform, 1.05f);
		_scroll_registry.add_registry_entry(_backperson_right.transform, 1.05f);
		_scroll_registry.add_registry_entry(_lockercrouchperson.transform, 1.15f);
		_scroll_registry.add_registry_entry(_midperson_left.transform, 1.3f);
		_scroll_registry.add_registry_entry(_midperson_right.transform, 1.3f);
		_scroll_registry.add_registry_entry(_frontperson_left.transform, 1.45f);
		_scroll_registry.add_registry_entry(_frontperson_right.transform, 1.45f);
		
		this.register_person_behaviours(_backperson_left, SPUtil.sec_to_tick(0.25f), 1);
		this.register_person_behaviours(_backperson_right, SPUtil.sec_to_tick(0.25f), 1);
		this.register_person_behaviours(_midperson_left, SPUtil.sec_to_tick(0.25f), 2);
		this.register_person_behaviours(_midperson_right, SPUtil.sec_to_tick(0.25f), 2);
		this.register_person_behaviours(_frontperson_left, SPUtil.sec_to_tick(0.25f), 4);
		this.register_person_behaviours(_frontperson_right, SPUtil.sec_to_tick(0.25f), 4);
		this.register_person_behaviours(_lockercrouchperson, SPUtil.sec_to_tick(0.25f), 1);
	
	}
	
	private void register_person_behaviours(Image person, float bobtime, float bobampl) {
		_scroll_registry.add_registry_behaviour(person.transform, MovingCharacterRegistryBehaviour.cons(person.transform, person.transform.localPosition).set_bob_params(bobtime, bobampl));
		_scroll_registry.add_registry_behaviour(person.transform, HideShowImageRegistryBehaviour.cons(person));
		_peoples.Add(person.transform);
	}
	
	public override string get_registered_name() { return "bg_school_hallway"; }
	
	public override void show_background(string name, string key) {
		bool show_people = key.Contains("showpeople");
		for (int i = 0; i < _peoples.Count; i++) {
			_scroll_registry.get_registry_behaviour<HideShowImageRegistryBehaviour>(_peoples[i]).set_visible_imm(show_people);
		}
	
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {	
	}
	
	private int _front_bob_mode = 0, _front_bob_count = 0;
	private float _front_bob_delay = 0;
	private int _mid_bob_mode = 0, _mid_bob_count = 0;
	private float _mid_bob_delay = 0;
	private int _back_bob_mode = 0, _back_bob_count = 0;
	private float _back_bob_delay = 0;
	
	private static void bob_group(ref int bob_mode, ref int bob_count, ref float bob_delay, ParallaxScrollRegistry registry, Transform left, Transform right) {
		bool trigger_next_mode = false;
		if (bob_mode == 1) {
			MovingCharacterRegistryBehaviour left_move = registry.get_registry_behaviour<MovingCharacterRegistryBehaviour>(left);
			if (left_move._is_bobbing == false) {
				bob_count--;
				if (bob_count > 0) {
					left_move.do_bob();
				} else {
					trigger_next_mode = true;
				}
			}
			
		} else if (bob_mode == 2) {
			MovingCharacterRegistryBehaviour right_move = registry.get_registry_behaviour<MovingCharacterRegistryBehaviour>(right);
			if (right_move._is_bobbing == false) {
				bob_count--;
				if (bob_count > 0) {
					right_move.do_bob();
				} else {
					trigger_next_mode = true;
				}
			}
			
		} else {
			bob_delay -= SPUtil.dt_scale_get();
			if (bob_delay <= 0) {
				trigger_next_mode = true;
			}
		}
		
		if (trigger_next_mode) {
			bob_mode = SPUtil.int_random(0,3);
			if (bob_mode == 0) {
				bob_delay = SPUtil.float_random(20,200);
			} else {
				bob_count = SPUtil.int_random(1,6);
			}
		}
	}
	
	public override void i_update(GameMain game) {
		
		BGSchoolHallway.bob_group(ref _front_bob_mode, ref _front_bob_count, ref _front_bob_delay, _scroll_registry, _frontperson_left.transform, _frontperson_right.transform);
		BGSchoolHallway.bob_group(ref _mid_bob_mode, ref _mid_bob_count, ref _mid_bob_delay, _scroll_registry, _midperson_left.transform, _midperson_right.transform);
		BGSchoolHallway.bob_group(ref _back_bob_mode, ref _back_bob_count, ref _back_bob_delay, _scroll_registry, _backperson_left.transform, _backperson_right.transform);
		
		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries(game);
		
		this.update_showing_mode(_fade_cover);
	}
}
