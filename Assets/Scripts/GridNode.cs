using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GridNode : MonoBehaviour {

	[SerializeField] private TextAsset _node_script_text;
	[SerializeField] private Text _title_ui_text;
	public NodeScript _node_script = new NodeScript();
	
	public void i_initialize() {
		_node_script.i_initialize(_node_script_text);
		
		this.gameObject.name = SPUtil.sprintf("Node (%d)",_node_script._id);
		_title_ui_text.text = _node_script._title;
	}
	
	public void i_update() {
	
	}
	
	
}
