using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridNode : MonoBehaviour {

	[SerializeField] private TextAsset _node_script_text;
	[SerializeField] private Text _title_ui_text;
	private NodeScript _node_script = new NodeScript();
	
	public void i_initialize() {
		_node_script.i_initialize(_node_script_text);
	
		_title_ui_text.text = "test titre";
	}
	
	public void i_update() {
	
	}
	
	
}
