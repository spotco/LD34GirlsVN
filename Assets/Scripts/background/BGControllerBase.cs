using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGControllerBase : MonoBehaviour {

	public static string KEY_DEFAULT = "default";
	
	protected void i_initialize_hidden(Image fade_cover) {
		fade_cover.color = new Color(0,0,0,1);
		this.gameObject.SetActive(false);
	}
	
	protected enum ShowingMode {
		Hidden,
		TransitionShowing,
		Showing,
		TransitionHidden
	};
	protected ShowingMode _current_showing_mode = ShowingMode.Hidden;
	protected void update_showing_mode(Image fade_cover) {
		if (_current_showing_mode == ShowingMode.TransitionShowing) {
			this.gameObject.SetActive(true);
			float t = fade_cover.color.a;
			t -= SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			
			if (t > 0) {
				fade_cover.color = new Color(0,0,0,t);
			} else {
				fade_cover.color = new Color(0,0,0,0);
				_current_showing_mode = ShowingMode.Showing;
			}
			
		} else if (_current_showing_mode == ShowingMode.Showing) {
			this.gameObject.SetActive(true);
			
		} else if (_current_showing_mode == ShowingMode.TransitionHidden) {
			this.gameObject.SetActive(true);
			float t = fade_cover.color.a;
			t += SPUtil.sec_to_tick(0.25f) * SPUtil.dt_scale_get();
			
			if (t <= 1) {
				fade_cover.color = new Color(0,0,0,t);
			} else {
				fade_cover.color = new Color(0,0,0,1);
				_current_showing_mode = ShowingMode.Hidden;
				this.gameObject.SetActive(false);
			}
			
		}
	}
	public void set_showing(bool val) {
		if (val) {
			_current_showing_mode = ShowingMode.TransitionShowing;
		} else {
			_current_showing_mode = ShowingMode.TransitionHidden;
		}
	}
	public virtual void imm_show_bgkey() {
	}
	
	public virtual Transform get_character_root() { return null; }
	public virtual void i_initialize(GameMain game) {}
	public virtual string get_registered_name() { return ""; }
	public virtual void show_background(string name, string key) {}
	public virtual void recieve_update_message(string strparam, float numparam1, float numparam2) {}
	public virtual void i_update(GameMain game) {}
	public virtual void on_hide(GameMain game) {}
	
	public virtual bool should_remain_active() { return _current_showing_mode != ShowingMode.Hidden; }
}

public class ParallaxScrollRegistry {
	public class RegistryEntry {
		public Vector2 _position;
		public float _scale_mult = 1;
		public List<RegistryBehaviour> _behaviours = new List<RegistryBehaviour>();
		public Transform _key;
	}
	
	public class RegistryBehaviour {
		public virtual void i_update(GameMain game, ParallaxScrollRegistry registry, RegistryEntry entry) {}
	}
	
	private Vector2 _scroll_position = new Vector2();
	public SPDict<Transform, RegistryEntry> _registry = new SPDict<Transform, RegistryEntry>();
	public static ParallaxScrollRegistry cons() { return new ParallaxScrollRegistry(); }
	
	public void add_registry_entry(Transform obj, float scale_mult) {
		_registry[obj] = new RegistryEntry() {
			_position = obj.localPosition,
			_scale_mult = scale_mult,
			_key = obj
		};
	}
	
	public void update_registry_entry_position(Transform obj, Vector2 pos) {
		_registry[obj]._position = pos;
	}
	
	public void set_scroll_position(Vector2 pos) {
		_scroll_position = pos;
	}
	
	public Vector2 get_scroll_offset_for_entry(Transform obj) {
		RegistryEntry entry = _registry[obj];
		return SPUtil.vec_scale(_scroll_position, entry._scale_mult);
	}
	
	public Vector2 get_scroll_position_for_entry(Transform obj) {
		RegistryEntry entry = _registry[obj];
		Vector3 scroll_offset = this.get_scroll_offset_for_entry(obj);
		return SPUtil.vec_add(entry._position, scroll_offset);
	}
	
	public void update_all_entries(GameMain game) {
		{
			List<Transform> registry_itr = _registry.key_itr();
			for (int i = 0; i < registry_itr.Count; i++) {
				registry_itr[i].localPosition = this.get_scroll_position_for_entry(registry_itr[i]);
			}
		}
		
		{
			List<Transform> registry_itr = _registry.key_itr();
			for (int i = 0; i < registry_itr.Count; i++) {
				RegistryEntry entry = _registry[registry_itr[i]];
				for (int j = 0; j < entry._behaviours.Count; j++) {
					entry._behaviours[j].i_update(game, this, entry);
				}
			}
		}
	}
	
	public void add_registry_behaviour(Transform obj, RegistryBehaviour behaviour) {
		if (!_registry.ContainsKey(obj)) {
			SPUtil.errf("No registry behaviour for %s",obj.name);
			return;
		}
		_registry[obj]._behaviours.Add(behaviour);
	}
	
	public T get_registry_behaviour<T>(Transform obj) {	
		RegistryEntry entry = _registry[obj];
		for (int i = 0; i < entry._behaviours.Count; i++) {
			T rtv;
			if (SPUtil.can_cast<T>(entry._behaviours[i], out rtv)) {
				return rtv;
			}
		}
		return default(T);
	}
}

public class HideShowObjectRegistryBehaviour : ParallaxScrollRegistry.RegistryBehaviour {

	public interface ImageTarget {
		void set_opacity(float val);
		float get_opacity();

		void set_visible(bool val);
		bool get_visible();
	}

	public class Image_ImageTargetAdapter : ImageTarget {
		public Image _img;
		public void set_opacity(float val) { _img.color = new Color(_img.color.r,_img.color.g,_img.color.b,val); }
		public float get_opacity() { return _img.color.a; }
		public void set_visible(bool val) { _img.gameObject.SetActive(val); }
		public bool get_visible() { return _img.gameObject.activeSelf; }
	}
	public class CanvasGroup_ImageTargetAdapter : ImageTarget {
		public CanvasGroup _cgroup;
		public void set_opacity(float val) { _cgroup.alpha = val; }
		public float get_opacity() { return _cgroup.alpha; }
		public void set_visible(bool val) { _cgroup.gameObject.SetActive(val); }
		public bool get_visible() { return _cgroup.gameObject.activeSelf; }
	}
	
	public static HideShowObjectRegistryBehaviour cons(ImageTarget img) {
		return (new HideShowObjectRegistryBehaviour()).i_cons(img);
	}
	
	private ImageTarget _image;
	private bool _target_visible;
	
	private HideShowObjectRegistryBehaviour i_cons(ImageTarget img) {
		_image = img;
		_target_visible = false;

		_image.set_visible(false);
		_image.set_opacity(0);
		return this;
	}
	
	public void set_visible(bool val) {
		_target_visible = val;
	}
	
	public void set_visible_imm(bool val) {
		_target_visible = val;
		if (_target_visible == true) {
			_image.set_opacity(1);
			_image.set_visible(true);

		} else {
			_image.set_opacity(0);
			_image.set_visible(false);
			
		}
	}
	
	public override void i_update(GameMain game, ParallaxScrollRegistry registry, ParallaxScrollRegistry.RegistryEntry entry) {
		if (_target_visible) {
			_image.set_visible(true);
			if (_image.get_opacity() < 1) {
				_image.set_opacity(_image.get_opacity() + (SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get()));
			}
			
		} else {
			if (_image.get_opacity() > 0) {
				_image.set_opacity(_image.get_opacity() - (SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get()));
				
			} else {
				_image.set_visible(false);
			}
			
		}
	}
}

//DEPRECATED DO NT USE
public class HideShowImageRegistryBehaviour : ParallaxScrollRegistry.RegistryBehaviour {
	public static HideShowImageRegistryBehaviour cons(Image img) {
		return (new HideShowImageRegistryBehaviour()).i_cons(img);
	}
	
	private Image _image;
	private bool _target_visible;
	
	private HideShowImageRegistryBehaviour i_cons(Image img) {
		_image = img;
		_target_visible = false;
		_image.gameObject.SetActive(false);
		_image.color = new Color(_image.color.a, _image.color.g, _image.color.b, 0);
		return this;
	}
	
	public void set_visible(bool val) {
		_target_visible = val;
	}
	
	public void set_visible_imm(bool val) {
		_target_visible = val;
		if (_target_visible == true) {
			_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1);
			_image.gameObject.SetActive(true);
			
		} else {
			_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
			_image.gameObject.SetActive(false);
			
		}
	}
	
	public override void i_update(GameMain game, ParallaxScrollRegistry registry, ParallaxScrollRegistry.RegistryEntry entry) {
		if (_target_visible) {
			_image.gameObject.SetActive(true);
			if (_image.color.a < 1) {
				_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _image.color.a + (SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get()));
			}
			
		} else {
			if (_image.color.a > 0) {
				_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _image.color.a - (SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get()));
				
			} else {
				_image.gameObject.SetActive(false);
			}
			
		}
	}
}
//DEPRECATED TO NOT USE

public class MovingCharacterRegistryBehaviour : ParallaxScrollRegistry.RegistryBehaviour {
	public static MovingCharacterRegistryBehaviour cons(Transform transform, Vector2 tar_lpos) {
		return (new MovingCharacterRegistryBehaviour()).i_cons(transform, tar_lpos);
	}
	
	private Transform _transform;
	private Vector2 _tar_lpos, _cur_lpos;
	
	public bool _is_bobbing;
	private float _bob_ct;
	
	public float _bob_speed;
	public float _bob_ampl;
	
	private MovingCharacterRegistryBehaviour i_cons(Transform transform, Vector2 tar_lpos) {
		_transform = transform;
		_cur_lpos = tar_lpos;
		_tar_lpos = _cur_lpos;
		
		_is_bobbing = false;
		_bob_speed = 0;
		_bob_ampl = 0;
		return this;
	}
	
	public void move_to(Vector2 lpos) {
		_tar_lpos = lpos;
	}
	
	public MovingCharacterRegistryBehaviour set_bob_params(float speed, float ampl) {
		_bob_speed = speed;
		_bob_ampl = ampl;
		return this;
	}
	
	public void do_bob() {
		if (_is_bobbing == false) {
			_is_bobbing = true;
			_bob_ct = 0;
		}
	}
	
	public override void i_update(GameMain game, ParallaxScrollRegistry registry, ParallaxScrollRegistry.RegistryEntry entry) {
		_cur_lpos.x = SPUtil.drpt(_cur_lpos.x, _tar_lpos.x, 1/10.0f);
		_cur_lpos.y = SPUtil.drpt(_cur_lpos.y, _tar_lpos.y, 1/10.0f);
		
		float bob_y_delta = 0;
		if (_is_bobbing) {
			_bob_ct = _bob_ct + _bob_speed * SPUtil.dt_scale_get();
			if (_bob_ct >= 1) {
				_bob_ct = 1;
				_is_bobbing = false;
			}
			float bob_curve = SPUtil.bezier_val_for_t(
				new Vector2(0,0),
				new Vector2(0.5f,2),
				new Vector2(0.5f,0),
				new Vector2(1,0),
				_bob_ct
			).y;
			bob_y_delta = Mathf.Abs(bob_curve * _bob_ampl);
		}
		
		_transform.localPosition = _cur_lpos + registry.get_scroll_offset_for_entry(entry._key) + (new Vector2(0, bob_y_delta));
	}
}