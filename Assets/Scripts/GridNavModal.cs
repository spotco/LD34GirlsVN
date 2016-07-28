using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_position_anchor;
	[SerializeField] private Transform _grid_map_scale_anchor;
	[SerializeField] public Transform _line_root;
	[SerializeField] private Transform _grid_node_root;
	[SerializeField] private GridNavCharacter _character_proto;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private InventoryOverlay _inventory_overlay;
	
	[SerializeField] private Image _outline_back;
	[SerializeField] private Image _outline_front;
	
	[SerializeField] private NodeAnimRoot _anim_root_proto;
	[SerializeField] private LineProtoRoot _line_root_proto;
	
	[System.NonSerialized] public SPDict<int,GridNode> _id_to_gridnode = new SPDict<int, GridNode>();
	private GridNavCharacter _selector_character;
	[System.NonSerialized] public GridNode _current_node;
//	[System.NonSerialized] public GridNode _selected_node;
	
//	private Vector3 _target_grid_map_anchor_position;
//	public float _touch_trigger_delay = 0;

	
	private enum State {
		InitialUpdate,
		WaitingForInput,
		MovingToNode,
		TriggerNodeOpenAttempt,
		WaitingForLockedMessage,
		WaitingForUnlockAnimation,
		WaitingForEventFinish,
		WaitingForShowNewNodes	
	}
	private State _current_state;
	private float _state_anim_ct;
			
	public void i_initialize(GameMain game) {
		_anim_root_proto.gameObject.SetActive(false);
		_line_root_proto.gameObject.SetActive(false);
		
		_character_proto.gameObject.SetActive(false);
		_selector_character = SPUtil.proto_clone(_character_proto.gameObject).GetComponent<GridNavCharacter>();
		
		this.gameObject.SetActive(true);
		_canvas_group.alpha = 0;
		_inventory_overlay.i_initialize();
		
		for (int i = 0; i < _grid_node_root.childCount; i++) {
			GameObject itr = _grid_node_root.GetChild(i).gameObject;
			if (itr.GetComponent<GridNode>() != null) {
				GridNode itr_gridnode = itr.GetComponent<GridNode>();
				itr_gridnode.i_initialize(game,this,_anim_root_proto, _line_root_proto);
				if (!_id_to_gridnode.ContainsKey(itr_gridnode._node_script._id)) {
					_id_to_gridnode[itr_gridnode._node_script._id] = itr_gridnode;
				}
			}
		}
		
		for (int i = 0; i < _id_to_gridnode.key_itr().Count; i++) {
			_id_to_gridnode[_id_to_gridnode.key_itr()[i]].calculate_directional_bindings(this);
			_id_to_gridnode[_id_to_gridnode.key_itr()[i]].set_unidirectional_reverse_links(this);
		}
		for (int i = 0; i < _id_to_gridnode.key_itr().Count; i++) {
			_id_to_gridnode[_id_to_gridnode.key_itr()[i]].create_link_sprites(this);
		}
		
		_current_node = _id_to_gridnode[GameMain.NODE_START_INDEX];
		_current_state = State.InitialUpdate;
		
//		this.set_current_node(_id_to_gridnode[GameMain.NODE_START_INDEX]);
//		this.update_accessible();
	}
	
	public void i_update(GameMain game) {
	
//		if (game._popups.has_active_popup()) return;
//		
//		_grid_map_anchor.transform.localPosition = new Vector3(
//			SPUtil.drpt(_grid_map_anchor.transform.localPosition.x,_target_grid_map_anchor_position.x,1/10.0f),
//			SPUtil.drpt(_grid_map_anchor.transform.localPosition.y,_target_grid_map_anchor_position.y,1/10.0f)
//		);
//		_selector.localPosition = new Vector3(
//			_selected_node.transform.localPosition.x,
//			_selected_node.transform.localPosition.y + 80 + Mathf.Sin(_selector_anim_t) * 10.0f
//		);
//
		
		_inventory_overlay.i_update(game,this);
		
		for (int i = 0; i < _id_to_gridnode.key_itr().Count; i++) {
			_id_to_gridnode[_id_to_gridnode.key_itr()[i]].i_update(game,this);
		}
		
		Vector2 selector_position = _selector_character.transform.localPosition;
		Vector2 grid_map_anchor_position = _grid_map_position_anchor.transform.localPosition;
		float grid_map_anchor_zoom = _grid_map_scale_anchor.transform.localScale.x;
		
		switch (_current_state) {
		case State.InitialUpdate: {
			this.attempt_open_node_and_update_state(game, _current_node);
			grid_map_anchor_zoom = 3;
			selector_position = _current_node.get_selector_stand_position();
			grid_map_anchor_position = GridNavCharacter.convert_character_position_to_position_anchor_focus(selector_position);
			
			
		} break;
		case State.WaitingForEventFinish: {
			grid_map_anchor_zoom = 3;
			selector_position = _current_node.get_selector_stand_position();
			grid_map_anchor_position = GridNavCharacter.convert_character_position_to_position_anchor_focus(selector_position);
			
		} break;
		case State.WaitingForShowNewNodes: {
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 1, 1/15.0f);
			selector_position = _current_node.get_selector_stand_position();
			
			// TODO -- node popin
			_state_anim_ct += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			if (_state_anim_ct >= 1) {
				_current_state = State.WaitingForInput;
				_state_anim_ct = 0;
			}
			
		} break;
		
		
		case State.WaitingForInput: {
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 1, 1/15.0f);
			
			selector_position = _current_node.get_selector_stand_position();
			
			Vector2 grid_map_anchor_current_node_focus_point = GridNavCharacter.convert_character_position_to_position_anchor_focus(_current_node._focus_point);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
			
			GridNode move_to_node = null;
			if (game._controls.get_control_just_released(ControlManager.Control.MoveUp)) {
				move_to_node = _current_node.get_directional_link(GridNode.Directional.Up);
				
			} else if (game._controls.get_control_just_released(ControlManager.Control.MoveDown)) {
				move_to_node = _current_node.get_directional_link(GridNode.Directional.Down);
				
			} else if (game._controls.get_control_just_released(ControlManager.Control.MoveLeft)) {
				move_to_node = _current_node.get_directional_link(GridNode.Directional.Left);
				
			} else if (game._controls.get_control_just_released(ControlManager.Control.MoveRight)) {
				move_to_node = _current_node.get_directional_link(GridNode.Directional.Right);
				
			} else if (game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
				GridNode touched_node = null;
				for (int i = 0; i < _current_node._node_script._links.Count; i++) {
					GridNode itr_node = _id_to_gridnode[_current_node._node_script._links[i]];
					if (SPUtil.rect_transform_contains_screen_point(itr_node.cached_recttransform_get(),game._controls.get_touch_pos())) {
						touched_node = itr_node;
						break;
					}
				}
				if (touched_node != null) {
					move_to_node = touched_node;
				}
			}
			
			if (move_to_node != null) {
				_current_node = move_to_node;
			}
		
		} break;
		
		case State.MovingToNode: {
		
		} break;
		case State.TriggerNodeOpenAttempt: {
		
		} break;
		case State.WaitingForLockedMessage: {
		
		} break;
		case State.WaitingForUnlockAnimation: {
		
		} break;
		}
		
		_selector_character.transform.localPosition = selector_position;
		_grid_map_position_anchor.transform.localPosition = grid_map_anchor_position;
		_grid_map_scale_anchor.transform.localScale = SPUtil.valv(grid_map_anchor_zoom);
	}
	
	private void attempt_open_node_and_update_state(GameMain game, GridNode node) {
		if (GameMain.IGNORE_ITEM_REQ) {
			this.trigger_node_event(game,node);
			_current_state = State.WaitingForEventFinish;
			
		} else if (node._node_script._affinity_requirement) {
			if (game._affinity >= GameMain.AFFINITY_REQUIREMENT) {
				this.trigger_node_event(game,node);
				_current_state = State.WaitingForEventFinish;
				
			} else {
				game._popups.add_popup("Friendship not at that level.");
				game._music.fade_bgm_for_time(0.75f);
				game._music.play_sfx("map_no");
				_current_state = State.WaitingForLockedMessage;
			}
			
		} else if (node._is_locked) {
			if (this.node_can_unlock(game, node)) {
				for (int i = 0; i < node._node_script._requirement_items.Count; i++) {
					string itr = node._node_script._requirement_items[i];
					game._inventory.remove_item(itr);
				}
				_current_state = State.WaitingForUnlockAnimation;
				//this.trigger_node_event(node);
				
			} else {
				game._popups.add_popup("Locked!");
				game._music.fade_bgm_for_time(0.75f);
				game._music.play_sfx("map_no");
				_current_state = State.WaitingForLockedMessage;
			}
			
		} else {
			this.trigger_node_event(game,node);
			_current_state = State.WaitingForEventFinish;
		}
	}
	
	private void trigger_node_event(GameMain game, GridNode node) {
		if (node.play_map_yes_sfx()) {
			game._music.fade_bgm_for_time(0.75f);
			game._music.play_sfx("map_yes");
		}
		node._is_locked = false;
		node._visited = true;
		game.start_event_modal(node._node_script);
	}
	
	public void return_from_event_modal(GameMain game) {
		this.update_accessible();
		_current_state = State.WaitingForShowNewNodes;
		_state_anim_ct = 0;
	}
	
	private Queue<int> __update_accessible_to_search = new Queue<int>();
	private HashSet<int> __update_accessible_searched = new HashSet<int>();
	public void update_accessible() {
		for (int i = 0; i < _id_to_gridnode.key_itr().Count; i++) {
			GridNode itr_node = _id_to_gridnode[_id_to_gridnode.key_itr()[i]];
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
	
	private float _outline_back_anim_t = 0, _outline_front_anim_t = 0;
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
	
	private bool node_can_unlock(GameMain game, GridNode node) {
		bool can_unlock = true;
		for (int i = 0; i < node._node_script._requirement_items.Count; i++) {
			string itr = node._node_script._requirement_items[i];
			if (!game._inventory._items.Contains(itr)) {
				can_unlock = false;
			}
		}
		return can_unlock;
	}
	
//	private void control_move_set_selected_node(GridNode tar) {
//		if (tar != null) {
//			_selected_node = tar;
//			if (_selected_node._visited) {
//				_current_node = tar;
//			}
//		}
//	}
	
//	public void i_update(GameMain game) {
//		if (game._popups.has_active_popup()) return;
//		
//		_grid_map_anchor.transform.localPosition = new Vector3(
//			SPUtil.drpt(_grid_map_anchor.transform.localPosition.x,_target_grid_map_anchor_position.x,1/10.0f),
//			SPUtil.drpt(_grid_map_anchor.transform.localPosition.y,_target_grid_map_anchor_position.y,1/10.0f)
//		);
//		_selector.localPosition = new Vector3(
//			_selected_node.transform.localPosition.x,
//			_selected_node.transform.localPosition.y + 80 + Mathf.Sin(_selector_anim_t) * 10.0f
//		);
//		_selector_anim_t += 0.15f;
//		
//		_inventory_overlay.i_update(game,this);
//		
//		foreach (int id in _id_to_gridnode.Keys) {
//			_id_to_gridnode[id].i_update(game,this);
//		}
//		
//		List<GridNode> selection_list = this.get_selection_list(_selected_node);
//		if (_touch_trigger_delay > 0) {
//		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveUp)) {
//			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
//				return (target.transform.localPosition.y > cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,1)) : -1;
//			});
//			control_move_set_selected_node(tar);
//			
//		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveDown)) {
//			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
//				return (target.transform.localPosition.y < cur.transform.localPosition.y) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(0,-1)) : -1;
//			});
//			control_move_set_selected_node(tar);
//			
//		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveLeft)) {
//			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
//				return (target.transform.localPosition.x < cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(-1,0)) : -1;
//			});
//			control_move_set_selected_node(tar);
//			
//		} else if (game._controls.get_control_just_released(ControlManager.Control.MoveRight)) {
//			GridNode tar = GridNavModal.cond_select_with_filter(_selected_node,selection_list,(GridNode cur, GridNode target) => {
//				return (target.transform.localPosition.x > cur.transform.localPosition.x) ? SPUtil.vec_dist(SPUtil.vec_sub(target.transform.localPosition,cur.transform.localPosition).normalized,new Vector2(1,0)) : -1;
//			});
//			control_move_set_selected_node(tar);
//			
//		} else if (game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
//			GridNode touched_node = null;
//			for (int i = 0; i < selection_list.Count; i++) {
//				if (SPUtil.rect_transform_contains_screen_point(selection_list[i].cached_recttransform_get(),game._controls.get_touch_pos())) {
//					touched_node = selection_list[i];
//					break;
//				}
//			}
//			if (touched_node != null && _selected_node != touched_node) {
//				_selected_node = touched_node;
//				this.selected_node_do_sfx(game);
//				_touch_trigger_delay = 25;
//			
//			} else if (!_selected_node._visited && SPUtil.rect_transform_contains_screen_point(_selected_node.cached_recttransform_get(),game._controls.get_touch_pos())) {
//				this.selected_node_do_sfx(game);
//				_touch_trigger_delay = 25;
//			} 
//			
//		} else if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
//			this.selected_node_do_sfx(game);
//			this.selected_node_do_action(game);
//		}
//		
//		this.set_focus_node(_selected_node);
//		game._background.load_background(_selected_node._node_script._background, _selected_node._node_script._background_key);
//		
//		if (_touch_trigger_delay > 0) {
//			_touch_trigger_delay -= SPUtil.dt_scale_get();
//			if (SPUtil.vec_dist(_grid_map_anchor.transform.localPosition,_target_grid_map_anchor_position) < 5) {
//				_touch_trigger_delay = 0;
//			}
//			if (!(_touch_trigger_delay > 0)) {
//				this.selected_node_do_action(game);
//			}
//		}
//		if (!(_touch_trigger_delay > 0)) {	
//			if ((this._current_node != null && !this._current_node._visited) && !GameMain.NO_EVENTS) {
//				this._current_node._visited = true;
//				game.start_event_modal(this._current_node._node_script);
//				return;
//			}
//		}
//	}
	
//	private void selected_node_do_sfx(GameMain game) {
//		if (GameMain.IGNORE_ITEM_REQ) {
//			if (!_selected_node._visited) {
//				if (_selected_node.play_map_yes_sfx()) {
//					game._music.fade_bgm_for_time(0.75f);
//					game._music.play_sfx("map_yes");
//				}
//			} else {
//				game._music.play_sfx("dialogue_button_press");
//			}
//			
//		} else if (_selected_node._node_script._affinity_requirement) {			
//		} else if (_selected_node._is_locked) {			
//		} else {
//			if (_selected_node._visited) {
//				game._music.play_sfx("dialogue_button_press");
//			} else {
//				game._music.fade_bgm_for_time(0.75f);
//				if (_selected_node.play_map_yes_sfx()) {
//					game._music.play_sfx("map_yes");
//				}
//			}
//		}
//	}
	
//	private void selected_node_do_action(GameMain game) {
//		if (GameMain.IGNORE_ITEM_REQ) {
//			this.set_current_node(_selected_node);
//			
//		} else if (_selected_node._node_script._affinity_requirement) {
//			game._music.fade_bgm_for_time(0.75f);
//			if (game._affinity >= GameMain.AFFINITY_REQUIREMENT) {
//				this.set_current_node(_selected_node);
//				game._music.play_sfx("map_yes");
//			} else {
//				game._popups.add_popup("Friendship not at that level.");
//				game._music.play_sfx("map_no");
//			}
//			
//		} else if (_selected_node._is_locked) {
//			game._music.fade_bgm_for_time(0.75f);
//			if (this.selected_node_can_unlock(game)) {
//				for (int i = 0; i < _selected_node._node_script._requirement_items.Count; i++) {
//					string itr = _selected_node._node_script._requirement_items[i];
//					game._inventory.remove_item(itr);
//				}
//				_selected_node._is_locked = false;
//				this.set_current_node(_selected_node);
//				if (_selected_node.play_map_yes_sfx()) {
//					game._music.play_sfx("map_yes");
//				}
//				
//			} else {
//				game._popups.add_popup("Locked!");
//				game._music.play_sfx("map_no");
//				this.set_current_node(_current_node);
//			}
//			
//		} else {
//			this.set_current_node(_selected_node);
//		}
//	}
	
//	private static GridNode cond_select_with_filter(GridNode current, List<GridNode> list, System.Func<GridNode,GridNode,float> filter) {
//		float min_dist = float.MaxValue;
//		GridNode rtv = current;
//		for (int i = 0; i < list.Count; i++) {
//			float val = filter(current,list[i]);
//			if (val > 0 && val < min_dist) {
//				min_dist = val;
//				rtv = list[i];
//			}
//		}
//		return rtv;
//	}
	
//	public List<GridNode> __cached_selection_list = new List<GridNode>();
//	public List<GridNode> get_selection_list(GridNode target) {
//		__cached_selection_list.Clear();
//		if (!target._visited) {
//			__cached_selection_list.Add(_current_node);
//			return __cached_selection_list;
//		}
//		for (int i = 0; i < target._node_script._links.Count; i++) {
//			GridNode itr_gridnode = _id_to_gridnode[target._node_script._links[i]];
//			if (target._visited || itr_gridnode._visited) {
//				__cached_selection_list.Add(itr_gridnode);
//			}
//		}
//		if (!target._visited) {
//			for (int i = 0; i < target._unidirectional_reverse_links.Count; i++) {
//				GridNode itr_gridnode = _id_to_gridnode[target._unidirectional_reverse_links[i]];
//				if (itr_gridnode._visited) {
//					__cached_selection_list.Add(itr_gridnode);
//				}
//			}
//		}
//		return __cached_selection_list;
//	}
	
//	private void set_focus_node(GridNode tar_node) {
//		_target_grid_map_anchor_position = tar_node._focus_point;
//	}
	
//	private void set_current_node(GridNode tar_node) {
//		_current_node = tar_node;
//		_selected_node = tar_node;
//	}
	
}
