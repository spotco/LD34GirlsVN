using UnityEngine;
using System.Collections;

public class NodeScriptEvent_RemoveItem : NodeScriptEvent {
	public string _item;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._inventory.remove_item(_item);
		modal.advance_script();
	}
	
}
