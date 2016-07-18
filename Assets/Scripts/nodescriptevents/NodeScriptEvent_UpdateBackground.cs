using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NodeScriptEvent_UpdateBackground : NodeScriptEvent {
	public string _strparam;
	public float _numparam1;
	public float _numparam2;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._background.dispatch_update_message_to_active(_strparam, _numparam1, _numparam2);
		modal.advance_script();
	}
}
