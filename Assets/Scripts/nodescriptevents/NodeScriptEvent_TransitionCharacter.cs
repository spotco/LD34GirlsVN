using UnityEngine;
using System.Collections;

public class NodeScriptEvent_TransitionCharacter : NodeScriptEvent {
	public string _character;
	public string _image;
	public float _xscale;

	public override void i_update(GameMain game, EventModal modal) {
		EventCharacter tar = modal.cond_get_character_of_name(_character);
		if (tar != null) {
			Sprite char_sprite = Resources.Load<Sprite>("img/character/"+_image);
			if (char_sprite == null) {
				SPUtil.logf("no character found of path(%s)",_character);
				modal.advance_script();
				return;
			}
			tar.set_image(char_sprite,_image);
			tar.set_facing(_xscale);
			/*
			tar.transform.localScale = SPUtil.valv(0.75f);
			tar._image.transform.localScale = new Vector3(
				_xscale,
				tar._image.transform.localScale.y,
				tar._image.transform.localScale.z
			);
			*/
			
		}
		modal.advance_script();
	}

}

/*

*/