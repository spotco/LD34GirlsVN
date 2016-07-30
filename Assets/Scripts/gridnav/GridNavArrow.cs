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
		ShowingToHidden
	}
	enum SelectedMode {
		Selected,
		Unselected
	}
	private ShowingMode _showing_mode;
	private SelectedMode _selected_mode;
	private float _anim_ct;
	
	public GridNavArrow i_initialize(GridNode.Directional directional) {
		
		Vector2 dir_vec = GridNode.directional_to_vector(directional);
		_arrow.transform.localRotation = SPUtil.set_rotation_quaternion(_arrow.transform.localRotation, new Vector3(0,0,SPUtil.rad_to_deg(Mathf.Atan2(dir_vec.y,dir_vec.x)) + 180));
		
		_showing_mode = ShowingMode.Hidden;
		_selected_mode = SelectedMode.Unselected;
		_anim_ct = 0;
		
		return this;
	}
	
	public void set_is_showing_is_selected(bool showing, bool selected, bool imm = false) {
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
			
		} else if (_showing_mode == ShowingMode.HiddenToShowing) {
			_anim_ct += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.lerp(0,0.75f,_anim_ct);
			
			if (_anim_ct >= 1) {
				_showing_mode = ShowingMode.Showing;
			}
			
		} else if (_showing_mode == ShowingMode.Showing) {
			_canvas_group.alpha = 0.75f;
			float tar_scale = 1;
			if (_selected_mode == SelectedMode.Selected) {
				tar_scale = 1.5f;
			}
			this.transform.localScale = SPUtil.valv(SPUtil.drpt(this.transform.localScale.x,tar_scale,1/10.0f));
		
		} else if (_showing_mode == ShowingMode.ShowingToHidden) {
			_anim_ct += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.lerp(0.75f,0,_anim_ct);
			
			if (_anim_ct >= 1) {
				_showing_mode = ShowingMode.Hidden;
			}
		}
	}
	
	public void i_update() {
		state_update();
	}

}
