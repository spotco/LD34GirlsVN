using UnityEngine;
using System.Collections;

public class NodeScriptEvent_PlaySFX : NodeScriptEvent {
	public string _sfx;

	public override void i_update(GameMain game, EventModal modal) {
		game._music.play_sfx(_sfx);
		modal.advance_script();
	}
}
