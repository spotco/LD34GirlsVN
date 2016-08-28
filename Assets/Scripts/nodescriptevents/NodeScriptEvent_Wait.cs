using UnityEngine;
using System.Collections;

public class NodeScriptEvent_Wait : NodeScriptEvent {
	public float _wait_time;
	private float _ct;
	
	public override void i_initialize(GameMain game, EventModal modal) {
		_ct = _wait_time;
	}
	
	public override void i_update(GameMain game, EventModal modal) {
		_ct -= SPUtil.dt_scale_get();
		if (_ct <= 0) {
			modal.advance_script();
		}
	}
}
