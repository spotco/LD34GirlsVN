using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SPSpriteAnimator {
	
	public class RawImageTargetAdapter : Target {
		public RawImage _image;
		private Rect _rect;
		public Rect get_tex_rect() {
			return _rect;
		}
		
		public void set_tex_rect(Rect rect) {
			_rect = rect;
			float tex_wid = _image.texture.width;
			float tex_hei = _image.texture.height;
			
			float uv_x = rect.x / tex_wid;
			float uv_y = 1.0f - ((rect.y + rect.height) / tex_hei);
			float uv_wid = rect.width / tex_wid;
			float uv_hei = rect.height / tex_hei;
			
			_image.uvRect = new Rect(uv_x,uv_y,uv_wid,uv_hei);
		}
		
		private float _rotation = 0;
		public void set_rotation(float val) {
			_rotation = val;
			_image.transform.localRotation = SPUtil.set_rotation_quaternion(_image.transform.localRotation, new Vector3(0,0,_rotation));
		}
		public float get_rotation() { 
			return _rotation;
		}
		public void set_scale(float val) {
			_image.transform.localScale = SPUtil.valv(val);
		}
		public float get_scale() { 
			return _image.transform.localScale.x; 
		}
		public void set_opacity(float val) {
			Color color = _image.color;
			color.a = val;
			_image.color = color;
		}
		public float get_opacity() { 
			return _image.color.a; 
		}
		public void set_pos(float x, float y) {
			_image.transform.localPosition = new Vector2(x,y);
		}
		public Vector2 get_pos() { 
			return _image.transform.localPosition; 
		}
		public void set_texture(Texture val) {
			_image.texture = val;
			_image.SetNativeSize();
		}
		public void set_name(string val) {
			_image.gameObject.name = val;
		}
		public void add_to_parent(Transform parent) {
			_image.transform.SetParent(parent);
		}
		public RectTransform get_recttransform() {
			return _image.rectTransform;
		}
	}
	
	public interface Target {
		void set_tex_rect(Rect rect);
		
		void set_rotation(float val);
		float get_rotation();
		void set_scale(float val);
		float get_scale();
		void set_opacity(float val);
		float get_opacity();
		void set_pos(float x, float y);
		Vector2 get_pos();
		void set_texture(Texture val);
		void set_name(string val);
		void add_to_parent(Transform parent);
		
		RectTransform get_recttransform();
	}
	
	private class SPSpriteAnimator_Animation {
		public List<Rect> _frames;
		public float _duration;
		public bool _repeating;
	}
	
	private Dictionary<string,SPSpriteAnimator_Animation> _anim_name_to_anim = new Dictionary<string,SPSpriteAnimator_Animation>();
	private Target _target;
	private float _ct;
	private int _i;
	private string _current_anim_name;
	private int _anim_i_offset;
	
	public static SPSpriteAnimator cons(Target target) {
		return (new SPSpriteAnimator()).i_cons(target);
	}
	
	public SPSpriteAnimator i_cons(Target target) {
		_target = target;
		return this;
	}
	
	public void set_target(Target tar) {
		_target = tar;
	}
	
	public SPSpriteAnimator add_anim(string name, List<Rect> frames, float duration, bool repeating = true) {
		_anim_name_to_anim[name] = new SPSpriteAnimator_Animation() {
			_frames = frames,
			_duration = duration,
			_repeating = repeating
		};
		return this;
	}
	
	public SPSpriteAnimator set_anim_i_offset(int i) {
		_anim_i_offset = i;
		return this;
	}
	
	private bool _is_finished;
	public bool is_finished() {
		return _is_finished;
	}
	public bool is_current_anim_repeating() {
		return this.current_anim()._repeating;
	}
	
	public SPSpriteAnimator play_anim(string name, bool force = false) {
		if (!_anim_name_to_anim.ContainsKey(name)) {
			SPUtil.logf("ANIM %s not found",name);
			return this;
		}
		if (_current_anim_name != name || force) {
			_current_anim_name = name;
			_i = _anim_i_offset;
			_ct = this.current_anim()._duration;
			if (_target != null) {
				_target.set_tex_rect(this.current_frame());
			}
			_is_finished = false;
		}
		return this;
	}
	
	public void set_anim_duration(string name, float duration) {
		_anim_name_to_anim[name]._duration = duration;
	}
	
	private SPSpriteAnimator_Animation current_anim() {
		return _anim_name_to_anim[_current_anim_name];
	}
	private Rect current_frame() {
		return this.current_anim()._frames[_i];
	}
	
	public void i_update() {
		if (_is_finished || _target == null || _current_anim_name == null || this.current_anim()._duration <= 0) return;
		_target.set_tex_rect(this.current_frame());
		
		_ct -= SPUtil.dt_scale_get();
		while (_ct <= 0) {
			_ct += this.current_anim()._duration;
			if (_i+1 >= this.current_anim()._frames.Count) {
				if (this.current_anim()._repeating) {
					_i = 0;
				} else {
					_is_finished = true;
					break;
				}
			} else {
				_i++;
			}
		}
	}
	
}