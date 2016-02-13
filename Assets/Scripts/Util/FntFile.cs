using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class RFnt {
	public static string OSAKA = "osaka";
}

public class FntFile {

	public static FntFile cons_from_string(string file) {
		FntFile rtv = new FntFile();
		
		string[] lines = file.Split('\n');
		
		FntFile.parse_header(lines[0],rtv);
		FntFile.parse_common(lines[1],rtv);
		
		for (int i = 4; i < lines.Length; i++) {
			CharInfo itr = FntFile.parse_char(lines[i]);
			if (itr != null) {
				rtv.chars[itr.letter] = itr;
			}
		}
			
		return rtv;
	}
	
	private static void parse_header(string line, FntFile rtv) {
		string[] tokens = line.Split(' ');
		for (int i = 0; i < tokens.Length; i++) {
			string itr = tokens[i];
			if (itr.Length > 0 && itr.Contains("=")) {
				string[] kvp = itr.Split('=');
				string key = kvp[0];
				string value = kvp[1];
				if (key == "stretchH") {
					rtv.stretchH = Convert.ToSingle(value);
					
				} else if (key == "spacing") {
					string[] xy = value.Split(',');
					rtv.spacing.x = Convert.ToSingle(xy[0]);
					rtv.spacing.y = Convert.ToSingle(xy[1]);
				}
			
			}
		}
	}
	
	private static void parse_common(string line, FntFile rtv) {
		string[] tokens = line.Split(' ');
		for (int i = 0; i < tokens.Length; i++) {
			string itr = tokens[i];
			if (itr.Length > 0 && itr.Contains("=")) {
				string[] kvp = itr.Split('=');
				string key = kvp[0];
				string value = kvp[1];
				if (key == "lineHeight") {
					rtv.common.lineHeight = Convert.ToSingle(value);
				} else if (key == "base") {
					rtv.common.Base = Convert.ToSingle(value);
				} else if (key == "scaleW") {
					rtv.common.scaleW = Convert.ToSingle(value);
				} else if (key == "scaleH") {
					rtv.common.scaleH = Convert.ToSingle(value);
				}
			}
		}
	}
	
	private static CharInfo parse_char(string line) {
		if (line.Length <= 0) return null;
		CharInfo neu_char = new CharInfo();
		string[] tokens = line.Split(' ');
		for (int i = 0; i < tokens.Length; i++) {
			string itr = tokens[i];
			if (itr.Length > 0 && itr.Contains("=")) {
				string key = itr.Split('=')[0];
				string value = itr.Substring(itr.IndexOf('=')+1);
				
				if (key == "id") {
					neu_char.id = Convert.ToInt32(value);
				} else if (key == "x") {
					neu_char.x = Convert.ToSingle(value);
				} else if (key == "y") {
					neu_char.y = Convert.ToSingle(value);
				} else if (key == "width") {
					neu_char.width = Convert.ToSingle(value);
				} else if (key == "height") {
					neu_char.height = Convert.ToSingle(value);
				} else if (key == "xoffset") {
					neu_char.xoffset = Convert.ToSingle(value);
				} else if (key == "yoffset") {
					neu_char.yoffset = Convert.ToSingle(value);
				} else if (key == "xadvance") {
					neu_char.xadvance = Convert.ToSingle(value);
				} else if (key == "letter") {
					neu_char.letter = value.Substring(1,value.Length-2);
				}				
			}
		}
		
		if (neu_char.letter == "") {
			SPUtil.logf("FntFile no letter for char id(%d)",neu_char.id);
		}
		return neu_char;
	}
	

	public float stretchH;
	public Vector2 spacing;
	
	public class Common {
		public float lineHeight;
		public float Base;
		public float scaleW;
		public float scaleH;
	}
	
	 public Common common = new Common();
	
	public class CharInfo {
		public int id;
		public float x;
		public float y;
		public float width;
		public float height;
		public float xoffset;
		public float yoffset;
		public float xadvance;
		public string letter = "";
	}
	
	public Dictionary<string,CharInfo> chars = new Dictionary<string, CharInfo>();
	private string char_to_key_map(char c) {
		if (c == ' ') return "space";
		return c.ToString();
	}
	public bool contains_char(char c) {
		return chars.ContainsKey(this.char_to_key_map(c));
	}
	public CharInfo charinfo_for_char(char c) {
		return chars[this.char_to_key_map(c)];
	}
}