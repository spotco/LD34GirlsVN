using UnityEngine;
using System.Collections;

public class NodeScriptEvent_CharacterEffect : NodeScriptEvent {
	
	public string _character;
	public string _effect;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter tar = modal.cond_get_character_of_name(_character);
		if (tar != null) {
			if (_effect == "managodparticles") {
				tar.add_effect(game,GodPowerCharacterEffect.cons());
			} else if (_effect == "heartburst") {
			
			} else {
				SPUtil.errf("No such charactereffect(%s)",_effect);
			}
		} else {
			SPUtil.errf("No such effect(%s)",_character);
		}
		modal.advance_script();
	}
}
