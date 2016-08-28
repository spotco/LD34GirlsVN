using UnityEngine;
using System.Collections;

public class NodeScriptEvent_GridNavFocusAt : NodeScriptEvent {

	public const int FOCUS_ON_CURRENT_ID = -100;
	
	public int _focus_on_node_id;
	public Vector2 _focus_offset;
	public float _focus_zoom;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._grid_nav_modal._dialogue_manager.set_focus_on_node(_focus_on_node_id,_focus_offset,_focus_zoom);
		modal.advance_script();
	}
}
