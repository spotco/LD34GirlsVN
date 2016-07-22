using UnityEngine;
using System.Collections;

public class NodeScriptEvent_ShowCharacter : NodeScriptEvent {
	public string _character;
	public string _image;
	public float _xpos;
	public float _ypos;
	public float _xscale;
	public bool _imm;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter neu_char = modal.add_character(game,_character,_image);
		if (_imm) {
			neu_char.imm_show();
		}
		if (neu_char != null) {
			neu_char.transform.localPosition = new Vector3(
				_xpos,
				neu_char.transform.localPosition.y + _ypos
			);
			
			neu_char.set_facing(_xscale);
		}
		modal.advance_script();
	}
	
}
