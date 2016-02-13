using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueBubble : MonoBehaviour {

	[SerializeField] private ScrollText _primary_text;
	[SerializeField] private Text _name_text;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private Image _primary_background;
	[SerializeField] private Image _name_background;
	[SerializeField] private Image _cursor;
	[SerializeField] private RawImage _rendered_text;
	
	[SerializeField] private Outline _primary_text_outline;
	[SerializeField] private Outline _name_text_outline;
	
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
	
	public FlashEvery _dialogue_scroll_sound_flash;
	
	public void i_initialize(GameMain game, NodeScriptEvent_Dialogue dialogue) {
		_script = dialogue;
		_dialogue_scroll_sound_flash = FlashEvery.cons(5);
		
		_primary_text.gameObject.SetActive(false);
		_rendered_text.texture = game._sptext.get_tex();
		
		if (dialogue._character == NodeScriptEvent_Dialogue.CHARACTER_NARRATOR) {
			_name_background.gameObject.SetActive(false);
		} else {
			_name_text.text = dialogue._character;
		}
		
		this.apply_style(dialogue);
		
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
			
			_dialogue_scroll_sound_flash.i_update();
			if (_dialogue_scroll_sound_flash.do_flash()) {
				this.text_scroll_tick_sound(game);
			}
			
			_cursor.gameObject.SetActive(false);
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				_primary_text.finish();
				game._music.play_sfx("dialogue_button_press");
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
				game._music.play_sfx("dialogue_button_press");
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
	
	private static Dictionary<string,Sprite> __name_to_bgsprite = new Dictionary<string, Sprite>();
	private Sprite cond_get_bgsprite(string name) {
		name = "img/ui/dialogue_bubble_"+name;
		
		
		if (__name_to_bgsprite.ContainsKey(name)) return __name_to_bgsprite[name];
		Sprite bg_sprite = Resources.Load<Sprite>(name);
		if (bg_sprite != null) {
			__name_to_bgsprite[name] = bg_sprite;
		} else {
			Debug.LogError("not found:"+name);
		}
		return bg_sprite;
	}
	
	public void apply_style(NodeScriptEvent_Dialogue script_event) {
		Color outline_color;
		if (script_event._character == "Kurumi") {
			_text_scroll_sound = TEXT_SCROLL_SFX_KURUMI;
			_primary_background.sprite = this.cond_get_bgsprite("kurumi");
			_name_background.sprite = this.cond_get_bgsprite("kurumi");
			outline_color = new Color(95/255.0f,115/255.0f,88/255.0f,1);
		
		} else if (script_event._character == "Mana" || script_event._character == "Pink Hair" || script_event._character == "Hero") {
			_text_scroll_sound = TEXT_SCROLL_SFX_MANA;
			_primary_background.sprite = this.cond_get_bgsprite("mana");
			_name_background.sprite = this.cond_get_bgsprite("mana");
			outline_color = new Color(108/255.0f,99/255.0f,132/255.0f,1);
		
		} else if (script_event._character == "Raichi" || script_event._character == "Older Student") {
			_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
			_primary_background.sprite = this.cond_get_bgsprite("raichi");
			_name_background.sprite = this.cond_get_bgsprite("raichi");
			outline_color = new Color(85/255.0f,99/255.0f,125/255.0f,1);
		
		} else if (script_event._character == "Simone") {
			_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
			_primary_background.sprite = this.cond_get_bgsprite("simone");
			_name_background.sprite = this.cond_get_bgsprite("simone");
			outline_color = new Color(127/255.0f,121/255.0f,85/255.0f,1);
		
		} else {
			
			if (script_event._character == NodeScriptEvent_Dialogue.CHARACTER_NARRATOR) {
				_text_scroll_sound = TEXT_SCROLL_SFX_NARRATOR;
			} else {
				_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
			}
		
			_primary_background.sprite = this.cond_get_bgsprite("generic");
			_name_background.sprite = this.cond_get_bgsprite("generic");
			outline_color = new Color(94/255.0f,94/255.0f,94/255.0f,1);
		}
		_name_text_outline.effectColor = outline_color;
		_primary_text_outline.effectColor = outline_color;
	}
	
	private static string TEXT_SCROLL_SFX_NARRATOR = "text_scroll_2";
	private static string TEXT_SCROLL_SFX_KURUMI = "text_scroll_4";
	private static string TEXT_SCROLL_SFX_RAICHI = "text_scroll_1";
	private static string TEXT_SCROLL_SFX_MANA = "text_scroll_6";
	
	private string _text_scroll_sound = DialogueBubble.TEXT_SCROLL_SFX_NARRATOR;
	
	private void text_scroll_tick_sound(GameMain game) {
		game._music.play_sfx(_text_scroll_sound);
	}
	
	
	
	
}
