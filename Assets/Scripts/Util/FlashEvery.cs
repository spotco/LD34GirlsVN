using UnityEngine;
using System.Collections.Generic;

public class FlashEvery {
	public float _max_time;
	public float _time;
	public static FlashEvery cons(float time) {
		return new FlashEvery() { _max_time = time };
	}
	public void i_update() {
		_time -= SPUtil.dt_scale_get();
	}
	public bool do_flash() {
		bool rtv = _time <= 0;
		if (rtv) _time = _max_time;
		return rtv;
	}
}

public class FlashCount {
	private List<float> _counts = new List<float>();
	private bool _sorted = true;
	private int _i = 0;
	public static FlashCount cons() {
		return (new FlashCount());
	}
	public FlashCount add_flash_at(float time) {
		_counts.Add(time);
		_sorted = false;
		return this;
	}
	public void reset() {
		_i = 0;
	}
	public bool do_flash_given_time(float time) {
		if (!_sorted) {
			_sorted = true;
			_counts.Sort((float a, float b)=>{return (int)(b-a);});
		}
		if (_i >= _counts.Count) return false;
		bool rtv = _counts[_i] >= time;
		if (rtv) _i++;
		return rtv;
	}
}