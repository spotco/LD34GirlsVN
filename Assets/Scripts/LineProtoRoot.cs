using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LineProtoRoot : MonoBehaviour {
	
	private Material _line_fade_edges_shader_material;
	public RectTransform _rect_transform;
	public Image _image;
	private Canvas _parent_canvas;
	
	private float _anim_t = 0;
	
	public void i_initialize() {
		_image = this.GetComponent<Image>();
		_line_fade_edges_shader_material = new Material(_image.material);
		_image.material = _line_fade_edges_shader_material;
		
		_rect_transform = this.GetComponent<RectTransform>();
		_parent_canvas = this.GetComponentInParent<Canvas>();
	}
	
	public void i_update(bool selected) {
		Vector2 rect_top_left = _parent_canvas.transform.InverseTransformPoint(_rect_transform.TransformPoint(new Vector2(_rect_transform.rect.xMin, _rect_transform.rect.yMax)));
		Vector2 rect_top_right = _parent_canvas.transform.InverseTransformPoint(_rect_transform.TransformPoint(new Vector2(_rect_transform.rect.xMax, _rect_transform.rect.yMax)));
		
		_line_fade_edges_shader_material.SetVector("_SpriteLeft", rect_top_left);
		_line_fade_edges_shader_material.SetVector("_SpriteRight", rect_top_right);
		
		float anim_speed = 0.002f;
		if (selected) {
			anim_speed = 0.005f;
		}
		
		_anim_t = _anim_t - anim_speed * SPUtil.dt_scale_get();
		if (_anim_t < 0) {
			_anim_t = 1 + _anim_t;
		}
		
		_line_fade_edges_shader_material.SetVector("_AnimT", new Vector4(_anim_t,0,0,0));
	}
	
}
