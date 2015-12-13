using UnityEngine;
using System.Collections;

public class NodeScriptEvent_ChangeBackground : NodeScriptEvent {
	public string _background;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._background.load_background(_background);
		modal.advance_script();
	}

}
