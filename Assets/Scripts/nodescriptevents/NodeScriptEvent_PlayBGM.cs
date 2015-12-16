using UnityEngine;
using System.Collections;

public class NodeScriptEvent_PlayBGM : NodeScriptEvent {
	public string _bgm;
	
	public override void i_update(GameMain game, EventModal modal) {
		game._music.load_music(_bgm);
		modal.advance_script();
	}
}
