using UnityEngine;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_anchor;
	private Dictionary<int,GridNode> _id_to_gridnode = new Dictionary<int, GridNode>();
	
	private GridNode _current_node;
	private int _selected_node_cursor_index;
	
	private Vector3 _target_grid_map_anchor_position;
			
	public void i_initialize(GameMain game) {
		this.set_active(false);
		
		for (int i = 0; i < _grid_map_anchor.childCount; i++) {
			GameObject itr = _grid_map_anchor.GetChild(i).gameObject;
			if (itr.GetComponent<GridNode>() != null) {
				GridNode itr_gridnode = itr.GetComponent<GridNode>();
				itr_gridnode.i_initialize();
				if (!_id_to_gridnode.ContainsKey(itr_gridnode._node_script._id)) {
					_id_to_gridnode[itr_gridnode._node_script._id] = itr_gridnode;
				} else {
					SPUtil.logf("Duplicate gridnode id(%d) on gameobject(%s)",itr_gridnode._node_script._id,itr.name);
				}
				
			} else {
				SPUtil.logf("GridMapAnchor Child(%s) no GridNode component",itr.name);
			}
		}
		
		this.set_current_node(_id_to_gridnode[1]);
	}
	
	public void i_update(GameMain game) {
		List<GridNode> selection_list = this.get_selection_list();
		if (game._controls.get_control_just_released(ControlManager.Control.ButtonB)) {
			_selected_node_cursor_index = (_selected_node_cursor_index + 1) % selection_list.Count;
		}
		
		this.set_focus_node(selection_list[_selected_node_cursor_index]);
		
		_grid_map_anchor.transform.localPosition = new Vector3(
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.x,_target_grid_map_anchor_position.x,1/10.0f),
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.y,_target_grid_map_anchor_position.y,1/10.0f)
		);
		
	}
	
	private List<GridNode> __cached_selection_list = new List<GridNode>();
	private List<GridNode> get_selection_list() {
		__cached_selection_list.Clear();
		__cached_selection_list.Add(_current_node);
		for (int i = 0; i < _current_node._node_script._links.Count; i++) {
			__cached_selection_list.Add(_id_to_gridnode[_current_node._node_script._links[i]]);
		}
		return __cached_selection_list;
	}
	
	private void set_focus_node(GridNode tar_node) {
		_target_grid_map_anchor_position = new Vector3(
			-tar_node.transform.localPosition.x,
			-tar_node.transform.localPosition.y
		);
	}
	
	private void set_current_node(GridNode tar_node) {
		_current_node = tar_node;
		_selected_node_cursor_index = 0;
	}
	
	public void set_active(bool val) {
		this.gameObject.SetActive(val);
	}
	
}
