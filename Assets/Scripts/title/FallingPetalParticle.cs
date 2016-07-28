using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FallingPetalParticle {
	
	public interface BoundedParent {
		SPHitRect get_screen_bounds();
	}
	
	public static FallingPetalParticle cons(RectTransform img, BoundedParent title_modal) {
		return (new FallingPetalParticle()).i_cons(img, title_modal);
	}
	
	private RectTransform _img;
	private Vector3 _rotation, _vrotation; 
	private Vector2 _vel;
	
	private FallingPetalParticle i_cons(RectTransform img, BoundedParent title_modal) {
		_img = img;
		
		_img.gameObject.SetActive(false);
		
		return this;
	}
	
	public void spawn(BoundedParent title_modal) {
		_img.gameObject.SetActive(true);
		
		SPHitRect title_bounds = title_modal.get_screen_bounds();
		
		if (SPUtil.float_random(0,2) <= 1) {
			_img.anchoredPosition = new Vector2(
				SPUtil.float_random( SPUtil.lerp(title_bounds._x1,title_bounds._x2,0.25f) ,title_bounds._x2),
				title_bounds._y2 + 100
			);
		} else {
			_img.anchoredPosition = new Vector2(
				title_bounds._x2 + 100,
				SPUtil.float_random( SPUtil.lerp(title_bounds._y1,title_bounds._y2,0.25f),title_bounds._y2)
			);
		}
		
		_rotation = new Vector3(0, 0, SPUtil.float_random(20, 80));
		_vrotation = new Vector3(
			SPUtil.float_random(-2,2),
			SPUtil.float_random(-4,4),
			SPUtil.float_random(-3,3)
		);
		_vel = new Vector2(
			SPUtil.float_random(-5,-1.5f),
			SPUtil.float_random(-5,-1.5f)
		);
		_img.localScale = SPUtil.valv(Mathf.Clamp(SPUtil.y_for_point_of_2pt_line(new Vector2(8,1),new Vector2(1,0.25f), _vel.magnitude),0.25f,1));
		
		this.i_update(title_modal);
	}
	
	public void i_update(BoundedParent title_modal) {
		_img.anchoredPosition = SPUtil.vec_add(
			_img.anchoredPosition,
			SPUtil.vec_scale(_vel, SPUtil.dt_scale_get())
		);
		_rotation = SPUtil.vec_add(
			_rotation,
			SPUtil.vec_scale(_vrotation, SPUtil.dt_scale_get())
		);
		_img.localRotation = SPUtil.set_rotation_quaternion(_img.localRotation, _rotation);
	}
	
	public bool should_remove(BoundedParent title_modal) {
		SPHitRect title_bounds = title_modal.get_screen_bounds();
		Vector2 pos = _img.anchoredPosition;
		return (pos.x < title_bounds._x1) || (pos.y < title_bounds._y1);
	}
	
	public void do_remove(BoundedParent title_modal) {
		_img.gameObject.SetActive(false);
	}
	
}
