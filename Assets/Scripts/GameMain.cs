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
	public static bool MUTE = false;
	public static bool IGNORE_ITEM_REQ = true;
	public static int NODE_START_INDEX = 5;
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

// WRITING:
/*
{"type":"dialogue","character":"Kurumi","text":"[b4]#Stay safe. Don't wander alone at night. Dad.#@"},
{"type":"showcharacter","character":"Kurumi","image":"char_kurumi_normal_puzzled","xpos":-300,"xscale":1},
{"type":"dialogue","character":"Kurumi","text":"..."},

I take a moment and think of how best to respond.

---

This is a turning point, my first choice.
Whatever's decided here could affect many things further on.
Here, I've got two options.

Should I respond immediately..?
Or... Maybe I should wait for tomorrow.

---

I'll admit that this move has been tough.
Everything happened so suddenly, and I'd be lying if I said I wasn't upset.
I still think it's odd that my parents had to stay behind.
But... Whatever it is they're doing, I'm sure they're thinking of me.
It's a reassuring thought.
I type up a quick response and send it back.
I then turn off my phone and put it in my bag as I head to my room.

A bright, beaming smile bursts across my face.
I'm... excited!

---

I'd be lying if I said I wasn't upset.
I'm hundreds of miles away from home, all alone in an unfamiliar city. It all happened so suddenly.
I hardly even had time to say goodbye to all my friends.
So, I should probably wait until I calm down a bit before responding.
If I sounded too emotional, that'd probably just make my parents concerned.
I then turn off my phone and put it in my bag as I head to my room.

I felt a bit nervous. How is it all going to go tomorrow?
I took a deep breath. That calmed me down a bit.
Well, no help in worrying about it all night!

---

{"type":"dialogue","text":"I type up a quick response and send it back.","xpos":0,"ypos":0},
{"type":"dialogue","text":"I then turn off my phone and put it on my nightstand.","xpos":0,"ypos":0},
{"type":"hidecharacter","character":"Kurumi"},

{"type":"changebackground","background":"bg_black"},
{"type":"dialogue","text":"Ever since I was little, I wondered what it would be like to move to a big city.","xpos":0,"ypos":0},
{"type":"dialogue","text":"In our small town, nothing ever changed. I was with the same people, seeing the same places every day.","xpos":0,"ypos":0},
{"type":"dialogue","text":"But now... %This is a new opportunity.","xpos":0,"ypos":0},
{"type":"dialogue","text":"In this city... Everything is different.","xpos":0,"ypos":0},
{"type":"dialogue","text":"A bright, beaming smile bursts across my face.","xpos":0,"ypos":0},

{"type":"dialogue","text":"I'm... excited!","xpos":0,"ypos":0}
*/

// procedurally generate node positions
// selecting node, fade out node text. bottom right show subtitle of scene
// node8 will not load bug when go bottom route
// debug skip gridnav dialogue bug
// text quality not dependant on window size bugfix (move SPText to RawImage with adapter
						
//	save/load implementation
//	end to title UIs
//  smart travel for clicks
//
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
