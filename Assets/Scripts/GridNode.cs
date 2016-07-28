using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNode : MonoBehaviour {

	[SerializeField] private TextAsset _node_script_text;
	private float _anim_theta;
	
	public NodeScript _node_script = new NodeScript();
	
	private NodeAnimRoot _self_nodeanimroot;
	public bool _is_locked;
	private Dictionary<int,LineProtoRoot> _id_to_line = new Dictionary<int, LineProtoRoot>();
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
		Right
	}
	private static Vector2 directional_to_vector(Directional input) {
		switch (input) {
		case Directional.Up: return new Vector2(0,1);
		case Directional.Down: return new Vector2(0,-1);
		case Directional.Left: return new Vector2(-1,0);
		case Directional.Right: return new Vector2(1,0);
		default: return Vector2.zero;
		}
	}
	private static Directional inverse_directional(Directional input) {
		switch (input) {
		case Directional.Up: return Directional.Down;
		case Directional.Down: return Directional.Up;
		case Directional.Left: return Directional.Right;
		case Directional.Right: return Directional.Left;
		default: return Directional.Up;
		}
	}
	
	private SPDict<Directional, GridNode> _directional_links = new SPDict<Directional, GridNode>();
	public bool has_directional_link(Directional dir) {
		return _directional_links.ContainsKey(dir);
	}
	public GridNode get_directional_link(Directional dir) {
		if (!this.has_directional_link(dir)) return null;
		return _directional_links[dir];
	}
	
	public void calculate_directional_bindings(GridNavModal grid_nav) {
		List<GridNode> remaining_grid_nodes = new List<GridNode>();
		for (int i = 0; i < _node_script._links.Count; i++) {
			remaining_grid_nodes.Add(grid_nav._id_to_gridnode[_node_script._links[i]]);
		}
		
		List<Directional> remaining_directionals = new List<Directional>() { Directional.Up, Directional.Down, Directional.Left, Directional.Right };
		
		for (int i = 0; i < _directional_links.key_itr().Count; i++) {
			Directional itr_directional = _directional_links.key_itr()[i];
			GridNode itr_grid_node = _directional_links[itr_directional];
			remaining_directionals.Remove(itr_directional);
			remaining_grid_nodes.Remove(itr_grid_node);
		}
		
		// need to do minimize-sum-of-min-distances algorithm here
		float limit_max = 20;
		while (remaining_grid_nodes.Count > 0 && remaining_directionals.Count > 0) {
			for (int i_directional = remaining_directionals.Count-1; i_directional >= 0; i_directional--) {
				Directional itr_directional = remaining_directionals[i_directional];
				Vector2 itr_directional_dir = GridNode.directional_to_vector(itr_directional);
				
				GridNode found_grid_node = null;
				float min_angle_found = limit_max + 1;
				
				for (int i_grid_node = 0; i_grid_node < remaining_grid_nodes.Count; i_grid_node++) {
					GridNode itr_grid_node = remaining_grid_nodes[i_grid_node];
					Vector2 cur_to_itr_dir = SPUtil.vec_sub(itr_grid_node.get_center_position(),this.get_center_position()).normalized;
					float cmp_angle = SPUtil.rad_to_deg(Mathf.Acos(SPUtil.vec_dot(itr_directional_dir,cur_to_itr_dir)));
					
					if (cmp_angle < min_angle_found) {
						found_grid_node = itr_grid_node;
						min_angle_found = cmp_angle;
					}
				}
				
				if (found_grid_node != null && min_angle_found < limit_max) {
					remaining_directionals.Remove(itr_directional);
					remaining_grid_nodes.Remove(found_grid_node);
					
					_directional_links[itr_directional] = found_grid_node;
					
					Directional itr_directional_inverse = GridNode.inverse_directional(itr_directional);
					if (found_grid_node._node_script._links.Contains(_node_script._id) && !found_grid_node._directional_links.ContainsKey(itr_directional_inverse)) {
						found_grid_node._directional_links[GridNode.inverse_directional(itr_directional)] = this;
					}
				}
			}
			limit_max += 10;
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
		tar.i_update(state == LineState.ActiveSelected);
		
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
		
		tar._image.color = new Color(
			tar_color.r, tar_color.g, tar_color.b, SPUtil.lmovto(tar._image.color.a, tar_color.a, 0.05f * SPUtil.dt_scale_get())
		);
		
		tar._rect_transform.localScale = new Vector3(
			tar._rect_transform.localScale.x, 
			SPUtil.drpt(tar._rect_transform.localScale.y,tar_height,1/10.0f), 
			tar._rect_transform.localScale.z
		);
	}
	
	public void i_update(GameMain game, GridNavModal grid_nav) {
		foreach (int itr_id in _id_to_line.Keys) {
			if (grid_nav._current_node != this) {
				this.set_line_state(itr_id, LineState.NotSelected);
			} else {
				this.set_line_state(itr_id, LineState.ActiveSelected);
			}
		}
		
		if (grid_nav._current_node == this) {
			_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Selected);
		} else {
			_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Unselected);
		}
		
		_self_nodeanimroot.i_update();
	
//		foreach (int itr_id in _id_to_line.Keys) {
//			if (grid_nav._selected_node != this) {
//				this.set_line_state(itr_id, LineState.NotSelected);
//			} else {
//				if (!_accessible || !grid_nav._id_to_gridnode[itr_id]._accessible) {
//					this.set_line_state(itr_id, LineState.NotAccessible);
//					
//				} else if (grid_nav._selected_node._visited) {
//					if (_unidirectional_reverse_links.Contains(itr_id)) {
//						this.set_line_state(itr_id, LineState.NotSelected);
//					} else {
//						if (SPUtil.rect_transform_contains_screen_point(grid_nav._id_to_gridnode[itr_id].cached_recttransform_get(),game._controls.get_touch_pos())) {
//							this.set_line_state(itr_id, LineState.ActiveSelected);
//						} else {
//							this.set_line_state(itr_id, LineState.ActiveNotSelected);
//						}
//					}
//				} else {
//					if (_visited) {
//						if (grid_nav._id_to_gridnode[itr_id]._visited) {
//							this.set_line_state(itr_id, LineState.ActiveSelected);
//						} else {
//							this.set_line_state(itr_id, LineState.NotSelected);
//						}
//					} else {
//						if (grid_nav._id_to_gridnode[itr_id]._node_script._id == grid_nav._current_node._node_script._id) {
//							this.set_line_state(itr_id, LineState.ActiveNotSelected);
//						} else {
//							this.set_line_state(itr_id, LineState.NotAccessible);
//						}
//					}
//				}
//			}
//		}
//		
//		bool is_touching = SPUtil.rect_transform_contains_screen_point(this.cached_recttransform_get(),game._controls.get_touch_pos());
//		bool cur_node_accessible = grid_nav._current_node._node_script._links.Contains(_node_script._id);
//		bool selected_node_is_current_node = grid_nav._selected_node == grid_nav._current_node;
//		bool this_is_current_node = this == grid_nav._current_node;
//		bool selected_node_accessible = grid_nav._selected_node == null ? false : 
//			(((grid_nav._selected_node._node_script._links.Contains(_node_script._id) || grid_nav._selected_node._unidirectional_reverse_links.Contains(_node_script._id)
//			) && grid_nav._selected_node._visited) || this_is_current_node);
//		
//		bool use_accessible = selected_node_is_current_node ? (cur_node_accessible) : (selected_node_accessible);
//		
//		bool is_active_locked = _is_locked && !_visited;
//		
//		if (grid_nav._selected_node == this || (this_is_current_node && is_touching)) {
//			if (is_active_locked) {
//				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Selected);
//				
//			} else if (_visited) {
//				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Selected);
//				
//			} else {
//				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Selected);
//				
//			}
//			
//		} else {
//			if (!_accessible) {
//				_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Hidden);
//				
//			} else if (_visited) {
//				if (is_touching && use_accessible) {
//					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Selected);
//				} else {
//					if (use_accessible) {
//						_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_Unselected);
//					} else {
//						_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Visited_NotCurNodeAccessible);
//					}
//				}
//				
//			} else {
//				if (is_touching && is_active_locked && use_accessible) {
//					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Selected);
//					
//				} else if (is_touching && use_accessible) {
//					_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Selected);
//					
//				} else {
//					if (is_active_locked) {
//						if (use_accessible) {
//							_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_Unselected);
//						} else {
//							_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Locked_NotCurNodeAccessible);
//						}
//					} else if (use_accessible) {
//						_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_Unselected);
//					} else {
//						_self_nodeanimroot.set_anim_state(NodeAnimRoot.AnimState.Unvisited_NotCurNodeAccessible);
//
//					}
//				}
//			}
//		}
//		
//		_self_nodeanimroot.i_update();
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
	
	public Vector2 get_selector_stand_position() {
		return SPUtil.vec_add(this.transform.localPosition, new Vector2(0, 40));
	}
	
	public Vector2 get_center_position() {
		return this.transform.localPosition;
	}
	
}
