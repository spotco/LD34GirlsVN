using UnityEngine;
using System.Collections;

public class NodeScriptEvent_ChangeBackground : NodeScriptEvent {
	public string _background;
	public string _key;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._background.load_background(_background, _key);
		modal.advance_script();
	}

}
