using UnityEngine;
using System.Collections;

public class NodeScriptEvent_AddItem : NodeScriptEvent {
	public string _item;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._inventory.add_item(_item);
		modal.advance_script();
		game._popups.add_popup(SPUtil.sprintf("Got <color=\"#FF00FF\">%s</color>.",_item));
		game._music.play_sfx("item_get");
	}
	
}