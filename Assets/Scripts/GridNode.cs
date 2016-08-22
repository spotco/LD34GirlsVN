using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNode : MonoBehaviour {

	[SerializeField] private TextAsset _node_script_text;
	private float _anim_theta;
	
	public NodeScript _node_script = new NodeScript();
	
	private NodeAnimRoot _self_nodeanimroot;
	public bool _is_locked;
	private SPDict<int,LineProtoRoot> _id_to_line = new SPDict<int, LineProtoRoot>();
	public bool _accessible;
	public bool _visited;
	private static Font __cached_font;
	private LineProtoRoot _line_proto;
	
	private void hide_legacy_elements() {
		Destroy(this.GetComponent<Image>());
		Destroy(this.transform.FindChild("LockImage").gameObject);
		Destroy(this.transform.FindChild("LockIcon").gameObject);
		Destroy(this.transform.FindChild("Text").gameObject);
	}
	
	public void i_initialize(GameMain game, GridNavModal grid_nav, NodeAnimRoot proto_nodeanimroot, LineProtoRoot proto_line) {
		this.hide_legacy_elements();
		_self_nodeanimroot = SPUtil.proto_clone(proto_nodeanimroot.gameObject).GetComponent<NodeAnimRoot>();
		_self_nodeanimroot.transform.SetParent(this.gameObject.transform);
		_self_nodeanimroot.transform.localPosition = SPUtil.valv(0);
		_self_nodeanimroot.i_initialize();
		
		_line_proto = proto_line;
		
		_node_script.i_initialize(game,_node_script_text);
		
		if (__cached_font == null) {
			__cached_font = Resources.Load<Font>("osaka.unicode");
		}
		
		_self_nodeanimroot.set_font(__cached_font);
		_self_nodeanimroot.set_text(_node_script._title);
		
		this.gameObject.name = SPUtil.sprintf("Node (%d)",_node_script._id);
		
		
		if (_node_script._affinity_requirement) {
			_is_locked = true;
			_self_nodeanimroot._item_icon.sprite = game._inventory.icon_for_item("item_heart");
		} else {
			_is_locked = _node_script._requirement_items.Count > 0;
			if (_is_locked) {
				_self_nodeanimroot._item_icon.sprite = game._inventory.icon_for_item(_node_script._requirement_items[0]);
			}
		}
		
		_accessible = false;
		_visited = false;
	}
	
	public void set_showing(bool val, bool imm, bool selected) {
		
		NodeAnimRoot.AnimState tar_animstate = NodeAnimRoot.AnimState.None;
		if (val) {
			if (_is_locked) {
				if (selected) {
					tar_animstate = NodeAnimRoot.AnimState.Locked_Selected;
				} else {
					tar_animstate = NodeAnimRoot.AnimState.Locked_Unselected;
				}
			} else if (_visited) {
				if (selected) {
					tar_animstate = NodeAnimRoot.AnimState.Visited_Selected;
				} else {
					tar_animstate = NodeAnimRoot.AnimState.Visited_Unselected;
				}
				
			} else {
				if (selected) {
					tar_animstate = NodeAnimRoot.AnimState.Unvisited_Selected;
				} else {
					tar_animstate = NodeAnimRoot.AnimState.Unvisited_Unselected;
				}
			}
			

		}
	
		if (_self_nodeanimroot.get_anim_state() == NodeAnimRoot.AnimState.Hidden) {
			_self_nodeanimroot.set_anim_state(tar_animstate);
			if (imm) {
				_self_nodeanimroot.set_transition_state(NodeAnimRoot.AnimTransitionState.None);
				_self_nodeanimroot.i_update(null,null); //lol
			} else {
				_self_nodeanimroot.set_transition_state(NodeAnimRoot.AnimTransitionState.PopIn);
			}
			
		} else { // showing
			if (tar_animstate != NodeAnimRoot.AnimState.None && tar_animstate != _self_nodeanimroot.get_anim_state()) {
				_self_nodeanimroot.set_anim_state(tar_animstate);
				if (imm) {
					_self_nodeanimroot.set_transition_state(NodeAnimRoot.AnimTransitionState.None);
					_self_nodeanimroot.i_update(null,null); //lol
				}
			}
		
			if (!val) {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Hidden);
			}
		}	
	}
	public bool is_anim_finished() {
		return _self_nodeanimroot.get_transition_state() == NodeAnimRoot.AnimTransitionState.None;
	}
	
	public void set_unlocked() {
		_is_locked = false;
		_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Unselected);
		_self_nodeanimroot.set_transition_state(NodeAnimRoot.AnimTransitionState.RemoveLock);
	}
	
	public List<int> _unidirectional_reverse_links = new List<int>();
	public void set_unidirectional_reverse_links(GridNavModal grid_nav) {
		for (int i = 0; i < _node_script._links.Count; i++) {
			int itr_id = _node_script._links[i];
			GridNode linked_node = grid_nav._id_to_gridnode[itr_id];
			if (!linked_node._node_script._links.Contains(_node_script._id)) {
				linked_node._unidirectional_reverse_links.Add(_node_script._id);
			}
		}
	}
	
	public Vector2 _focus_point = new Vector2();
	public void recalculate_focus_point(GridNavModal grid_nav) {
		Vector2 tar_pos = Vector2.zero;
		tar_pos = new Vector3(
			this.transform.localPosition.x,
			this.transform.localPosition.y
		);
		
		if (_visited) {
			for (int i = 0; i < this._node_script._links.Count; i++) {
				GridNode itr_node = grid_nav._id_to_gridnode[this._node_script._links[i]];
				if (!itr_node._visited) {
					tar_pos.x = SPUtil.running_avg(tar_pos.x, itr_node.transform.localPosition.x, i+2);
					tar_pos.y = SPUtil.running_avg(tar_pos.y, itr_node.transform.localPosition.y, i+2);
				}
			}
		}
		
		_focus_point = tar_pos;
	}
	
	public void create_link_sprites(GridNavModal grid_nav) {
		List<int> all_links = new List<int>();
		all_links.AddRange(_node_script._links);
		all_links.AddRange(_unidirectional_reverse_links);
		for (int i = 0; i < all_links.Count; i++) {
			int itr_id = all_links[i];
			
			if (!grid_nav._id_to_gridnode.ContainsKey(itr_id)) {
				Debug.LogError(SPUtil.sprintf("ERROR! Canvas->GameMain->BackgroundImage->GridNav->GridMapAnchor does not contain node of id(%d)",itr_id));
				continue;
			}
			
			GridNode other_node = grid_nav._id_to_gridnode[itr_id];
			Vector3 lpos_delta = SPUtil.vec_sub(other_node.transform.localPosition,this.transform.localPosition);
			
			LineProtoRoot neu_line = SPUtil.proto_clone(_line_proto.gameObject).GetComponent<LineProtoRoot>();
			neu_line.i_initialize();
			neu_line._rect_transform.sizeDelta = new Vector2(lpos_delta.magnitude, neu_line.GetComponent<RectTransform>().sizeDelta.y);
			
			neu_line._rect_transform.localEulerAngles = new Vector3(0,0,SPUtil.dir_ang_deg(lpos_delta.x,lpos_delta.y));
			neu_line._rect_transform.SetParent(grid_nav._line_root);
			neu_line._rect_transform.localPosition = this.cached_recttransform_get().localPosition;
			
			neu_line._image.color = new Color(
				neu_line._image.color.r,
				neu_line._image.color.g,
				neu_line._image.color.b,
				0
			);
			
			_id_to_line[itr_id] = neu_line;
		}
	}
	
	public enum Directional {
		Up,
		Down,
		Left,
		Right,
		None
	}
	public static Vector2 directional_to_vector(Directional input) {
		switch (input) {
		case Directional.Up: return new Vector2(0,1);
		case Directional.Down: return new Vector2(0,-1);
		case Directional.Left: return new Vector2(-1,0);
		case Directional.Right: return new Vector2(1,0);
		default: return new Vector2(0,-1);
		}
	}
	public static Directional inverse_directional(Directional input) {
		switch (input) {
		case Directional.Up: return Directional.Down;
		case Directional.Down: return Directional.Up;
		case Directional.Left: return Directional.Right;
		case Directional.Right: return Directional.Left;
		default: return Directional.Up;
		}
	}
	private static List<Directional> __all_directionals = null;
	public static List<Directional> all_directionals() {
		if (__all_directionals == null) {
			__all_directionals = new List<Directional>() { Directional.Up, Directional.Down, Directional.Left, Directional.Right };
		}
		return __all_directionals;
	}
	
	private SPDict<Directional, GridNode> _directional_links = new SPDict<Directional, GridNode>();
	public bool has_directional_link(Directional dir) {
		return _directional_links.ContainsKey(dir);
	}
	public GridNode get_directional_link(Directional dir) {
		if (!this.has_directional_link(dir)) return null;
		return _directional_links[dir];
	}
	
	private static void r_calculate_directional_bindings(
		SPDict<Directional,GridNode> current, 
		SPDict<Directional,GridNode> min_binding, 
		ref float min_binding_dist, 
		GridNode self, 
		List<GridNode> remaining_grid_nodes, 
		List<Directional> remaining_directionals) {

		if (remaining_grid_nodes.Count == 0 || remaining_directionals.Count == 0) {

			float cmp_sum = 0;
			for (int i_current = 0; i_current < current.key_itr().Count; i_current++) {
				Directional i_current_directional = current.key_itr()[i_current];
				GridNode i_current_gridnode = current[i_current_directional];
				
				Vector2 cur_to_itr_dir = SPUtil.vec_sub(i_current_gridnode.get_center_position(),self.get_center_position()).normalized;
				Vector2 i_current_directional_dir = GridNode.directional_to_vector(i_current_directional);
					
				cmp_sum += SPUtil.rad_to_deg(Mathf.Acos(SPUtil.vec_dot(i_current_directional_dir,cur_to_itr_dir)));
			}
			
			if (cmp_sum < min_binding_dist) {
				min_binding_dist = cmp_sum;
				min_binding.Clear();
				for (int i_current = 0; i_current < current.key_itr().Count; i_current++) {
					Directional i_current_directional = current.key_itr()[i_current];
					min_binding[i_current_directional] = current[i_current_directional];
				}
			}
			
			return;
		}
		
		for (int i_gridnode = 0; i_gridnode < remaining_grid_nodes.Count; i_gridnode++) {
			GridNode itr_gridnode = remaining_grid_nodes[i_gridnode];
			remaining_grid_nodes.RemoveAt(i_gridnode);
			
			for (int i_directional = 0; i_directional < remaining_directionals.Count; i_directional++) {
				Directional itr_directional = remaining_directionals[i_directional];
				remaining_directionals.RemoveAt(i_directional);
				
				current[itr_directional] = itr_gridnode;
				GridNode.r_calculate_directional_bindings(current, min_binding, ref min_binding_dist, self, remaining_grid_nodes, remaining_directionals);
				current.Remove(itr_directional);
				
				remaining_directionals.Insert(i_directional, itr_directional);
			}
			
			remaining_grid_nodes.Insert(i_gridnode, itr_gridnode);
		}
	}
	
	private static SPDict<Directional,GridNode> __current_calculate_directional_bindings = new SPDict<Directional, GridNode>();
	private static SPDict<Directional,GridNode> __min_calculate_directional_bindings = new SPDict<Directional, GridNode>();
	private static List<GridNode> __remaining_grid_nodes = new List<GridNode>();
	private static List<Directional> __remaining_directionals = new List<Directional>();
	
	public void calculate_directional_bindings(GridNavModal grid_nav) {
		__current_calculate_directional_bindings.Clear();
		__min_calculate_directional_bindings.Clear();
		__remaining_grid_nodes.Clear();
		__remaining_directionals.Clear();
		
		for (int i = 0; i < GridNode.all_directionals().Count; i++) {
			__remaining_directionals.Add(GridNode.all_directionals()[i]);
		}
		
		for (int i = 0; i < _node_script._links.Count; i++) {
			__remaining_grid_nodes.Add(grid_nav._id_to_gridnode[_node_script._links[i]]);
		}

		float min_dist = float.MaxValue;
		GridNode.r_calculate_directional_bindings(
			__current_calculate_directional_bindings, 
			__min_calculate_directional_bindings, 
			ref min_dist, 
			this, 
			__remaining_grid_nodes, 
			__remaining_directionals);
			
		for (int i_min = 0; i_min < __min_calculate_directional_bindings.key_itr().Count; i_min++) {
			Directional i_current_directional = __min_calculate_directional_bindings.key_itr()[i_min];
			_directional_links[i_current_directional] = __min_calculate_directional_bindings[i_current_directional];
		}
	}
	
	private enum LineState {
		ActiveSelected,
		ActiveNotSelected,
		NotSelected,
		NotAccessible
	}
	
	private void set_line_state(int id, LineState state) {
		LineProtoRoot tar = _id_to_line[id];
		if (state == LineState.NotAccessible) {
			tar._image.color = new Color(tar._image.color.r,tar._image.color.g,tar._image.color.b,0);
			tar._rect_transform.localScale = new Vector3(tar._rect_transform.localScale.x, 0, tar._rect_transform.localScale.z);
			
			tar.gameObject.SetActive(false);
			
			return;
		}
		
		tar.gameObject.SetActive(true);
		tar.set_selected(state == LineState.ActiveSelected);
		
		Color tar_color;
		float tar_height;
		if (state == LineState.ActiveSelected) {
			tar_color = new Color(1,1,1,1);
			tar_height = 1.2f;
		} else if (state == LineState.ActiveNotSelected) {
			tar_color = new Color(1,1,1,0.5f);
			tar_height = 0.85f;
		} else {
			tar_color = new Color(1,1,1,0);
			tar_height = 0.85f;
		}
		
		tar.set_tar_color(tar_color);
		tar.set_tar_height(tar_height);
	}
	
	public void i_anim_update(GameMain game, GridNavModal grid_nav) {
		if (_self_nodeanimroot.get_anim_state() == NodeAnimRoot.AnimState.Hidden) {
			for (int i = 0; i < _id_to_line.key_itr().Count; i++) {
				int itr_id = _id_to_line.key_itr()[i];
				this.set_line_state(itr_id, LineState.NotAccessible);
			}
			return;
		}
		
		for (int i = 0; i < _id_to_line.key_itr().Count; i++) {
			int itr_id = _id_to_line.key_itr()[i];
			_id_to_line[itr_id].i_update();	
		}
		_self_nodeanimroot.i_update(game, grid_nav);
	}
	
	public void i_active_update(GameMain game, GridNavModal grid_nav) {		
		if (!_accessible) {
			_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Hidden);
			
		} else if (_is_locked) {
			if (grid_nav._current_node == this || grid_nav.is_selected_node(game, this)) {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Selected);
				
			} else if (grid_nav.get_all_can_move_to_nodes().ContainsKey(_node_script._id)) {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Unselected);
				
			} else {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_NotCurNodeAccessible);
				
			}
			
		} else if (grid_nav._current_node == this && (grid_nav._current_state == GridNavModal.State.WaitingForInput || grid_nav._current_state == GridNavModal.State.NodeOpenWaitAnim)) {
			if (_visited) {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Unselected);
			} else {
				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Selected);
			}
			
		} else {
			if (grid_nav.is_selected_node(game, this)) {
				if (_visited) {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Selected);
				} else {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Selected);
				}
			} else if (grid_nav.get_all_can_move_to_nodes().ContainsKey(_node_script._id) || grid_nav._current_node == this) {
				if (_visited) {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Unselected);
				} else {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Unselected);
				}
			} else {
				if (_visited) {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_NotCurNodeAccessible);
				} else {
					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_NotCurNodeAccessible);
				}
			}
		}
		this.i_update_linestates(game,grid_nav);	
	}
	
	public void i_update_linestates(GameMain game, GridNavModal grid_nav) {
		for (int i = 0; i < _id_to_line.key_itr().Count; i++) {
			int itr_id = _id_to_line.key_itr()[i];
			if (grid_nav._current_node == this) {
				GridNode itr_target_node = grid_nav._id_to_gridnode[itr_id];
				int itr_key = itr_target_node._node_script._id;
				
				if (grid_nav.get_all_can_move_to_nodes().ContainsKey(itr_key) && grid_nav._active_gridnodes.ContainsKey(itr_key)) {
					if (grid_nav.is_selected_node(game, itr_target_node)) {
						this.set_line_state(itr_id, LineState.ActiveSelected);
					} else {
						this.set_line_state(itr_id, LineState.ActiveNotSelected);
					}
					
				} else {
					this.set_line_state(itr_id, LineState.NotSelected);
					
				}
				
			} else {
				this.set_line_state(itr_id, LineState.NotSelected);
			}
		}
	}
	
	private RectTransform __rect_transform;
	public RectTransform cached_recttransform_get() {
		if (__rect_transform == null) {
			__rect_transform = this.GetComponent<RectTransform>();
		}
		return __rect_transform;
	}
	
	public bool play_map_yes_sfx() {
		return _node_script._id != 10 && _node_script._id != GameMain.NODE_START_INDEX;
	}
	
	public enum StandAnchor {
		Solo,
		DoubleLeft,
		DoubleRight,
		TripleLeft,
		TripleCenter,
		TripleRight
	}
	
	public Vector2 get_stand_position_for_anchor(StandAnchor anchor) {
		float scale_mult;
		if (_visited) {
			scale_mult = _self_nodeanimroot._node_visited.transform.localScale.x;
		} else {
			scale_mult = _self_nodeanimroot._node_unvisited_top.transform.localScale.x;
		}
		
		Vector2 unscaled_anchor = new Vector2(0,0);
		
		switch (anchor) {
		case (StandAnchor.Solo): {
			unscaled_anchor.y = 25;
		} break;
		case (StandAnchor.DoubleLeft): {
			unscaled_anchor.x = -17;
			unscaled_anchor.y = 25;
		} break;
		case (StandAnchor.DoubleRight): {
			unscaled_anchor.x = 17;
			unscaled_anchor.y = 25;
		} break;
		case (StandAnchor.TripleLeft): {
			unscaled_anchor.x = -25;
			unscaled_anchor.y = 25;
		} break;
		case (StandAnchor.TripleCenter): {
			unscaled_anchor.x = 0;
			unscaled_anchor.y = 17;
		} break; 
		case (StandAnchor.TripleRight): {
			unscaled_anchor.x = 25;
			unscaled_anchor.y = 25;
		} break;
		default: break;
		}
		
		return SPUtil.vec_add(
			this.transform.localPosition,
			SPUtil.vec_scale(unscaled_anchor, scale_mult)
		);
	}
	
	public bool get_show_preview_chars_case(GameMain game, GridNavModal gridnav) {
		return !this._visited && (gridnav._current_node == this || gridnav.is_selected_node(game,this));
	}
	
	public bool stand_shift_case(GameMain game, GridNavModal gridnav) {
		bool moving_to_node_conditional_test = gridnav._current_state == GridNavModal.State.MovingToNode && gridnav._moving_to_node_target == this;
		bool current_node_conditional_test = gridnav._current_state != GridNavModal.State.MovingToNode && gridnav._current_node == this;
		return ((moving_to_node_conditional_test || current_node_conditional_test) && SPUtil.vec_dist(this.get_center_position(),gridnav._selector_character.transform.localPosition) < 75);
	}
	
	public Vector2 get_selector_stand_position(GameMain game, GridNavModal gridnav) {
		if (this.get_show_preview_chars_case(game,gridnav)) {
			if (_node_script._previewchars.Count > 1) {
				return this.get_stand_position_for_anchor(StandAnchor.TripleCenter);
			} else if (_node_script._previewchars.Count == 1) {
				return this.get_stand_position_for_anchor(StandAnchor.DoubleLeft);
			} else {
				return this.get_stand_position_for_anchor(StandAnchor.Solo);
			}
		} else {
			return this.get_stand_position_for_anchor(StandAnchor.Solo);
		}
	}
	
	public Vector2 get_center_position() {
		return this.transform.localPosition;
	}
	
}
