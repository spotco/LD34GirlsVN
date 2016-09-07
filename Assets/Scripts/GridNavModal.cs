using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_position_anchor;
	[SerializeField] private Transform _grid_map_scale_anchor;
	[SerializeField] public Transform _line_root;
	[SerializeField] private Transform _grid_node_root;
	[SerializeField] private GridNavSelectorCharacter _character_proto;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private InventoryOverlay _inventory_overlay;
	
	[SerializeField] private Image _outline_back;
	[SerializeField] private Image _outline_front;
	
	[SerializeField] private NodeAnimRoot _anim_root_proto;
	[SerializeField] private LineProtoRoot _line_root_proto;
	[SerializeField] private GridNavArrow _nav_arrow_proto;
	[SerializeField] private Transform _particle_root;
	
	[SerializeField] public GridCharacterManager _character_manager;
	public GridNavModalDialogueManager _dialogue_manager = new GridNavModalDialogueManager();
	
	private SPDict<GridNode.Directional, GridNavArrow> _directional_to_arrow = new SPDict<GridNode.Directional, GridNavArrow>();
	
	[System.NonSerialized] public SPDict<int,GridNode> _id_to_gridnode = new SPDict<int, GridNode>();
	
	public SPSet<int> _active_gridnodes = new SPSet<int>();
	private List<int> _enqueued_to_show_gridnodes = new List<int>();
	
	[System.NonSerialized] public GridNavSelectorCharacter _selector_character;
	[System.NonSerialized] public GridNode _current_node;
	
	public GridNode _moving_to_node_target = null;
	private bool _moving_to_node_trigger_at_end = false;
	private enum SelectionMode {
		None,
		Cursor,
		Directional
	}
	
	private SelectionMode _selection_mode = SelectionMode.None;
	private Vector2 _last_touch_pos = Vector2.zero;
	private GridNode _selected_node = null;
	private GridNode.Directional _facing_directional = GridNode.Directional.None;
	private GridNode.Directional _moving_cancel_directional = GridNode.Directional.None;
	private Vector2 _panning_offset = Vector2.zero;
	
	private GridNode _wait_for_unlock_target = null;

	public enum State {
		InitialUpdate,
		WaitingForInput,
		PanningMode,
		DialogueMode,
		MovingToNode,
		MovingToNodeTriggerAtEndDelay,
		NodeOpenWaitAnim,
		WaitingForLockedMessage,
		WaitingForUnlockAnimation,
		WaitingForEventFinish,
		WaitingForShowNewNodes	
	}
	public State _current_state = State.InitialUpdate;
	private float _state_anim_ct = 0;
	private float _state_anim_ct_incr = 0;
	
	public SPParticleSystem<SPParticle> _particles;
			
	public void i_initialize(GameMain game) {
		_anim_root_proto.gameObject.SetActive(false);
		_line_root_proto.gameObject.SetActive(false);
		_nav_arrow_proto.gameObject.SetActive(false);
		
		_dialogue_manager.i_initialize(game,this);
		_character_manager.i_initialize(game,this);
		
		_particles = SPParticleSystem<SPParticle>.cons(_particle_root);
		
		for (int i = 0; i < GridNode.all_directionals().Count; i++) {
			GridNode.Directional itr_directional = GridNode.all_directionals()[i];
			GridNavArrow neu_arrow = SPUtil.proto_clone(_nav_arrow_proto.gameObject).GetComponent<GridNavArrow>().i_initialize(itr_directional);
			_directional_to_arrow[itr_directional] = neu_arrow;
			
			neu_arrow.set_is_showing_is_selected(false,false,true);
			neu_arrow.gameObject.name = itr_directional.ToString();
		}
		
		_character_proto.gameObject.SetActive(false);
		_selector_character = SPUtil.proto_clone(_character_proto.gameObject).GetComponent<GridNavSelectorCharacter>();
		_selector_character.i_initialize(game);
		
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
	}
	
	public void i_update(GameMain game) {	
		if (game._popups.has_active_popup()) return;
		
		this.recalc_all_can_move_to_nodes();
		
		Vector2 selector_position = _selector_character.transform.localPosition;
		Vector2 grid_map_anchor_position = _grid_map_position_anchor.transform.localPosition;
		float grid_map_anchor_zoom = _grid_map_scale_anchor.transform.localScale.x;
		bool selector_character_selected = false;
		
		bool should_show_nav_arrow = false;
		
		switch (_current_state) {
		case State.InitialUpdate: {
			grid_map_anchor_zoom = 3;
			selector_position = _current_node.get_selector_stand_position(game,this);
			grid_map_anchor_position = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(selector_position);
			
			_current_state = State.WaitingForEventFinish;
			this.trigger_node_event(game, _current_node);
			
			
		} break;
		case State.WaitingForEventFinish: {
			grid_map_anchor_zoom = 3;
			selector_position = _current_node.get_selector_stand_position(game,this);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
			grid_map_anchor_position = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(selector_position);
			
		} break;
		case State.WaitingForShowNewNodes: {
		
			for (int i = 0; i < _active_gridnodes.key_itr().Count; i++) { // lines updating during animation
				_id_to_gridnode[_active_gridnodes.key_itr()[i]].i_update_linestates(game,this);
			}
		
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 1, 1/15.0f);
			selector_position = _current_node.get_selector_stand_position(game,this);
			
			if (game._controls.get_debug_skip()) {
				for (int i = 0; i < _enqueued_to_show_gridnodes.Count; i++) {
					_active_gridnodes.Add(_enqueued_to_show_gridnodes[i]);
				}
				_enqueued_to_show_gridnodes.Clear();
			}
			
			if (_enqueued_to_show_gridnodes.Count > 0) {
				int tar_id = _enqueued_to_show_gridnodes[0];
				GridNode tar_node = _id_to_gridnode[tar_id];
				
				Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(tar_node.get_center_position());
				grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
				grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
				
				tar_node.set_showing(true, false, false);
				
				if (tar_node.is_anim_finished()) {
					_enqueued_to_show_gridnodes.RemoveAt(0);
					_active_gridnodes.Add(tar_id);	
				}
				
			} else {
				_state_anim_ct = 0;
				if (_current_node._node_script._post_show_events.Count > 0) {
					_current_state = State.DialogueMode;
					_dialogue_manager.load_dialogue(game,this,_current_node._node_script._post_show_events);
				} else {
					_current_state = State.WaitingForInput;
				}
			}
			
		} break;
		case State.WaitingForInput: {
			for (int i = 0; i < _active_gridnodes.key_itr().Count; i++) { // this is done first since this will change gridnode animroot state
				_id_to_gridnode[_active_gridnodes.key_itr()[i]].i_active_update(game,this);
			}
		
			should_show_nav_arrow = true;
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 0.85f, 1/15.0f);
			
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
			
			game._background.load_background(_current_node._node_script._background, _current_node._node_script._background_key);
			
			_panning_offset = Vector2.zero;
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(_current_node._focus_point) + _panning_offset;
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
			
			bool any_directional_is_down = false;
			bool any_directional_just_released = false;
			for (int i = 0; i < GridNode.all_directionals().Count; i++) {
				GridNode.Directional itr = GridNode.all_directionals()[i];
				ControlManager.Control itr_key = GridNavModal.directional_to_key(itr);
				if (game._controls.get_control_down(itr_key) && this.get_directional_move_attempt_node(itr) != null) {
					any_directional_is_down = true;
				}
				if (game._controls.get_control_just_released(itr_key) && this.get_directional_move_attempt_node(itr) != null) {
					any_directional_just_released = true;
				}
			}
			
			bool mouse_over_selector_character = SPUtil.rect_transform_contains_screen_point(_selector_character.get_recttransform(), game._controls.get_touch_pos());
			if (_selection_mode == SelectionMode.Cursor) {
				selector_character_selected = mouse_over_selector_character;
			} else {
				selector_character_selected = game._controls.get_control_down(ControlManager.Control.ButtonC);
			}
			bool touched_selector_character = mouse_over_selector_character && game._controls.get_control_just_pressed(ControlManager.Control.TouchClick);

			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				this.attempt_open_node_and_update_state(game, _current_node);
				
			} else if (!touched_selector_character && game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
				if (SPUtil.rect_transform_contains_screen_point(_current_node.cached_recttransform_get(), game._controls.get_touch_pos())) {
					this.attempt_open_node_and_update_state(game,_current_node);
					
				} else if (_selected_node != null) {
					if (this.get_all_can_move_to_nodes().ContainsKey(_selected_node._node_script._id)) {
						this.gridnav_arrow_trigger_for_target_node(game,_selected_node);
						_current_state = State.MovingToNode;
						_moving_to_node_target = _selected_node;
						_moving_to_node_trigger_at_end = true;
						_state_anim_ct = 0;
						
					}
				}
				
			} else if (!any_directional_is_down && any_directional_just_released) {
				if (_selected_node != null) {
					if (this.get_all_can_move_to_nodes().ContainsKey(_selected_node._node_script._id)) {
						this.gridnav_arrow_trigger_for_target_node(game,_selected_node);
						_current_state = State.MovingToNode;
						_moving_to_node_target = _selected_node;
						_moving_to_node_trigger_at_end = false;
						_state_anim_ct = 0;
						
					}	
				}
			} else if (game._controls.get_control_just_pressed(ControlManager.Control.ButtonB)) {
				_current_state = State.PanningMode;
				
			} else if (touched_selector_character || game._controls.get_control_just_released(ControlManager.Control.ButtonC)) {
				if (_current_node._node_script._idle_events.Count > 0) {
					_current_state = State.DialogueMode;
					_dialogue_manager.load_dialogue(game,this,_current_node._node_script._idle_events);
				}
			}
			
		} break;
		case State.DialogueMode: {
			_dialogue_manager.i_update(game,this);
			
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Slide);
			
			_dialogue_manager.get_anchor_position_and_zoom(game,this,ref grid_map_anchor_position,ref grid_map_anchor_zoom);
			
			if (_dialogue_manager.is_finished(game)) {
				_dialogue_manager.exit_gridnav_state_dialogue_mode(game,this);
				_current_state = State.WaitingForInput;
			}
			
		} break;
		case State.PanningMode: {
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 0.5f, 1/15.0f);
			
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
			
			Vector2 move_vec = Vector2.zero;
			if (game._controls.get_control_down(ControlManager.Control.MoveUp)) {
				move_vec.y = 1;
			} else if (game._controls.get_control_down(ControlManager.Control.MoveDown)) {
				move_vec.y = -1;
			}	
			if (game._controls.get_control_down(ControlManager.Control.MoveLeft)) {
				move_vec.x = -1;
			} else if (game._controls.get_control_down(ControlManager.Control.MoveRight)) {
				move_vec.x = 1;
			}
			if (move_vec.magnitude > 0) {
				Vector2 tar_panning_offset = SPUtil.vec_scale(move_vec.normalized, 200);
				_panning_offset = tar_panning_offset;
			} else {
				_panning_offset = Vector2.zero;
			}
			
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(_current_node._focus_point + _panning_offset);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
			
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonB)) {
				_current_state = State.WaitingForInput;
			}
		} break;
		case State.MovingToNode: {
			
			// cancel-reverse movement
			{
				bool directional_pressed = false;
				GridNode.Directional directional = GridNode.Directional.None;
				for (int i = 0; i < GridNode.all_directionals().Count; i++) {
					GridNode.Directional itr_directional = GridNode.all_directionals()[i];
					ControlManager.Control itr_control = GridNavModal.directional_to_key(itr_directional);
					if (game._controls.get_control_just_released(itr_control)) {
						directional_pressed = true;
						directional = itr_directional;
						break;
					}
				}
				
				if (directional_pressed) {
					GridNode.Directional directional_inverse = GridNode.inverse_directional(directional);
					if (_current_node.has_directional_link(directional_inverse) && _current_node.get_directional_link(directional_inverse) == _moving_to_node_target) {
						_state_anim_ct = 1 - _state_anim_ct;
						GridNode neu_current = _moving_to_node_target;
						GridNode neu_target = _current_node;
						_current_node = neu_current;
						_moving_to_node_target = neu_target;
							
					}
				}
			}
		
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(_selector_character.transform.localPosition);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
		
			if (_state_anim_ct <= 0) {
				_state_anim_ct = 0;
				_state_anim_ct_incr = 1 / (SPUtil.vec_dist(_current_node.get_center_position(), _moving_to_node_target.get_center_position()) / 5.0f);
			}
			
			_state_anim_ct += _state_anim_ct_incr * SPUtil.dt_scale_get();
			
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 1.0f, 1/15.0f);
			Vector2 selector_tar_pos = Vector2.Lerp(_current_node.get_center_position(), _moving_to_node_target.get_center_position(), _state_anim_ct);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
			
			if (_state_anim_ct >= 1) {
				_state_anim_ct = 0;
				_current_node = _moving_to_node_target;
				if (_moving_to_node_trigger_at_end) {
					_current_state = State.MovingToNodeTriggerAtEndDelay;
				} else {
					_current_state = State.WaitingForInput;
				}
			}
			
		} break;
		case State.MovingToNodeTriggerAtEndDelay: {
		
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
			
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(_selector_character.transform.localPosition);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
			
			if (SPUtil.vec_dist(selector_tar_pos,selector_position) < 5) {
				_current_state = State.WaitingForInput;
				this.attempt_open_node_and_update_state(game, _current_node);
			}
		
		} break;
		case State.NodeOpenWaitAnim: {
		
			game._background.load_background(_current_node._node_script._background, _current_node._node_script._background_key);
			
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 3, 1/15.0f);
			
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(selector_position);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/10.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/10.0f);
			
			game._background.load_background(_current_node._node_script._background, _current_node._node_script._background_key);
		
			_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Yay);
			
			_state_anim_ct += SPUtil.sec_to_tick(0.65f) * SPUtil.dt_scale_get();
			
			if (_state_anim_ct >= 1) {
				_state_anim_ct = 0;
				_current_state = State.WaitingForEventFinish;
				this.trigger_node_event(game, _current_node);
			}
			
		} break;
		case State.WaitingForLockedMessage: {
			_current_state = State.WaitingForInput;
			
		} break;
		case State.WaitingForUnlockAnimation: {
			grid_map_anchor_zoom = SPUtil.drpt(grid_map_anchor_zoom, 1.75f, 1/15.0f);
			
			Vector2 selector_tar_pos = _current_node.get_selector_stand_position(game,this);
			selector_position.x = SPUtil.drpt(selector_position.x, selector_tar_pos.x, 1/10.0f);
			selector_position.y = SPUtil.drpt(selector_position.y, selector_tar_pos.y, 1/10.0f);
			
			Vector2 grid_map_anchor_current_node_focus_point = GridNavSelectorCharacter.convert_character_position_to_position_anchor_focus(_selector_character.transform.localPosition);
			grid_map_anchor_position.x = SPUtil.drpt(grid_map_anchor_position.x, grid_map_anchor_current_node_focus_point.x, 1/15.0f);
			grid_map_anchor_position.y = SPUtil.drpt(grid_map_anchor_position.y, grid_map_anchor_current_node_focus_point.y, 1/15.0f);
		
			if (_wait_for_unlock_target.is_anim_finished()) {
				_current_state = State.WaitingForInput;
				this.attempt_open_node_and_update_state(game, _wait_for_unlock_target);
				_wait_for_unlock_target = null;
			}
			
		} break;
		}
		_selector_character.transform.localPosition = selector_position;
		_selector_character.set_selected(selector_character_selected);
		_grid_map_position_anchor.transform.localPosition = grid_map_anchor_position;
		_grid_map_scale_anchor.transform.localScale = SPUtil.valv(grid_map_anchor_zoom);
		
		this.i_update_grid_nav_arrows(game, should_show_nav_arrow);
		_character_manager.i_update(game,this);
		
		_inventory_overlay.i_update(game,this);
		_particles.i_update(game, this);
		for (int i = 0; i < _id_to_gridnode.key_itr().Count; i++) {
			_id_to_gridnode[_id_to_gridnode.key_itr()[i]].i_anim_update(game,this);
		}
		this.i_update_selected_node(game);
		
		_selector_character.i_update(game);
	}
	
	private void gridnav_arrow_trigger_for_target_node(GameMain game, GridNode node) {
		for (int i = 0; i < GridNode.all_directionals().Count; i++) {
			GridNode.Directional itr_directional = GridNode.all_directionals()[i];
			if (this.get_directional_move_attempt_node(itr_directional) == node) {
				_directional_to_arrow[itr_directional].trigger_selected();
				break;
			}
		}
	}
	
	private void i_update_grid_nav_arrows(GameMain game, bool should_show_nav_arrow) {
		for (int i = 0; i < _directional_to_arrow.key_itr().Count; i++) {
			GridNode.Directional itr_directional = _directional_to_arrow.key_itr()[i];
			GridNavArrow itr_arrow = _directional_to_arrow[itr_directional];
			ControlManager.Control itr_control = GridNavModal.directional_to_key(itr_directional);
			
			GridNode move_to_node = this.get_directional_move_attempt_node(itr_directional);
			if (should_show_nav_arrow && move_to_node != null) {
				if (_selection_mode == SelectionMode.Cursor && this.is_selected_node(game, move_to_node)) {
					itr_arrow.set_is_showing_is_selected(true,true);
				} else if (_selection_mode == SelectionMode.Directional && game._controls.get_control_down(itr_control)) {
					itr_arrow.set_is_showing_is_selected(true,true);
				} else {
					itr_arrow.set_is_showing_is_selected(true,false);
				}
				
				
				float dist = SPUtil.vec_dist(_current_node.get_center_position(), move_to_node.get_center_position());
				float offset_dist = 75;
				
				if (dist > 200) {
					offset_dist = dist / 2.0f;
				} else if (!_current_node._visited) {
					offset_dist = 100;
				}
				itr_arrow.transform.localPosition = SPUtil.vec_add(
					_current_node.get_center_position(),
					SPUtil.vec_scale(
					SPUtil.vec_sub(
						move_to_node.get_center_position(), 
						_current_node.get_center_position()
					).normalized, offset_dist));
				
			} else {
				itr_arrow.set_is_showing_is_selected(false,false);
			}
			
			itr_arrow.i_update();
		}
	}
	
	private static ControlManager.Control directional_to_key(GridNode.Directional input) {
		switch (input) {
		case GridNode.Directional.Up: return ControlManager.Control.MoveUp;
		case GridNode.Directional.Down: return ControlManager.Control.MoveDown;
		case GridNode.Directional.Left: return ControlManager.Control.MoveLeft;
		case GridNode.Directional.Right: return ControlManager.Control.MoveRight;
		default: return ControlManager.Control.None;
		}
	}
	
	public bool is_selected_node(GameMain game, GridNode node) {
		if (_current_state == State.MovingToNode) {
			return node == _moving_to_node_target;
		}
		return _selected_node == node;
	}
	
	private void i_update_selected_node(GameMain game) {
		if (_current_state != State.WaitingForInput) {
			_selection_mode = SelectionMode.None;
			_selected_node = null;
			return;
		}
		
		bool selection_mode_set_this_frame = false;
		
		Vector2 cur_touch_pos = game._controls.get_touch_pos();
		if (cur_touch_pos != _last_touch_pos) {
			_selection_mode = SelectionMode.Cursor;
			selection_mode_set_this_frame = true;
		}
		
		if (!selection_mode_set_this_frame) {
			for (int i = 0; i < GridNode.all_directionals().Count; i++) {
				GridNode.Directional itr_dir = GridNode.all_directionals()[i];
				ControlManager.Control itr_ctrl = GridNavModal.directional_to_key(itr_dir);
			
				if (game._controls.get_control_just_pressed(itr_ctrl)) {
					_selection_mode = SelectionMode.Directional;
					selection_mode_set_this_frame = true;
					break;
				}
			}
		}
		
		_last_touch_pos = cur_touch_pos;

		
		if (_selection_mode == SelectionMode.Cursor) {
			bool found = false;
			for (int i = 0; i < this.get_all_can_move_to_nodes().key_itr().Count; i++) {
				GridNode itr = _id_to_gridnode[this.get_all_can_move_to_nodes().key_itr()[i]];
				if (SPUtil.rect_transform_contains_screen_point(itr.cached_recttransform_get(), game._controls.get_touch_pos())) {
					_selected_node = itr;
					found = true;
					break;
				}	
			}
			if (!found) {
				_selected_node = null;
			}
		
		} else if (_selection_mode == SelectionMode.Directional) {
			
			// set _facing_directional, _selected_node
			for (int i = 0; i < GridNode.all_directionals().Count; i++) {
				GridNode.Directional itr_directional = GridNode.all_directionals()[i];
				ControlManager.Control itr_control = GridNavModal.directional_to_key(itr_directional);
				if (game._controls.get_control_just_pressed(itr_control) && this.get_directional_move_attempt_node(itr_directional) != null) {
					_facing_directional = itr_directional;
					_selected_node = this.get_directional_move_attempt_node(itr_directional);
					break;
				}
			}
			
			// set key_just_released
			bool key_just_released = false;
			for (int i = 0; i < GridNode.all_directionals().Count; i++) {
				GridNode.Directional itr_directional = GridNode.all_directionals()[i];
				ControlManager.Control itr_control = GridNavModal.directional_to_key(itr_directional);
				if (game._controls.get_control_just_released(itr_control)) {
					key_just_released = true;
					break;
				}
			}
			
			if (key_just_released) {
				for (int i = 0; i < GridNode.all_directionals().Count; i++) {
					GridNode.Directional itr = GridNode.all_directionals()[i];
					ControlManager.Control itr_key = GridNavModal.directional_to_key(itr);
					
					if (game._controls.get_control_down(itr_key) && this.get_directional_move_attempt_node(itr) != null) {
						_selected_node = this.get_directional_move_attempt_node(itr);
						_facing_directional = itr;
						break;
					}
				}
			}
			
		} else {
			_selected_node = null;
		}
	}
	
	private static SPDict<int,int> __all_canmoveto = new SPDict<int,int>();
	public SPDict<int,int> get_all_can_move_to_nodes() { return __all_canmoveto; }
	private void recalc_all_can_move_to_nodes() {
		__all_canmoveto.Clear();
		for (int i = 0; i < GridNode.all_directionals().Count; i++) {
			GridNode dir_move_node = this.get_directional_move_attempt_node(GridNode.all_directionals()[i]);
			if (dir_move_node != null) {
				__all_canmoveto[dir_move_node._node_script._id] = 1;
			}
		}
	}
	
	public GridNode get_directional_move_attempt_node(GridNode.Directional dir) {
		if (_current_node._visited) {
			if (_current_node.has_directional_link(dir)) return _current_node.get_directional_link(dir);
		} else {
			if (_current_node.has_directional_link(dir) && _current_node.get_directional_link(dir)._visited) return _current_node.get_directional_link(dir);
			for (int i = 0; i < _current_node._unidirectional_reverse_links.Count; i++) {
				int itr_id = _current_node._unidirectional_reverse_links[i];
				GridNode itr_node = _id_to_gridnode[itr_id];
				GridNode.Directional dir_inverse = GridNode.inverse_directional(dir);
				if (itr_node._visited && itr_node.has_directional_link(dir_inverse) && itr_node.get_directional_link(dir_inverse) == _current_node) {
					return _id_to_gridnode[itr_id];
				}
			}
		}
		return null;
	}
	
	private void attempt_open_node_and_update_state(GameMain game, GridNode node) {
		if (node._visited) {
			return;
			
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
			if (this.node_can_unlock(game, node) || GameMain.IGNORE_ITEM_REQ) {
				for (int i = 0; i < node._node_script._requirement_items.Count; i++) {
					string itr = node._node_script._requirement_items[i];
					game._inventory.remove_item(itr);
				}
				
				_wait_for_unlock_target = node;
				_wait_for_unlock_target.set_unlocked();
				_current_state = State.WaitingForUnlockAnimation;
				
			} else {
				game._popups.add_popup("Locked!");
				game._music.fade_bgm_for_time(0.75f);
				game._music.play_sfx("map_no");
				_current_state = State.WaitingForLockedMessage;
			}
			
		} else {
			_state_anim_ct = 0;
			_current_state = State.NodeOpenWaitAnim;
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
		_selector_character.set_anim_mode(GridNavSelectorCharacter.AnimMode.Move);
		
		for (int i = _active_gridnodes.key_itr().Count-1; i >= 0; i--) {
			int itr_id = _active_gridnodes.key_itr()[i];
			GridNode itr = _id_to_gridnode[itr_id];
			if (!_accessible_grid_nodes.ContainsKey(itr_id)) {
				itr.set_showing(false, true, false);
				_active_gridnodes.Remove(itr_id);
			}
		}
		
		for (int i = 0; i < _accessible_grid_nodes.key_itr().Count; i++) {
			GridNode itr = _id_to_gridnode[_accessible_grid_nodes.key_itr()[i]];
			int itr_id = itr._node_script._id;
			if (!_active_gridnodes.ContainsKey(itr_id)) {
				if (!_enqueued_to_show_gridnodes.Contains(itr_id)) {
					_enqueued_to_show_gridnodes.Add(itr_id);
				}
			}
		}
		
		_current_state = State.WaitingForShowNewNodes;
		_state_anim_ct = 0;
		
		_current_node.set_showing(true, true, false);
	}
	
	private Queue<int> __update_accessible_to_search = new Queue<int>();
	private HashSet<int> __update_accessible_searched = new HashSet<int>();
	private SPDict<int,int> _accessible_grid_nodes = new SPDict<int, int>();
	public void update_accessible() {
		_accessible_grid_nodes.Clear();
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
			_accessible_grid_nodes[itr_node._node_script._id] = 1;
			
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
}
