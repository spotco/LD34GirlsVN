using UnityEngine;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_anchor;
	[SerializeField] public Transform _line_root;
	[SerializeField] private Transform _selector;
	[SerializeField] private CanvasGroup _canvas_group;
	
	private float _selector_anim_t;
	[System.NonSerialized] public Dictionary<int,GridNode> _id_to_gridnode = new Dictionary<int, GridNode>();
	
	[System.NonSerialized] public GridNode _current_node;
	[System.NonSerialized] public int _selected_node_cursor_index;
	
	private Vector3 _target_grid_map_anchor_position;
			
	public void i_initialize(GameMain game) {
		this.gameObject.SetActive(true);
		_canvas_group.alpha = 0;
		
		for (int i = 0; i < _grid_map_anchor.childCount; i++) {
			GameObject itr = _grid_map_anchor.GetChild(i).gameObject;
			if (itr.GetComponent<GridNode>() != null) {
				GridNode itr_gridnode = itr.GetComponent<GridNode>();
				itr_gridnode.i_initialize();
				if (!_id_to_gridnode.ContainsKey(itr_gridnode._node_script._id)) {
					_id_to_gridnode[itr_gridnode._node_script._id] = itr_gridnode;
				} else {
					//SPUtil.logf("Duplicate gridnode id(%d) on gameobject(%s)",itr_gridnode._node_script._id,itr.name);
				}
				
			} else {
				//SPUtil.logf("GridMapAnchor Child(%s) no GridNode component",itr.name);
			}
		}
		
		foreach (int id in _id_to_gridnode.Keys) {
			_id_to_gridnode[id].post_initialize(this);
		}
		
		this.set_current_node(_id_to_gridnode[1]);
	}
	
	public void anim_update(GameMain game) {
		if (game._active_modal == this) {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,1,1/10.0f);
		} else {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,0,1/10.0f);
		}
	}
	
	public void i_update(GameMain game) {
		List<GridNode> selection_list = this.get_selection_list();
		if (game._controls.get_control_just_released(ControlManager.Control.MoveUp)) {
			int tar = GridNavModal.cond_select_with_filter(selection_list[_selected_node_cursor_index],selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.y > cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,1)) : -1;
			});
			_selected_node_cursor_index = tar >= 0 ? tar : _selected_node_cursor_index;
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveDown)) {
			int tar = GridNavModal.cond_select_with_filter(selection_list[_selected_node_cursor_index],selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.y < cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,-1)) : -1;
			});
			_selected_node_cursor_index = tar >= 0 ? tar : _selected_node_cursor_index;
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveLeft)) {
			int tar = GridNavModal.cond_select_with_filter(selection_list[_selected_node_cursor_index],selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.x < cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(-1,0)) : -1;
			});
			_selected_node_cursor_index = tar >= 0 ? tar : _selected_node_cursor_index;
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveRight)) {
			int tar = GridNavModal.cond_select_with_filter(selection_list[_selected_node_cursor_index],selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.x > cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(1,0)) : -1;
			});
			_selected_node_cursor_index = tar >= 0 ? tar : _selected_node_cursor_index;
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.ButtonB)) {
			_selected_node_cursor_index = (_selected_node_cursor_index + 1) % selection_list.Count;
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
			this.set_current_node(selection_list[_selected_node_cursor_index]);
		}
		
		foreach (int id in _id_to_gridnode.Keys) {
			_id_to_gridnode[id].i_update(game,this);
		}
		
		this.set_focus_node(selection_list[_selected_node_cursor_index]);
		
		_grid_map_anchor.transform.localPosition = new Vector3(
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.x,_target_grid_map_anchor_position.x,1/10.0f),
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.y,_target_grid_map_anchor_position.y,1/10.0f)
		);
		_selector.localPosition = new Vector3(
			_current_node.transform.localPosition.x,
			_current_node.transform.localPosition.y + 60 + Mathf.Sin(_selector_anim_t) * 10.0f
		);
		_selector_anim_t += 0.15f;
	}
	
	private static int cond_select_with_filter(GridNode current, List<GridNode> list, System.Func<GridNode,GridNode,float> filter) {
		float min_dist = float.MaxValue;
		int rtv = -1;
		for (int i = 0; i < list.Count; i++) {
			float val = filter(current,list[i]);
			if (val > 0 && val < min_dist) {
				min_dist = val;
				rtv = i;
			}
		}
		return rtv;
	}
	
	private List<GridNode> __cached_selection_list = new List<GridNode>();
	public List<GridNode> get_selection_list() {
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
	
}
