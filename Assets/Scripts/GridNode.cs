using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNode : MonoBehaviour {

	public class Line {
		public Image _image;
		public RectTransform _rect_transform;
	}

	[SerializeField] private TextAsset _node_script_text;
	[SerializeField] private Image _image;
	[SerializeField] private Text _title_ui_text;
	private float _anim_theta;
	private Outline _title_ui_outline;
	[SerializeField] private Image _line_proto;
	public NodeScript _node_script = new NodeScript();
	
	[SerializeField] private Image _lock_icon;
	[SerializeField] private Image _lock_item_icon;
	public bool _is_locked;
	
	private Dictionary<int,GridNode.Line> _id_to_line = new Dictionary<int, GridNode.Line>();
	
	private Sprite img_current;
	private Color color_current = new Color(191/255.0f,67/255.0f,0/255.0f,1);
	private Sprite img_unvisited;
	private Color color_unvisited = new Color(159/255.0f,69/255.0f,255/255.0f,1);
	private Sprite img_visited;
	private Color color_visited  = new Color(113/255.0f,113/255.0f,113/255.0f,1);
	
	public bool _accessible;
	public bool _visited;
	
	private static Font __cached_font;
	
	public void i_initialize(GameMain game, GridNavModal grid_nav) {
		_node_script.i_initialize(game,_node_script_text);
		
		if (__cached_font == null) {
			__cached_font = Resources.Load<Font>("osaka.unicode");
		}
		_title_ui_text.font = __cached_font;
		
		_title_ui_outline = _title_ui_text.GetComponent<Outline>();
		
		img_current = Resources.Load<Sprite>("img/grid/node_current");
		img_unvisited = Resources.Load<Sprite>("img/grid/node_visited");
		img_visited = Resources.Load<Sprite>("img/grid/node_unvisited");
		
		this.gameObject.name = SPUtil.sprintf("Node (%d)",_node_script._id);
		_title_ui_text.text = _node_script._title;
		
		_lock_icon.gameObject.SetActive(false);
		_lock_item_icon.gameObject.SetActive(false);
		if (_node_script._affinity_requirement) {
			_is_locked = false;
			_lock_icon.sprite = Resources.Load<Sprite>("img/ui/affinity_heart");
			_lock_icon.SetNativeSize();
			
		} else {
			_is_locked = _node_script._requirement_items.Count > 0;
			if (_is_locked) {
				_lock_item_icon.sprite = game._inventory.icon_for_item(_node_script._requirement_items[0]);
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
			-this.transform.localPosition.x,
			-this.transform.localPosition.y
		);
		
		if (_visited) {
			for (int i = 0; i < this._node_script._links.Count; i++) {
				GridNode itr_node = grid_nav._id_to_gridnode[this._node_script._links[i]];
				if (!itr_node._visited) {
					tar_pos.x = SPUtil.running_avg(tar_pos.x, -itr_node.transform.localPosition.x, i+2);
					tar_pos.y = SPUtil.running_avg(tar_pos.y, -itr_node.transform.localPosition.y, i+2);
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
			
			Image neu_line = SPUtil.proto_clone(_line_proto.gameObject).GetComponent<Image>();
			neu_line.GetComponent<RectTransform>().sizeDelta = new Vector2(lpos_delta.magnitude, neu_line.GetComponent<RectTransform>().sizeDelta.y);
			
			neu_line.transform.localEulerAngles = new Vector3(0,0,SPUtil.dir_ang_deg(lpos_delta.x,lpos_delta.y));
			neu_line.transform.SetParent(grid_nav._line_root);
			
			neu_line.color = new Color(
				neu_line.color.r,
				neu_line.color.g,
				neu_line.color.b,
				0
			);
			
			_id_to_line[itr_id] = new GridNode.Line() {
				_image = neu_line,
				_rect_transform = neu_line.GetComponent<RectTransform>()
			};
		}
	}
	
	private enum LineState {
		ActiveSelected,
		ActiveNotSelected,
		NotSelected,
		NotAccessible
	}
	
	private void set_line_state(int id, LineState state) {
		Line tar = _id_to_line[id];
		if (state == LineState.NotAccessible) {
			tar._image.color = new Color(tar._image.color.r,tar._image.color.g,tar._image.color.b,0);
			tar._rect_transform.sizeDelta = new Vector2(tar._rect_transform.sizeDelta.x,0);
			return;
		}
		
		Color tar_color;
		float tar_height;
		if (state == LineState.ActiveSelected) {
			tar_color = new Color(1,1,1,1);
			tar_height = 10;
		} else if (state == LineState.ActiveNotSelected) {
			tar_color = new Color(1,1,1,0.3f);
			tar_height = 5;
		} else {
			tar_color = new Color(1,1,1,0);
			tar_height = 5;
		}
		tar._image.color = new Color(
			SPUtil.drpt(tar._image.color.r,tar_color.r,1/10.0f),
			SPUtil.drpt(tar._image.color.g,tar_color.g,1/10.0f),
			SPUtil.drpt(tar._image.color.b,tar_color.b,1/10.0f),
			SPUtil.drpt(tar._image.color.a,tar_color.a,1/10.0f)
		);
		tar._rect_transform.sizeDelta = new Vector2(
			tar._rect_transform.sizeDelta.x,
			SPUtil.drpt(tar._rect_transform.sizeDelta.y,tar_height,1/10.0f)
		); 
	}
	
	public void i_update(GameMain game, GridNavModal grid_nav) {
		foreach (int itr_id in _id_to_line.Keys) {
			if (grid_nav._selected_node != this) {
				this.set_line_state(itr_id, LineState.NotSelected);
			} else {
				if (!_accessible || !grid_nav._id_to_gridnode[itr_id]._accessible) {
					this.set_line_state(itr_id, LineState.NotAccessible);
					
				} else if (grid_nav._selected_node._visited) {
					if (_unidirectional_reverse_links.Contains(itr_id)) {
						this.set_line_state(itr_id, LineState.NotSelected);
					} else {
						if (SPUtil.rect_transform_contains_screen_point(grid_nav._id_to_gridnode[itr_id].cached_recttransform_get(),game._controls.get_touch_pos())) {
							this.set_line_state(itr_id, LineState.ActiveSelected);
						} else {
							this.set_line_state(itr_id, LineState.ActiveNotSelected);
						}
					}
				} else {
					if (grid_nav._id_to_gridnode[itr_id]._visited) {
						this.set_line_state(itr_id, LineState.ActiveSelected);
					} else {
						this.set_line_state(itr_id, LineState.NotSelected);
					}
				}
			}
		}
		
		
		float tar_scale = 1;
		if (grid_nav._selected_node == this) {
			this.gameObject.SetActive(true);
			if (_visited) {
				_image.sprite = img_visited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_visited);
				_title_ui_text.transform.localEulerAngles = new Vector3(0,0,0);
				
			} else {
				_image.sprite = img_unvisited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_unvisited);
				_anim_theta += 0.05f;
				_title_ui_text.transform.localEulerAngles = new Vector3(0,0,Mathf.Sin(_anim_theta)*7.5f);
				
			}
			if (_visited) {
				tar_scale = 1.0f;
			} else {
				tar_scale = 1.35f;
			}
			
			
		
		} else {
			if (!_accessible) {
				this.gameObject.SetActive(false);
			} else if (_visited) {
				this.gameObject.SetActive(true);
				_image.sprite = img_visited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_visited);
			} else {
				this.gameObject.SetActive(true);
				_image.sprite = img_unvisited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_unvisited);
			}
			
			if (grid_nav.__cached_selection_list.Contains(this) && SPUtil.rect_transform_contains_screen_point(this.cached_recttransform_get(),game._controls.get_touch_pos())) {
				tar_scale = 1.1f;
			} else {
				tar_scale = 0.85f;
			}
			
			_title_ui_text.transform.localEulerAngles = new Vector3(0,0,0);
			
		}
		this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,tar_scale,1/10.0f));
		
		if (!_visited) {
			if (_node_script._affinity_requirement) {
				_lock_icon.gameObject.SetActive(true);
				_lock_item_icon.gameObject.SetActive(false);
				_title_ui_text.gameObject.SetActive(false);
			
			} else if (_is_locked) {
				_lock_icon.gameObject.SetActive(true);
				_lock_item_icon.gameObject.SetActive(true);
				_title_ui_text.gameObject.SetActive(false);
			} else {
				_lock_icon.gameObject.SetActive(false);
				_lock_item_icon.gameObject.SetActive(false);
				_title_ui_text.gameObject.SetActive(true);
			}
		} else {
			_lock_icon.gameObject.SetActive(false);
			_lock_item_icon.gameObject.SetActive(false);
			_title_ui_text.gameObject.SetActive(true);
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
		return _node_script._id != 10;
	}
	
	
}
