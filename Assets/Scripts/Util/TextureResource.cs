using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

public class RTex {
	public static string BLANK = "blank";
	public static string OSAKA_FNT = "osaka";
	public static string KURUMI_MAP_CHAR_SS = "img/grid/cursor_char/kurumi_map";
	
	public static string NODE_UNLOCK_PARTICLE = "img/grid/neu/node_unlock_particle";
	public static string NODE_UNLOCK_SHINE_1 = "img/grid/neu/node_unlock_shine";
	public static string NODE_UNLOCK_SHINE_2 = "img/grid/neu/node_unlock_shine_2";
}

public class TextureResource {

	public static TextureResource inst() { return GameMain._context._tex_resc; }

	public class TextureResourceValue {
		public Texture _tex;
		public Dictionary<string,Material> _shaderkey_to_material = new Dictionary<string, Material>();
		public Dictionary<string,Sprite> _spritekey_to_material = new Dictionary<string, Sprite>();  
	}

	public Dictionary<string,TextureResourceValue> _key_to_resourcevalue;
	public Dictionary<Texture,string> _tex_to_key;

	public static TextureResource cons() {
		return (new TextureResource()).i_cons();
	}

	private TextureResource i_cons() {
		_key_to_resourcevalue = new Dictionary<string, TextureResourceValue>();
		_tex_to_key = new Dictionary<Texture, string>();

		FieldInfo[] fields = typeof(RTex).GetFields(BindingFlags.Public | BindingFlags.Static);
		foreach (FieldInfo itr in fields) {
			if (itr.FieldType == typeof(string)) {	
				string value = (string)itr.GetValue(null);
				_key_to_resourcevalue[value] = cons_texture_resource_value(value);
				_tex_to_key[_key_to_resourcevalue[value]._tex] = value;
			}
		}

		return this;
	}

	private Texture load_texture_from_streamingassets(string path) {
		Debug.LogWarning("texture from streaming:"+path);
		path = System.IO.Path.Combine(Application.streamingAssetsPath, path+".png");
		Texture2D rtv = new Texture2D(0,0,TextureFormat.ARGB32, false);
		rtv.LoadImage(SPUtil.streaming_asset_load(path));
		rtv.filterMode = FilterMode.Trilinear;
		return rtv;
	}

	private TextureResourceValue cons_texture_resource_value(string texkey) {
		Texture tex = 
		//null;
		//Resources.Load<Texture2D>(CachedStreamingAssets.texture_key_to_resource_path(texkey));
		Resources.Load<Texture2D>(texkey);
		tex.filterMode = FilterMode.Trilinear;
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
	
	public Sprite get_sprite(string texkey, Rect sprite) {
		TextureResourceValue tar = _key_to_resourcevalue[texkey];
		int sprite_x = (int)sprite.x;
		int sprite_y = (int)sprite.y;
		int sprite_wid = (int)sprite.width;
		int sprite_hei = (int)sprite.height;
		
		string sprite_key = string.Format("{0}_{1}_{2}_{3}",sprite_x,sprite_y,sprite_wid,sprite_hei);
		
		if (!tar._spritekey_to_material.ContainsKey(sprite_key)) {
			Sprite neu = Sprite.Create((Texture2D)this.get_tex(texkey), sprite, new Vector2(0,0));
			tar._spritekey_to_material[sprite_key] = neu;
		}
		return tar._spritekey_to_material[sprite_key];
	}
	
	public string tex_to_key(Texture tex) {
		return _tex_to_key[tex];
	}

}
