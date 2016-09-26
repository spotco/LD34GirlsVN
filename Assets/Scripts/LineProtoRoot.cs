using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LineProtoRoot : SPBaseBehavior {
	
	private Material _line_fade_edges_shader_material;
	public RectTransform _rect_transform;
	public Image _image;
	private Canvas _parent_canvas;
	
	private SPMaterialModifier _material_modif;
	
	private float _anim_t = 0;
	private bool _selected = false;
	private Color _tar_color = new Color(1,1,1,0);
	private float _tar_height = 0;
	
	public void i_initialize() {
		_image = this.GetComponent<Image>();
		_image.mainTexture.wrapMode = TextureWrapMode.Repeat;
		_line_fade_edges_shader_material = new Material(_image.material);
		_image.material = _line_fade_edges_shader_material;
		
		_material_modif = this.gameObject.AddComponent<SPMaterialModifier>();
		_material_modif.i_initialize();
		
		_rect_transform = this.GetComponent<RectTransform>();
		_parent_canvas = this.GetComponentInParent<Canvas>();
		
		this._image.color = _tar_color;
		this._rect_transform.localScale = new Vector3(
			this._rect_transform.localScale.x, 
			_tar_height,
			this._rect_transform.localScale.z
		);
	}
	
	public void set_selected(bool selected) {
		_selected = selected;
	}
	
	public void set_tar_color(Color color) {
		_tar_color = color;
	}
	
	public void set_tar_height(float height) {
		_tar_height = height;
	}
	
	public void i_update() {
		if (this._image.color.a == 0 && _tar_color.a == 0) {
			return;
		}
		
		Vector2 rect_top_left = _parent_canvas.transform.InverseTransformPoint(_rect_transform.TransformPoint(new Vector2(_rect_transform.rect.xMin, _rect_transform.rect.yMax)));
		Vector2 rect_top_right = _parent_canvas.transform.InverseTransformPoint(_rect_transform.TransformPoint(new Vector2(_rect_transform.rect.xMax, _rect_transform.rect.yMax)));
		
		_material_modif.set_vector("_SpriteLeft", rect_top_left);
		_material_modif.set_vector("_SpriteRight", rect_top_right);
		
		float anim_speed = 0.002f;
		if (_selected) {
			anim_speed = 0.005f;
		}
		
		_anim_t = _anim_t + anim_speed * SPUtil.dt_scale_get();
		if (_anim_t > 1) {
			_anim_t = (_anim_t % 1.0f);
		}
		
		_material_modif.set_vector("_AnimT", new Vector4(1-_anim_t,0,0,0));
		_material_modif.finish_set();
		
		this._image.color = new Color(
			_tar_color.r, _tar_color.g, _tar_color.b, SPUtil.lmovto(this._image.color.a, _tar_color.a, 0.05f * SPUtil.dt_scale_get())
		);
		
		this._rect_transform.localScale = new Vector3(
			this._rect_transform.localScale.x, 
			SPUtil.drpt(this._rect_transform.localScale.y,_tar_height,1/10.0f), 
			this._rect_transform.localScale.z
		);
	}
	
}
