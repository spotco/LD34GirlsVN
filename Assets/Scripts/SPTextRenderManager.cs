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
	
	//private SPText _sptext;
	//private RenderTexture _render_tex;
	
	//public Texture get_tex() { return _render_tex; }
	
	//public SPText get_text() { return _sptext; }
	
	
	//private Camera _render_camera;
	
	
	private SPTextRenderManager i_cons() {
		this.gameObject.transform.position = new Vector3(10000,0,0);
	
		//_sptext = SPText.cons_text(RTex.OSAKA_FNT, RFnt.OSAKA, SPText.SPTextStyle.cons(Vector4.zero, Vector4.zero, Vector4.zero, 0, 0));
		//_sptext.gameObject.transform.parent = this.transform;
		//_sptext.set_scale(0.0225f);
		//_sptext.set_u_pos(-11.79f,4.16f);
		//_sptext.set_u_z(1);
		//_sptext.set_text_anchor(0,1);
		
		//this.set_text_outline_color(new Vector4(95/255.0f,115/255.0f,88/255.0f,1));
		
//		_render_tex = new RenderTexture(378*4,148*4,32);
//		_render_tex.filterMode = FilterMode.Trilinear;
//		_render_tex.antiAliasing = 8;
//		
//		
//		_render_camera = this.gameObject.AddComponent<Camera>();
//		_render_camera.targetTexture = _render_tex;
//		_render_camera.orthographic = true;
		//UnityStandardAssets.ImageEffects.BlurOptimized blur = _render_camera.gameObject.AddComponent<UnityStandardAssets.ImageEffects.BlurOptimized>();
		//blur.downsample = 0;
		//blur.blurSize = 1.25f;
		//blur.blurIterations = 1;
		
		if (DEBUG_CANVAS) {
//			Canvas test_canvas = (new GameObject("test_canvas")).AddComponent<Canvas>();
//			test_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//			RawImage test_rawimage = (new GameObject("test_image")).AddComponent<RawImage>();
//			test_rawimage.transform.parent = test_canvas.transform;
//			test_rawimage.texture = _render_tex;
//			test_rawimage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
//			test_rawimage.GetComponent<RectTransform>().sizeDelta = new Vector2(250,125);
		}
		
		return this;
	}
	
	public void set_string(SPText sptext, string text, string full_string) {
		sptext.set_markup_text(this.input_str_insert_linebreaks(sptext, text, full_string));
	}
	
	public void set_text_outline_color(SPText sptext, Color color) {
	
		float test_val = 0.95f;
	
		sptext.set_default_style(SPText.SPTextStyle.cons(
			new Vector4(color.r, color.g, color.b, color.a), 
			new Vector4(test_val,test_val,test_val,1), 
			new Vector4(0,0,0,0), 0, 0));
	}
	public void set_bold_color(SPText sptext, Color normal_outline_color) {
		// purple
		Color bold_fill_color = new Color(235/255.0f,185/255.0f,255/255.0f,1);
		Color bold_outline_color = new Color(118/255.0f,100/255.0f,127/255.0f,1);
		
		sptext.add_style("b", SPText.SPTextStyle.cons(
			new Vector4(bold_outline_color.r, bold_outline_color.g, bold_outline_color.b, bold_outline_color.a), 
			new Vector4(bold_fill_color.r, bold_fill_color.g, bold_fill_color.b, bold_fill_color.a), 
			new Vector4(0,0,0,0), 3.5f, -0.75f));
		sptext.add_style("b2", SPText.SPTextStyle.cons(
			new Vector4(normal_outline_color.r, normal_outline_color.g, normal_outline_color.b, normal_outline_color.a),
			new Vector4(0.93f, 0.93f, 0.93f, 1),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
		sptext.add_style("b3", SPText.SPTextStyle.cons(
			new Vector4(normal_outline_color.r, normal_outline_color.g, normal_outline_color.b, normal_outline_color.a),
			new Vector4(1,1,1,1),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
		sptext.add_style("b4", SPText.SPTextStyle.cons(
			new Vector4(normal_outline_color.r, normal_outline_color.g, normal_outline_color.b, normal_outline_color.a),
			new Vector4(0.95f,0.95f,0.95f,1),
			new Vector4(0,0,0,0), 1.0f, -0.5f));
			
		Color k_bold_fill_color = new Color(252/255.0f,185/255.0f,148/255.0f,1);
		Color k_bold_outline_color = new Color(100/255.0f,95/255.0f,90/255.0f,1);
		sptext.add_style("bk", SPText.SPTextStyle.cons(
			new Vector4(k_bold_outline_color.r, k_bold_outline_color.g, k_bold_outline_color.b, k_bold_outline_color.a),
			new Vector4(k_bold_fill_color.r, k_bold_fill_color.g, k_bold_fill_color.b, k_bold_fill_color.a),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
			
		Color r_bold_fill_color = new Color(189/255.0f,208/255.0f,255/255.0f,1);
		Color r_bold_outline_color = new Color(88/255.0f,93/255.0f,106/255.0f,1);
		sptext.add_style("br", SPText.SPTextStyle.cons(
			new Vector4(r_bold_outline_color.r, r_bold_outline_color.g, r_bold_outline_color.b, r_bold_outline_color.a),
			new Vector4(r_bold_fill_color.r, r_bold_fill_color.g, r_bold_fill_color.b, r_bold_fill_color.a),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
			
		Color s_bold_fill_color = new Color(251/255.0f,244/255.0f,189/255.0f,1);
		Color s_bold_outline_color = new Color(117/255.0f,115/255.0f,95/255.0f,1);
		sptext.add_style("bs", SPText.SPTextStyle.cons(
			new Vector4(s_bold_outline_color.r, s_bold_outline_color.g, s_bold_outline_color.b, s_bold_outline_color.a),
			new Vector4(s_bold_fill_color.r, s_bold_fill_color.g, s_bold_fill_color.b, s_bold_fill_color.a),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
	}
	
	public void clear(SPText sptext) {
		sptext.clear();
	}
	
	private StringBuilder __input_str_insert_linebreaks = new StringBuilder("");
	private string input_str_insert_linebreaks(SPText sptext, string input, string full_string) {
		StringBuilder rtv = __input_str_insert_linebreaks;
		rtv.Remove(0,rtv.Length);
		input = input.Replace('#','"');
		string[] tokens = input.Split(' ');
		string[] full_tokens = full_string.Split(' ');
		float cur_line_length = 0;
		
		for (int i = 0; i < tokens.Length; i++) {
			string itr_token = tokens[i];
			string itr_full_token = full_tokens[i];
			
			float itr_full_token_length = this.str_token_length(sptext, itr_full_token);
			int LINE_LENGTH = 825;
			
			if (itr_full_token.Contains("%")) {
				string[] split_lines = itr_token.Split('%');
				string[] split_full_lines = itr_full_token.Split('%');
				if (cur_line_length + this.str_token_length(sptext, split_full_lines[0]) > LINE_LENGTH) {
					rtv.Append("\n");
				} else {
					rtv.Append(" ");
				}
				for (int j = 0; j < split_lines.Length; j++) {
					if (j != 0) {
						rtv.Append("\n");
					}
					rtv.Append(split_lines[j]);
					cur_line_length = this.str_token_length(sptext, split_lines[j]);
				}
				
			} else if (cur_line_length + itr_full_token_length > LINE_LENGTH && i != 0) {
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
	
	private float str_token_length(SPText sptext, string token) {
		FntFile fntfile = sptext.fnt_file();
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
		//_sptext.i_update();
	}

}
