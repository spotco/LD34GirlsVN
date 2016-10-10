using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class SPTextRenderUtil {
	
	public static void set_string(SPText sptext, string text, string full_string) {
		sptext.set_markup_text(SPTextRenderUtil.input_str_insert_linebreaks(sptext, text, full_string));
	}
	
	public static void set_text_outline_color(SPText sptext, Color color) {
		float test_val = 0.95f;
	
		sptext.set_default_style(SPText.SPTextStyle.cons(
			new Vector4(color.r, color.g, color.b, color.a), 
			new Vector4(test_val,test_val,test_val,1), 
			new Vector4(0,0,0,0), 0, 0));
	}
	public static void set_bold_color(SPText sptext, Color normal_outline_color) {
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
			
		Color n_bold_fill_color = SPUtil.color_from_bytes(207, 255, 206, 255);
		Color n_bold_outline_color = SPUtil.color_from_bytes(75, 102, 70, 255);
		sptext.add_style("bn", SPText.SPTextStyle.cons(
			new Vector4(n_bold_outline_color.r, n_bold_outline_color.g, n_bold_outline_color.b, n_bold_outline_color.a),
			new Vector4(n_bold_fill_color.r, n_bold_fill_color.g, n_bold_fill_color.b, n_bold_fill_color.a),
			new Vector4(0,0,0,0), 3.5f, -0.75f));
	
	}
	
	public static void clear(SPText sptext) {
		sptext.clear();
	}
	
	private static StringBuilder __input_str_insert_linebreaks = new StringBuilder("");
	public static string input_str_insert_linebreaks(SPText sptext, string input, string full_string) {
		StringBuilder rtv = __input_str_insert_linebreaks;
		rtv.Remove(0,rtv.Length);
		input = input.Replace('#','"');
		string[] tokens = input.Split(' ');
		string[] full_tokens = full_string.Split(' ');
		float cur_line_length = 0;
		
		float line_length = sptext.get_size().x / sptext.get_text_scale();
		for (int i = 0; i < tokens.Length; i++) {
			string itr_token = tokens[i];
			string itr_full_token = full_tokens[i];
			
			float itr_full_token_length = SPTextRenderUtil.str_token_length(sptext, itr_full_token);
			
			
			if (itr_full_token.Contains("%")) {
				string[] split_lines = itr_token.Split('%');
				string[] split_full_lines = itr_full_token.Split('%');
				if (cur_line_length + SPTextRenderUtil.str_token_length(sptext, split_full_lines[0]) > line_length) {
					rtv.Append("\n");
				} else {
					rtv.Append(" ");
				}
				for (int j = 0; j < split_lines.Length; j++) {
					if (j != 0) {
						rtv.Append("\n");
					}
					rtv.Append(split_lines[j]);
					cur_line_length = SPTextRenderUtil.str_token_length(sptext, split_lines[j]);
				}
				
			} else if (cur_line_length + itr_full_token_length > line_length && i != 0) {
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
	
	private static float str_token_length(SPText sptext, string token) {
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
}
