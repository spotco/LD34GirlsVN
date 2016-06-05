using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TitleModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Text _text;
	[SerializeField] private CanvasGroup _canvas_group;
	[SerializeField] private GameObject _proto_logoparticle;
	
	public enum Mode {
		Hide,
		FadeIn,
		Hold,
		FadeOut
	}
	public Mode _current_mode;
	
	private bool _end_screen;
	private float _anim_t;
	
	private RectTransform _rect_transform;
	private List<TitleParticle> _inactive_particles = new List<TitleParticle>();
	private List<TitleParticle> _active_particles = new List<TitleParticle>();
	private FlashEvery _do_spawn_particle = new FlashEvery() {
		_max_time = 5
	};
	
	public void i_initialize(GameMain game) {
		_rect_transform = this.GetComponent<RectTransform>();
		this.gameObject.SetActive(false);
		_current_mode = Mode.Hide;
		_end_screen = false;
		
		_proto_logoparticle.SetActive(false);
		for (int i = 0; i < 25; i++) {
			_inactive_particles.Add(TitleParticle.cons(
				SPUtil.proto_clone(_proto_logoparticle).GetComponent<RectTransform>(),
				this
			));
		}
	}
	public void i_update(GameMain game) {
		if (_current_mode == Mode.Hold && !_end_screen) {
			if (game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				game._music.play_sfx("map_yes");
				game._active_modal = game._grid_nav_modal;
				_current_mode = Mode.FadeOut;
			}
		}
		
		_do_spawn_particle.i_update();
		if (_do_spawn_particle.do_flash() && _inactive_particles.Count > 0) {
			TitleParticle spawn_particle = _inactive_particles[0];
			_inactive_particles.RemoveAt(0);
			
			spawn_particle.spawn(this);
			_active_particles.Add(spawn_particle);
		}
		for (int i = _active_particles.Count-1;i >= 0; i--) {
			TitleParticle itr = _active_particles[i];
			itr.i_update(this);
			if (itr.should_remove(this)) {
				itr.do_remove(this);
				_active_particles.RemoveAt(i);
				_inactive_particles.Add(itr);
			}
		}
		
	}
	
	public void set_text(string val) {
		_text.text = val;
	}
	
	public void set_end_screen() {
		_end_screen = true;	
	}
	
	public void anim_update(GameMain game) {
		if (_current_mode == Mode.Hide) {
			this.gameObject.SetActive(false);
			
		} else if (_current_mode == Mode.FadeIn) {
			this.gameObject.SetActive(true);
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha + 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha >= 1) {
				_current_mode = Mode.Hold;
			}
			
		} else if (_current_mode == Mode.Hold) {
			_anim_t += SPUtil.dt_scale_get();
			if (!_end_screen) {
				if (_anim_t > 40) {
					_text.gameObject.SetActive(!_text.gameObject.activeSelf);
					_anim_t = 0;
				}
			} else {
				_text.gameObject.SetActive(true);
			}
			
		
		} else if (_current_mode == Mode.FadeOut) {
			this.gameObject.SetActive(true);
			_canvas_group.alpha = Mathf.Min(_canvas_group.alpha - 0.05f * SPUtil.dt_scale_get(),1);
			if (_canvas_group.alpha <= 0) {
				_current_mode = Mode.Hide;
			}
		}
	}
	
	public SPHitRect get_title_screen_bounds() {
		Rect rect = _rect_transform.rect;
		return new SPHitRect() {
			_x1 = rect.xMin,
			_x2 = rect.xMax,
			_y1 = rect.yMin,
			_y2 = rect.yMax
		};
	}
}
