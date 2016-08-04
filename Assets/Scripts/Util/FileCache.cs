using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class FileCache {

	private static FileCache __inst;
	public static FileCache inst() { return __inst; }

	public static FileCache cons() {
		FileCache rtv = null;
		rtv = (new FileCache());
		__inst = rtv;
		return rtv.i_cons();
	}

	private Dictionary<string,Dictionary<string,Rect>> _texkey_to_rectkey_to_rect = new Dictionary<string,Dictionary<string,Rect>>();

	private FileCache i_cons() {
		return this;
	}

	public Rect get_texrect(string texkey, string rectname) {
		if (!_texkey_to_rectkey_to_rect.ContainsKey(texkey)) add_plist_file_to_cache(texkey);
		if (_texkey_to_rectkey_to_rect[texkey].ContainsKey(rectname)) {
			return _texkey_to_rectkey_to_rect[texkey][rectname];
		} else {
			SPUtil.logf("get_texrect(%s,%s) not found",texkey,rectname);
			return new Rect();
		}
	}

	public List<Rect> get_rects_list(string texkey, string rect_format_str, int min, int max, bool append_empty = false) {
		List<Rect> rtv = new List<Rect>();
		for (int i = min; i < max; i++) {
			rtv.Add(this.get_texrect(texkey,SPUtil.sprintf(rect_format_str,i)));
		}
		if (append_empty) {
			rtv.Add(new Rect());
		}
		return rtv;
	}

	private void add_plist_file_to_cache(string texkey) {
		Debug.LogWarning("plist from streaming:"+texkey);
		_texkey_to_rectkey_to_rect[texkey] = new Dictionary<string,Rect>();

		Dictionary<string,object> frames = (Dictionary<string,object>)((Dictionary<string,object>) PlistCS.Plist.readPlistSource(
			this.load_text_file_from_path(texkey,"")))["frames"];
		foreach (string key in frames.Keys) {
			Dictionary<string,object> frame = (Dictionary<string,object>)frames[key];
			string texture_rect_string = (string)frame["textureRect"];
			
			MatchCollection matches = Regex.Matches(texture_rect_string,"([0-9]+)");
			
			List<int> coords = new List<int>();
			foreach (Match match in matches) {
				foreach (Capture capture in match.Captures) {
					coords.Add(System.Int32.Parse(capture.Value));
				}
			}
			_texkey_to_rectkey_to_rect[texkey][key] = new Rect(coords[0],coords[1],coords[2],coords[3]);
		}
	}

	private string load_text_file_from_path(string key, string suffix = ".plist") {
		//string path = System.IO.Path.Combine(Application.streamingAssetsPath, key+suffix);
		//return System.Text.Encoding.UTF8.GetString(SPUtil.streaming_asset_load(path));
		TextAsset text_asset = Resources.Load<TextAsset>(key);
		return text_asset.text;
	}
	
	private Dictionary<string,FntFile> _key_to_fntfile = new Dictionary<string, FntFile>();
	public FntFile get_fntfile(string key) {
		if (!_key_to_fntfile.ContainsKey(key)) {
			_key_to_fntfile[key] = FntFile.cons_from_string(this.load_text_file_from_path(key,".fnt"));
		}
		return _key_to_fntfile[key];
	}

}

