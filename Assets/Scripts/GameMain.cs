using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameMain : MonoBehaviour {

	public interface Modal {
		void i_initialize(GameMain game);
		void i_update(GameMain game);
		void anim_update(GameMain game);
	}

	[SerializeField] private EventModal _event_modal;
	[SerializeField] private GridNavModal _grid_nav_modal;
	[SerializeField] private Image _background_image;
	
	public ControlManager _controls;
	public Modal _active_modal;
	private List<Modal> _all_modals;
	
	/*
	TODO:
	movecharacter action
	
	no dialogue 
	
	dialogue window styles
	
	inventory ui
	additem
	removeitem
	locked nodes system
	
	hidecharacter action
	
	affinity system + ui
	add affinity
	
	background transitions
	
	(final grid design)
	(final art)
	(menu transitions)
	*/
	
	public void Start () {
		_all_modals = new List<Modal>() { _event_modal, _grid_nav_modal };
		_controls = ControlManager.cons();
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].i_initialize(this);
		}
		
		_active_modal = _grid_nav_modal;
	}
	
	public void Update () {
		_controls.i_update();
		_active_modal.i_update(this);
		
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].anim_update(this);
		}
	}
	
	public void start_event_modal(NodeScript script) {
		_event_modal.load_script(this,script);
		_active_modal = _event_modal;
	}
	
	public void finish_event_modal() {
		_active_modal = _grid_nav_modal;
	}
	
}
