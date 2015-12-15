using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameMain : MonoBehaviour {
	
	public static int AFFINITY_REQUIREMENT = 5;
	public static bool NO_EVENTS = false;
	public static bool DEBUG_CONTROLS = true;
	public static bool MUTE = true;
	public static bool IGNORE_ITEM_REQ = false;
	public static int NODE_START_INDEX = 35;
	
	public interface Modal {
		void i_initialize(GameMain game);
		void i_update(GameMain game);
		void anim_update(GameMain game);
	}

	[SerializeField] private EventModal _event_modal;
	[SerializeField] private GridNavModal _grid_nav_modal;
	[SerializeField] public BackgroundManager _background;
	[SerializeField] public MusicManager _music;
	[SerializeField] public PopupManager _popups;
	
	public ControlManager _controls;
	public Modal _active_modal;
	public Inventory _inventory;
	
	private List<Modal> _all_modals;
	
	public int _affinity;
	
	/*
	TODO:
	do not show unaccessible nodes
	consistent raichi senpai
	
	TODO
	35,37,38,40,41
	
	43,44,45,47
	48,49,50
	51,52,53
	
	(final art)
	(start menu, end menu)
	*/
	
	public void Start () {
		Application.targetFrameRate = 30;
		_all_modals = new List<Modal>() { _event_modal, _grid_nav_modal };
		_controls = ControlManager.cons();
		_inventory = new Inventory();
		_popups.i_initialize(this);
		
		_affinity = 0;
		
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].i_initialize(this);
		}
		
		_active_modal = _grid_nav_modal;
	}
	
	public void Update () {
		_controls.i_update();
		_active_modal.i_update(this);
		_popups.i_update(this);
		
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].anim_update(this);
		}
		
		_background.i_update();
		_music.i_update ();
	}
	
	public void start_event_modal(NodeScript script) {
		_event_modal.load_script(this,script);
		_active_modal = _event_modal;
	}
	
	public void finish_event_modal() {
		_active_modal = _grid_nav_modal;
	}
}
