using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueBubble : SPBaseBehavior {

	[SerializeField] private Text _name_text;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private Image _primary_background;
	[SerializeField] private Image _name_background;
	[SerializeField] private RectTransform _nametag;
	[SerializeField] private Image _cursor;
	[SerializeField] private Image _cursor_shadow;
	[SerializeField] private SPText _rendered_text;
	
	private ScrollText _primary_text = new ScrollText();
	[SerializeField] private Outline _name_text_outline;
	
	private float _cursor_yvel;
	private float _cursor_start_ypos;
	private float _cursor_scale_mult = 0;
	
	public enum Mode {
		FadeIn,
		TextIn,
		Finished,
		FadeOut,
		DoRemove
	}
	
	public Mode _current_mode;
	private float _anim_t;
	private float _name_bounce_t;
	
	public Vector3 _name_initial_pos;
	
	public NodeScriptEvent_Dialogue _script;
	
	public FlashEvery _dialogue_scroll_sound_flash;
	
	public static DialogueBubble cons(GameMain game, NodeScriptEvent_Dialogue dialogue, DialogueBubble proto) {
		DialogueBubble rtv = game._objpool.spbasebehavior_depool<DialogueBubble>(false);
		if (rtv == null) {
			rtv = SPUtil.proto_clone(proto.gameObject).GetComponent<DialogueBubble>();
			rtv.depool();
			rtv._name_initial_pos = rtv._nametag.transform.localPosition;
		}
		SPUtil.proto_copy_transform(rtv.gameObject,proto.gameObject);
		rtv.i_cons(game,dialogue);
		return rtv;
	}
	
	public void cleanup(GameMain game) {
		_rendered_text.clear();
		_current_mode = Mode.FadeIn;
		game._objpool.spbasebehavior_repool<DialogueBubble>(this);
	}
	
	private void i_cons(GameMain game, NodeScriptEvent_Dialogue dialogue) {
		this.gameObject.SetActive(true);
		//_rendered_text.gameObject.SetActive(true);
		_script = dialogue;
		_dialogue_scroll_sound_flash = FlashEvery.cons(5);
		
		//game._sptext.get_text().transform.SetParent(_rendered_text.transform);
		
		_rendered_text.i_cons_text(RTex.OSAKA_FNT, RFnt.OSAKA, SPText.SPTextStyle.cons(Vector4.zero, Vector4.zero, Vector4.zero, 0, 0));
		_rendered_text.clear();
		
		//game._sptext.clear();
		//_rendered_text.texture = game._sptext.get_tex();
		
		if (dialogue._character == NodeScriptEvent_Dialogue.CHARACTER_NARRATOR) {
			_nametag.gameObject.SetActive(false);
		} else {
			_nametag.gameObject.SetActive(true);
			_name_text.text = dialogue._character;
		}
		
		this.apply_style(game._sptext, dialogue);
		
		
		_primary_text.reset();
		_primary_text._text = _rendered_text;
		_primary_text._text_manager = game._sptext;
		_primary_text.load(dialogue._text);
		
		_current_mode = Mode.FadeIn;
		
		_anim_t = 0;
		_canvas_group.alpha = 0;
		this.transform.localScale = SPUtil.valv(1.2f);
		this.transform.localPosition = new Vector2(dialogue._xpos,dialogue._ypos);
		_cursor.gameObject.SetActive(false);
		_cursor.color = new Color(1,1,1,1);
		_cursor.transform.localScale = SPUtil.valv(1);
		
		_cursor_shadow.transform.position = _cursor.transform.position;
		
		_cursor_yvel = 0;
		_cursor_start_ypos = -60;
		
		_name_bounce_t = 0;
		_nametag.transform.localPosition = _name_initial_pos;
	}
	
	public void i_update(GameMain game) {
		
		//_rendered_text.i_update();
		
		if (_current_mode == Mode.FadeIn) {
			_anim_t = Mathf.Clamp(_anim_t + SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get(),0,1);
			_cursor.gameObject.SetActive(false);
			
			_canvas_group.alpha = _anim_t;
			this.transform.localScale = SPUtil.valv(SPUtil.y_for_point_of_2pt_line(new Vector2(0,1.2f),new Vector2(1,1),
				SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0.2f,-0.2f), new Vector2(0.8f,1.2f), new Vector2(1,1), _anim_t).y
			));
			
			if (_anim_t >= 1) {
				_current_mode = Mode.TextIn;
			}
		
		} else if (_current_mode == Mode.TextIn) {
			_primary_text.i_update();
			
			_dialogue_scroll_sound_flash.i_update();
			if (_dialogue_scroll_sound_flash.do_flash()) {
				this.text_scroll_tick_sound(game);
			}
			
			this.transform.localScale = SPUtil.valv(1);
			
			_cursor.gameObject.SetActive(false);
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA) ||
			    game._controls.get_control_down(ControlManager.Control.ButtonB) ||
				game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
				_primary_text.finish();
				game._music.play_sfx("dialogue_button_press");
			}
			
			_name_bounce_t = (_name_bounce_t + 0.15f * SPUtil.dt_scale_get()) % Mathf.PI;
			_nametag.transform.localPosition = SPUtil.vec_add(_name_initial_pos, new Vector2(0, Mathf.Abs(Mathf.Sin(_name_bounce_t) * 4)));
			
			if (_primary_text.finished()) {
				_current_mode = Mode.Finished;
				_anim_t = 51;
				
				_cursor.transform.localPosition = new Vector3(_cursor.transform.localPosition.x, _cursor_start_ypos, _cursor.transform.localPosition.z);
				_cursor.color = new Color(1,1,1,0);
				_cursor_scale_mult = 0;
			}
		
		} else if (_current_mode == Mode.Finished) {
			if (_name_bounce_t < Mathf.PI) {
				_name_bounce_t = Mathf.Clamp(_name_bounce_t + 0.15f * SPUtil.dt_scale_get(),0,Mathf.PI);
				_nametag.transform.localPosition = SPUtil.vec_add(_name_initial_pos, new Vector2(0, Mathf.Abs(Mathf.Sin(_name_bounce_t) * 6)));
			}
			
			_cursor_scale_mult = SPUtil.drpt(_cursor_scale_mult, 1, 1/3.0f);
			_cursor.color = new Color(1,1,1,SPUtil.drpt(_cursor.color.a,1,1/8.0f));
			this.cursor_anim_update();
			
			_cursor.gameObject.SetActive(true);
			this.transform.localScale = SPUtil.valv(1);
			
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA) ||
			    game._controls.get_control_down(ControlManager.Control.ButtonB) ||
				game._controls.get_control_just_released(ControlManager.Control.TouchClick)) {
				game._music.play_sfx("dialogue_button_press");
				_current_mode = Mode.FadeOut;
				_anim_t = 0;
			}
		
		} else if (_current_mode == Mode.FadeOut) {
			_cursor_scale_mult = SPUtil.drpt(_cursor_scale_mult, 0, 1/10.0f);
			_cursor.color = new Color(1,1,1,SPUtil.drpt(_cursor.color.a,0,1/10.0f));
			this.cursor_anim_update();
			
			_anim_t = Mathf.Clamp(_anim_t + SPUtil.sec_to_tick(0.15f) * SPUtil.dt_scale_get(),0,1);
			this.transform.localScale = SPUtil.valv(1);
			_canvas_group.alpha = SPUtil.y_for_point_of_2pt_line(new Vector2(0,1.0f),new Vector2(1,0),_anim_t);
			if (_anim_t >= 1) {
				_current_mode = Mode.DoRemove;
			}	
		}
		
		_cursor_shadow.transform.localScale = _cursor.transform.localScale;
		_cursor_shadow.gameObject.SetActive(_cursor.gameObject.activeSelf);
		_cursor_shadow.transform.position = _cursor.transform.position;
		
	}
	
	private void cursor_anim_update() {
		float cursor_ypos = _cursor.transform.localPosition.y;
		_cursor_yvel -= 0.15f * SPUtil.dt_scale_get();
		cursor_ypos += _cursor_yvel * SPUtil.dt_scale_get();
		
		_cursor.transform.localScale = new Vector3(
			SPUtil.drpt(_cursor.transform.localScale.x, SPUtil.y_for_point_of_2pt_line(new Vector2(-3.5f,1.35f),new Vector2(0,1), _cursor_yvel), 1/5.0f) * _cursor_scale_mult,
			SPUtil.drpt(_cursor.transform.localScale.y, SPUtil.y_for_point_of_2pt_line(new Vector2(-3.5f,1),new Vector2(0,1.1f), _cursor_yvel), 1/5.0f) * _cursor_scale_mult,
			1
		);
		if (_cursor_yvel < 0 && cursor_ypos < -75) {
			cursor_ypos = -75;
			_cursor_yvel = 2.63f;
		}
		_cursor.transform.localPosition = new Vector3(_cursor.transform.localPosition.x, cursor_ypos, _cursor.transform.localPosition.z);
	}
	
	private static Dictionary<string,Sprite> __name_to_bgsprite = new Dictionary<string, Sprite>();
	private Sprite cond_get_bgsprite(string name) {
		name = "img/ui/neu_dialogue_bubble_"+name;
		
		
		if (__name_to_bgsprite.ContainsKey(name)) return __name_to_bgsprite[name];
		Sprite bg_sprite = Resources.Load<Sprite>(name);
		if (bg_sprite != null) {
			__name_to_bgsprite[name] = bg_sprite;
		} else {
			Debug.LogError("not found:"+name);
		}
		return bg_sprite;
	}
	
	private static Dictionary<string,Sprite> __name_to_nametagsprite = new Dictionary<string, Sprite>();
	private Sprite cond_get_nametagsprite(string name) {
		name = "img/ui/neu_dialogue_nametag_"+name;
		
		
		if (__name_to_bgsprite.ContainsKey(name)) return __name_to_bgsprite[name];
		Sprite bg_sprite = Resources.Load<Sprite>(name);
		if (bg_sprite != null) {
			__name_to_bgsprite[name] = bg_sprite;
		} else {
			Debug.LogError("not found:"+name);
		}
		return bg_sprite;
	}
	
	public void apply_style(SPTextRenderManager text_renderer, NodeScriptEvent_Dialogue script_event) {
		
		Color outline_color;
				
		if (script_event._character == "Kurumi" || script_event._character == "Me") {
			_text_scroll_sound = TEXT_SCROLL_SFX_KURUMI;
			_primary_background.sprite = this.cond_get_bgsprite("kurumi");
			_name_background.sprite = this.cond_get_nametagsprite("kurumi");
			
			outline_color = new Color(117/255.0f,106/255.0f,102/255.0f,1);
			
		} else if (script_event._character == "Mana" || script_event._character == "Pink Hair" || script_event._character == "Hero") {
			_text_scroll_sound = TEXT_SCROLL_SFX_MANA;
			_primary_background.sprite = this.cond_get_bgsprite("mana");
			_name_background.sprite = this.cond_get_nametagsprite("mana");
			outline_color = new Color(108/255.0f,99/255.0f,132/255.0f,1);
		
		} else if (script_event._character == "Raichi" || script_event._character == "Student") {
			_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
			_primary_background.sprite = this.cond_get_bgsprite("raichi");
			_name_background.sprite = this.cond_get_nametagsprite("raichi");
			outline_color = new Color(85/255.0f,99/255.0f,125/255.0f,1);
		
		} else if (script_event._character == "Simone") {
			_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
			_primary_background.sprite = this.cond_get_bgsprite("simone");
			_name_background.sprite = this.cond_get_nametagsprite("simone");
			outline_color = new Color(127/255.0f,121/255.0f,85/255.0f,1);
		
		} else {
			
			if (script_event._character == NodeScriptEvent_Dialogue.CHARACTER_NARRATOR) {
				_text_scroll_sound = TEXT_SCROLL_SFX_NARRATOR;
			} else {
				_text_scroll_sound = TEXT_SCROLL_SFX_RAICHI;
				
			}
			_primary_background.sprite = this.cond_get_bgsprite("generic");
			_name_background.sprite = this.cond_get_nametagsprite("generic");
			outline_color = new Color(94/255.0f,94/255.0f,94/255.0f,1);
		}
		_name_text_outline.effectColor = outline_color;
		text_renderer.set_text_outline_color(_rendered_text, outline_color);
		text_renderer.set_bold_color(_rendered_text, outline_color);
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
