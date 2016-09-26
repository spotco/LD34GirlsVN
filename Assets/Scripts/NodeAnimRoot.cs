using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NodeAnimRoot : MonoBehaviour {
	
	private static Color TEXT_COLOR_UNVISITED = new Color(160/255.0f,53/255.0f,148/255.0f,1);
	private static Color TEXT_COLOR_VISITED  = new Color(122/255.0f,99/255.0f,119/255.0f,1);
	
	[SerializeField] private GameObject _unvisited_root;
	[SerializeField] private GameObject _visited_root;
	[SerializeField] private GameObject _locked_root;
	[SerializeField] private CanvasGroup _item_splat_root;
	[SerializeField] private SPText _sptext;
	
	private CanvasGroup _canvas_group;
	
	[SerializeField] private Image _node_unvisited_backspin;
	[SerializeField] private Image _node_unvisited_topexpandspin;
	[SerializeField] private Image _node_unvisited_expandback;
	[SerializeField] public Image _node_unvisited_top;
	
	private float _node_unvisited_backspin_rotation_t = 0, _node_unvisited_topexpandspin_t = 0, _node_unvisited_expandback_t = 0;
	
	[SerializeField] public Image _node_visited;
	
	[SerializeField] private Image _node_locked_chain_tl;
	[SerializeField] private Image _node_locked_chain_tr;
	[SerializeField] private Image _node_locked_chain_bl;
	[SerializeField] private Image _node_locked_chain_br;
	[SerializeField] private Image _node_locked_lock;
	
	[SerializeField] private Image _node_unlock_shine_1;
	[SerializeField] private Image _node_unlock_shine_2;
	[SerializeField] private Image _unlock_particle_proto;
	
	[SerializeField] public Image _item_icon;
	private float _item_splat_root_rotation_theta;
	
	public void i_initialize() {
		_canvas_group = this.GetComponent<CanvasGroup>();
		
		_transition_state = AnimTransitionState.None;
		_anim_t = 0;
		
		_sptext.i_cons_text(
			RTex.OSAKA_FNT, 
			RFnt.OSAKA, 
			SPText.SPTextStyle.cons(
				SPUtil.color_from_bytes(160,53,148,255),
				SPUtil.color_from_bytes(255,255,255,255), 
				SPUtil.color_from_bytes(160,53,148,255), 
				0, 
				0
			)
		);
		_sptext.set_text_anchor(0.5f, 0.5f);
		_sptext.set_horiz_center_text(true);
		
		this.set_anim_state(AnimState.Hidden);
	}
	
	public enum AnimState {
		None,
		Hidden,
		Visited_Selected,
		Visited_Unselected,
		
		Unvisited_Selected,
		Unvisited_Unselected,
		
		Visited_NotCurNodeAccessible,
		Unvisited_NotCurNodeAccessible,
		
		Locked_Unselected,
		Locked_Selected,
		Locked_NotCurNodeAccessible
	}
	private AnimState _current_state;
	public AnimState get_anim_state() { return _current_state; }
	
	public enum AnimTransitionState {
		None,
		PopIn,
		RemoveLock
	}
	private AnimTransitionState _transition_state;
	public AnimTransitionState get_transition_state() { return _transition_state; }
	
	private float _anim_t = 0;
	
	public void set_anim_state(AnimState target_state) {
		switch (target_state) {
		case AnimState.Hidden: {
			_unvisited_root.SetActive(false);
			_visited_root.SetActive(false);
			_locked_root.SetActive(false);
			this.set_text_visible(false);
		
		} break;
		case AnimState.Visited_Selected:
		case AnimState.Visited_NotCurNodeAccessible:
		case AnimState.Visited_Unselected: {
			_unvisited_root.SetActive(false);
			_visited_root.SetActive(true);
			_locked_root.SetActive(false);
			
			this.set_text_visible(true);
			this.set_color(TEXT_COLOR_VISITED);
			
		} break;
		
		case AnimState.Unvisited_NotCurNodeAccessible:
		case AnimState.Unvisited_Selected : {		
			_unvisited_root.SetActive(true);
			_visited_root.SetActive(false);
			_locked_root.SetActive(false);
			
			this.set_text_visible(true);
			this.set_color(TEXT_COLOR_UNVISITED);
		
		} break;
		case AnimState.Unvisited_Unselected: {
			_unvisited_root.SetActive(true);
			_visited_root.SetActive(false);
			_locked_root.SetActive(false);
			
			this.set_text_visible(true);
			this.set_color(TEXT_COLOR_UNVISITED);
		
		} break;
		case AnimState.Locked_Selected:
		case AnimState.Locked_NotCurNodeAccessible:
		case AnimState.Locked_Unselected: {
			_unvisited_root.SetActive(false);
			_visited_root.SetActive(false);
			_locked_root.SetActive(true);
			
			this.set_text_visible(false);
			
			_transition_state = AnimTransitionState.None;
			
		} break;
		default: break;
		}
		
		_item_splat_root.gameObject.SetActive(_locked_root.activeSelf);
		if (!_item_splat_root.gameObject.activeSelf) {
			_item_splat_root.transform.localScale = SPUtil.valv(0);
			_item_splat_root.alpha = 0;
		}
		
		_current_state = target_state;
	}
	
	public void set_transition_state(AnimTransitionState state) {
		switch(state) {
		case (AnimTransitionState.PopIn):{
			_transition_state = AnimTransitionState.PopIn;
			_anim_t = 0;
		} break;
		case (AnimTransitionState.RemoveLock):{
			_transition_state = AnimTransitionState.RemoveLock;
			_anim_t = 0;
		} break;
		default: break;
		}
	}
	
	private List<Image> __all_lock_chains = null;
	private List<Image> all_lock_chains() {
		if (__all_lock_chains == null) {
			__all_lock_chains = new List<Image>() { _node_locked_chain_bl, _node_locked_chain_br, _node_locked_chain_tl, _node_locked_chain_tr };
		}
		return __all_lock_chains;
	}
	
	public void i_update(GameMain game, GridNavModal grid_nav) {
		_sptext.i_update();
		
		float spin_vt_scale = 1;
		float tar_alpha = 1;
		bool sync_text_to_node_unvisited_top = true;
	
		switch (_transition_state) {
		case AnimTransitionState.RemoveLock: {
			
			if (game._controls.get_debug_skip()) {
				_anim_t = 1;
			}
			
			const float TOTAL_ANIM_TIME = 2.5f;
			float frame_tick = SPUtil.sec_to_tick(TOTAL_ANIM_TIME) * SPUtil.dt_scale_get();
			
			const float MODE_1_T = 0.65f;
			const float MODE_2_T = 0.95f;
			
			if (_anim_t < MODE_1_T) {
				if (_anim_t <= 0) {
					Vector2 center_pos = SPUtil.canvas_recttransform_relative_transform_from_to_pctfrom(_node_locked_lock.rectTransform, grid_nav._particles.get_parent(), 0.5f, 0.5f);
					grid_nav._particles.add_particle(SPConfigAnimParticle.cons()
						.set_ctmax(SPUtil.sec_to_ct(TOTAL_ANIM_TIME * (MODE_1_T - 0)))
						.set_texture(game._tex_resc.get_tex(RTex.NODE_UNLOCK_SHINE_1))
						.set_pos(center_pos.x,center_pos.y)
						.set_alpha(0.1f, 0.95f)
						.set_scale(0.25f,0.45f)
					);
				}
			
				float mode_t = (_anim_t - 0) / (MODE_1_T - 0);
				float tar_hei_pct = 1 - mode_t;
				
				for (int i = 0; i < this.all_lock_chains().Count; i++) {
					Image itr_chain = this.all_lock_chains()[i];
					
					itr_chain.fillAmount = tar_hei_pct;
					
					Vector2 itr_pos_tl = SPUtil.canvas_recttransform_relative_transform_from_to_pctfrom(itr_chain.rectTransform, grid_nav._particles.get_parent(), 0, tar_hei_pct);
					Vector2 itr_pos_tr = SPUtil.canvas_recttransform_relative_transform_from_to_pctfrom(itr_chain.rectTransform, grid_nav._particles.get_parent(), 1, tar_hei_pct);
					
					Vector2 spawn_pos = Vector2.Lerp(itr_pos_tl, itr_pos_tr, SPUtil.float_random(0,1));
					
					grid_nav._particles.add_particle(SPConfigAnimParticle.cons()
						.set_ctmax(15)
						.set_texture(game._tex_resc.get_tex(RTex.NODE_UNLOCK_PARTICLE))
						.set_pos(spawn_pos.x,spawn_pos.y)
						.set_vel(SPUtil.float_random(-0.5f,0.5f),SPUtil.float_random(-0.5f,0.5f))
						.set_alpha(0.95f, 0)
						.set_vr(SPUtil.float_random(-10,10))
						.set_rotation(SPUtil.float_random(-180,180))
						.set_scale(0.25f,0.25f)
					);
				}
				
				if (_anim_t + frame_tick >= MODE_1_T) {
					{
						Vector2 center_pos = SPUtil.canvas_recttransform_relative_transform_from_to_pctfrom(_node_locked_lock.rectTransform, grid_nav._particles.get_parent(), 0.5f, 0.5f);
						grid_nav._particles.add_particle(SPConfigAnimParticle.cons()
							.set_ctmax(SPUtil.sec_to_ct(TOTAL_ANIM_TIME * (MODE_2_T - MODE_1_T)) + 10)
							.set_texture(game._tex_resc.get_tex(RTex.NODE_UNLOCK_SHINE_2))
							.set_pos(center_pos.x,center_pos.y)
							.set_alpha(0.95f, 0.25f)
							.set_scale(0.45f,0.30f)
						);
					}
					
					for (int i = 0; i < 30; i++) {
						float rad = SPUtil.float_random(-3.14f, 3.14f);
						Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
						
						Vector2 spawn_dir = SPUtil.vec_add(dir, new Vector2(0.5f,0.5f));
						Vector2 spawn_pos = SPUtil.canvas_recttransform_relative_transform_from_to_pctfrom(_node_locked_lock.rectTransform, grid_nav._particles.get_parent(), spawn_dir.x, spawn_dir.y);
						
						dir = SPUtil.vec_scale(dir, SPUtil.float_random(0.25f, 1.75f));
						
						grid_nav._particles.add_particle(SPConfigAnimParticle.cons()
							.set_ctmax(35)
							.set_texture(game._tex_resc.get_tex(RTex.NODE_UNLOCK_PARTICLE))
							.set_pos(spawn_pos.x,spawn_pos.y)
							.set_vel(dir.x, dir.y)
							.set_alpha(0.95f, 0f)
							.set_vr(SPUtil.float_random(-10,10))
							.set_rotation(SPUtil.float_random(-180,180))
							.set_scale(0.5f,0.85f)
						);
					}
					
					_node_locked_lock.gameObject.SetActive(false);
					for (int i = 0; i < this.all_lock_chains().Count; i++) {
						this.all_lock_chains()[i].gameObject.SetActive(false);
						this.all_lock_chains()[i].fillAmount = 1;
					}
					_unvisited_root.SetActive(true);
					_visited_root.SetActive(false);
					_locked_root.SetActive(false);
					
					this.set_text_visible(true);
					
					sync_text_to_node_unvisited_top = false;
					this.set_text_scale(SPUtil.valv(0.5f));
					
				}
			} else if (_anim_t < MODE_2_T) {
				sync_text_to_node_unvisited_top = false;
				float mode_t = (_anim_t - MODE_1_T) / (MODE_2_T - MODE_1_T);
				this.set_text_scale(SPUtil.valv(SPUtil.bezier_val_for_t(
					new Vector2(0,0.5f),
					new Vector2(0,1),
					new Vector2(0.5f,_node_unvisited_top.transform.localScale.x * 1.25f),
					new Vector2(1,_node_unvisited_top.transform.localScale.x),
					mode_t
				).y));
				
			} else if (_anim_t >= 1) {
				_current_state = AnimState.Unvisited_Selected;
				_transition_state = AnimTransitionState.None;
			}
			
			_anim_t += frame_tick;
			
		} break;
		case AnimTransitionState.PopIn: {
			_anim_t += SPUtil.sec_to_tick(0.85f) * SPUtil.dt_scale_get();
			
			const float MODE_1_T = 0.35f;
			const float MODE_2_T = 0.7f;
			const float MODE_3_T = 1.0f;
			
			if (_anim_t < MODE_1_T) {
				_node_unvisited_backspin.gameObject.SetActive(false);
				_node_unvisited_topexpandspin.gameObject.SetActive(false);
				_node_unvisited_expandback.gameObject.SetActive(true);
				_node_unvisited_top.gameObject.SetActive(true);
				
				float mode_t = (_anim_t - 0) / (MODE_1_T - 0);
				
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
			
			if (_current_state == AnimState.Unvisited_NotCurNodeAccessible || _current_state == AnimState.Visited_NotCurNodeAccessible || _current_state == AnimState.Locked_NotCurNodeAccessible) {
				spin_vt_scale = 0.5f;
				tar_alpha = 0.5f;
				_node_unvisited_backspin.transform.localScale = SPUtil.valv(SPUtil.drpt(_node_unvisited_backspin.transform.localScale.x,0.6f,1/5.0f));
				_node_unvisited_expandback.transform.localScale = _node_unvisited_backspin.transform.localScale;
				_node_unvisited_top.transform.localScale = _node_unvisited_expandback.transform.localScale;
			
			} else if (_current_state == AnimState.Unvisited_Selected || _current_state == AnimState.Visited_Selected || _current_state == AnimState.Locked_Selected) {
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
		
		if (_current_state == AnimState.Locked_Selected) {
			_item_splat_root.transform.localScale = SPUtil.valv(SPUtil.drpt(_item_splat_root.transform.localScale.x, 1, 1/10.0f));
			_item_splat_root.alpha = SPUtil.drpt(_item_splat_root.alpha, 1, 1/10.0f);
			_item_splat_root.transform.localPosition = new Vector3(
				SPUtil.drpt(_item_splat_root.transform.localPosition.x, 65, 1/12.5f),
				SPUtil.drpt(_item_splat_root.transform.localPosition.y, 66, 1/13.5f),
				_item_splat_root.transform.localPosition.z
			);
		} else {
			_item_splat_root.transform.localScale = SPUtil.valv(SPUtil.drpt(_item_splat_root.transform.localScale.x, 0, 1/10.0f));
			_item_splat_root.alpha = SPUtil.drpt(_item_splat_root.alpha, 0, 1/10.0f);
			_item_splat_root.transform.localPosition = new Vector3(
				SPUtil.drpt(_item_splat_root.transform.localPosition.x, 0, 1/13.5f),
				SPUtil.drpt(_item_splat_root.transform.localPosition.y, 0, 1/12.5f),
				_item_splat_root.transform.localPosition.z
			);
		}
		_item_splat_root_rotation_theta = (_item_splat_root_rotation_theta + SPUtil.dt_scale_get() * 0.045f) % (Mathf.PI * 2);
		_item_splat_root.transform.localRotation = SPUtil.set_rotation_quaternion(_item_icon.transform.localRotation, new Vector3(0,0, Mathf.Sin(_item_splat_root_rotation_theta) * 15));
		
		_node_unvisited_backspin_rotation_t = (_node_unvisited_backspin_rotation_t + 0.02f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		_node_unvisited_topexpandspin_t = (_node_unvisited_topexpandspin_t + 0.04f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		_node_unvisited_expandback_t = (_node_unvisited_expandback_t + 0.1f * spin_vt_scale * SPUtil.dt_scale_get()) % 360;
		
		_node_unvisited_backspin.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_backspin.transform.localRotation, new Vector3(0,0,-_node_unvisited_backspin_rotation_t));
		_node_unvisited_topexpandspin.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_topexpandspin.transform.localRotation, new Vector3(0,0,_node_unvisited_topexpandspin_t));
		_node_unvisited_expandback.transform.localRotation = SPUtil.set_rotation_quaternion(_node_unvisited_expandback.transform.localRotation, new Vector3(0,0,_node_unvisited_expandback_t));
		_node_unvisited_top.transform.localRotation = _node_unvisited_expandback.transform.localRotation;
		
		_node_visited.transform.localRotation = SPUtil.set_rotation_quaternion(_node_visited.transform.localRotation, new Vector3(0,0,-_node_unvisited_backspin_rotation_t));
		
		_node_visited.transform.localScale = _node_unvisited_top.transform.localScale;
		_locked_root.transform.localScale = _node_unvisited_top.transform.localScale;
		if (sync_text_to_node_unvisited_top) {
			this.set_text_scale(_node_unvisited_top.transform.localScale);
		}
		_canvas_group.alpha = SPUtil.lmovto(_canvas_group.alpha, tar_alpha, 0.05f * SPUtil.dt_scale_get());
	}
	
	private string _cached_text_str = "";
	public void set_text(string text) {
	
		text = SPTextRenderUtil.input_str_insert_linebreaks(_sptext, text, text);
	
		_cached_text_str = text;
		if (_sptext.gameObject.activeSelf) {
			_sptext.set_markup_text(_cached_text_str);
		}
	}
	private void set_color(Color color) {
		if (_sptext.get_default_style()._stroke != ((Vector4)color)) {
			_sptext.set_default_colors(color, SPUtil.color_from_bytes(255,255,255,255), color);
		}
	}
	private void set_text_scale(Vector3 scale) {
		_sptext.transform.localScale = scale;
	}
	private void set_text_visible(bool val) {
		if (val) {
			if (!_sptext.gameObject.activeSelf) {
				_sptext.set_markup_text(_cached_text_str);
			}
			_sptext.gameObject.SetActive(true);	
		} else {
			if (_sptext.gameObject.activeSelf) {
				_sptext.clear();
			}
			_sptext.gameObject.SetActive(false);
		}
	}
}
