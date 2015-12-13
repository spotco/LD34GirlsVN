using UnityEngine;
using System.Collections;

public class NodeScriptEvent_MoveCharacter : NodeScriptEvent {
	public string _character;
	public float _xto;
	
	private bool _first_update;
	private float _anim_t;
	private float _frame_dt;
	
	public override void i_initialize(GameMain game, EventModal modal) {
		_first_update = false;
	}
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter tar = modal.cond_get_character_of_name(_character);
		if (tar == null) {
			modal.advance_script();
			return;
		}
		if (!_first_update) {
			_first_update = true;
			_anim_t = 0;
			float dist = Mathf.Abs(_xto - tar._image.transform.localPosition.x);
			
		}
	
	}
	
}
