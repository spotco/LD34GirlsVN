using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_anchor;
	[SerializeField] public Transform _line_root;
	[SerializeField] private Transform _selector;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private InventoryOverlay _inventory_overlay;
	
	[SerializeField] private Image _outline_back;
	[SerializeField] private Image _outline_front;
	
	[SerializeField] private NodeAnimRoot _anim_root_proto;
	[SerializeField] private LineProtoRoot _line_root_proto;
	
	[System.NonSerialized] public HashSet<int> _visited_node_ids = new HashSet<int>();
	
	private float _outline_back_anim_t = 0, _outline_front_anim_t = 0;
	
	private float _selector_anim_t;
	[System.NonSerialized] public Dictionary<int,GridNode> _id_to_gridnode = new Dictionary<int, GridNode>();
	
	[System.NonSerialized] public GridNode _current_node;
	[System.NonSerialized] public GridNode _selected_node;
	
	private Vector3 _target_grid_map_anchor_position;
	public float _touch_trigger_delay = 0;
			
	public void i_initialize(GameMain game) {
		_anim_root_proto.gameObject.SetActive(false);
		_line_root_proto.gameObject.SetActive(false);
		
		this.gameObject.SetActive(true);
		_canvas_group.alpha = 0;
		
		_inventory_overlay.i_initialize();
		
		for (int i = 0; i < _grid_map_anchor.childCount; i++) {
			GameObject itr = _grid_map_anchor.GetChild(i).gameObject;
			if (itr.GetComponent<GridNode>() != null) {
				GridNode itr_gridnode = itr.GetComponent<GridNode>();
				itr_gridnode.i_initialize(game,this,_anim_root_proto, _line_root_proto);
				if (!_id_to_gridnode.ContainsKey(itr_gridnode._node_script._id)) {
					_id_to_gridnode[itr_gridnode._node_script._id] = itr_gridnode;
				}
			}
		}
		
		foreach (int id in _id_to_gridnode.Keys) {
			_id_to_gridnode[id].set_unidirectional_reverse_links(this);
		}
		
		foreach (int id in _id_to_gridnode.Keys) {
			_id_to_gridnode[id].create_link_sprites(this);
		}
		
		this.set_current_node(_id_to_gridnode[GameMain.NODE_START_INDEX]);
		this.update_accessible();
	}
	
	private Queue<int> __update_accessible_to_search = new Queue<int>();
	private HashSet<int> __update_accessible_searched = new HashSet<int>();
	public void update_accessible() {
		foreach (int id in _id_to_gridnode.Keys) {
			GridNode itr_node = _id_to_gridnode[id];
			itr_node._accessible = false;
			
			itr_node.recalculate_focus_point(this);
		}
		__update_accessible_to_search.Clear();
		__update_accessible_searched.Clear();
		__update_accessible_to_search.Enqueue(_current_node._node_script._id);
		__update_accessible_searched.Add(_current_node._node_script._id);
		
		while (__update_accessible_to_search.Count > 0) {
			GridNode itr_node = _id_to_gridnode[__update_accessible_to_search.Dequeue()];
			itr_node._accessible = true;
			if (itr_node._visited) {
				for (int i = 0; i < itr_node._node_script._links.Count; i++) {
					int itr_link_id = itr_node._node_script._links[i];
					if (!__update_accessible_searched.Contains(itr_link_id)) {
						__update_accessible_to_search.Enqueue(itr_link_id);
						__update_accessible_searched.Add(itr_link_id);
					}
				}
			}
		}
	}
	
	public void anim_update(GameMain game) {
		if (game._active_modal == this) {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,1,1/10.0f);
			
			_outline_back_anim_t = SPUtil.drpt(_outline_back_anim_t, 1, 1/10.0f);
			_outline_front_anim_t = SPUtil.drpt(_outline_front_anim_t, 1, 1/20.0f);
			
		} else {
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,0,1/10.0f);
			
			_outline_back_anim_t = SPUtil.drpt(_outline_back_anim_t, 0, 1/10.0f);
			_outline_front_anim_t = SPUtil.drpt(_outline_front_anim_t, 0, 1/20.0f);
		}
		
		_outline_back.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,1), _outline_back_anim_t);
		_outline_back.rectTransform.localScale = Vector3.Lerp(SPUtil.valv(1.25f), SPUtil.valv(1), _outline_back_anim_t);
		
		_outline_front.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,1), _outline_front_anim_t);
		_outline_front.rectTransform.localScale = Vector3.Lerp(SPUtil.valv(1.25f), SPUtil.valv(1), _outline_front_anim_t);
	}
	
	private void control_move_set_selected_node(GridNode tar) {
		if (tar != null) {
			_selected_node = tar;
			if (_selected_node._visited) {
				_current_node = tar;
			}
		}
	}
	
	public void i_update(GameMain game) {
		if (game._popups.has_active_popup()) return;
		
		_grid_map_anchor.transform.localPosition = new Vector3(
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.x,_target_grid_map_anchor_position.x,1/10.0f),
			SPUtil.drpt(_grid_map_anchor.transform.localPosition.y,_target_grid_map_anchor_position.y,1/10.0f)
		);
		_selector.localPosition = new Vector3(
			_selected_node.transform.localPosition.x,
			_selected_node.transform.localPosition.y + 80 + Mathf.Sin(_selector_anim_t) * 10.0f
		);
		_selector_anim_t += 0.15f;
		
		_inventory_overlay.i_update(game,this);
		
		foreach (int id in _id_to_gridnode.Keys) {
			_id_to_gridnode[id].i_update(game,this);
		}
		
		List<GridNode> selection_list = this.get_selection_list(_selected_node);
		if (_touch_trigger_delay > 0) {
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveUp)) {
			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.y > cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,1)) : -1;
			});
			control_move_set_selected_node(tar);
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveDown)) {
			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.y < cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,-1)) : -1;
			});
			control_move_set_selected_node(tar);
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveLeft)) {
			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.x < cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(-1,0)) : -1;
			});
			control_move_set_selected_node(tar);
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveRight)) {
			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
				return (target.transform.localPosition.x > cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(1,0)) : -1;
			});
			control_move_set_selected_node(tar);
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
			GridNode touched_node = null;
			for (int i = 0; i < selection_list.Count; i++) {
				if (SPUtil.rect_transform_contains_screen_point(selection_list[i].cached_recttransform_get(),game._controls.get_touch_pos())) {
					touched_node = selection_list[i];
					break;
				}
			}
			if (touched_node != null && _selected_node != touched_node) {
				_selected_node = touched_node;
				this.selected_node_do_sfx(game);
				_touch_trigger_delay = 25;
			
			} else if (!_selected_node._visited && SPUtil.rect_transform_contains_screen_point(_selected_node.cached_recttransform_get(),game._controls.get_touch_pos())) {
				this.selected_node_do_sfx(game);
				_touch_trigger_delay = 25;
			} 
			
		} else if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
			this.selected_node_do_sfx(game);
			this.selected_node_do_action(game);
		}
		
		this.set_focus_node(_selected_node);
		game._background.load_background(_selected_node._node_script._background);
		
		if (_touch_trigger_delay > 0) {
			_touch_trigger_delay -= SPUtil.dt_scale_get();
			if (SPUtil.vec_dist(_grid_map_anchor.transform.localPosition,_target_grid_map_anchor_position) < 5) {
				_touch_trigger_delay = 0;
			}
			if (!(_touch_trigger_delay > 0)) {
				this.selected_node_do_action(game);
			}
		}
		if (!(_touch_trigger_delay > 0)) {	
			if ((this._current_node != null && !this._current_node._visited) && !GameMain.NO_EVENTS) {
				this._current_node._visited = true;
				game.start_event_modal(this._current_node._node_script);
				return;
			}
		}
	}
	
	private void selected_node_do_sfx(GameMain game) {
		if (GameMain.IGNORE_ITEM_REQ) {
			if (!_selected_node._visited) {
				if (_selected_node.play_map_yes_sfx()) {
					game._music.fade_bgm_for_time(0.75f);
					game._music.play_sfx("map_yes");
				}
			} else {
				game._music.play_sfx("dialogue_button_press");
			}
			
		} else if (_selected_node._node_script._affinity_requirement) {			
		} else if (_selected_node._is_locked) {			
		} else {
			if (_selected_node._visited) {
				game._music.play_sfx("dialogue_button_press");
			} else {
				game._music.fade_bgm_for_time(0.75f);
				if (_selected_node.play_map_yes_sfx()) {
					game._music.play_sfx("map_yes");
				}
			}
		}
	}
	
	private void selected_node_do_action(GameMain game) {
		if (GameMain.IGNORE_ITEM_REQ) {
			this.set_current_node(_selected_node);
			
		} else if (_selected_node._node_script._affinity_requirement) {
			game._music.fade_bgm_for_time(0.75f);
			if (game._affinity >= GameMain.AFFINITY_REQUIREMENT) {
				this.set_current_node(_selected_node);
				game._music.play_sfx("map_yes");
			} else {
				game._popups.add_popup("Friendship not at that level.");
				game._music.play_sfx("map_no");
			}
			
		} else if (_selected_node._is_locked) {
			game._music.fade_bgm_for_time(0.75f);
			if (this.selected_node_can_unlock(game)) {
				for (int i = 0; i < _selected_node._node_script._requirement_items.Count; i++) {
					string itr = _selected_node._node_script._requirement_items[i];
					game._inventory.remove_item(itr);
				}
				_selected_node._is_locked = false;
				this.set_current_node(_selected_node);
				if (_selected_node.play_map_yes_sfx()) {
					game._music.play_sfx("map_yes");
				}
				
			} else {
				game._popups.add_popup("Locked!");
				game._music.play_sfx("map_no");
				this.set_current_node(_current_node);
			}
			
		} else {
			this.set_current_node(_selected_node);
		}
	}
	
	private bool selected_node_can_unlock(GameMain game) {
		bool can_unlock = true;
		for (int i = 0; i < _selected_node._node_script._requirement_items.Count; i++) {
			string itr = _selected_node._node_script._requirement_items[i];
			if (!game._inventory._items.Contains(itr)) {
				can_unlock = false;
			}
		}
		return can_unlock;
	}
	
	private static GridNode cond_select_with_filter(GridNode current, List<GridNode> list, System.Func<GridNode,GridNode,float> filter) {
		float min_dist = float.MaxValue;
		GridNode rtv = current;
		for (int i = 0; i < list.Count; i++) {
			float val = filter(current,list[i]);
			if (val > 0 && val < min_dist) {
				min_dist = val;
				rtv = list[i];
			}
		}
		return rtv;
	}
	
	public List<GridNode> __cached_selection_list = new List<GridNode>();
	public List<GridNode> get_selection_list(GridNode target) {
		__cached_selection_list.Clear();
		if (!target._visited) {
			__cached_selection_list.Add(_current_node);
			return __cached_selection_list;
		}
		for (int i = 0; i < target._node_script._links.Count; i++) {
			GridNode itr_gridnode = _id_to_gridnode[target._node_script._links[i]];
			if (target._visited || itr_gridnode._visited) {
				__cached_selection_list.Add(itr_gridnode);
			}
		}
		if (!target._visited) {
			for (int i = 0; i < target._unidirectional_reverse_links.Count; i++) {
				GridNode itr_gridnode = _id_to_gridnode[target._unidirectional_reverse_links[i]];
				if (itr_gridnode._visited) {
					__cached_selection_list.Add(itr_gridnode);
				}
			}
		}
		return __cached_selection_list;
	}
	
	private void set_focus_node(GridNode tar_node) {
		_target_grid_map_anchor_position = tar_node._focus_point;
	}
	
	private void set_current_node(GridNode tar_node) {
		_current_node = tar_node;
		_selected_node = tar_node;
	}
	
}
