using UnityEngine;
using System.Collections;

public class NodeScriptEvent_MoveCharacter : NodeScriptEvent {
	public string _character;
	public float _xto;
	
	private bool _first_update;
	private float _anim_t;
	private float _frame_dt;
	private float _xstart;
	
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
			float dist = Mathf.Abs(_xto - tar.transform.localPosition.x);
			_frame_dt = 1.0f / (dist / 10.0f);
			_xstart = tar.transform.localPosition.x;
		}
		
		_anim_t = Mathf.Min(_anim_t + _frame_dt*SPUtil.dt_scale_get(),1);
		
		tar.transform.localPosition = new Vector2(
			SPUtil.lerp(_xstart,_xto,SPUtil.bezier_val_for_t(new Vector2(0,0),new Vector2(0.5f,0),new Vector2(0.5f,1),new Vector2(1,1),_anim_t).y), 
			tar.transform.localPosition.y
		);
		
		if (_anim_t >= 1) {
			modal.advance_script();
		}
		
	
	}
	
}
