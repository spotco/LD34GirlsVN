using UnityEngine;
using System.Collections;

public class NodeScriptEvent_AddItem : NodeScriptEvent {
	public string _item;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._inventory.add_item(_item);
		modal.advance_script();
	}
	
}