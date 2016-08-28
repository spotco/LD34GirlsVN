using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavArrow : MonoBehaviour {

	[SerializeField] private Image _arrow_back;
	[SerializeField] private Image _arrow;
	[SerializeField] private CanvasGroup _canvas_group;
	
	enum ShowingMode {
		Hidden,
		HiddenToShowing,
		Showing,
		ShowingToHidden,
		SelectedShowingToHidden
	}
	enum SelectedMode {
		Selected,
		Unselected
	}
	private ShowingMode _showing_mode;
	private SelectedMode _selected_mode;
	private float _anim_ct;
	private RectTransform _rect_transform;
	
	private float _cursor_yvel;
	
	public GridNavArrow i_initialize(GridNode.Directional directional) {
		
		Vector2 dir_vec = GridNode.directional_to_vector(directional);
		_arrow.transform.localRotation = SPUtil.set_rotation_quaternion(_arrow.transform.localRotation, new Vector3(0,0,SPUtil.rad_to_deg(Mathf.Atan2(dir_vec.y,dir_vec.x)) + 180));
		
		_showing_mode = ShowingMode.Hidden;
		_selected_mode = SelectedMode.Unselected;
		_anim_ct = 0;
		_rect_transform = _arrow_back.GetComponent<RectTransform>();
		
		_cursor_yvel = 0;
		
		return this;
	}
	
	public void set_is_showing_is_selected(bool showing, bool selected, bool imm = false) {
		if (_showing_mode == ShowingMode.SelectedShowingToHidden && imm == false) {
			return;
		}
		_selected_mode = selected ? SelectedMode.Selected : SelectedMode.Unselected;
		
		if (imm) {
			if (showing) {
				_showing_mode = ShowingMode.Showing;
			} else {
				_showing_mode = ShowingMode.Hidden;
			}
			state_update();
			return;
		}
		
		if (showing && _showing_mode != ShowingMode.Showing && _showing_mode != ShowingMode.HiddenToShowing) {
			_showing_mode = ShowingMode.HiddenToShowing;
			_anim_ct = 0;
			
		} else if (!showing && _showing_mode != ShowingMode.Hidden && _showing_mode != ShowingMode.ShowingToHidden) {
			_showing_mode = ShowingMode.ShowingToHidden;
			_anim_ct = 0;
		}
	}
	
	private void state_update() {
		if (_showing_mode == ShowingMode.Hidden) {
			_canvas_group.alpha = 0;
			this.transform.localScale = SPUtil.valv(1);
			this.cursor_to_default_anim_update();
			
		} else if (_showing_mode == ShowingMode.HiddenToShowing) {
			_anim_ct += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.lerp(0,0.5f,_anim_ct);
			
			this.cursor_to_default_anim_update();
			float scx = _arrow_back.transform.localScale.x;
			float scy = _arrow_back.transform.localScale.y;
			_arrow_back.transform.localScale = new Vector3(
				SPUtil.drpt(scx,0.85f,1/5.0f),
				SPUtil.drpt(scy,0.85f,1/5.0f),
				1
			);
			if (_anim_ct >= 1) {
				_showing_mode = ShowingMode.Showing;
			}
			
			
		} else if (_showing_mode == ShowingMode.Showing) {
			
			float tar_scale = 0.85f;
			if (_selected_mode == SelectedMode.Selected) {
				this.cursor_anim_update();
				tar_scale = 1.15f;
				_canvas_group.alpha = 0.75f;
			} else {
				this.cursor_to_default_anim_update();
				_canvas_group.alpha = 0.5f;
			}
			this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,tar_scale,1/5.0f));
		
		} else if (_showing_mode == ShowingMode.ShowingToHidden) {
			_anim_ct += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.lerp(0.75f,0,_anim_ct);
			
			this.cursor_to_default_anim_update();
			float scx = _arrow_back.transform.localScale.x;
			float scy = _arrow_back.transform.localScale.y;
			_arrow_back.transform.localScale = new Vector3(
				SPUtil.drpt(scx,0,1/5.0f),
				SPUtil.drpt(scy,0,1/5.0f),
				1
			);
			
			if (_anim_ct >= 1) {
				_showing_mode = ShowingMode.Hidden;
			}
			
		} else if (_showing_mode == ShowingMode.SelectedShowingToHidden) {
			_anim_ct += SPUtil.sec_to_tick(0.45f) * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.drpt(_canvas_group.alpha,0,1/10.0f);
			
			float scx = _arrow_back.transform.localScale.x;
			float scy = _arrow_back.transform.localScale.y;
			_arrow_back.transform.localScale = new Vector3(
				SPUtil.drpt(scx,1.5f,1/7.0f),
				SPUtil.drpt(scy,1.5f,1/7.0f),
				1
			);
			if (_anim_ct >= 1) {
				_showing_mode = ShowingMode.Hidden;
			}
		
		} else if (_showing_mode == ShowingMode.Hidden) {
			_canvas_group.alpha = 0;
			this.cursor_to_default_anim_update();
			_arrow_back.transform.localScale = SPUtil.valv(0);
		}
	}
	
	public void trigger_selected() {
		_showing_mode = ShowingMode.SelectedShowingToHidden;
		_anim_ct = 0;
	}
	
	private void cursor_anim_update() {
		float cursor_ypos = _arrow_back.transform.localPosition.y;
		_cursor_yvel -= 0.1f * SPUtil.dt_scale_get();
		cursor_ypos += _cursor_yvel * SPUtil.dt_scale_get();
		
		_arrow_back.transform.localScale = new Vector3(
			SPUtil.drpt(_arrow_back.transform.localScale.x, SPUtil.y_for_point_of_2pt_line(new Vector2(-3.5f,1.35f),new Vector2(0,1), _cursor_yvel), 1/5.0f),
			SPUtil.drpt(_arrow_back.transform.localScale.y, SPUtil.y_for_point_of_2pt_line(new Vector2(-3.5f,1),new Vector2(0,1.1f), _cursor_yvel), 1/5.0f),
			1
		);
		if (_cursor_yvel < 0 && cursor_ypos < 0) {
			cursor_ypos = 0;
			_cursor_yvel = 1.45f;
		}
		_arrow_back.transform.localPosition = new Vector3(_arrow_back.transform.localPosition.x, cursor_ypos, _arrow_back.transform.localPosition.z);
	}
	
	private void cursor_to_default_anim_update() {
		_arrow_back.transform.localScale = new Vector3(
			SPUtil.drpt(_arrow_back.transform.localScale.x, 1, 1/5.0f),
			SPUtil.drpt(_arrow_back.transform.localScale.y, 1, 1/5.0f),
			1
		);
		float cursor_ypos = _arrow_back.transform.localPosition.y;
		cursor_ypos = SPUtil.drpt(cursor_ypos,0,1/5.0f);
		
		_arrow_back.transform.localPosition = new Vector3(_arrow_back.transform.localPosition.x, cursor_ypos, _arrow_back.transform.localPosition.z);
		_cursor_yvel = 0;
	}
	
	public RectTransform get_recttransform() { return _rect_transform; }
	
	public void i_update() {
		state_update();
	}

}
