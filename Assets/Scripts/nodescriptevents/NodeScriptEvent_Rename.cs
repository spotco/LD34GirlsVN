using UnityEngine;
using System.Collections;

public class NodeScriptEvent_Rename : NodeScriptEvent {
	
	public string _name_start;
	public string _name_end;
	
	public override void i_update(GameMain game, EventModal modal) {
		modal.rename(_name_start,_name_end);
		modal.advance_script();
	}
}
