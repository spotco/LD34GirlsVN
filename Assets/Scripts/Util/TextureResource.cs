using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class RTex {
	public static string BLANK = "blank";
	public static string OSAKA_FNT = "osaka";
	public static string KURUMI_MAP_CHAR_SS = "img/grid/cursor_char/kurumi_map";
}

public class TextureResource {

	public static TextureResource inst() { return GameMain._context._tex_resc; }

	public class TextureResourceValue {
		public Texture _tex;
		public Dictionary<string,Material> _shaderkey_to_material = new Dictionary<string, Material>();
	}

	public Dictionary<string,TextureResourceValue> _key_to_resourcevalue;

	public static TextureResource cons() {
		return (new TextureResource()).i_cons();
	}

	private TextureResource i_cons() {
		_key_to_resourcevalue = new Dictionary<string, TextureResourceValue>();

		FieldInfo[] fields = typeof(RTex).GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (FieldInfo itr in fields) {
			if (itr.FieldType == typeof(string)) {	
				string value = (string)itr.GetValue(null);
				_key_to_resourcevalue[value] = cons_texture_resource_value(value);
			}
		}

		return this;
	}

	private Texture load_texture_from_streamingassets(string path) {
		Debug.LogWarning("texture from streaming:"+path);
		path = System.IO.Path.Combine(Application.streamingAssetsPath, path+".png");
		Texture2D rtv = new Texture2D(0,0,TextureFormat.ARGB32,false);
		rtv.LoadImage(SPUtil.streaming_asset_load(path));
		rtv.filterMode = FilterMode.Point;
		return rtv;
	}

	private TextureResourceValue cons_texture_resource_value(string texkey) {
		Texture tex = 
		//null;
		//Resources.Load<Texture2D>(CachedStreamingAssets.texture_key_to_resource_path(texkey));
		Resources.Load<Texture2D>(texkey);
		if (tex == null) {
			tex = this.load_texture_from_streamingassets(texkey);
		}
		tex.filterMode = FilterMode.Point;
		return new TextureResourceValue() {
			_tex = tex
		};
	}

	public Texture get_tex(string key) {
		return _key_to_resourcevalue[key]._tex;
	}

	public Material get_material_default(string texkey) {
		return get_material(texkey,RShader.DEFAULT);
	}

	public Material get_material(string texkey, string shaderkey) {
		TextureResourceValue tar = _key_to_resourcevalue[texkey];
		if (!tar._shaderkey_to_material.ContainsKey(shaderkey)) {
			tar._shaderkey_to_material[shaderkey] = new Material(ShaderResource.get_shader(shaderkey));
			tar._shaderkey_to_material[shaderkey].SetTexture("_MainTex",this.get_tex(texkey));
		}
		return tar._shaderkey_to_material[shaderkey];

	}

}
