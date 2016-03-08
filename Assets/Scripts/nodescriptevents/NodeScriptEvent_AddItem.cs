using UnityEngine;
using System.Collections;

public class NodeScriptEvent_AddItem : NodeScriptEvent {
	public string _item;
	
	private bool _popup_added;
	private Popup _popup;
	
	public override void i_initialize(GameMain game, EventModal modal) {
		_popup_added = false;
	}
	
	public override void i_update(GameMain game, EventModal modal) {
		if (!_popup_added) {
			game._inventory.add_item(_item);
			game._popups.add_popup(SPUtil.sprintf("Got <color=\"#FF00FF\">%s</color>.",_item));
			game._music.play_sfx("item_get");
			game._music.fade_bgm_for_time(1.0f);
			_popup_added = true;
		} else {
			if (_popup == null) {
				modal.advance_script();
			}
		}
	}
	
}