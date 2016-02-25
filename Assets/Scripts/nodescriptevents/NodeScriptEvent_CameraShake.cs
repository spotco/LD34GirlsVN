using UnityEngine;
using System.Collections;

public class NodeScriptEvent_CameraShake : NodeScriptEvent {

	public bool _long = false;

	public override void i_update(GameMain game, EventModal modal) {
		if (_long) {
			game._camera_controller.long_shake();
		} else {
			game._camera_controller.short_shake();
		}
		modal.advance_script();
	}
}
