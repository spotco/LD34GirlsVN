using UnityEngine;
using System.Collections;

public class NodeScriptEvent_ShowCharacter : NodeScriptEvent {
	public string _character;
	public string _image;
	public float _xpos;
	public float _xscale;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter neu_char = modal.add_character(_character,_image);
		if (neu_char != null) {
			neu_char.transform.localPosition = new Vector3(
				_xpos,
				neu_char.transform.localPosition.y
			);
			neu_char._image.SetNativeSize();
			neu_char.transform.localScale = SPUtil.valv(_xscale * 0.75f);
		}
		modal.advance_script();
	}
	
}
