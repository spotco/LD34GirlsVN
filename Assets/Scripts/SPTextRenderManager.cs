using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class SPTextRenderManager : MonoBehaviour {
	
	private const bool DEBUG_CANVAS = false;
	
	public static SPTextRenderManager cons() {
		GameObject camera_obj = new GameObject("VNSPTextManager");
		return camera_obj.AddComponent<SPTextRenderManager>().i_cons();
	}
	
	private SPText _sptext;
	private RenderTexture _render_tex;
	public Texture get_tex() { return _render_tex; }
	private Camera _render_camera;
	
	
	private SPTextRenderManager i_cons() {
		this.gameObject.transform.position = new Vector3(10000,0,0);
	
		_sptext = SPText.cons_text(RTex.OSAKA_FNT, RFnt.OSAKA, SPText.SPTextStyle.cons(Vector4.zero, Vector4.zero, Vector4.zero, 0, 0));
		_sptext.gameObject.transform.parent = this.transform;
		_sptext.set_scale(0.0225f);
		_sptext.set_u_pos(-11.79f,4.16f);
		_sptext.set_u_z(1);
		_sptext.set_text_anchor(0,1);
		
		this.set_text_outline_color(new Vector4(95/255.0f,115/255.0f,88/255.0f,1));
		
		_render_tex = new RenderTexture(378*4,148*4,32);
		_render_tex.filterMode = FilterMode.Trilinear;
		_render_tex.antiAliasing = 8;
		
		
		_render_camera = this.gameObject.AddComponent<Camera>();
		_render_camera.targetTexture = _render_tex;
		_render_camera.orthographic = true;
		UnityStandardAssets.ImageEffects.BlurOptimized blur = _render_camera.gameObject.AddComponent<UnityStandardAssets.ImageEffects.BlurOptimized>();
		blur.downsample = 0;
		blur.blurSize = 1.25f;
		blur.blurIterations = 1;
		
		if (DEBUG_CANVAS) {
			Canvas test_canvas = (new GameObject("test_canvas")).AddComponent<Canvas>();
			test_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			RawImage test_rawimage = (new GameObject("test_image")).AddComponent<RawImage>();
			test_rawimage.transform.parent = test_canvas.transform;
			test_rawimage.texture = _render_tex;
			test_rawimage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
			test_rawimage.GetComponent<RectTransform>().sizeDelta = new Vector2(250,125);
		}
		
		return this;
	}
	
	public void set_string(string text, string full_string) {
		_sptext.set_markup_text(this.input_str_insert_linebreaks(text, full_string));
	}
	
	public void set_text_outline_color(Color color) {
		_sptext.set_default_style(SPText.SPTextStyle.cons(
			new Vector4(color.r, color.g, color.b, color.a), 
			new Vector4(1,1,1,1), 
			new Vector4(0,0,0,0), 0, 0));
	}
	public void set_bold_color(Color outline_color, Color fill_color) {
		_sptext.add_style("b", SPText.SPTextStyle.cons(
			new Vector4(outline_color.r, outline_color.g, outline_color.b, outline_color.a), 
			new Vector4(fill_color.r, fill_color.g, fill_color.b, fill_color.a), 
			new Vector4(0,0,0,0), 3.5f, -0.75f));
		_sptext.add_style("b2", SPText.SPTextStyle.cons(
			new Vector4(outline_color.r, outline_color.g, outline_color.b, outline_color.a),
			new Vector4(0.93f, 0.93f, 0.93f, 1),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
		_sptext.add_style("b3", SPText.SPTextStyle.cons(
			new Vector4(outline_color.r, outline_color.g, outline_color.b, outline_color.a),
			new Vector4(1,1,1,1),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
	}
	
	public void clear() {
		_sptext.clear();
	}
	
	private string input_str_insert_linebreaks(string input, string full_string) {
		StringBuilder rtv = new StringBuilder("");
		string[] tokens = input.Split(' ');
		string[] full_tokens = full_string.Split(' ');
		float cur_line_length = 0;
		
		for (int i = 0; i < tokens.Length; i++) {
			string itr_token = tokens[i];
			string itr_full_token = full_tokens[i];
			
			float itr_full_token_length = this.str_token_length(itr_full_token);
			int LINE_LENGTH = 825;
			
			if (itr_full_token.Contains("%")) {
				string[] split_lines = itr_token.Split('%');
				string[] split_full_lines = itr_full_token.Split('%');
				if (cur_line_length + this.str_token_length(split_full_lines[0]) > LINE_LENGTH) {
					rtv.Append("\n");
				} else {
					rtv.Append(" ");
				}
				for (int j = 0; j < split_lines.Length; j++) {
					if (j != 0) {
						rtv.Append("\n");
					}
					rtv.Append(split_lines[j]);
					cur_line_length = this.str_token_length(split_lines[j]);
				}
				
			} else if (cur_line_length + itr_full_token_length > LINE_LENGTH) {
				rtv.Append("\n");
				rtv.Append(itr_token);
				cur_line_length = itr_full_token_length;
			} else {
				if (i != 0) {
					rtv.Append(" ");
				}
				rtv.Append(itr_token);
				cur_line_length += itr_full_token_length;
			}
		}
		return rtv.ToString();
	}
	
	private float str_token_length(string token) {
		FntFile fntfile = _sptext.fnt_file();
		float rtv = 0;
		bool tag_mode = false;
		for (int i = 0; i < token.Length; i++) {
			char itr = token[i];
			
			if (itr == '[') {
				tag_mode = true;
			} else if (itr == ']') {
				tag_mode = false;
			} else if (itr == '@') {
			} else if (!tag_mode) {
				if (fntfile.contains_char(itr)) {
					rtv += fntfile.charinfo_for_char(itr).xadvance;
				}
			}
		}
		return rtv;
	}
	
	public void i_update(GameMain game) {
		_sptext.i_update();
	}

}
