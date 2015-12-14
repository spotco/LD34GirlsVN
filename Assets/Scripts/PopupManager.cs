using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour {

	[SerializeField] private Popup _popup_proto;
	private List<Popup> _active_popups = new List<Popup>();
	
	public void i_initialize(GameMain game) {
		_popup_proto.gameObject.SetActive(false);
	}
	
	public void i_update(GameMain game) {
		for (int i = _active_popups.Count-1; i >= 0; i--) {
			Popup itr = _active_popups[i];
			itr.i_update();
			if (itr._current_mode == Popup.Mode.Hold && game._controls.get_control_just_released(ControlManager.Control.ButtonA)) {
				itr._current_mode = Popup.Mode.FadeOut;
			}
			
			if (itr._current_mode == Popup.Mode.DoRemove) {
				_active_popups.RemoveAt(i);
				GameObject.Destroy(itr.gameObject);
			}
		}
	}
	
	public void add_popup(string text, bool show_heart = false) {
		Popup neu = SPUtil.proto_clone(_popup_proto.gameObject).GetComponent<Popup>();
		neu.i_initialize(text);
		if (show_heart) {
			neu.show_heart();
		}
		_active_popups.Add(neu);
	}
	
	public bool has_active_popup() {
		for (int i = 0; i < _active_popups.Count; i++) {
			if (_active_popups[i]._current_mode == Popup.Mode.FadeIn || _active_popups[i]._current_mode == Popup.Mode.Hold) {
				return true;
			}
		}
		return false;
	}
	
}
