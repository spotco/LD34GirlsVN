using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameMain : MonoBehaviour {

	public static GameMain _context;
	public ObjectPool _objpool;
	public TextureResource _tex_resc;
	public FileCache _file_cache;
	
	public static int AFFINITY_REQUIREMENT = 9;
	public static bool NO_EVENTS = false;
	public static bool DEBUG_CONTROLS = true;
	public static bool MUTE = true;
	public static bool IGNORE_ITEM_REQ = true;
	public static int NODE_START_INDEX = 10;
	public static bool SKIP_TITLE = true;
	
	public interface Modal {
		void i_initialize(GameMain game);
		void i_update(GameMain game);
		void anim_update(GameMain game);
	}

	[SerializeField] public EventModal _event_modal;
	[SerializeField] public GridNavModal _grid_nav_modal;
	[SerializeField] public BackgroundManager _background;
	[SerializeField] public TitleModal _title;
	[SerializeField] public MusicManager _music;
	[SerializeField] public PopupManager _popups;
	
	public ControlManager _controls;
	public Modal _active_modal;
	public Inventory _inventory;
	public GameCameraController _camera_controller;
	
	private List<Modal> _all_modals;
	
	public int _affinity;
	
	private RectTransform _self_rect;
	[System.NonSerialized] public Canvas _parent_canvas;

/*
DOC:
https://docs.google.com/document/d/1D3zT3HPpfU57bqfPFjvHfG2FL9Txj9Ot2xqI0Ibu9oQ/edit

adjust mana school ch1 leave class dialogue
adjust naoko school ch1 roof dialgue

IDEAS:
ch1 - fix node11 going to roof reasoning
ch1 - make a government officer character during evacuation
ch1 - improve return to school, involve MANA


ASSETS:
taxi on ride to new apartment
school outside, courtyard, hallway, classroom, roof dark
subway ride
city day
timed node nodes (mechanic)

BUG:
node 1 - map mode, take key for preview bg
node 16 to 17 (leaving), can go back to node 15

TECH:
selecting node, fade out node text. bottom right show subtitle of scene (subtitle with black faded box top left)
char name text replace with SPText
cache characters
sfx cache n pool text scroll sounds

CONCEPT:
YUUTO KATSURAGI
SIMONE DE LA VILLENEUVE
*/
	
	public void Start () {
		_self_rect = this.GetComponent<RectTransform>();
		_parent_canvas = this.GetComponentInParent<Canvas>();
		
		this.GetComponent<Mask>().enabled = true;
	
		_context = this;
		_objpool = ObjectPool.cons();
		_tex_resc = TextureResource.cons();
		_file_cache = FileCache.cons();
		_camera_controller = GameCameraController.cons();
		
		if (_event_modal == null) return;
	
		Application.targetFrameRate = 30;
		_all_modals = new List<Modal>() { _event_modal, _grid_nav_modal, _title };
		_controls = ControlManager.cons();
		_inventory = new Inventory();
		_popups.i_initialize(this);
		_background.i_initialize(this);
		
		_affinity = 0;
		
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].i_initialize(this);
		}
		
		if (GameMain.SKIP_TITLE == false)
		{
			_active_modal = _title;
			_title._current_mode = TitleModal.Mode.FadeIn;
		}
		else
		{
			_active_modal = _grid_nav_modal;
		}
	}
	
	public void Update () {
		if (_event_modal == null) return;
		
		if ((_self_rect.rect.height * _parent_canvas.scaleFactor) > Screen.height) {
			this.transform.localScale = SPUtil.valv((Screen.height) / (_self_rect.rect.height * _parent_canvas.scaleFactor));
		} else {
			this.transform.localScale = SPUtil.valv(1);
		}
		
		_controls.i_update();
		_active_modal.i_update(this);
		_popups.i_update(this);
		
		for (int i = 0; i < _all_modals.Count; i++) {
			_all_modals[i].anim_update(this);
		}
		
		_background.i_update(this);
		_music.i_update ();
		
		_camera_controller.i_update(this);
	}
	
	public void start_event_modal(NodeScript script) {
		_event_modal.load_script(this,script);
		_active_modal = _event_modal;
	}
	
	public void finish_event_modal() {
		_active_modal = _grid_nav_modal;
		_grid_nav_modal.return_from_event_modal(this);
	}
	
}
