using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private EventCharacter _proto_character;
	[SerializeField] private DialogueBubble _proto_dialogue_bubble;
	private Dictionary<string,EventCharacter> _name_to_character = new Dictionary<string, EventCharacter>();
	private List<DialogueBubble> _dialogue_bubbles = new List<DialogueBubble>();
	
	public void i_initialize(GameMain game) {
		this.gameObject.SetActive(true);
		_proto_dialogue_bubble.gameObject.SetActive(false);
		_proto_character.gameObject.SetActive(false);
		_canvas_group.alpha = 0;
	}
	
	public void anim_update(GameMain game) {
		if (game._active_modal == this) {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,1,1/10.0f);
		} else {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,0,1/10.0f);
		}
	}
	
	private int _script_index;
	private NodeScript _current_script;
	
	public void load_script(GameMain game, NodeScript script) {
		for (int i = _dialogue_bubbles.Count-1; i >= 0; i--) {
			DialogueBubble itr = _dialogue_bubbles[i];
			itr.cleanup(game);	
		}
		_dialogue_bubbles.Clear();
		
		_script_index = 0;
		_current_script = script;
		for (int i = 0; i < _current_script._events.Count; i++) {
			_current_script._events[i].i_initialize(game,this);
		}
		game._background.load_background(script._background, script._background_key);
		game._music.load_music (script._music);
		this.i_update(game);
	}
	
	private List<string> __to_remove_str = new List<string>();
	public void i_update(GameMain game) {
		if (game._popups.has_active_popup()) return;
	
		if (_current_script != null && _script_index <  _current_script._events.Count) {
			int last_index = _script_index;
			do {
				last_index = _script_index;
				_current_script._events[_script_index].i_update(game,this);
			} while (last_index != _script_index && _script_index <  _current_script._events.Count);
						
		} else {
			this.end_script_playback_and_close_modal(game);
		}
		
		if (game._controls.get_debug_skip()) {
			this.end_script_playback_and_close_modal(game);
		}
		
		__to_remove_str.Clear();
		foreach (string name in _name_to_character.Keys) {
			EventCharacter itr_char = _name_to_character[name];
			itr_char.i_update(game,this);
			if (itr_char._current_mode == EventCharacter.Mode.DoRemove) {
				// TODO -- pool
				itr_char.remove_all_effects(game,this);
				GameObject.Destroy(itr_char.gameObject);
				__to_remove_str.Add(name);
			}
		}
		for (int i = 0; i < __to_remove_str.Count; i++) {
			_name_to_character.Remove(__to_remove_str[i]);
		}
		__to_remove_str.Clear();
		
		for (int i = _dialogue_bubbles.Count-1; i >= 0; i--) {
			DialogueBubble itr = _dialogue_bubbles[i];
			itr.i_update(game,this);
			if (itr._current_mode == DialogueBubble.Mode.DoRemove) {
				itr.cleanup(game);
				_dialogue_bubbles.RemoveAt(i);
			} else if (itr._current_mode == DialogueBubble.Mode.TextIn) {
				if (_name_to_character.ContainsKey(itr._script._character)) {
					_name_to_character[itr._script._character].notify_talking();
				}
			}
		}
	}
	
	public void clear_removed_characters(GameMain game) {
		__to_remove_str.Clear();
		foreach (string name in _name_to_character.Keys) {
			EventCharacter itr_char = _name_to_character[name];
			if (itr_char._current_mode == EventCharacter.Mode.DoRemove) {
				GameObject.Destroy(itr_char.gameObject);
				__to_remove_str.Add(name);
			}
		}
		for (int i = 0; i < __to_remove_str.Count; i++) {
			_name_to_character.Remove(__to_remove_str[i]);
		}
		__to_remove_str.Clear();
	}
	
	private void end_script_playback_and_close_modal(GameMain game) {
		
		game._grid_nav_modal.update_accessible();
	
		foreach (string name in _name_to_character.Keys) {
			EventCharacter itr_char = _name_to_character[name];
			itr_char._current_mode = EventCharacter.Mode.DoRemove;
		}
		for (int i = _dialogue_bubbles.Count-1; i >= 0; i--) {
			DialogueBubble itr = _dialogue_bubbles[i];
			itr.cleanup(game);	
		}
		_dialogue_bubbles.Clear();
		game.finish_event_modal();
	}
	
	public void advance_script() {
		_script_index++;
	}
	public EventCharacter add_character(GameMain game, string name, string path) {
		Sprite char_sprite = Resources.Load<Sprite>("img/character/"+path);
		if (char_sprite == null) {
			SPUtil.logf("no character found of path(%s)",path);
			return null;
		}
		EventCharacter neu_char = SPUtil.proto_clone(_proto_character.gameObject).GetComponent<EventCharacter>();
		// TODO -- pool
		
		BGControllerBase active_bg_controller = game._background.get_latest_active_bgcontroller();
		if (active_bg_controller != null && active_bg_controller.get_character_root() != null) {
			neu_char.transform.parent = active_bg_controller.get_character_root();
		}
		
		neu_char.i_initialize();
		neu_char.set_image(char_sprite,path);
		
		if (_name_to_character.ContainsKey(name)) {
			_name_to_character[name].imm_hide();
			this.clear_removed_characters(game);
		}
		_name_to_character[name] = neu_char;
		return neu_char;
	}
	public DialogueBubble add_dialogue(GameMain game, NodeScriptEvent_Dialogue script_event) {
		DialogueBubble neu_bubble = DialogueBubble.cons(game, script_event, _proto_dialogue_bubble);
		_dialogue_bubbles.Add(neu_bubble);
		return neu_bubble;
	}
	public EventCharacter cond_get_character_of_name(string name) {
		EventCharacter rtv = null;
		_name_to_character.TryGetValue(name,out rtv);
		return rtv;
	}
	public void rename(string name_start, string name_end) {
		if (_name_to_character.ContainsKey(name_start)) {
			_name_to_character[name_end] = _name_to_character[name_start];
			_name_to_character.Remove(name_start);
		}
	}
	
	
}
