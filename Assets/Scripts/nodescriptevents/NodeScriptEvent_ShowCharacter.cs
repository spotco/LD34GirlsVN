using UnityEngine;
using System.Collections;

public class NodeScriptEvent_ShowCharacter : NodeScriptEvent {
	public string _character;
	public string _image;
	public float _xpos;
	public float _xscale;
	public bool _imm;
	
	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter neu_char = modal.add_character(_character,_image);
		if (_imm) {
			neu_char.imm_show();
		}
		if (neu_char != null) {
			neu_char.transform.localPosition = new Vector3(
				_xpos,
				neu_char.transform.localPosition.y
			);
			neu_char._image.SetNativeSize();
			neu_char.transform.localScale = SPUtil.valv(0.75f);
			neu_char._image.transform.localScale = new Vector3(
				_xscale,
				neu_char._image.transform.localScale.y,
				neu_char._image.transform.localScale.z
			);
		}
		modal.advance_script();
	}
	
}
