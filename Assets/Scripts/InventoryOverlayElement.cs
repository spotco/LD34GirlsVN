using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryOverlayElement : MonoBehaviour {
	
	[SerializeField] private Image _back;
	[SerializeField] private Image _image;
	public string _item_name;

	private float _anim_theta, _anim_theta_2;

	public void i_initialize(string item_name, GameMain game) {
		_item_name = item_name;
		_anim_theta = 0;
		_anim_theta_2 = 0;
		_back.transform.localScale = SPUtil.valv(1.5f);
	}
	
	public void i_update() {
		_back.transform.localScale = SPUtil.valv(SPUtil.drpt(_back.transform.localScale.x,1,1/10.0f));
	
		_anim_theta += 0.05f * SPUtil.dt_scale_get();
		_anim_theta_2 += 0.06f * SPUtil.dt_scale_get();
		_back.transform.localEulerAngles = new Vector3(0,0,Mathf.Sin(_anim_theta)*7.5f);
		_image.transform.localScale = SPUtil.valv(1+Mathf.Sin(_anim_theta_2)*0.05f);
	}


}
