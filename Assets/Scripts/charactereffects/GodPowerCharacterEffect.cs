using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GodPowerCharacterEffect : EventCharacter.Effect {

	public class GodPowerParticle : SPBaseBehavior {
		
		public float _anim_t;
		public Image _image;
	
		public static GodPowerParticle cons(GameMain game, Transform parent) {
			GodPowerParticle rtv = game._objpool.spbasebehavior_depool<GodPowerParticle>(true);
			if (rtv.GetComponent<Image>() == null) {
				rtv.gameObject.AddComponent<Image>();
				rtv.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("img/effects/mana_god_energy_particle");
				rtv.gameObject.GetComponent<Image>().SetNativeSize();
			}
			rtv._anim_t = 0;
			rtv._image = rtv.GetComponent<Image>();
			rtv.transform.SetParent(parent);
			rtv.transform.localPosition = SPUtil.valv(0);
			return rtv;
		}
		public static void repool(GameMain game, GodPowerParticle particle) {
			game._objpool.spbasebehavior_repool<GodPowerParticle>(particle);
		}
		
		public void scale_and_alpha_from_position(float y_min, float y_max) {
			float anim_param_t = SPUtil.y_for_point_of_2pt_line(new Vector2(y_min,0), new Vector2(y_max,1),this.transform.localPosition.y);
			float param_curve_t = anim_param_t < 0.5f ?
				SPUtil.bezier_val_for_t(new Vector2(0,0), new Vector2(0.5f,0), new Vector2(0f,1), new Vector2(0.5f,1), anim_param_t * 2).y :
				SPUtil.bezier_val_for_t(new Vector2(0.5f,1), new Vector2(1,1), new Vector2(0.5f,0), new Vector2(1,0), (anim_param_t - 0.5f) * 2).y;
			this.transform.localScale = new Vector2(0.75f, param_curve_t * param_curve_t * param_curve_t);
			
			this._image.color = new Color(1,1,1,Mathf.Clamp(param_curve_t,0.25f,1) * 0.85f);
		}
		
	}

	
	public static GodPowerCharacterEffect cons() { 
		return (new GodPowerCharacterEffect());
	}
	
	private Transform _front_root;
	private Transform _back_root;
	
	List<GodPowerParticle> _active_particles = new List<GodPowerParticle>();
	
	public override void on_added(GameMain game, EventModal modal, EventCharacter character) {
		{
			GameObject front_obj = new GameObject();
			front_obj.transform.parent = character.transform;
			front_obj.transform.localScale = SPUtil.valv(1);
			front_obj.transform.localPosition = SPUtil.valv(0);
			front_obj.name = "_front_root";
			_front_root = front_obj.transform;
			_front_root.SetAsLastSibling();
		}
		{
			GameObject back_obj = new GameObject();
			back_obj.transform.parent = character.transform;
			back_obj.transform.localScale = SPUtil.valv(1);
			back_obj.transform.localPosition = SPUtil.valv(0);
			back_obj.name = "_back_root";
			_back_root = back_obj.transform;
			_back_root.SetAsFirstSibling();
		}
		
		float x_min = character._image.rectTransform.rect.xMin;
		float x_max = character._image.rectTransform.rect.xMax;
		float y_min = character._image.rectTransform.rect.yMin;
		float y_max = character._image.rectTransform.rect.yMax;
		y_min -= (y_max - y_min) * 0.2f;
		
		for (float x = x_min; x < x_max; x += SPUtil.float_random(5,25)) {
			GodPowerParticle itr_particle = GodPowerParticle.cons(game, _front_root);
			if (SPUtil.int_random(0,3) == 0) {
				itr_particle.transform.SetParent(_front_root);
			} else {
				itr_particle.transform.SetParent(_back_root);
			}
			float y = SPUtil.float_random(y_min,y_max);
			itr_particle.transform.localPosition = new Vector2(x,y);
			itr_particle.scale_and_alpha_from_position(y_min, y_max);
			_active_particles.Add(itr_particle);
		}
		
	}
	
	public override void i_update(GameMain game, EventModal modal, EventCharacter character) {
		float y_min = character._image.rectTransform.rect.yMin;
		float y_max = character._image.rectTransform.rect.yMax;
		y_min -= (y_max - y_min) * 0.2f;
	
		for (int i = 0; i < _active_particles.Count; i++) {
			GodPowerParticle itr_particle = _active_particles[i];
			itr_particle.transform.localPosition = itr_particle.transform.localPosition + new Vector3(0, 10 * SPUtil.dt_scale_get(), 0);
			if (itr_particle.transform.localPosition.y > y_max) {
				if (SPUtil.int_random(0,3) == 0) {
					itr_particle.transform.SetParent(_front_root);
				} else {
					itr_particle.transform.SetParent(_back_root);
				}
			
				itr_particle.transform.localPosition = itr_particle.transform.localPosition - new Vector3(0,y_max - y_min);
			}
			itr_particle.scale_and_alpha_from_position(y_min,y_max);
		}
	}
	
	public override void do_remove(GameMain game, EventModal modal, EventCharacter character) {
		GameObject.Destroy(_front_root.gameObject);
		GameObject.Destroy(_back_root.gameObject);
	}
}
