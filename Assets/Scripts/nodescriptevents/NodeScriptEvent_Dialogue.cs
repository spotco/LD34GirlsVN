using UnityEngine;
using System.Collections;

public class NodeScriptEvent_Dialogue : NodeScriptEvent {

	public static string CHARACTER_NARRATOR = "CHARACTER_NARRATOR";

	public string _character;
	public string _text;
	public float _xpos, _ypos;
	
	private bool _added_bubble;
	private DialogueBubble _tar_bubble;
	
	public override void i_initialize(GameMain game, EventModal modal) {
		_added_bubble = false;
	}
	
	public override void i_update(GameMain game, EventModal modal) {
		if (!_added_bubble) {
			_added_bubble = true;
			_tar_bubble = modal.add_dialogue(game,this);
		} else {
			if (!_tar_bubble.is_active()) {
				modal.advance_script();
			}
		}
	}
}
