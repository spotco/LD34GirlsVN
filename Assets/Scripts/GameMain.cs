using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameMain : MonoBehaviour {

	public static GameMain _context;
	public ObjectPool _objpool;
	public TextureResource _tex_resc;
	public FileCache _file_cache;
	public SPTextRenderManager _sptext;
	
	public static int AFFINITY_REQUIREMENT = 9;
	public static bool NO_EVENTS = false;
	public static bool DEBUG_CONTROLS = true;
	public static bool MUTE = true;
	public static bool IGNORE_ITEM_REQ = true;
	public static int NODE_START_INDEX = 0;
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

//  other chibis implementation
//	save/load implementation
//	end to title UIs
//
//  backup old script	
//	heart particles
//  closer petals blurred and bigger in title
//  title petals also fade in
//	title screen animation ins
//
//	simone/mana fight more monsters, get in fight (competition), extra node?
//	good end expand sacrifice of mana, extra node
//	item ui anim
//	credits
//		-simone boo-hoo tell you a secret
//	
//	SPAnalytics
//	
//	cache characters
//	sfx cache n pool text scroll sounds
//	sfx load all at start
	
	public void Start () {
		_self_rect = this.GetComponent<RectTransform>();
		_parent_canvas = this.GetComponentInParent<Canvas>();
	
		_context = this;
		_objpool = ObjectPool.cons();
		_tex_resc = TextureResource.cons();
		_file_cache = FileCache.cons();
		_sptext = SPTextRenderManager.cons();
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
		_sptext.i_update(this);
		
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
