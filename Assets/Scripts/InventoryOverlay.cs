using UnityEngine;
using System.Collections.Generic;

public class InventoryOverlay : MonoBehaviour {

	[SerializeField] private InventoryOverlayElement _proto_element;
	
	private List<InventoryOverlayElement> _inventory_elements = new List<InventoryOverlayElement>();
	
	public void i_initialize() {
		_proto_element.gameObject.SetActive(false);
	}
	
	private HashSet<string> __rendered_items = new HashSet<string>();
	public void i_update(GameMain game, GridNavModal grid_nav) {
		__rendered_items.Clear();
		for (int i = _inventory_elements.Count - 1; i >= 0; i--) {
			InventoryOverlayElement itr = _inventory_elements[i];
			__rendered_items.Add(itr._item_name);
		}
		
		foreach (string itr in game._inventory._items) {
			if (!__rendered_items.Contains(itr)) {
				InventoryOverlayElement neu = SPUtil.proto_clone(_proto_element.gameObject).GetComponent<InventoryOverlayElement>();
				neu.i_initialize(itr,game);
				_inventory_elements.Add(neu);
			}
		}
		
		for (int i = _inventory_elements.Count - 1; i >= 0; i--) {
			InventoryOverlayElement itr = _inventory_elements[i];
			itr.i_update();
			if (!game._inventory._items.Contains(_inventory_elements[i]._item_name)) {
				_inventory_elements.RemoveAt(i);
				GameObject.Destroy(itr.gameObject);
			}
		}
		this.layout_elements();
	}
	
	private void layout_elements() {
		float layout_x = 0;
		for (int i = 0; i < _inventory_elements.Count; i++) {
			InventoryOverlayElement itr = _inventory_elements[i];
			itr.transform.localPosition = new Vector3(
				layout_x, itr.transform.localPosition.y
			);
			layout_x += 80;
		}
	}
	
}
