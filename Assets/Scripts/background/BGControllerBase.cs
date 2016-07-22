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
	
	public virtual Transform get_character_root() { return null; }
	public virtual void i_initialize(GameMain game) {}
	public virtual string get_registered_name() { return ""; }
	public virtual void show_background(string name, string key) {}
	public virtual void recieve_update_message(string strparam, float numparam1, float numparam2) {}
	public virtual void i_update(GameMain game) {}
	
	public virtual bool should_remain_active() { return _current_showing_mode != ShowingMode.Hidden; }
}

public class ParallaxScrollRegistry {
	public class RegistryEntry {
		public Vector2 _position;
		public float _scale_mult = 1;
	}
	
	private Vector2 _scroll_position = new Vector2();
	public SPDict<Transform, RegistryEntry> _registry = new SPDict<Transform, RegistryEntry>();
	public static ParallaxScrollRegistry cons() { return new ParallaxScrollRegistry(); }
	
	public void add_registry_entry(Transform obj, float scale_mult) {
		_registry[obj] = new RegistryEntry() {
			_position = obj.localPosition,
			_scale_mult = scale_mult
		};
	}
	
	public void update_registry_entry_position(Transform obj, Vector2 pos) {
		_registry[obj]._position = pos;
	}
	
	public void set_scroll_position(Vector2 pos) {
		_scroll_position = pos;
	}
	
	public Vector2 get_scroll_position_for_entry(Transform obj) {
		RegistryEntry entry = _registry[obj];
		return new Vector3(
			(_scroll_position.x * entry._scale_mult) + entry._position.x,
			(_scroll_position.y * entry._scale_mult) + entry._position.y,
			obj.transform.localPosition.z
		);
	}
	
	public void update_all_entries() {
		List<Transform> registry_itr = _registry.key_itr();
		for (int i = 0; i < registry_itr.Count; i++) {
			registry_itr[i].localPosition = this.get_scroll_position_for_entry(registry_itr[i]);
		}
	}
}

public class BackgroundObjectRegistry {
	public class RegistryEntry {
		public GameObject _obj;
		public List<RegistryBehaviour> _behaviours = new List<RegistryBehaviour>();
	}
	public class RegistryBehaviour {
		public virtual void i_update(GameMain game, RegistryEntry entry) {}
	}
	
	public SPDict<GameObject, RegistryEntry> _registry = new SPDict<GameObject, RegistryEntry>();
	public static BackgroundObjectRegistry cons() { return new BackgroundObjectRegistry(); }
	
	public void add_registry_behaviour(GameObject obj, RegistryBehaviour behaviour) {
		if (!_registry.ContainsKey(obj)) {
			_registry[obj] = new RegistryEntry() {
				_obj = obj
			};
		}
		_registry[obj]._behaviours.Add(behaviour);
	}
	
	public T get_registry_behaviour<T>(GameObject obj) {	
		RegistryEntry entry = _registry[obj];
		for (int i = 0; i < entry._behaviours.Count; i++) {
			T rtv;
			if (SPUtil.can_cast<T>(entry._behaviours[i], out rtv)) {
				return rtv;
			}
		}
		return default(T);
	}
	
	public void update_all_entries(GameMain game) {
		List<GameObject> registry_itr = _registry.key_itr();
		for (int i = 0; i < registry_itr.Count; i++) {
			RegistryEntry entry = _registry[registry_itr[i]];
			for (int j = 0; j < entry._behaviours.Count; j++) {
				entry._behaviours[j].i_update(game, entry);
			}
		}
	}
}

public class HideShowImageRegistryBehaviour : BackgroundObjectRegistry.RegistryBehaviour {
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
	
	public override void i_update(GameMain game, BackgroundObjectRegistry.RegistryEntry entry) {
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

public class MoveToRegistryBehaviour : BackgroundObjectRegistry.RegistryBehaviour {
	public static MoveToRegistryBehaviour cons(Transform transform, Vector2 tar_lpos) {
		return (new MoveToRegistryBehaviour()).i_cons(transform, tar_lpos);
	}
	
	private Transform _transform;
	private Vector2 _tar_lpos, _cur_lpos;
	
	private MoveToRegistryBehaviour i_cons(Transform transform, Vector2 tar_lpos) {
		_transform = transform;
		_cur_lpos = tar_lpos;
		_tar_lpos = _cur_lpos;
		return this;
	}
	
	public void move_to(Vector2 lpos) {
		_tar_lpos = lpos;
	}
	
	public override void i_update(GameMain game, BackgroundObjectRegistry.RegistryEntry entry) {
		_cur_lpos.x = SPUtil.drpt(_cur_lpos.x, _tar_lpos.x, 1/10.0f);
		_cur_lpos.y = SPUtil.drpt(_cur_lpos.y, _tar_lpos.y, 1/10.0f);
		_transform.localPosition = _cur_lpos;
	}
}

