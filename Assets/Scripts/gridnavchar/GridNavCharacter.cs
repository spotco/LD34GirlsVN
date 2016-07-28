using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridNavCharacter : MonoBehaviour {
	
	[SerializeField] private Image _image;
	
	public void i_initialize(GameMain game) {
	}
	
	public void i_update(GameMain game) {
	}
	
	
	public static Vector2 convert_character_position_to_position_anchor_focus(Vector2 char_pos) {
		return SPUtil.vec_scale(char_pos, -1);
	}
	
}
