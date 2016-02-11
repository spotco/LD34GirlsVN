using UnityEngine;
using System.Collections;

public class NodeScriptEvent_RemoveItem : NodeScriptEvent {
	public string _item;
	
	public override void i_update(GameMain game, EventModal modal) {
		modal.advance_script();
		game._popups.add_popup(SPUtil.sprintf("Used <color=\"#FF00FF\">%s</color>.",_item));
		game._music.play_sfx("item_use");
		game._music.fade_bgm_for_time(1.0f);
	}
	
}
