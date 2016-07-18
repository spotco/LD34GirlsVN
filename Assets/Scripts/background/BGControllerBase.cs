using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGControllerBase : MonoBehaviour {

	public static string KEY_DEFAULT = "default";
	
	protected enum ShowingMode {
		Hidden,
		TransitionShowing,
		Showing,
		TransitionHidden
	};
	protected ShowingMode _current_showing_mode = ShowingMode.Hidden;
	protected void update_showing_mode(Image fade_cover) {
		if (_current_showing_mode == ShowingMode.TransitionShowing) {
			float t = fade_cover.color.a;
			t -= SPUtil.sec_to_tick(1.5f) * SPUtil.dt_scale_get();
			
			if (t > 0) {
				fade_cover.color = new Color(0,0,0,t);
			} else {
				fade_cover.color = new Color(0,0,0,0);
				_current_showing_mode = ShowingMode.Showing;
			}
			
			
		
		} else if (_current_showing_mode == ShowingMode.TransitionHidden) {
			float t = fade_cover.color.a;
			t += SPUtil.sec_to_tick(1.5f) * SPUtil.dt_scale_get();
			
			if (t > 0) {
				fade_cover.color = new Color(0,0,0,t);
			} else {
				fade_cover.color = new Color(0,0,0,0);
				_current_showing_mode = ShowingMode.Hidden;
			}
		}
	}
	
	

	public virtual void i_initialize(GameMain game) {}
	public virtual string get_registered_name() { return ""; }
	public virtual void show_background(string name, string key) {}
	public virtual void i_update(GameMain game) {}
	
	public virtual bool should_remain_active() { return _current_showing_mode != ShowingMode.Hidden; }
	public virtual void set_showing(bool val) {}
}
