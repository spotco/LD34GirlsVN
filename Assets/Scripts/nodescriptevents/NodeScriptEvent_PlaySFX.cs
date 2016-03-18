using UnityEngine;
using System.Collections;

public class NodeScriptEvent_PlaySFX : NodeScriptEvent {
	public string _sfx;

	public override void i_update(GameMain game, EventModal modal) {
		if (_sfx == "school_bell") {
			game._music.fade_bgm_for_time(4.0f);
		} else if (_sfx == "transform") {
			game._music.fade_bgm_for_time(2.0f);
		} else if (_sfx == "phone_buzz") {
			game._music.fade_bgm_for_time(1.5f);
		} else if (_sfx == "phone_unlock") {
			game._music.fade_bgm_for_time(0.75f);
		} else if (_sfx == "stomach_growl") {
			game._music.fade_bgm_for_time(1.5f);
		}
		game._music.play_sfx(_sfx);
		modal.advance_script();
	}
}
