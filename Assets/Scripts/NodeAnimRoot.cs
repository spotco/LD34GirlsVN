using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NodeAnimRoot : MonoBehaviour {
	
	private static Color TEXT_COLOR_UNVISITED = new Color(160/255.0f,53/255.0f,148/255.0f,1);
	private static Color TEXT_COLOR_VISITED  = new Color(122/255.0f,99/255.0f,119/255.0f,1);
	
	[SerializeField] private GameObject _unvisited_root;
	[SerializeField] private GameObject _visited_root;
	[SerializeField] private Text _text;
	
	private Outline _text_outline;
	private Shadow _text_shadow;
	
	[SerializeField] private Image _node_unvisited_backspin;
	[SerializeField] private Image _node_unvisited_topexpandspin;
	[SerializeField] private Image _node_unvisited_expandback;
	[SerializeField] private Image _node_unvisited_top;
	
	private float _node_unvisited_backspin_rotation_t = 0, _node_unvisited_topexpandspin_t = 0, _node_unvisited_expandback_t = 0;
	
	[SerializeField] private Image _node_visited;
	
	public void i_initialize() {
		_text_outline = _text.GetComponent<Outline>();
		_text_shadow = _text.GetComponent<Shadow>();
		
		this.set_anim_state(AnimState.Hidden);
		_transition_state = AnimTransitionState.None;
		_anim_t = 0;
	}
	
	public enum AnimState {
		Hidden,
		Visited,
		
		Unvisited_Selected,
		Unvisited_Unselected
	}
	private AnimState _current_state;
	
	private enum AnimTransitionState {
		None,
		PopIn
	}
	private AnimTransitionState _transition_state;
	private AnimTransitionState _target_transition_state;
	private float _anim_t = 0;
	
	public void set_anim_state(AnimState target_state) {
		switch (target_state) {
		case AnimState.Hidden: {
			_unvisited_root.SetActive(false);
			_visited_root.SetActive(false);
			_text.gameObject.SetActive(false);
		
		} break;
		case AnimState.Visited: {
			_unvisited_root.SetActive(false);
			_visited_root.SetActive(true);
			_text.gameObject.SetActive(true);
			this.set_color(TEXT_COLOR_VISITED);
		
		} break;
		case AnimState.Unvisited_Selected : {		
			_unvisited_root.SetActive(true);
			_visited_root.SetActive(false);
			_text.gameObject.SetActive(true);
			this.set_color(TEXT_COLOR_UNVISITED);
		
		} break;
		case AnimState.Unvisited_Unselected: {
			_unvisited_root.SetActive(true);
			_visited_root.SetActive(false);
			_text.gameObject.SetActive(true);
			this.set_color(TEXT_COLOR_UNVISITED);
			
			if (_current_state == AnimState.Hidden) {
				_transition_state = AnimTransitionState.PopIn;
				
				_anim_t = 0;
				
				_node_unvisited_expandback.transform.localScale = SPUtil.valv(0);
				_node_unvisited_top.transform.localScale = SPUtil.valv(0);
			}
		
		} break;
		default: break;
		}
		_current_state = target_state;
	}
	
	public void i_update() {
	
		float spin_vt_scale = 1;
	
		switch (_transition_state) {
		case AnimTransitionState.PopIn: {
			_anim_t += SPUtil.sec_to_tick(1.2f);
			
			const float MODE_1_T = 0.35f;
			const float MODE_2_T = 0.7f;
			const float MODE_3_T = 1.0f;
			
			if (_anim_t < MODE_1_T) {
				_node_unvisited_backspin.gameObject.SetActive(false);
				_node_unvisited_topexpandspin.gameObject.SetActive(false);
				_node_unvisited_expandback.gameObject.SetActive(true);
				_node_unvisited_top.gameObject.SetActive(true);
				
				float mode_t = (_anim_t) / (MODE_1_T - 0);
				
				_node_unvisited_expandback.transform.localScale = SPUtil.valv(
					SPUtil.y_for_point_of_2pt_line(new Vector2(0,0), new Vector2(1,1), 
					SPUtil.bezier_val_for_t(
						new Vector2(0,0), new Vector2(1,0), new Vector2(1,0), new Vector2(1,1), mode_t).y
				));
					
				_node_unvisited_top.transform.localScale = SPUtil.valv(
					SPUtil.y_for_point_of_2pt_line(new Vector2(0,0), new Vector2(1,1), 
					SPUtil.bezier_val_for_t(
						new Vector2(0,0), new Vector2(0,1), new Vector2(0,1), new Vector2(1,1), mode_t).y
				));
				
			} else if (_anim_t < MODE_2_T) {
				_node_unvisited_backspin.gameObject.SetActive(true);
				_node_unvisited_topexpandspin.gameObject.SetActive(true);
				_node_unvisited_expandback.gameObject.SetActive(true);
				_node_unvisited_top.gameObject.SetActive(true);
				
				float mode_t = (_anim_t - MODE_1_T) / (MODE_2_T - MODE_1_T);
				
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,0.75f), new Vector2(0,1.35f), new Vector2(0.2f,1.35f), new Vector2(1,1.15f), mode_t).y
				);
				_node_unvisited_topexpandspin.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,0.75f), new Vector2(0,1.2f), new Vector2(0.2f,1.2f), new Vector2(1,1), mode_t).y
				);
				_node_unvisited_expandback.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,1), new Vector2(0.5f,1.25f), new Vector2(0.75f,1.25f), new Vector2(1,1), mode_t).y
				);
				_node_unvisited_top.transform.localScale = _node_unvisited_expandback.transform.localScale;
			
			} else {
				_node_unvisited_backspin.gameObject.SetActive(true);
				_node_unvisited_topexpandspin.gameObject.SetActive(true);
				_node_unvisited_expandback.gameObject.SetActive(true);
				_node_unvisited_top.gameObject.SetActive(true);
				
				float mode_t = (_anim_t - MODE_2_T) / (MODE_3_T - MODE_2_T);
				
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,1.15f), new Vector2(0.25f,0.6f), new Vector2(0.5f,1), new Vector2(1,1), mode_t).y
				);
				_node_unvisited_topexpandspin.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,1), new Vector2(0.25f,0.6f), new Vector2(0.5f,0.8f), new Vector2(1,0.5f), mode_t).y
				);
				_node_unvisited_expandback.transform.localScale = SPUtil.valv(
					SPUtil.bezier_val_for_t(
						new Vector2(0,1), new Vector2(0,0.6f), new Vector2(0.5f,1), new Vector2(1,1), mode_t).y
				);
				_node_unvisited_top.transform.localScale = _node_unvisited_expandback.transform.localScale;
			}
			
			if (_anim_t >= 1) {
				_transition_state = AnimTransitionState.None;
				
				_node_unvisited_backspin.gameObject.SetActive(true);
				_node_unvisited_topexpandspin.gameObject.SetActive(false);
				_node_unvisited_expandback.gameObject.SetActive(true);
				_node_unvisited_top.gameObject.SetActive(true);
				
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(1);
				_node_unvisited_expandback.transform.localScale = SPUtil.valv(1);
				_node_unvisited_top.transform.localScale = SPUtil.valv(1);
				
			}
		} break;
		case AnimTransitionState.None: {
			_node_unvisited_backspin.gameObject.SetActive(true);
			_node_unvisited_topexpandspin.gameObject.SetActive(false);
			_node_unvisited_expandback.gameObject.SetActive(true);
			_node_unvisited_top.gameObject.SetActive(true);
		
			if (_current_state == AnimState.Unvisited_Selected) {
				spin_vt_scale = 10.0f;
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(SPUtil.drpt(_node_unvisited_backspin.transform.localScale.x,1.5f,1/5.0f));
				_node_unvisited_expandback.transform.localScale = _node_unvisited_backspin.transform.localScale;
				_node_unvisited_top.transform.localScale = _node_unvisited_expandback.transform.localScale;
			
			} else {
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(SPUtil.drpt(_node_unvisited_backspin.transform.localScale.x,1,1/5.0f));
				_node_unvisited_expandback.transform.localScale = _node_unvisited_backspin.transform.localScale;
				_node_unvisited_top.transform.localScale = _node_unvisited_expandback.transform.localScale;
			}
			
		} break;
		}
		
		_node_unvisited_backspin_rotation_t = (_node_unvisited_backspin_rotation_t + 0.02f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		_node_unvisited_topexpandspin_t = (_node_unvisited_topexpandspin_t + 0.04f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		_node_unvisited_expandback_t = (_node_unvisited_expandback_t + 0.1f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		
		_node_unvisited_backspin.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_backspin.transform.localRotation, new Vector3(0,0,-_node_unvisited_backspin_rotation_t));
		_node_unvisited_topexpandspin.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_topexpandspin.transform.localRotation, new Vector3(0,0,_node_unvisited_topexpandspin_t));
		_node_unvisited_expandback.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_expandback.transform.localRotation, new Vector3(0,0,_node_unvisited_expandback_t));
		_node_unvisited_top.transform.localRotation = _node_unvisited_expandback.transform.localRotation;
		
		_node_visited.transform.localRotation = SPUtil.set_rotation_quaternion(_node_visited.transform.localRotation, new Vector3(0,0,-_node_unvisited_backspin_rotation_t));
		
		_text.transform.localScale = _node_unvisited_top.transform.localScale;
	}
	
	public void set_text(string text) {
		_text.text = text;
	}
	public void set_font(Font font) {
		_text.font = font;
	}
	private void set_color(Color color) {
		if (_text_shadow.effectColor != color) {
			_text_shadow.effectColor = color;
			_text_outline.effectColor = color;
		}	
	}	
}
