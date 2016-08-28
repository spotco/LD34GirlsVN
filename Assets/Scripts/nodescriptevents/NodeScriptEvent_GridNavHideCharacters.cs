using UnityEngine;
using System.Collections;

public class NodeScriptEvent_GridNavHideCharacters : NodeScriptEvent {

	public const int HIDE_CURRENT_ID = -100;
	public int _node_id;
	
	public override void i_update(GameMain game, EventModal modal) {
		int tar_node_id = _node_id == HIDE_CURRENT_ID ? game._grid_nav_modal._current_node._node_script._id : _node_id;
		if (game._grid_nav_modal._id_to_gridnode.ContainsKey(tar_node_id)) {
			GridNode node = game._grid_nav_modal._id_to_gridnode[tar_node_id];
			node._event_preview_chars.Clear();
		}
		modal.advance_script();
	}
}
