using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BGHillShrine : BGControllerBase {
	
	[SerializeField] private Image _fade_cover;
	[SerializeField] private Transform _scroll_anchor;
	
	[SerializeField] private Image _background;
	[SerializeField] private Image _citybackground;
	[SerializeField] private Image _foreground;
	
	[SerializeField] private CanvasGroup _frame1_mana;
	[SerializeField] private Image _frame1_yuuto;
	[SerializeField] private Image _frame2_mana;
	[SerializeField] private Image _frame2_mana_alt;
	[SerializeField] private Image _frame3_yuuto;
	
	[SerializeField] private Image _frame4_mana;
	[SerializeField] private Image _frame4_circle;
	[SerializeField] private Image _frame5_mana;
	[SerializeField] private Image _frame6_yuuto;
	
	[SerializeField] private Image _frame7_mana;
	[SerializeField] private Image _frame8_yuuto_mana;
	[SerializeField] private Image _frame9_yuuto_mana;
	
	[SerializeField] private Image _frame10_mana;
	[SerializeField] private Image _frame10_yuuto;
	[SerializeField] private Image _frame11_yuuto_mana;
	
	private ParallaxScrollRegistry _scroll_registry = new ParallaxScrollRegistry();
	private Vector2 _current_scroll_pos, _target_scroll_pos;
	private float _current_scale, _target_scale;
	
	public override void i_initialize(GameMain game) {
		this.i_initialize_hidden(_fade_cover);
		
		_current_scroll_pos = _scroll_anchor.transform.localPosition;
		_target_scroll_pos = _current_scroll_pos;
		
		_scroll_registry.add_registry_entry(_background.transform, 1);
		_scroll_registry.add_registry_entry(_citybackground.transform, 1.5f);
		
		
		_scroll_registry.add_registry_entry(_foreground.transform, 1.8f);
		
		_all_frameimages.Clear();
		
		float fg_scroll_scale = 1.95f;
		this.add_frameimg_registry_entry(_frame1_mana.transform, fg_scroll_scale);

		_frame1_mana.transform.Find("body_image").gameObject.SetActive(false);
		_frame1_mana.transform.Find("body_rawimage").gameObject.SetActive(true);
		_frame1_mana.transform.Find("body_rawimage").GetComponent<RawImage>().texture = game._anim_prefab_render_system.get_texture_for_animprefab("");


		this.add_frameimg_registry_entry(_frame1_yuuto.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame2_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame2_mana_alt.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame3_yuuto.transform, fg_scroll_scale);
		
		this.add_frameimg_registry_entry(_frame4_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame4_circle.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame5_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame6_yuuto.transform, fg_scroll_scale);
		
		this.add_frameimg_registry_entry(_frame7_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame8_yuuto_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame9_yuuto_mana.transform, fg_scroll_scale);
		
		this.add_frameimg_registry_entry(_frame10_mana.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame10_yuuto.transform, fg_scroll_scale);
		this.add_frameimg_registry_entry(_frame11_yuuto_mana.transform, fg_scroll_scale);
		
		_scroll_anchor.transform.localPosition = _current_scroll_pos = _target_scroll_pos = new Vector2(0,0);
		_current_scale = _target_scale = 1;	
	}
	
	private List<Transform> _all_frameimages = new List<Transform>();
	
	private void add_frameimg_registry_entry(Transform tar, float scroll_scale) {
		_scroll_registry.add_registry_entry(tar, scroll_scale);

		HideShowObjectRegistryBehaviour.ImageTarget tar_adapter = null;
		if (tar.GetComponent<CanvasGroup>()) {
			tar_adapter = new HideShowObjectRegistryBehaviour.CanvasGroup_ImageTargetAdapter() {
				_cgroup = tar.GetComponent<CanvasGroup>()
			};

		} else if (tar.GetComponent<Image>()) {
			tar_adapter = new HideShowObjectRegistryBehaviour.Image_ImageTargetAdapter() {
				_img = tar.GetComponent<Image>()
			};
		} else {
			SPUtil.errf("add_frameimg_registry_entry no CanvasGroup or Image(%s)",tar.name);
			return;
		}

		_scroll_registry.add_registry_behaviour(tar, HideShowObjectRegistryBehaviour.cons(
			tar_adapter
		));
		_all_frameimages.Add(tar);
	}
	
	public override string get_registered_name() { return "bg_hill_shrine"; }
	
	public override void imm_show_bgkey() {
		_current_scroll_pos = _target_scroll_pos;
		_current_scale = _target_scale;
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
		_scroll_anchor.transform.localScale = SPUtil.valv(_current_scale);	
	}
	
	private List<Transform> __shot_visible_characters = new List<Transform>();
	
	public override void show_background(string name, string key) {
		__shot_visible_characters.Clear();
	
		if (key == BGControllerBase.KEY_DEFAULT) {
			_target_scroll_pos = new Vector2(0,0);
			_target_scale = 1;

		} else if (key == "dreampreview") {
			_target_scroll_pos = new Vector2(0,-50);
			_target_scale = 1;

		} else if (key == "dreampreview_lfocus") {
			_target_scroll_pos = new Vector2(-80,50);
			_target_scale = 1.25f;

		} else if (key == "frame1_shot0") {
			_target_scroll_pos = new Vector2(0,-120);
			_target_scale = 1.3f;
			__shot_visible_characters.Add(_frame1_mana.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);
			
		} else if (key == "frame1_shot1") {
			_target_scroll_pos = new Vector2(0,40);
			_target_scale = 0.85f;
			__shot_visible_characters.Add(_frame1_mana.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);
		
		} else if (key == "frame2_shot1") {
			_target_scroll_pos = new Vector2(-115,88);
			_target_scale = 1.5f;
			__shot_visible_characters.Add(_frame1_mana.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);	
		
		} else if (key == "frame3_shot0") {
			_target_scroll_pos = new Vector2(80,88);
			_target_scale = 1.5f;
			__shot_visible_characters.Add(_frame1_mana.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);	
		
		} else if (key == "frame3_shot1") {
			_target_scroll_pos = new Vector2(25,93);
			_target_scale = 1.4f;
			__shot_visible_characters.Add(_frame2_mana_alt.transform);
			__shot_visible_characters.Add(_frame3_yuuto.transform);	
		
		} else if (key == "frame3_shot2") {
			_target_scroll_pos = new Vector2(25,93);
			_target_scale = 1.4f;
			__shot_visible_characters.Add(_frame2_mana_alt.transform);
			__shot_visible_characters.Add(_frame3_yuuto.transform);	
			__shot_visible_characters.Add(_frame4_circle.transform);
			
		} else if (key == "frame4_shot0") {
			_target_scroll_pos = new Vector2(-85,65);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame2_mana_alt.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);	
			
		} else if (key == "frame4_shot1") {
			_target_scroll_pos = new Vector2(-105,86);
			_target_scale = 1.6f;
			__shot_visible_characters.Add(_frame2_mana_alt.transform);
			__shot_visible_characters.Add(_frame1_yuuto.transform);	
			
		} else if (key == "frame5_shot0") {
			_target_scroll_pos = new Vector2(-44,75);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame5_mana.transform);
			__shot_visible_characters.Add(_frame3_yuuto.transform);	
			__shot_visible_characters.Add(_frame4_circle.transform);
		
		} else if (key == "frame6_shot0") {
			_target_scroll_pos = new Vector2(-98,90);
			_target_scale = 1.35f;
			__shot_visible_characters.Add(_frame7_mana.transform);
			__shot_visible_characters.Add(_frame3_yuuto.transform);	
			__shot_visible_characters.Add(_frame4_circle.transform);
		
		} else if (key == "frame7_shot0") {
			_target_scroll_pos = new Vector2(6,87);
			_target_scale = 1.15f;
			__shot_visible_characters.Add(_frame4_mana.transform);
			__shot_visible_characters.Add(_frame3_yuuto.transform);	
			__shot_visible_characters.Add(_frame4_circle.transform);
		
		} else if (key == "frame7_shot1") {
			_target_scroll_pos = new Vector2(6,60);
			_target_scale = 1f;
			__shot_visible_characters.Add(_frame4_mana.transform);
			
		} else if (key == "frame8_shot0") {
			_target_scroll_pos = new Vector2(9,-48);
			_target_scale = 1f;
			__shot_visible_characters.Add(_frame6_yuuto.transform);
			
		} else if (key == "frame8_shot1") {
			_target_scroll_pos = new Vector2(55,-30);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame8_yuuto_mana.transform);
			
		} else if (key == "frame9_shot0") {
			_target_scroll_pos = new Vector2(-4,-26);
			_target_scale = 1f;
			__shot_visible_characters.Add(_frame9_yuuto_mana.transform);
			
		} else if (key == "frame10_shot0") {
			_target_scroll_pos = new Vector2(-17,81);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame10_mana.transform);
			
		} else if (key == "frame10_shot1") {
			_target_scroll_pos = new Vector2(-7,66);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame10_mana.transform);
			__shot_visible_characters.Add(_frame10_yuuto.transform);
			
		} else if (key == "frame10_shot2") {
			_target_scroll_pos = new Vector2(12,75);
			_target_scale = 1.4f;
			__shot_visible_characters.Add(_frame10_mana.transform);
			__shot_visible_characters.Add(_frame10_yuuto.transform);
			
		} else if (key == "frame10_shot3") {
			_target_scroll_pos = new Vector2(-17,81);
			_target_scale = 1.55f;
			__shot_visible_characters.Add(_frame10_mana.transform);
			__shot_visible_characters.Add(_frame10_yuuto.transform);
			
		} else if (key == "frame11_shot0") {
			_target_scroll_pos = new Vector2(-24,58);
			_target_scale = 1.2f;
			__shot_visible_characters.Add(_frame11_yuuto_mana.transform);
			
		} else if (key == "frame11_shot1") {
			_target_scroll_pos = new Vector2(-6,-113);
			_target_scale = 1.3f;
			__shot_visible_characters.Add(_frame11_yuuto_mana.transform);
		}
		
		for (int i = 0; i < _all_frameimages.Count; i++) {
			Transform itr = _all_frameimages[i];
			if (__shot_visible_characters.Contains(itr)) {
				_scroll_registry.get_registry_behaviour<HideShowObjectRegistryBehaviour>(itr).set_visible(true);
			} else {
				_scroll_registry.get_registry_behaviour<HideShowObjectRegistryBehaviour>(itr).set_visible(false);
			}
		}
	}
	
	public override void recieve_update_message(string strparam, float numparam1, float numparam2) {
	}
	
	public override void i_update(GameMain game) {

		_current_scroll_pos.x = SPUtil.drpt(_current_scroll_pos.x, _target_scroll_pos.x, 1/30.0f);
		_current_scroll_pos.y = SPUtil.drpt(_current_scroll_pos.y, _target_scroll_pos.y, 1/30.0f);
		_scroll_anchor.transform.localPosition = _current_scroll_pos;
		
		_current_scale = SPUtil.drpt(_current_scale, _target_scale, 1/30.0f);
		_scroll_anchor.transform.localScale = SPUtil.valv(_current_scale);

		_scroll_registry.set_scroll_position(_scroll_anchor.localPosition);
		_scroll_registry.update_all_entries(game);
		
		this.update_showing_mode(_fade_cover);
	}
}
