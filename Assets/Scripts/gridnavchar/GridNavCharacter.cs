using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavCharacter : MonoBehaviour {
	
	[SerializeField] private RawImage _image;
	
	private SPSpriteAnimator.RawImageTargetAdapter _image_target;
	private SPSpriteAnimator _image_animator;
	
	private RectTransform _rect_transform;
	
	private Vector2 _last_position;
	
	public enum AnimMode {
		Move,
		Yay
	}
	public AnimMode _anim_mode;
	
	public void i_initialize(GameMain game) {
		_rect_transform = this.GetComponent<RectTransform>();
		_image.texture = game._tex_resc.get_tex(RTex.KURUMI_MAP_CHAR_SS);
		_image_target = new SPSpriteAnimator.RawImageTargetAdapter() {
			_image = _image
		};
		
		_image_animator = SPSpriteAnimator.cons(_image_target)
			.add_anim("idle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__idle_00%d.png", 1, 5), 15)
			.add_anim("down", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__down_00%d.png", 1, 5), 15)
			.add_anim("down_angle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__down_angle_00%d.png", 1, 5), 15)
			.add_anim("side", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__side_00%d.png", 1, 5), 15)
			.add_anim("up", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__up_00%d.png", 1, 5), 15)
			.add_anim("up_angle", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__up_angle_00%d.png", 1, 5), 15)
			.add_anim("yay", game._file_cache.get_rects_list(RTex.KURUMI_MAP_CHAR_SS, "map_kurumi__yay_00%d.png", 1, 5), 10, false)
			.play_anim("idle");	
		_rect_transform.sizeDelta = new Vector2(_rect_transform.sizeDelta.x, _rect_transform.sizeDelta.x * (_image_target.get_tex_rect().height / _image_target.get_tex_rect().width));
		
		_last_position = this.transform.localPosition;
		_anim_mode = AnimMode.Move;
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
	
	public void i_update(GameMain game) {
		if (_anim_mode == AnimMode.Yay) {
			
		} else if (_anim_mode == AnimMode.Move) {
		
			Vector2 pos_delta = SPUtil.vec_sub(this.transform.localPosition,_last_position);
			_last_position = this.transform.localPosition;
			
			if (pos_delta.magnitude < 0.5f) {
				_image_animator.play_anim("idle");
				
			} else {
				
				float scale_x = 1;
				float dir_angle = SPUtil.dir_ang_deg(pos_delta.x, pos_delta.y);
				string anim = "side";
				
				float pd8 = 360.0f / 8.0f;
				float cmp_a = dir_angle / pd8;
				
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
				
				_image_animator.play_anim(anim);
				_image_animator.set_anim_duration(anim, SPUtil.y_for_point_of_2pt_line(new Vector2(0,7), new Vector2(10,3), Mathf.Clamp(pos_delta.magnitude,0,10)));
				_image.transform.localScale = new Vector2(scale_x, 1);
				
			}
		}
		
		_image_animator.i_update();
		
	}
	
	
	public static Vector2 convert_character_position_to_position_anchor_focus(Vector2 char_pos) {
		return SPUtil.vec_scale(char_pos, -1);
	}
	
}
