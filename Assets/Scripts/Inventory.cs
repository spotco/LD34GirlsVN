using UnityEngine;
using System.Collections.Generic;

public class Inventory {

	public HashSet<string> _items = new HashSet<string>();
	
	public void add_item(string item) {
		_items.Add(item);
	}
	
	public void remove_item(string item) {
		_items.Remove(item);
	}
	
	private static Dictionary<string,Sprite> __name_to_bgsprite = new Dictionary<string, Sprite>();
	private Sprite cond_get_bgsprite(string name) {
		name = "img/item/item_"+name;
		if (__name_to_bgsprite.ContainsKey(name)) return __name_to_bgsprite[name];
		Sprite bg_sprite = Resources.Load<Sprite>(name);
		if (bg_sprite != null) {
			__name_to_bgsprite[name] = bg_sprite;
		} else {
			__name_to_bgsprite[name] = Resources.Load<Sprite>("img/item/item_generic");
			bg_sprite = __name_to_bgsprite[name];
			SPUtil.logf("No image for item(%s)",name);
		}
		return bg_sprite;
	}
	public Sprite icon_for_item(string item) {
		return this.cond_get_bgsprite(item);
	}

}
