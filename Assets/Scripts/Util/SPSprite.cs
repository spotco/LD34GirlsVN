using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SPNode), true)]
#endif

public interface SPAlphaGroupElement {
	void set_alpha_mult(float alpha_mult);
}

public class SPSprite : SPNode, SPAlphaGroupElement {
	public static SPSprite cons_sprite_texkey_texrect(string texkey, Rect texrect) {
		return SPNode.generic_cons<SPSprite>().i_cons_sprite_texkey_texrect(texkey,texrect);
	}

	public new static SPNode cons_node() { throw new System.Exception("SPSprite::cons_node"); }

	public override void repool() {
		_meshrenderer.material = null;
		SPNode.generic_repool<SPSprite>(this);
	}

	[SerializeField] private string _texkey;
	[SerializeField] private Vector4 _color;
	[SerializeField] private MaterialPropertyBlock _material_block;
	[SerializeField] private Rect _texrect;

	protected MeshRenderer _meshrenderer;
	protected MeshFilter _meshfilter;

	//subclasses must call this
	protected SPSprite i_cons_sprite_texkey_texrect(string texkey, Rect texrect) {
		if (_meshrenderer == null) {
			this.gameObject.AddComponent<MeshFilter>().mesh = MeshGen.get_unit_quad_mesh();
			this.gameObject.AddComponent<MeshRenderer>();
			_meshrenderer = this.gameObject.GetComponent<MeshRenderer>();
			_meshfilter = this.gameObject.GetComponent<MeshFilter>();
		}
		
		_material_color_params = null;
		
		this.set_alpha_mult(1);
		this.set_texkey(texkey);

		_meshrenderer.receiveShadows = false;
		_meshrenderer.useLightProbes = false;
		_meshrenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		_meshrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		this._has_set_initial_anchor_point = false;
		this.set_tex_rect(texrect);
		this.set_anchor_point(0.5f,0.5f);
		this.set_color(new Vector4(1,1,1,1));
		this.set_sort_z(_sort_z);
		this.set_layer(RLayer.DEFAULT);

		return this;
	}

	public SPSprite set_shader(string shader_key) {
		if (_texkey != null) {
			_meshrenderer.material = GameMain._context._tex_resc.get_material(_texkey,shader_key);
		} else {
			_meshrenderer.material.shader = ShaderResource.get_shader(shader_key);
		}
		return this;
	}

	public string texkey() { return _texkey; }
	public SPSprite set_texkey(string texkey) {
		_texkey = texkey;
		this.set_color(_color);
		return this;
	}

	public override SPNode set_opacity(float val) {
		_color.w = val;
		this.apply_material_params();
		return this;
	}

	public override float get_opacity() { return _color.w; }

	public SPSprite set_color(Vector4 color) {
		_color = color;
		
		// Shouldn't be here, but breaks stuff when removed.
		if (_texkey != null) { _meshrenderer.material = GameMain._context._tex_resc.get_material_default(_texkey); }

		this.apply_material_params();
		return this;
	}
	
	private void apply_material_params() {
		MeshRenderer renderer = _meshrenderer;
		if (_material_block == null) {
			_material_block = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(_material_block);
		}
		_material_block.Clear();
		
		Vector4 tar_color = _color;
		if (_alpha_mult != 1.0f) {
			tar_color.w = tar_color.w * _alpha_mult;
		}
		
		_material_block.AddColor("_Color", tar_color);
		if (_material_color_params != null) {
			foreach (string key in _material_color_params.Keys) {
				_material_block.AddColor(key,_material_color_params[key]);
			}
		}
		renderer.SetPropertyBlock(_material_block);
	}
	
	private Dictionary<string,Color> _material_color_params;
	public void add_material_color_param(string param, Color color) {
		if (_material_color_params == null) _material_color_params = new Dictionary<string, Color>();
		_material_color_params[param] = color;
		this.apply_material_params();
	}

	public Vector4 color() { return _color; }

	public const int VTX_0_0 = 0;
	public const int VTX_1_0 = 1;
	public const int VTX_1_1 = 2;
	public const int VTX_0_1 = 3;

	private Vector2[] __uvs = new Vector2[4];
	public Rect texrect() { return _texrect; }
	public SPSprite set_tex_rect(Rect texrect) {
		if (texrect.x == _texrect.x && texrect.y == _texrect.y && texrect.width == _texrect.width && texrect.height == _texrect.height) return this;
		_texrect = texrect;

		Mesh sprite_mesh = _meshfilter.mesh;
		Texture sprite_tex;
		if (_texkey != null) {
			sprite_tex = GameMain._context._tex_resc.get_tex(_texkey);
		} else if (_manually_set_texture != null) {
			sprite_tex = _manually_set_texture;
		} else {
			Debug.LogError("set_tex_rect texture is null");
			return this;
		}
		
		float tex_wid = sprite_tex.width;
		float tex_hei = sprite_tex.height;
		float x1 = texrect.x;
		float y1 = tex_hei-texrect.height - texrect.y;
		float x2 = texrect.x + texrect.width;
		float y2 = tex_hei-texrect.y;

		Vector2[] uvs = __uvs;
		uvs[VTX_0_0] = new Vector2(x1/tex_wid,y1/tex_hei); //(0,0)
		uvs[VTX_1_0] = new Vector2(x2/tex_wid,y1/tex_hei); //(1,0)
		uvs[VTX_1_1] = new Vector2(x2/tex_wid,y2/tex_hei); //(1,1)
		uvs[VTX_0_1] = new Vector2(x1/tex_wid,y2/tex_hei); //(0,1)
		sprite_mesh.uv = uvs;

		_has_set_initial_anchor_point = false;
		this.set_anchor_point(_anchorpoint.x,_anchorpoint.y);

		return this;
	}

	private Vector3[] __verts = new Vector3[4];
	private bool _has_set_initial_anchor_point = false;
	public override SPNode set_anchor_point(float x, float y) {
		if (_has_set_initial_anchor_point && x == this._anchorpoint.x && y == this._anchorpoint.y) return this;
		_has_set_initial_anchor_point = true;
		base.set_anchor_point(x,y);
		
		Mesh sprite_mesh = _meshfilter.mesh;

		float tex_wid = _texrect.width;
		float tex_hei = _texrect.height;
		
		
		Vector3[] verts = __verts;
		verts[VTX_0_0] = new Vector3(
			(-_anchorpoint.x) * tex_wid,
			(-_anchorpoint.y) * tex_hei
		);
		verts[VTX_1_0] = new Vector3(
			(-_anchorpoint.x + 1) * tex_wid,
			(-_anchorpoint.y) * tex_hei
		);
		verts[VTX_1_1] = new Vector3(
			(-_anchorpoint.x + 1) * tex_wid,
			(-_anchorpoint.y + 1) * tex_hei
		);
		verts[VTX_0_1] = new Vector3(
			(-_anchorpoint.x) * tex_wid,
			(-_anchorpoint.y + 1) * tex_hei
		);
		sprite_mesh.vertices = verts;
		sprite_mesh.RecalculateBounds();
		
		return this;
	}

	public Vector3 w_pos_of_vertex(int i) {
		MeshFilter mesh = _meshfilter;
		return this.transform.TransformPoint(mesh.sharedMesh.vertices[i]);
	}

	public Vector3 u_pos_of_vertex(int i) {
		return GameMain._context.transform.InverseTransformPoint(this.w_pos_of_vertex(i));
	}

	public override void set_sort_z(int zt) {
		if (_meshrenderer != null) _meshrenderer.sortingOrder = zt;
		base.set_sort_z(zt);
	}

	private Texture _manually_set_texture;
	public void manual_set_texture(Texture tex) {
		_texkey = null;
		_manually_set_texture = tex;
		_meshrenderer.material.SetTexture("_MainTex",tex);
	}
	public void manual_set_mesh_size(float tex_wid, float tex_hei) {
		Mesh sprite_mesh = _meshfilter.mesh;
		Vector3[] verts = sprite_mesh.vertices;
		verts[VTX_0_0] = new Vector3(
			(-_anchorpoint.x) * tex_wid,
			(-_anchorpoint.y) * tex_hei
		);
		verts[VTX_1_0] = new Vector3(
			(-_anchorpoint.x + 1) * tex_wid,
			(-_anchorpoint.y) * tex_hei
		);
		verts[VTX_1_1] = new Vector3(
			(-_anchorpoint.x + 1) * tex_wid,
			(-_anchorpoint.y + 1) * tex_hei
		);
		verts[VTX_0_1] = new Vector3(
			(-_anchorpoint.x) * tex_wid,
			(-_anchorpoint.y + 1) * tex_hei
		);
		sprite_mesh.vertices = verts;
		sprite_mesh.RecalculateBounds();
		_texrect = new Rect(0,0,tex_wid,tex_hei);
	}
	
	public void manual_set_vertex(int index, Vector3 val) {
		Mesh sprite_mesh = _meshfilter.mesh;
		Vector3[] verts = sprite_mesh.vertices;
		verts[index] = val;
		sprite_mesh.vertices = verts;
		sprite_mesh.RecalculateBounds();
	}
	
	public void manual_set_uv(int index, Vector2 val) {
		Mesh sprite_mesh = _meshfilter.mesh;
		Vector2[] uvs = __uvs;
		uvs[index] = val;
		sprite_mesh.uv = uvs;
	}
	
	public Vector2 uv_at_vertex(int index) {
		return __uvs[index];
	}
	
	public void set_layer(string layer_str) {
		this.gameObject.layer = RLayer.get_layer(layer_str);
	}
	
	private float _alpha_mult;
	public void set_alpha_mult(float alpha_mult) {
		_alpha_mult = alpha_mult;
		this.apply_material_params();
	}

}
