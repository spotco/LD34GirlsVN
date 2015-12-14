using UnityEngine;
using System.Collections;

public class NodeScriptEvent_HideCharacter : NodeScriptEvent {
	public string _character;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter tar = modal.cond_get_character_of_name(_character);
		if (tar != null) {
			tar._current_mode = EventCharacter.Mode.FadeOut;
		}
		modal.advance_script();
	}
	
}
