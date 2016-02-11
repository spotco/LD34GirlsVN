using UnityEngine;
using System.Collections;

public class NodeScriptEvent_AddAffinity : NodeScriptEvent {

	public override void i_update(GameMain game, EventModal modal) {
		modal.advance_script();
		game._affinity++;
		game._popups.add_popup("Your friendship has grown!",true);
		game._music.play_sfx("friendship_grown");
		game._music.fade_bgm_for_time(1.0f);
	}
}
