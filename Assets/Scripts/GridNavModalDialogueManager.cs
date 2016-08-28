using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavModalDialogueManager {
	
	private bool _dialogue_active;
	
	bool _has_focus_on_node;
	private int _focus_on_node_id;
	private Vector2 _focus_on_node_offset;
	private float _focus_on_node_zoom;
	
	public void i_initialize(GameMain game, GridNavModal gridnav) {
		_dialogue_active = false;
		_has_focus_on_node = false;
	}
	
	public void i_update(GameMain game, GridNavModal gridnav) {
		game._event_modal.i_update(game);
	}
	
	public bool is_finished(GameMain game) {
		return _dialogue_active == false && game._event_modal.fadeout_cleanup_finished();
	}
	
	public void set_focus_on_node(int id, Vector2 offset, float zoom) {
		_has_focus_on_node = true;
		_focus_on_node_id = id;
		_focus_on_node_offset = offset;
		_focus_on_node_zoom = zoom;
	}
	
	public void get_anchor_position_and_zoom(GameMain game, GridNavModal gridnav, ref Vector2 current_anchor, ref float current_zoom) {
		if (_has_focus_on_node == false) return;
		
		GridNode tar_node = null;
		if (_focus_on_node_id == NodeScriptEvent_GridNavFocusAt.FOCUS_ON_CURRENT_ID) {
			tar_node = gridnav._current_node;
		} else {
			if (!gridnav._id_to_gridnode.ContainsKey(_focus_on_node_id)) {
				SPUtil.errf("GridNavFocusAt has no node of id (%d)",_focus_on_node_id);
				return;
			}
			tar_node = gridnav._id_to_gridnode[_focus_on_node_id];
		}
		
		Vector2 tar_anchor = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(tar_node.get_center_position() + _focus_on_node_offset);
		
		current_anchor.x = SPUtil.drpt(current_anchor.x,tar_anchor.x,1/10.0f);
		current_anchor.y = SPUtil.drpt(current_anchor.y,tar_anchor.y,1/10.0f);
		current_zoom = SPUtil.drpt(current_zoom,_focus_on_node_zoom,1/10.0f);
	}
	
	private NodeScript _load_dialogue_script = new NodeScript();
	public void load_dialogue(GameMain game, GridNavModal gridnav, List<NodeScriptEvent> events) {
		_has_focus_on_node = false;
		_load_dialogue_script._events.Clear();
		for (int i = 0; i < events.Count; i++) {
			_load_dialogue_script._events.Add(events[i]);
		}
		_dialogue_active = true;
		game._event_modal._gridnav_dialogue_mode = true;
		game._event_modal.load_script(game,_load_dialogue_script);
	}
	
	public void finish_dialogue(GameMain game) {
		_dialogue_active = false;
	}

}
