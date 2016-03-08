using UnityEngine;
using System.Collections;

public class NodeScriptEvent_AddAffinity : NodeScriptEvent {

	private bool _popup_added;
	private Popup _popup;
	
	public override void i_initialize(GameMain game, EventModal modal) {
		_popup_added = false;
	}

	public override void i_update(GameMain game, EventModal modal) {
		if (!_popup_added) {
			game._affinity++;
			game._popups.add_popup("Your friendship has grown!",true);
			game._music.play_sfx("friendship_grown");
			game._music.fade_bgm_for_time(1.0f);
			_popup_added = true;
		} else {
			if (_popup == null) {
				modal.advance_script();
			}
		}
	}
}
