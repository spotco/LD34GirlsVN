using UnityEngine;
using System.Collections.Generic;

public abstract class NodeScriptEvent {
	public virtual void i_initialize(GameMain game, EventModal modal) {}
	public virtual void i_update(GameMain game, EventModal modal) {}
}

public class NodeScript {
	
	public int _id = 0;
	public string _title = "";
	public string _background = "";
	public string _background_key = "";
	public string _music = "";
	public string _sfx = "";
	public List<string> _requirement_items = new List<string>();
	
	public List<NodeScriptEvent> _events = new List<NodeScriptEvent>();
	
	public List<NodeScriptEvent> _idle_events = new List<NodeScriptEvent>();
	public List<NodeScriptEvent> _post_show_events = new List<NodeScriptEvent>();
	
	public List<int> _links = new List<int>();
	public List<string> _previewchars = new List<string>();
	public bool _affinity_requirement;
	
	public NodeScript i_initialize(GameMain game, TextAsset text) {
		JSONObject root;
		try {
			root = JSONObject.Parse(text.text);
		} catch (System.Exception e) {
			Debug.LogError(SPUtil.sprintf("JSON FILE(%s) INVALID",text.name));
			return this;
		}
		_id = (int) root.GetNumber("id");
		_title = root.GetString("title");
		_background = root.GetString("background");
		_background_key = root.ContainsKey("backgroundkey") ? root.GetString("backgroundkey") : BGControllerBase.KEY_DEFAULT;
		_music = root.GetString("music");
		_affinity_requirement = root.ContainsKey("affinityrequirement");
		
		if (root.ContainsKey("previewchar")) {
			JSONArray previewchars = root.GetArray("previewchar");
			for (int i = 0; i < previewchars.Length; i++) {
				_previewchars.Add(previewchars[i].Str);
			}
		}
		
		if (root.ContainsKey("idleevents")) {
			NodeScript.load_nodescriptevents_from_json(game, root.GetArray("idleevents"),_idle_events);
		}
		
		if (root.ContainsKey("postshowevents")) {
			NodeScript.load_nodescriptevents_from_json(game, root.GetArray("postshowevents"),_post_show_events);
		}

		JSONArray requirement_item_json = root.GetArray("requirementitem");
		for (int i = 0; i < requirement_item_json.Length; i++) {
			_requirement_items.Add(requirement_item_json[i].Str);
		}
		
		JSONArray event_json = root.GetArray("event");
		NodeScript.load_nodescriptevents_from_json(game,event_json,_events);
		
		JSONArray links_json = root.GetArray("links");
		for (int i = 0; i < links_json.Length; i++) {
			_links.Add((int)links_json[i].Number);
		}
		
		return this;
	}
	
	public static void load_nodescriptevents_from_json(GameMain game, JSONArray event_json, List<NodeScriptEvent> nodescript_events) {
		for (int i = 0; i < event_json.Length; i++) {
			JSONObject itr = event_json[i].Obj;
			string type = itr.GetString("type");
			NodeScriptEvent itr_neu = null;
			if (type == "showcharacter") {
				itr_neu = new NodeScriptEvent_ShowCharacter () {
					_character = itr.GetString ("character"),
					_image = itr.GetString ("image"),
					_xpos = (float)itr.GetNumber ("xpos"),
					_ypos = itr.ContainsKey("ypos") ? ((float)itr.GetNumber("ypos")) : 0,
					_xscale = (float)itr.GetNumber ("xscale"),
					_imm = itr.ContainsKey("imm")
				};
				
			} else if (type == "changebackground") {
				itr_neu = new NodeScriptEvent_ChangeBackground () {
					_background = itr.GetString ("background"),
					_key = itr.ContainsKey("key") ? itr.GetString("key") : BGControllerBase.KEY_DEFAULT
				};
				
			} else if (type == "updatebackground") {
				itr_neu = new NodeScriptEvent_UpdateBackground() {
					_strparam = itr.GetString("strparam"),
					_numparam1 = itr.ContainsKey("numparam1") ? ((float)itr.GetNumber("numparam1")) : 0,
					_numparam2 = itr.ContainsKey("numparam2") ? ((float)itr.GetNumber("numparam2")) : 0
				};
				
			} else if (type == "dialogue") {
				itr_neu = new NodeScriptEvent_Dialogue () {
					_character = itr.ContainsKey ("character") ? itr.GetString ("character") : NodeScriptEvent_Dialogue.CHARACTER_NARRATOR,
					_text = itr.GetString ("text"),
					_xpos = itr.ContainsKey("xpos") ? ((float)itr.GetNumber ("xpos")) : 0,
					_ypos = itr.ContainsKey("ypos") ? ((float)itr.GetNumber ("ypos")) : -130
				};
				
			} else if (type == "transitioncharacter") {
				itr_neu = new NodeScriptEvent_TransitionCharacter () {
					_character = itr.GetString ("character"),
					_image = itr.GetString ("image"),
					_xscale =  itr.ContainsKey("xscale") ? (float)itr.GetNumber ("xscale") : 1
				};
				
			} else if (type == "movecharacter") {
				itr_neu = new NodeScriptEvent_MoveCharacter () {
					_character = itr.GetString ("character"),
					_xto = (float)itr.GetNumber ("xto")
				};
				
			} else if (type == "additem") {
				itr_neu = new NodeScriptEvent_AddItem () {
					_item = itr.GetString ("item")
				};
				
			} else if (type == "removeitem") {
				itr_neu = new NodeScriptEvent_RemoveItem () {
					_item = itr.GetString ("item")
				};
				
			} else if (type == "hidecharacter") {
				itr_neu = new NodeScriptEvent_HideCharacter () {
					_character = itr.GetString ("character"),
					_imm = itr.ContainsKey("imm")
				};
				
			} else if (type == "charactereffect") {
				itr_neu = new NodeScriptEvent_CharacterEffect () {
					_character = itr.GetString("character"),
					_effect = itr.GetString("effect")
				};
				
			} else if (type == "addaffinity") {
				itr_neu = new NodeScriptEvent_AddAffinity ();
				
			} else if (type == "rename") {
				itr_neu = new NodeScriptEvent_Rename() {
					_name_start = itr.GetString("namestart"),
					_name_end = itr.GetString("nameend")
				};
				
			} else if (type == "playSFX") {
				string sfx_name = itr.GetString("sfx");
				itr_neu = new NodeScriptEvent_PlaySFX () {
					_sfx = sfx_name	
				};
				game._music.cond_load_sound_of_name(sfx_name);
				
			} else if (type == "playBGM") {
				string bgm_name = itr.GetString("bgm");
				itr_neu = new NodeScriptEvent_PlayBGM() {
					_bgm = bgm_name
				};
				game._music.cond_load_sound_of_name(bgm_name);
				
			} else if (type == "titleend") {
				itr_neu = new NodeScriptEvent_TitleEnd();
				
			} else if (type == "camerashake") {
				itr_neu = new NodeScriptEvent_CameraShake() {
					_long = itr.GetString("length") == "long"
				};
				
			} else if (type == "gridnavfocusat") {
				itr_neu = new NodeScriptEvent_GridNavFocusAt() {
					_focus_on_node_id = itr.ContainsKey("nodeid") ? ((int)itr.GetNumber("nodeid")) : NodeScriptEvent_GridNavFocusAt.FOCUS_ON_CURRENT_ID,
					_focus_zoom = itr.ContainsKey("zoom") ? ((float)itr.GetNumber("zoom")) : 1.0f,
					_focus_offset = new Vector2(
						itr.ContainsKey("offsetx") ? ((float)itr.GetNumber("offsetx")) : 0,
						itr.ContainsKey("offsety") ? ((float)itr.GetNumber("offsety")) : 0
					)
				};
				
			} else if (type == "gridnavhidechars") {
				itr_neu = new NodeScriptEvent_GridNavHideCharacters() {
					_node_id = itr.ContainsKey("nodeid") ? ((int)itr.GetNumber("nodeid")) : NodeScriptEvent_GridNavHideCharacters.HIDE_CURRENT_ID
				};
				
			} else if (type == "gridnavshowchar") {
				itr_neu = new NodeScriptEvent_GridNavShowCharacter() {
					_name = itr.GetString("name"),
					_node_id = itr.ContainsKey("nodeid") ? ((int)itr.GetNumber("nodeid")) : NodeScriptEvent_GridNavShowCharacter.SHOW_CURRENT_ID
				};
				
			} else if (type == "wait") {
				itr_neu = new NodeScriptEvent_Wait() {
					_wait_time = ((float)itr.GetNumber("waittime"))
				};
				
			} else {
				SPUtil.logf("unknown type %s",type);
			} 
			
			if (itr_neu != null) {
				nodescript_events.Add(itr_neu);
			}
		}
	}
	
}

public class NodeScriptEvent_TitleEnd : NodeScriptEvent {
	
	private bool _triggered = false;
	
	public override void i_update(GameMain game, EventModal modal) {
		if (!_triggered) {
			_triggered = true;
			game._title.set_text("The End.");
			game._title._current_mode = TitleModal.Mode.FadeIn;
			game._title.set_end_screen();
		}		
	}
}

