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
	private Outline _title_ui_outline;
	[SerializeField] private Image _line_proto;
	public NodeScript _node_script = new NodeScript();
	
	private Dictionary<int,GridNode.Line> _id_to_line = new Dictionary<int, GridNode.Line>();
	
	private Sprite img_current;
	private Color color_current = new Color(191/255.0f,67/255.0f,0/255.0f,1);
	private Sprite img_unvisited;
	private Color color_unvisited = new Color(159/255.0f,69/255.0f,255/255.0f,1);
	private Sprite img_visited;
	private Color color_visited  = new Color(113/255.0f,113/255.0f,113/255.0f,1);
	
	private bool _visited;
	
	public void i_initialize() {
		_node_script.i_initialize(_node_script_text);
		
		_title_ui_outline = _title_ui_text.GetComponent<Outline>();
		
		img_current = Resources.Load<Sprite>("img/grid/node_current");
		img_unvisited = Resources.Load<Sprite>("img/grid/node_visited");
		img_visited = Resources.Load<Sprite>("img/grid/node_unvisited");
		
		this.gameObject.name = SPUtil.sprintf("Node (%d)",_node_script._id);
		_title_ui_text.text = _node_script._title;
		
		_visited = false;
	}
	
	public void post_initialize(GridNavModal grid_nav) {
		for (int i = 0; i < _node_script._links.Count; i++) {
			int itr_id = _node_script._links[i];
			
			GridNode other_node = grid_nav._id_to_gridnode[itr_id];
			Vector3 lpos_delta = SPUtil.vec_sub(other_node.transform.localPosition,this.transform.localPosition);
			
			Image neu_line = SPUtil.proto_clone(_line_proto.gameObject).GetComponent<Image>();
			neu_line.GetComponent<RectTransform>().sizeDelta = new Vector2(lpos_delta.magnitude, neu_line.GetComponent<RectTransform>().sizeDelta.y);
			
			neu_line.transform.localEulerAngles = new Vector3(0,0,SPUtil.dir_ang_deg(lpos_delta.x,lpos_delta.y));
			neu_line.transform.parent = grid_nav._line_root;
			
			_id_to_line[itr_id] = new GridNode.Line() {
				_image = neu_line,
				_rect_transform = neu_line.GetComponent<RectTransform>()
			};
		}
	}
	
	private enum LineState {
		ActiveSelected,
		ActiveNotSelected,
		NotSelected
	}
	
	private void set_line_state(int id, LineState state) {
		Line tar = _id_to_line[id];
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
		GridNode tar_selected = grid_nav.get_selection_list()[grid_nav._selected_node_cursor_index];
		foreach (int itr_id in _id_to_line.Keys) {
			if (grid_nav._current_node != this) {
				this.set_line_state(itr_id, LineState.NotSelected);
			} else {
				if (tar_selected._node_script._id == itr_id) {
					this.set_line_state(itr_id, LineState.ActiveSelected);
				} else {
					this.set_line_state(itr_id, LineState.ActiveNotSelected);
				}
			}
		}
		
		if (grid_nav._current_node == this && !_visited) {
			_visited = true;
			game.start_event_modal(_node_script);
		}
		
		float tar_scale = 1;
		if (grid_nav._current_node == this) {
			_image.sprite = img_current;
			tar_scale = 1;
			SPUtil.set_outline_effect_color(_title_ui_outline,color_current);
			
		} else if (tar_selected == this) {
			if (_visited) {
				_image.sprite = img_visited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_visited);
			} else {
				_image.sprite = img_unvisited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_unvisited);
			}
			tar_scale = 1.2f;
		
		} else {
			if (_visited) {
				_image.sprite = img_visited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_visited);
			} else {
				_image.sprite = img_unvisited;
				SPUtil.set_outline_effect_color(_title_ui_outline,color_unvisited);
			}
			tar_scale = 0.85f;
			
		}
		this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,tar_scale,1/10.0f));
	}
	
	
}
