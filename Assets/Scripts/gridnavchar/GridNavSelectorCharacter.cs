using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavSelectorCharacter : MonoBehaviour {
	
	[SerializeField] private RawImage _image;
	
	private SPSpriteAnimator.RawImageTargetAdapter _image_target;
	private SPSpriteAnimator _image_animator;
	
	private RectTransform _rect_transform;
	public RectTransform get_recttransform() { return _rect_transform; }
	
	private Vector2 _last_position;
	private float _last_scale_x;
	private string _last_anim;
	private float _time_since_last_anim_update;
	
	public enum AnimMode {
		Move,
		Yay,
		Slide
	}
	public AnimMode _anim_mode;
	private bool _selected;
	
	private Vector2 _smoothed_facing = new Vector2(0,-1);
	
	public void i_initialize(GameMain game) {
		_rect_transform = this.GetComponent<RectTransform>();
		_image.texture = game._tex_resc.get_tex(RTex.KURUMI_MAP_CHAR_SS);
		_image_target = new SPSpriteAnimator.RawImageTargetAdapter() {
			_image = _image
		};
		
		_image_animator = SPSpriteAnimator.cons(_image_target)
			.add_anim("idle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_idle_00%d.png", 1, 6), 8)
			.add_anim("down", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_walk_down_00%d.png", 1, 4), 8)
			.add_anim("down_angle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_walk_down_angle_00%d.png", 1, 4), 8)
			.add_anim("side", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_walk_side_00%d.png", 1, 4), 8)
			.add_anim("up", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_walk_up_00%d.png", 1, 4), 8)
			.add_anim("up_angle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_walk_up_angle_00%d.png", 1, 4), 8)
			.add_anim("yay", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi_yay_00%d.png", 1, 5), 8, false)
			.play_anim("idle");	
		_rect_transform.sizeDelta = new Vector2(_rect_transform.sizeDelta.x, _rect_transform.sizeDelta.x * (_image_target.get_tex_rect().height / _image_target.get_tex_rect().width));
		
		_last_position = this.transform.localPosition;
		_anim_mode = AnimMode.Move;
		
		_smoothed_facing = new Vector2(0,-1);
		_last_scale_x = 1;
		_last_anim = "idle";
		_time_since_last_anim_update = 1000;
		
		_selected = false;
	}
	
	public void set_anim_mode(AnimMode mode) {
		AnimMode prev_mode = _anim_mode;
		_anim_mode = mode;
		
		if (prev_mode != _anim_mode) {
			if (_anim_mode == AnimMode.Yay) {
				_image_animator.play_anim("yay");
			} else if (_anim_mode == AnimMode.Move) {
				_image_animator.play_anim("idle");
			}
		}

	}
	
	public void set_selected(bool selected) {
		_selected = selected;
	}
	
	public void i_update(GameMain game) {
	
		if (_anim_mode == AnimMode.Yay) {
			
		} else if (_anim_mode == AnimMode.Move) {
		
			Vector2 pos_delta = SPUtil.vec_sub(this.transform.localPosition,_last_position);
			_last_position = this.transform.localPosition;
			
			bool is_idle = pos_delta.magnitude < 0.5f;
			Vector2 tar_dir;
			{
				Vector2 idle_left = new Vector2(-1,-1).normalized;
				Vector2 idle_right = new Vector2(1,-1).normalized;
			
				if (_last_scale_x < 0) {
					tar_dir = idle_left;
				} else {
					tar_dir = idle_right;
				}
			}
			
			if (!is_idle) {
				tar_dir = new Vector2(pos_delta.x, pos_delta.y).normalized;
			}
			
			float tar_dir_angle = SPUtil.dir_ang_deg(tar_dir.x, tar_dir.y);
			float facing_dir_angle = SPUtil.dir_ang_deg(_smoothed_facing.x, _smoothed_facing.y);
			
			float tar_facing_angle_delta = SPUtil.shortest_angle(facing_dir_angle,tar_dir_angle);
			
			if (is_idle) {
				if (Mathf.Abs(tar_facing_angle_delta) < 5) {
					_image_animator.play_anim("idle");
					_smoothed_facing = tar_dir;
					
				} else {
					this.character_rotate_by_delta(facing_dir_angle, tar_facing_angle_delta, pos_delta);
				}
				
			} else {
				this.character_rotate_by_delta(facing_dir_angle, tar_facing_angle_delta, pos_delta);
			}
		}
		_time_since_last_anim_update += SPUtil.dt_scale_get();
		_image_animator.i_update();
		
		if (_selected) {
			_rect_transform.localScale = SPUtil.valv(SPUtil.drpt(_rect_transform.localScale.x,1.35f,1/10.0f));
		} else {
			_rect_transform.localScale = SPUtil.valv(SPUtil.drpt(_rect_transform.localScale.x,1,1/10.0f));
		}
		
	}
	
	private void character_rotate_by_delta(float facing_dir_angle, float tar_facing_angle_delta, Vector2 pos_delta) {
		float cmp_a = facing_dir_angle + tar_facing_angle_delta * SPUtil.drpty(1/5.0f);
		float scale_x;
		string anim;
		GridNavSelectorCharacter.dir_to_anim_and_scale(cmp_a, out scale_x, out anim);
		
		_last_scale_x = scale_x;
		_smoothed_facing = SPUtil.ang_deg_dir(cmp_a);
		_image_animator.set_anim_duration(anim, SPUtil.y_for_point_of_2pt_line(new Vector2(0,10), new Vector2(10,2), Mathf.Clamp(pos_delta.magnitude,2,10)));
		
		if (_time_since_last_anim_update >= 5) {
			_image_animator.play_anim(anim);
			_image.transform.localScale = new Vector2(scale_x, 1);
			
			_time_since_last_anim_update = 0;
		}
	}
	
	
	private const float PD_8 = 360.0f / 8.0f;
	private static void dir_to_anim_and_scale(float cmp_a, out float scale_x, out string anim) {
		cmp_a = cmp_a / PD_8;
		{
			if (cmp_a > -0.5f && cmp_a < 0.5f) {
				// r
				scale_x = 1;
				anim = "side";
				
			} else if (cmp_a > 0.5f && cmp_a < 1.5f) {
				// ru
				scale_x = 1;
				anim = "up_angle";
				
			} else if (cmp_a > 1.5f && cmp_a < 2.5f) {
				// u
				scale_x = 1;
				anim = "up";
				
			} else if (cmp_a > 2.5f && cmp_a < 3.5f) {
				// lu
				scale_x = -1;
				anim = "up_angle";
				
			} else if (cmp_a > 3.5f || cmp_a < -3.5f) {
				// l
				scale_x = -1;
				anim = "side";
				
			} else if (cmp_a > -3.5f && cmp_a < -2.5f) {
				// ld
				scale_x = -1;
				anim = "down_angle";
				
			} else if (cmp_a > -2.5f && cmp_a < -1.5f) {
				// d
				scale_x = 1;
				anim = "down";
				
			} else {
				scale_x = 1;
				anim = "down_angle";
			}
		}
	}
	
	
	public static Vector2 convert_character_position_to_position_anchor_focus(Vector2 char_pos) {
		return SPUtil.vec_scale(char_pos, -1);
	}
	
}
