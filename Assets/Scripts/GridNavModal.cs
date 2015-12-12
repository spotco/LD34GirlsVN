using UnityEngine;
using System.Collections.Generic;

public class GridNavModal : MonoBehaviour, GameMain.Modal {
	
	[SerializeField] private Transform _grid_map_anchor;
	private Dictionary<int,GridNode> _id_to_gridnode = new Dictionary<int, GridNode>();
			
	public void i_initialize(GameMain game) {
		this.set_active(false);
		
		for (int i = 0; i < _grid_map_anchor.childCount; i++) {
			GameObject itr = _grid_map_anchor.GetChild(i).gameObject;
			if (itr.GetComponent<GridNode>() != null) {
				GridNode itr_gridnode = itr.GetComponent<GridNode>();
				itr_gridnode.i_initialize();
				if (!_id_to_gridnode.ContainsKey(itr_gridnode._node_script._id)) {
					_id_to_gridnode[itr_gridnode._node_script._id] = itr_gridnode;
				} else {
					SPUtil.logf("Duplicate gridnode id(%d) on gameobject(%s)",itr_gridnode._node_script._id,itr.name);
				}
				
			} else {
				SPUtil.logf("GridMapAnchor Child(%s) no GridNode component",itr.name);
			}
		}
		
	}
	public void i_update(GameMain game) {
		
	}
	public void set_active(bool val) {
		this.gameObject.SetActive(val);
	}
	
}
