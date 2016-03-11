using UnityEngine;
using System.Collections;

public class NodeScriptEvent_HideCharacter : NodeScriptEvent {
	public string _character;
	public bool _imm;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter tar = modal.cond_get_character_of_name(_character);
		if (tar != null) {
			if (_imm) {
				tar.imm_hide();
				modal.clear_removed_characters(game);
			}
			tar._current_mode = EventCharacter.Mode.FadeOut;
		}
		modal.advance_script();
	}
	
}
