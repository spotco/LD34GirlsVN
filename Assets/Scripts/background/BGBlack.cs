using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGBlack : BGControllerBase, FallingPetalParticle.BoundedParent {
	
	[SerializeField] private RawImage _scrolling_background;
	[SerializeField] private Image _logo;
	[SerializeField] private GameObject _proto_fallingpetal_particle;
	[SerializeField] private CanvasGroup _fallingpetal_particle_canvas_group;
	
	private List<FallingPetalParticle> _inactive_particles = new List<FallingPetalParticle>();
	private List<FallingPetalParticle> _active_particles = new List<FallingPetalParticle>();
	private FlashEvery _do_spawn_particle = new FlashEvery() {
		_max_time = 5
	};
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	
	private RectTransform _rect_transform;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
		
		_rect_transform = this.GetComponent<RectTransform>();
	
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_proto_fallingpetal_particle.SetActive(false);
		for (int i = 0; i < 25; i++) {
			_inactive_particles.Add(FallingPetalParticle.cons(
				SPUtil.proto_clone(_proto_fallingpetal_particle).GetComponent<RectTransform>(),
				this
			));
		}
	}
	
	public override void on_hide(GameMain game) {
		for (int i = _active_particles.Count-1;i >= 0; i--) {
			FallingPetalParticle itr = _active_particles[i];
			itr.do_remove(this);
			_active_particles.RemoveAt(i);
			_inactive_particles.Add(itr);
		}
	}
	
	public override string get_registered_name() { return "bg_black"; }
	
	public override void show_background(string name, string key) {
		_logo.gameObject.SetActive(key.Contains("showlogo"));
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {
		
		Rect uv_rect = _scrolling_background.uvRect;
		uv_rect.x = (uv_rect.x - SPUtil.sec_to_tick(40) * SPUtil.dt_scale_get()) % 1.0f;
		uv_rect.y = (uv_rect.y + SPUtil.sec_to_tick(44) * SPUtil.dt_scale_get()) % 1.0f;
		_scrolling_background.uvRect = uv_rect;
		
		_do_spawn_particle.i_update();
		if (_do_spawn_particle.do_flash() && _inactive_particles.Count > 0) {
			FallingPetalParticle spawn_particle = _inactive_particles[0];
			_inactive_particles.RemoveAt(0);
			
			spawn_particle.spawn(this);
			_active_particles.Add(spawn_particle);
		}
		for (int i = _active_particles.Count-1;i >= 0; i--) {
			FallingPetalParticle itr = _active_particles[i];
			itr.i_update(this);
			if (itr.should_remove(this)) {
				itr.do_remove(this);
				_active_particles.RemoveAt(i);
				_inactive_particles.Add(itr);
			}
		}
		
		this.update_showing_mode(_fade_cover);
	}
	
	public SPHitRect get_screen_bounds() {
		Rect rect = _rect_transform.rect;
		return new SPHitRect() {
			_x1 = rect.xMin,
			_x2 = rect.xMax,
			_y1 = rect.yMin,
			_y2 = rect.yMax
		};
	}
	
}
