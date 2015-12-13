using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueBubble : MonoBehaviour {

	[SerializeField] private ScrollText _primary_text;
	[SerializeField] private Text _name_text;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private Image _primary_background;
	[SerializeField] private Image _name_background;
	[SerializeField] private Image _cursor;
	
	public enum Mode {
		FadeIn,
		TextIn,
		Finished,
		FadeOut,
		DoRemove
	}
	
	public Mode _current_mode;
	private float _anim_t;
	
	public NodeScriptEvent_Dialogue _script;
	
	public void i_initialize(NodeScriptEvent_Dialogue dialogue) {
		_script = dialogue;
		_name_text.text = dialogue._character;
		_primary_text.load(dialogue._text);
		_current_mode = Mode.FadeIn;
		
		_anim_t = 0;
		_canvas_group.alpha = 0;
		this.transform.localScale = SPUtil.valv(1.2f);
		this.transform.localPosition = new Vector2(dialogue._xpos,dialogue._ypos);
		
	}
	
	public void i_update(GameMain game, EventModal modal) {
		if (_current_mode == Mode.FadeIn) {
			_anim_t += 0.05f * SPUtil.dt_scale_get();
			_cursor.gameObject.SetActive(false);
			
			_canvas_group.alpha = _anim_t;
			this.transform.localScale = SPUtil.valv(SPUtil.y_for_point_of_2pt_line(new Vector2(0,1.2f),new Vector2(1,1),_anim_t));
			
			if (_anim_t >= 1) {
				_current_mode = Mode.TextIn;
			}
		
		} else if (_current_mode == Mode.TextIn) {
			_primary_text.i_update();
			_cursor.gameObject.SetActive(false);
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				_primary_text.finish();
			}
			if (_primary_text.finished()) {
				_current_mode = Mode.Finished;
			}
		
		} else if (_current_mode == Mode.Finished) {
			_anim_t += SPUtil.dt_scale_get();
			if (_anim_t > 20) {
				_cursor.gameObject.SetActive(!_cursor.gameObject.activeSelf);
				_anim_t = 0;
			}
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				_current_mode = Mode.FadeOut;
				_anim_t = 0;
			}
		
		} else if (_current_mode == Mode.FadeOut) {
			_cursor.gameObject.SetActive(false);
			_anim_t += 0.05f * SPUtil.dt_scale_get();
			_canvas_group.alpha = SPUtil.y_for_point_of_2pt_line(new Vector2(0,1.0f),new Vector2(1,0),_anim_t);
			if (_anim_t >= 1) {
				_current_mode = Mode.DoRemove;
			}	
		}
	}
	
	
	
	
}
