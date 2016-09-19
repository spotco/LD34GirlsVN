using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class SPText : MonoBehaviour {

	public class SPTextCharacter :  GenericPooledObject {
		public static SPTextCharacter cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			return ObjectPool.inst().generic_depool<SPTextCharacter>().i_cons_texkey_rect(texkey,rect, style);
		}
		private PooledRawImageTarget _pooled_img_target;
		private SPSpriteAnimator.Target _img;
		//private MeshRenderer _mesh_renderer;
		private float _opacity;
		private SPText.SPTextStyle _style;
		private Vector2 _start_pos;
		
		private float _animate_in_t;
		
		public void add_to_parent(Transform parent) { 
			_img.add_to_parent(parent); 
			_img.set_scale(1);
		}
		public void depool() {
			_pooled_img_target = ObjectPool.inst().spbasebehavior_depool<PooledRawImageTarget>();
			_img = _pooled_img_target.get_target();
			//_img = SPSprite.cons_sprite_texkey_texrect(RTex.BLANK,new Rect(0,0,0,0));
			//_mesh_renderer = _img.gameObject.GetComponent<MeshRenderer>();
		}
		public void repool() {
			ObjectPool.inst().spbasebehavior_repool<PooledRawImageTarget>(_pooled_img_target);
			
			_pooled_img_target = null;
			_img = null;
			
			//_mesh_renderer = null;
			//_img.repool();
			//_img = null;
		}
		private SPTextCharacter i_cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			//_img.set_texkey(texkey);
			//_img.set_tex_rect(rect);
			//_img.set_shader(RShader.SPTEXTCHARACTER);
			
			_img.set_texture(GameMain._context._tex_resc.get_tex(texkey));
			_img.set_tex_rect(rect);
			_img.get_recttransform().sizeDelta = new Vector2(rect.width, rect.height);
			
			
			_opacity = 1;
			_alpha_mult = 1;
			_start_pos = Vector2.zero;
			this.set_style(style);
			_animate_in_t = 0;
			return this;
		}
		public void i_update(float time) {
			if (_animate_in_t < 1) {
				this.i_update_animate_text_in(_animate_in_t);
				_animate_in_t = Mathf.Clamp(_animate_in_t + SPUtil.sec_to_tick(0.1f) * SPUtil.dt_scale_get(), 0, 1);
				
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
			this.set_opacity(SPUtil.y_for_point_of_2pt_line(new Vector2(0,0), new Vector2(1,1), 
				SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0,1), new Vector2(0,1), new Vector2(1,1), anim_t).y));
			_img.set_pos(
				_start_pos.x,
				_start_pos.y + SPUtil.bezier_val_for_t(new Vector2(0,25), new Vector2(0.8f,50), new Vector2(0.8f,-50), new Vector2(1,0), 
					SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0,1), new Vector2(0,1), new Vector2(1,1), anim_t).y).y
			);
			_img.set_scale(SPUtil.y_for_point_of_2pt_line(new Vector2(0,0.5f),new Vector2(1,1.0f),anim_t));
		}
		
		private MaterialPropertyBlock _material_block;
		public void set_style(SPText.SPTextStyle style) {
			_style = style;
//			if (_material_block == null) {
//				_material_block = new MaterialPropertyBlock();
//				_mesh_renderer.GetPropertyBlock(_material_block);
//			}
//			_material_block.Clear();
//			_material_block.AddColor("_fill_color",style._fill);
//			_material_block.AddColor("_stroke_color",style._stroke);
//			_material_block.AddColor("_shadow_color",style._shadow);
//			_material_block.AddFloat("_opacity",_opacity * _alpha_mult);
//			_mesh_renderer.SetPropertyBlock(_material_block);
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
		public void set_char_name(char c) { _img.set_name(SPUtil.sprintf("SPTextCharacter(%c)",c)); }
		//public void set_manual_sort_z_order(int zord) { _img.set_manual_sort_z_order(zord); }
		public void cleanup() {
			ObjectPool.inst().generic_repool<SPTextCharacter>(this);
		}
		
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
	}

	public static SPText cons_text(string texkey, string fntkey, SPTextStyle default_style) {
		GameObject obj = new GameObject();
		obj.name = "SPText";
		obj.AddComponent<RectTransform>();
		SPText rtv = obj.AddComponent<SPText>();
		rtv.i_cons_text(texkey,fntkey,default_style);
		return rtv;
		//return SPNode.generic_cons<SPText>().i_cons_text(texkey,fntkey, default_style);
	}
	public new static SPNode cons_node() { throw new System.Exception("SPText::cons_node"); }
	
	public void repool() {
		this.cleanup_existing_characters();
//		_pivot_node.repool();
//		_pivot_node = null;
//		SPNode.generic_repool<SPText>(this);
	}
	
	[SerializeField] private Transform _pivot_node;
	private Vector2 _text_anchor;
	private float _text_scale;
	
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
		//this.set_u_pos(0,0);
		//this.set_u_z(0);
		//this.set_rotation(0);
		//this.set_scale(1);
	
		//_pivot_node = SPNode.cons_node();
		//_pivot_node.set_name("_pivot_node");
		//this.add_child(_pivot_node);
		
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
		_text_scale = 0.45f;
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
	
	public void i_update() {
	
		return;
	
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
	public SPText set_markup_text(string markup_string) {
		if (_cached_string == markup_string) return this;
		_cached_string = markup_string;
		
		string display_string;
		Dictionary<int,SPTextStyle> style_map;
		if (markup_string.Contains("[")) {
			this.markup_string_out_display_string_and_map(markup_string, out display_string, out style_map);
		} else {
			display_string = markup_string;
			style_map = new Dictionary<int, SPTextStyle>();
			for (int i = 0; i < display_string.Length; i++) {
				style_map[i] = _default_style;
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
		for (int i = 0; i < display_string.Length; i++) {
			char c = display_string[i];
			
			if (c == '\n') {
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
			
			SPTextStyle itr_style = style_map[i];
			SPTextCharacter neu_char;
			if (!match_prev_display_str) {
				neu_char = SPTextCharacter.cons_texkey_rect(_texkey, rect, itr_style);
				neu_char.set_char_name(c);
				neu_char.set_opacity(_opacity);
				neu_char.add_to_parent(_pivot_node);
				neu_char.set_alpha_mult(_alpha_mult);
				_characters.Add(neu_char);
			} else {
				neu_char = _characters[i_character];
			}
			neu_char.set_u_pos(fontPos.x,fontPos.y);
			SPUtil.logf("char(%s) upos(%.2f,%.2f)",c,fontPos.x,fontPos.y);
			
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
		return this;
	}
	
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
	public new void set_opacity(float val) {
		_opacity = val;
		for (int i = 0; i < _characters.Count; i++) {
			_characters[i].set_opacity(val);
		}	
	}
	
	public void set_text_anchor(float x, float y) {
		_text_anchor = new Vector2(x,y);
		this.update_pivot_text_anchor();
	}
	
	private void update_pivot_text_anchor() {
		//_pivot_node.set_u_pos(-_text_anchor.x * _rendered_size.x, -_text_anchor.y * _rendered_size.y);
		
		
		SPUtil.logf("update_pivot_text_anchor text_anchor(%.2f,%.2f) rendered_size(%.2f,%.2f)",_text_anchor.x,_text_anchor.y,_rendered_size.x,_rendered_size.y);
		_pivot_node.transform.localPosition = new Vector2(
			(-_text_anchor.x * _rendered_size.x) * _text_scale, 
			(-_text_anchor.y * _rendered_size.y) * _text_scale
		);
	}
	
	private int _zord = GameAnchorZ.HUD_BASE;
	public new void set_manual_sort_z_order(int zord) {
//		_zord = zord;
//		for (int i = 0; i < this._characters.Count; i++) {
//			this._characters[i]._img.set_manual_sort_z_order(_zord);
//		}
	}
	
	private string _layer = RLayer.DEFAULT;
	public void set_layer(string layer) {
//		_layer = layer;
//		for (int i = 0; i < this._characters.Count; i++) {
//			this._characters[i]._img.set_layer(_layer);
//		}
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