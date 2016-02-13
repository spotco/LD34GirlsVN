using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class VNSPTextManager : MonoBehaviour {
	
	private const bool DEBUG_CANVAS = false;
	
	public static VNSPTextManager cons() {
		GameObject camera_obj = new GameObject("VNSPTextManager");
		return camera_obj.AddComponent<VNSPTextManager>().i_cons();
	}
	
	private SPText _sptext;
	private RenderTexture _render_tex;
	public Texture get_tex() { return _render_tex; }
	private Camera _render_camera;
	
	
	private VNSPTextManager i_cons() {
	
		this.gameObject.transform.position = new Vector3(10000,0,0);
	
		_sptext = SPText.cons_text(RTex.OSAKA_FNT, RFnt.OSAKA, SPText.SPTextStyle.cons(new Vector4(95/255.0f,115/255.0f,88/255.0f,1), new Vector4(1,1,1,1), new Vector4(0,0,0,0),0,0));
		_sptext.gameObject.transform.parent = this.transform;
		_sptext.set_scale(0.025f);
		_sptext.set_u_pos(-11.79f,4.16f);
		_sptext.set_u_z(1);
		_sptext.set_text_anchor(0,1);
		_sptext.set_markup_text(this.input_str_insert_linebreaks("A new city with a new home and new life. I haven't even had time to write back to my old friends!"));
		
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
	
	private string input_str_insert_linebreaks(string input) {
		StringBuilder rtv = new StringBuilder("");
		string[] tokens = input.Split(' ');
		float cur_line_length = 0;
		for (int i = 0; i < tokens.Length; i++) {
			string itr_token = tokens[i];
			float itr_token_length = this.str_token_length(itr_token);
			if (cur_line_length + itr_token_length > 800) {
				rtv.Append("\n");
				rtv.Append(itr_token);
				rtv.Append(" ");
				cur_line_length = itr_token_length;
			} else {
				rtv.Append(itr_token);
				rtv.Append(" ");
				cur_line_length += itr_token_length;
			}
		}
		return rtv.ToString();
	}
	
	private float str_token_length(string token) {
		FntFile fntfile = _sptext.fnt_file();
		float rtv = 0;
		for (int i = 0; i < token.Length; i++) {
			char itr = token[i];
			if (fntfile.contains_char(itr)) {
				rtv += fntfile.charinfo_for_char(itr).xadvance;
			}
		}
		return rtv;
	}
	
	public void i_update(GameMain game) {}

}
