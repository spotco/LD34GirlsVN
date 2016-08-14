using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SPTimedSpriteAnimator {
	
	public static SPTimedSpriteAnimator cons(SPSpriteAnimator.Target target) {
		return (new SPTimedSpriteAnimator()).i_cons(target);
	}
	
	private SPSpriteAnimator.Target _target;
	private Dictionary<float,Rect> _time_to_frames = new Dictionary<float, Rect>();
	private List<float> _sorted_times = new List<float>();
	public SPTimedSpriteAnimator i_cons(SPSpriteAnimator.Target target) {
		this.set_target(target);
		return this;
	}
	
	public SPTimedSpriteAnimator set_target(SPSpriteAnimator.Target target) {
		_target = target;
		return this;
	}
	
	public SPTimedSpriteAnimator add_frame_at_time(Rect frame, float time) {
		_time_to_frames[time] = frame;
		_sorted_times.Add(time);
		_sorted_times.Sort((float a, float b) => { return SPUtil.sig(a-b); });
		return this;
	}
	
	public SPTimedSpriteAnimator show_frame_for_time(float t) {
		float key = _sorted_times[_sorted_times.Count-1];
		for (int i = 0; i < _sorted_times.Count; i++) {
			if (t <= _sorted_times[i]) {
				key = _sorted_times[i];
				break;
			}
		}
		_target.set_tex_rect(_time_to_frames[key]);
		return this;
	}
	
}