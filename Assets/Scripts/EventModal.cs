using UnityEngine;
using System.Collections;

public class EventModal : MonoBehaviour, GameMain.Modal {

	public void i_initialize(GameMain game) {
		this.set_active(false);
	}
	public void i_update(GameMain game) {
	
	}
	public void set_active(bool val) {
		this.gameObject.SetActive(val);
	}
	
}
