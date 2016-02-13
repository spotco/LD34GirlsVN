using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class SPText : SPNode, SPAlphaGroupElement {

	public class SPTextCharacter : SPNodeHierarchyElement, GenericPooledObject, SPAlphaGroupElement {
		public static SPTextCharacter cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			return ObjectPool.inst().generic_depool<SPTextCharacter>().i_cons_texkey_rect(texkey,rect, style);
		}
		public SPSprite _img;
		private MeshRenderer _mesh_renderer;
		private float _opacity;
		private SPText.SPTextStyle _style;
		private Vector2 _start_pos;
		
		public void add_to_parent(SPNode parent) { parent.add_child(_img); }
		public void depool() {
			_img = SPSprite.cons_sprite_texkey_texrect(RTex.BLANK,new Rect(0,0,0,0));
			_mesh_renderer = _img.gameObject.GetComponent<MeshRenderer>();
		}
		public void repool() {
			_mesh_renderer = null;
			_img.repool();
			_img = null;
		}
		private SPTextCharacter i_cons_texkey_rect(string texkey, Rect rect, SPText.SPTextStyle style) {
			_img.set_texkey(texkey);
			_img.set_tex_rect(rect);
			_img.set_shader(RShader.SPTEXTCHARACTER);
			_opacity = 1;
			_alpha_mult = 1;
			_start_pos = Vector2.zero;
			this.set_style(style);
			return this;
		}
		public void i_update(float time) {
			this.set_opacity(1);
			_img.set_scale(1);
			_img.set_u_pos(
				_start_pos.x,
				_start_pos.y + _style._amplitude * Mathf.Sin(time)
			);
		}
		public void i_update_animate_text_in(float anim_t) {
			this.set_opacity(anim_t);
			_img.set_u_pos(
				_start_pos.x,
				_start_pos.y + 10 * anim_t
			);
			_img.set_scale(anim_t);
		}
		
		private MaterialPropertyBlock _material_block;
		public void set_style(SPText.SPTextStyle style) {
			_style = style;
			if (_material_block == null) {
				_material_block = new MaterialPropertyBlock();
				_mesh_renderer.GetPropertyBlock(_material_block);
			}
			_material_block.Clear();
			_material_block.AddColor("_fill_color",style._fill);
			_material_block.AddColor("_stroke_color",style._stroke);
			_material_block.AddColor("_shadow_color",style._shadow);
			_material_block.AddFloat("_opacity",_opacity * _alpha_mult);
			_mesh_renderer.SetPropertyBlock(_material_block);
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
			_img.set_u_pos(_start_pos.x,_start_pos.y); 
		}
		public void set_char_name(char c) { _img.set_name(SPUtil.sprintf("SPTextCharacter(%c)",c)); }
		public void set_manual_sort_z_order(int zord) { _img.set_manual_sort_z_order(zord); }
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
		return SPNode.generic_cons<SPText>().i_cons_text(texkey,fntkey, default_style);
	}
	public new static SPNode cons_node() { throw new System.Exception("SPText::cons_node"); }
	
	public override void repool() {
		this.cleanup_existing_characters();
		_pivot_node.repool();
		_pivot_node = null;
		SPNode.generic_repool<SPText>(this);
	}
	
	private SPNode _pivot_node;
	private Vector2 _text_anchor;
	
	private string _texkey;
	private FntFile _bmfont_cfg;
	public FntFile fnt_file() { return _bmfont_cfg; }
	private List<SPTextCharacter> _characters;
	private Vector2 _rendered_size;
	
	private string _cached_string;
	private SPTextStyle _default_style;
	private Dictionary<string,SPTextStyle> _name_to_styles;
	
	private float _time;
	private float _animate_text_in_ct;
	
	private SPText i_cons_text(string texkey, string fntkey, SPTextStyle default_style) {
		this.set_u_pos(0,0);
		this.set_u_z(0);
		this.set_rotation(0);
		this.set_scale(1);
	
		_pivot_node = SPNode.cons_node();
		_pivot_node.set_name("_pivot_node");
		this.add_child(_pivot_node);
		
		_texkey = texkey;
		_bmfont_cfg = FileCache.inst().get_fntfile(fntkey);
		_characters = new List<SPTextCharacter>();
		_rendered_size = Vector2.zero;
		_cached_string = "";
		_name_to_styles = new Dictionary<string, SPTextStyle>();
		_time = 0;
		_animate_text_in_ct = 0;
		_alpha_mult = 1;
		this.set_opacity(1);
		
		_default_style = default_style;
		_text_anchor = Vector2.zero;
		return this;
	}
	
	public void add_style(string name, SPTextStyle style) {
		_name_to_styles[name] = style;
	}
	
	public void i_update() {
		float itr_anim_time = _time;
		for (int i = 0; i < _characters.Count; i++) {
			SPTextCharacter itr = _characters[i];
			float animate_in_t = Mathf.Clamp(_animate_text_in_ct-i,0,1);
			if (animate_in_t < 1) {
				itr.i_update_animate_text_in(animate_in_t);
			} else {
				itr.i_update(itr_anim_time);
				itr_anim_time += itr.get_time_incr();
			}
		}
		_time += SPUtil.dt_scale_get() * 0.05f;
		_animate_text_in_ct += SPUtil.dt_scale_get() * 0.33f;
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
	
	private void cleanup_existing_characters() {
		for (int i = 0; i < _characters.Count; i++) {
			_characters[i].cleanup();
		}
		_characters.Clear();
	}
	
	public SPText set_markup_text(string markup_string) {
		if (_cached_string == markup_string) return this;
		_cached_string = markup_string;
		this.cleanup_existing_characters();
		
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
			
			fontDef = _bmfont_cfg.charinfo_for_char(c);
			
			Rect rect = new Rect(fontDef.x,fontDef.y,fontDef.width,fontDef.height);
			
			SPTextStyle itr_style = style_map[i];
			SPTextCharacter neu_char = SPTextCharacter.cons_texkey_rect(_texkey, rect, itr_style);
			neu_char.set_char_name(c);
			neu_char.set_manual_sort_z_order(_zord);
			neu_char._img.set_layer(_layer);
			neu_char.set_opacity(_opacity);
			neu_char.add_to_parent(_pivot_node);
			neu_char.set_alpha_mult(_alpha_mult);
			_characters.Add(neu_char);
			
			float yoffset = _bmfont_cfg.common.lineHeight - fontDef.yoffset;
			Vector2 fontPos = new Vector2(
				nextFontPositionX + fontDef.xoffset + fontDef.width*0.5f,
				nextFontPositionY + yoffset - rect.size.y*0.5f
			);
			neu_char.set_u_pos(fontPos.x,fontPos.y);
			
			float adv = 2;
			if (c == 'l') {
				adv = 4.5f;
			} else if (c == 'w') {
				adv = 3;
			}
			
			nextFontPositionX += fontDef.xadvance + adv;
			
			if (longestLine < nextFontPositionX) longestLine = nextFontPositionX;
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
		return this;
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
		_pivot_node.set_u_pos(-_text_anchor.x * _rendered_size.x, -_text_anchor.y * _rendered_size.y);
	}
	
	private int _zord = GameAnchorZ.HUD_BASE;
	public new void set_manual_sort_z_order(int zord) {
		_zord = zord;
		for (int i = 0; i < this._characters.Count; i++) {
			this._characters[i]._img.set_manual_sort_z_order(_zord);
		}
	}
	
	private string _layer = RLayer.DEFAULT;
	public void set_layer(string layer) {
		_layer = layer;
		for (int i = 0; i < this._characters.Count; i++) {
			this._characters[i]._img.set_layer(_layer);
		}
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