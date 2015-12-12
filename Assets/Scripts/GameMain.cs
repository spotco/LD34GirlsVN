using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class GameMain : MonoBehaviour {

	public interface Modal {
		void i_initialize(GameMain game);
		void i_update(GameMain game);
		void set_active(bool val);
	}

	[SerializeField] private EventModal _event_modal;
	[SerializeField] private GridNavModal _grid_nav_modal;
	[SerializeField] private Image _background_image;
	
	public ControlManager _controls;
	
	private Modal _active_modal;
	
	public void Start () {
		_controls = ControlManager.cons();
		foreach (GameMain.Modal itr in (new List<Modal>() { _event_modal, _grid_nav_modal })) {
			itr.i_initialize(this);
		}
		
		_active_modal = _grid_nav_modal;
		_active_modal.set_active(true);
	}
	
	public void Update () {
		_controls.i_update();
		_active_modal.i_update(this);
	}
}
