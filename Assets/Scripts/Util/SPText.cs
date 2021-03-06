﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class SPText : MonoBehaviour {

	public class SPTextCharacter :  GenericPooledObject {
		public static SPTextCharacter cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			return ObjectPool.inst().generic_depool<SPTextCharacter>().i_cons_texkey_rect(texkey,rect, style);
		}
		private PooledSpriteImageTarget _pooled_img_target;
		private SPSpriteAnimator.Target _img;
		private string _texkey;
		private float _opacity;
		private SPText.SPTextStyle _style;
		private Vector2 _start_pos;
		
		private float _animate_in_t;
		
		public int _line_index = 0;
		
		public void add_to_parent(Transform parent) {
			_img.add_to_parent(parent); 
		}
		
		private static string SPTEXTCHARACTER_SPRITEKEY = "SPTextCharacter";
		
		public void depool() {
			_pooled_img_target = ObjectPool.inst().spbasebehavior_depool<PooledSpriteImageTarget>(true,SPTEXTCHARACTER_SPRITEKEY);
			if (_pooled_img_target.gameObject.GetComponent<Shadow>() == null) {
				Shadow shadow = _pooled_img_target.gameObject.AddComponent<Shadow>();
				shadow.effectDistance = new Vector2(2,-2);
			}
		}
		public void repool() {
			_pooled_img_target.get_image().material = null;
			ObjectPool.inst().spbasebehavior_repool<PooledSpriteImageTarget>(_pooled_img_target,SPTEXTCHARACTER_SPRITEKEY);
			
			_pooled_img_target = null;
			_img = null;
		}
		public void cleanup() {
			ObjectPool.inst().generic_repool<SPTextCharacter>(this);
		}
		
		private SPTextCharacter i_cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			_texkey = texkey;
			_img = _pooled_img_target.get_target(GameMain._context._tex_resc.get_tex(texkey));
			
			_img.set_texture(GameMain._context._tex_resc.get_tex(texkey));
			_img.set_tex_rect(rect);
			_img.get_recttransform().sizeDelta = new Vector2(rect.width, rect.height);
			
			
			_opacity = 1;
			_alpha_mult = 1;
			_start_pos = Vector2.zero;
			this.set_style(style);
			_animate_in_t = 0;
			this.i_update_animate_text_in(_animate_in_t);
			
			return this;
		}
		public void i_update(float time) {
			if (_animate_in_t < 1) {
				this.i_update_animate_text_in(_animate_in_t);
				_animate_in_t = Mathf.Clamp(_animate_in_t + SPUtil.sec_to_tick(0.15f) * SPUtil.dt_scale_get(), 0, 1);
				
			} else {
				this.set_opacity(1);
				_img.set_scale(1);
				_img.set_pos(
					_start_pos.x,
					_start_pos.y + _style._amplitude * Mathf.Sin(time)
				);
			}
		}
		
		private void i_update_animate_text_in(float anim_t) {		
			this.set_opacity(SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0.5f,1), new Vector2(0.5f,1), new Vector2(1,1),anim_t).y);
			
			float y_offset = 0;
			if (anim_t < 0.5f) {
				y_offset = SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0.5f,0), new Vector2(0.5f,1), new Vector2(0.65f,0), anim_t / 0.5f).y;
			} else {
				y_offset = SPUtil.bezier_val_for_t(new Vector2(0.65f,0), new Vector2(0.8f,-0.75f), new Vector2(0.8f,0), new Vector2(1,0), (anim_t - 0.5f)/0.5f).y;
			}
			y_offset *= 25f;
			
			_img.set_pos(
				_start_pos.x,
				_start_pos.y + y_offset
			);
			_img.set_scale(SPUtil.y_for_point_of_2pt_line(new Vector2(0,0.5f),new Vector2(1,1.0f),anim_t));
		}
		
		public void set_style(SPText.SPTextStyle style) {
			_style = style;
			float alpha = _opacity * _alpha_mult;
			int alpha_key = ((int) (alpha * 10));
			string key = style.get_key(alpha_key);
			
			Material mat = TextureResource.inst().get_material(_texkey,RShader.SPTEXTCHARACTER,key);
			if (mat == null) {
				float alpha_rounded = alpha_key / 10.0f;
			
				mat = TextureResource.inst().create_material(_texkey,RShader.SPTEXTCHARACTER,key);
				mat.SetVector("_fill_color",_style._fill);
				mat.SetVector("_stroke_color",_style._stroke);
				mat.SetVector("_shadow_color",style._shadow);
				mat.SetFloat("_opacity", alpha_rounded);
			}
			_pooled_img_target.get_image().material = mat;
		}
		
		public void set_opacity(float val) {
			if (_opacity == val) return;
			_opacity = val;
			this.set_style(_style);
		}
		
		public float get_time_incr() {
			return _style._time_incr;
		}
		
		public void set_u_pos(float x, float y) { 
			_start_pos = new Vector2(x,y);
			_img.set_pos(_start_pos.x,_start_pos.y); 
		}
		public Vector2 get_u_pos() { return _start_pos; }
		public void set_char_name(char c) { _img.set_name(SPUtil.sprintf("SPTextCharacter(%c)",c)); }
		
		private float _alpha_mult;
		public void set_alpha_mult(float alpha_mult) {
			_alpha_mult = alpha_mult;
			this.set_style(_style);
		}
	}
	
	public class SPTextStyle {
		public Vector4 _stroke;
		public Vector4 _fill;
		public Vector4 _shadow;
		public float _amplitude;
		public float _time_incr;
		public static SPTextStyle cons(Vector4 stroke, Vector4 fill, Vector4 shadow, float amplitude, float time_incr) {
			SPTextStyle rtv = new SPTextStyle();
			rtv._stroke = stroke;
			rtv._fill = fill;
			rtv._shadow = shadow;
			rtv._amplitude = amplitude;
			rtv._time_incr = time_incr;
			return rtv;
		}
		
		public string get_key(int alpha) {
			int stroke_r = (int)( _stroke[0] * 255);
			int stroke_g = (int)( _stroke[1] * 255);
			int stroke_b = (int)( _stroke[2] * 255);
			int stroke_a = (int)( _stroke[3] * 255);
			
			int fill_r = (int)( _fill[0] * 255);
			int fill_g = (int)( _fill[1] * 255);
			int fill_b = (int)( _fill[2] * 255);
			int fill_a = (int)( _fill[3] * 255);
			
			int shadow_r = (int)( _shadow[0] * 255);
			int shadow_g = (int)( _shadow[1] * 255);
			int shadow_b = (int)( _shadow[2] * 255);
			int shadow_a = (int)( _shadow[3] * 255);
		
			return string.Format("stroke({0},{1},{2},{3})_fill({4},{5},{6},{7})_shadow({8},{9},{10},{11}))_alpha({12})",
				stroke_r,stroke_g,stroke_b,stroke_a,
				fill_r,fill_g,fill_b,fill_a,
				shadow_r,shadow_g,shadow_b,shadow_a,
				alpha
			);
		}
	}

	public static SPText cons_text(string texkey, string fntkey, SPTextStyle default_style) {
		GameObject obj = new GameObject();
		obj.name = "SPText";
		obj.AddComponent<RectTransform>();
		SPText rtv = obj.AddComponent<SPText>();
		rtv.i_cons_text(texkey,fntkey,default_style);
		return rtv;
	}
	
	public void repool() {
		this.cleanup_existing_characters();
	}
	
	private Transform _pivot_node;
	private Vector2 _text_anchor;
	private float _text_scale;
	private RectTransform _rect_transform;
	
	private string _texkey;
	private FntFile _bmfont_cfg;
	public FntFile fnt_file() { return _bmfont_cfg; }
	private List<SPTextCharacter> _characters;
	private Vector2 _rendered_size;
	
	private string _cached_string;
	private SPTextStyle _default_style;
	private Dictionary<string,SPTextStyle> _name_to_styles;
	
	private float _time;
	
	public SPText i_cons_text(string texkey, string fntkey, SPTextStyle default_style) {		
		_rect_transform = this.GetComponent<RectTransform>();
		this.set_horiz_center_text(false);
		_texkey = texkey;
		_bmfont_cfg = FileCache.inst().get_fntfile(fntkey);
		_characters = new List<SPTextCharacter>();
		_rendered_size = Vector2.zero;
		_cached_string = "";
		_name_to_styles = new Dictionary<string, SPTextStyle>();
		_time = 0;
		_alpha_mult = 1;
		this.set_opacity(1);
		
		_default_style = default_style;
		_text_anchor = new Vector2(0,1);
		_text_scale = 0.35f;
		
		GameObject pivot_node_obj = new GameObject("_pivot_node");
		pivot_node_obj.AddComponent<RectTransform>();
		_pivot_node = pivot_node_obj.transform;
		_pivot_node.SetParent(this.transform);
		_pivot_node.localPosition = SPUtil.valv(0);
		
		_pivot_node.transform.localScale = SPUtil.valv(_text_scale);
		this.update_pivot_text_anchor();
		return this;
	}
	
	public void add_style(string name, SPTextStyle style) {
		_name_to_styles[name] = style;
	}
	
	public void set_default_style(SPTextStyle style) {
		_default_style = style;
	}
	public SPTextStyle get_default_style() { return _default_style; }
	
	public Vector2 get_size() {
		return _rect_transform.rect.size;
	}
	
	public float get_text_scale() {
		return _text_scale;
	}
	
	public void i_update() {
		float itr_anim_time = _time;
		for (int i = 0; i < _characters.Count; i++) {
			SPTextCharacter itr = _characters[i];

			itr.i_update(itr_anim_time);
			itr_anim_time += itr.get_time_incr();
		}
		_time += SPUtil.dt_scale_get() * 0.05f;
	}
	
	private void markup_string_out_display_string_and_map(string markup_string, out string display_string, out Dictionary<int,SPTextStyle> style_map) {
		StringBuilder rtv_display_string = new StringBuilder();
		Dictionary<int,SPTextStyle> rtv_style_map = new Dictionary<int, SPTextStyle>();
		StringBuilder current_style = new StringBuilder();
		bool in_tag_mode = false;
		
		for (int i = 0; i < markup_string.Length; i++) {
			char itr = markup_string[i];
			switch (itr) {
			case '[':{
				in_tag_mode = true;
				current_style.Remove(0,current_style.Length);
			} break;
			case ']':{
				in_tag_mode = false;
			} break;
			case '@':{
				current_style.Remove(0,current_style.Length);
			} break;
			default:{
				if (in_tag_mode) {
					current_style.Append(itr);
				} else {
					rtv_display_string.Append(itr);
					SPTextStyle style_obj;
					_name_to_styles.TryGetValue(current_style.ToString(), out style_obj);
					if (style_obj == null) {
						style_obj = _default_style;
					}
					rtv_style_map[rtv_display_string.Length-1] = style_obj;
				}
			} break;
			}
		}
		
		display_string = rtv_display_string.ToString();
		style_map = rtv_style_map;
	}
	
	private void cleanup_existing_characters(int index_start = 0) {
		int check_count = _characters.Count - index_start;
		if (check_count <= 0) return;
		for (int i = index_start; i < _characters.Count; i++) {
			_characters[i].cleanup();
		}
		_characters.RemoveRange(index_start, check_count);
	}
	
	public void clear() {
		this.cleanup_existing_characters(0);
		_prev_display_str = "";
	}
	
	private string _prev_display_str = "";
	private Dictionary<int,SPTextStyle> _style_map;
	public SPText set_markup_text(string markup_string) {
		if (_cached_string == markup_string) return this;
		_cached_string = markup_string;
		
		string display_string;
		
		if (markup_string.Contains("[")) {
			this.markup_string_out_display_string_and_map(markup_string, out display_string, out _style_map);
		} else {
			display_string = markup_string;
			_style_map = new Dictionary<int, SPTextStyle>();
			for (int i = 0; i < display_string.Length; i++) {
				_style_map[i] = _default_style;
			}
		}
		
		if (display_string.Length == 0) return this;
		
		int quantityOfLines = 1;
		for (int i = 0; i < display_string.Length-1; i++) {
			if (display_string[i] == '\n') quantityOfLines++;
		}
		float totalHeight = _bmfont_cfg.common.lineHeight * quantityOfLines;
		float nextFontPositionX = 0;
		float nextFontPositionY = -(_bmfont_cfg.common.lineHeight - totalHeight);
		float longestLine = 0;
		
		FntFile.CharInfo fontDef = null;
		
		bool match_prev_display_str = true;
		
		int i_character = 0;
		int line_index = 0;
		
		for (int i = 0; i < display_string.Length; i++) {
			char c = display_string[i];
			
			if (c == '\n') {
				line_index++;
				nextFontPositionX = 0;
				nextFontPositionY -= _bmfont_cfg.common.lineHeight;
				continue;
			}
			if (!_bmfont_cfg.contains_char(c)) {
				Debug.LogError(SPUtil.sprintf("attempted to use character not defined in this bitmap:(%c)",c));
				continue;
			}
			
			if (!(match_prev_display_str && _prev_display_str.Length > i && c == _prev_display_str[i])) {
				if (match_prev_display_str) {
					this.cleanup_existing_characters(i);
				}
				match_prev_display_str = false;
			}
			
			fontDef = _bmfont_cfg.charinfo_for_char(c);
			
			Rect rect = new Rect(fontDef.x,fontDef.y,fontDef.width,fontDef.height);
			
			float yoffset = _bmfont_cfg.common.lineHeight - fontDef.yoffset;
			Vector2 fontPos = new Vector2(
				nextFontPositionX + fontDef.xoffset + fontDef.width*0.5f,
				nextFontPositionY + yoffset - rect.size.y*0.5f
			);
			
			SPTextStyle itr_style = _style_map[i];
			SPTextCharacter neu_char;
			if (!match_prev_display_str) {
				neu_char = SPTextCharacter.cons_texkey_rect(_texkey, rect, itr_style);
				neu_char.set_char_name(c);				
				neu_char.add_to_parent(_pivot_node);
				neu_char.set_alpha_mult(_alpha_mult);
				_characters.Add(neu_char);
			} else {
				neu_char = _characters[i_character];
			}
			neu_char._line_index = line_index;
			neu_char.set_u_pos(fontPos.x,fontPos.y);
			
			float adv = SPText.adv_for_char(c);
			nextFontPositionX += fontDef.xadvance + adv;
			if (longestLine < nextFontPositionX) longestLine = nextFontPositionX;
			i_character++;
		}
		
		Vector2 tmpSize = Vector2.zero;
		if (fontDef != null && fontDef.xadvance < fontDef.width) {
			tmpSize.x = longestLine + fontDef.width - fontDef.xadvance;
		} else {
			tmpSize.x = longestLine;
		}
		tmpSize.y = totalHeight;
		_rendered_size = tmpSize;
		this.update_pivot_text_anchor();
		_prev_display_str = display_string;
		
		if (_do_horiz_center_text) {
			int itr_line_index = 0;	
			while (itr_line_index <= line_index) {
				List<SPTextCharacter> center_line = __center_line;
				center_line.Clear();
				
				float x_min = Mathf.Infinity;
				float x_max = Mathf.NegativeInfinity;
				
				for (int i = 0; i < _characters.Count; i++) {
					SPTextCharacter itr_char = _characters[i];
					if (itr_char._line_index == itr_line_index) {
						center_line.Add(itr_char);
						x_min = Mathf.Min(x_min, itr_char.get_u_pos().x);
						x_max = Mathf.Max(x_max, itr_char.get_u_pos().x);
					}
				}
				
				float mid = SPUtil.lerp(x_min,x_max,0.5f);
				float delta = _rendered_size.x / 2.0f - mid;
				
				for (int i = 0; i < center_line.Count; i++) {
					SPTextCharacter itr_char = center_line[i];
					itr_char.set_u_pos(
						itr_char.get_u_pos().x + delta,
						itr_char.get_u_pos().y
					);
				}		
				itr_line_index++;
			}
			__center_line.Clear();
		}
		
		
		return this;
	}
	private static List<SPTextCharacter> __center_line = new List<SPTextCharacter>();
	
	private static float adv_for_char(char c) {
		switch (c) {
			case 'l': return 4.5f;
			case 'w': return 3;
			case 'f': return 3;
			case ' ': return 3;
			case 'c': return 3;
			default: return 2.5f;
		}
	}
	
	private float _opacity;
	public void set_opacity(float val) {
		_opacity = val;
		for (int i = 0; i < _characters.Count; i++) {
			_characters[i].set_opacity(val);
		}	
	}
	
	private bool _do_horiz_center_text;
	public void set_horiz_center_text(bool val) {
		_do_horiz_center_text = val;
	}
	
	public void set_default_colors(Vector4 stroke, Vector4 fill, Vector4 shadow) {
		_default_style._stroke = stroke;
		_default_style._fill = fill;
		_default_style._shadow = shadow;
		
		for (int i = 0; i < _characters.Count; i++) {
			if (_style_map[i] == _default_style) {
				_characters[i].set_style(_default_style);
			}
		}
	}
	
	public void set_text_anchor(float x, float y) {
		_text_anchor = new Vector2(x,y);
		this.update_pivot_text_anchor();
	}
	
	private void update_pivot_text_anchor() {
		_pivot_node.transform.localPosition = new Vector2(
			(-_text_anchor.x * _rendered_size.x) * _text_scale, 
			(-_text_anchor.y * _rendered_size.y) * _text_scale
		);
	}
	
	private float _alpha_mult;
	public void set_alpha_mult(float alpha_mult) {
		if (!SPUtil.flt_cmp_delta(_alpha_mult,alpha_mult,0.01f)) {
			_alpha_mult = alpha_mult;
			for (int i = 0; i < _characters.Count; i++) {
				_characters[i].set_alpha_mult(_alpha_mult);
			}
		}
	}

}