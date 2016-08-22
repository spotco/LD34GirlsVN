using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridCharacterManager : MonoBehaviour {

	[SerializeField] private GridCharacterAsset _mana_normal;
	[SerializeField] private GridCharacterAsset _mana_hero;
	[SerializeField] private GridCharacterAsset _raichi_normal;
	[SerializeField] private GridCharacterAsset _simone_hero;
	
	private Dictionary<string,GridCharacterAsset> _name_to_asset_proto = new Dictionary<string, GridCharacterAsset>();
	
	public void i_initialize(GameMain game, GridNavModal gridnav) {
		this.register_asset(game,gridnav,"mana",_mana_normal);
		this.register_asset(game,gridnav,"mana_hero",_mana_hero);
		this.register_asset(game,gridnav,"raichi",_raichi_normal);
		this.register_asset(game,gridnav,"simone_hero",_simone_hero);
	}
	
	private void register_asset(GameMain game, GridNavModal gridnav, string name, GridCharacterAsset asset) {
		asset.gameObject.SetActive(false);
		_name_to_asset_proto[name] = asset;
	}
	
	private static string make_preview_char_key(int node_id, string index) { return string.Format("{0}-{1}",node_id,index); }
	private SPDict<string,PreviewCharacter> _key_to_previewcharacter = new SPDict<string, PreviewCharacter>();
	
	private SPDict<string,SPPair<int,string>> __active_previewcharacter_keys = new SPDict<string,SPPair<int,string>>();
	private List<Transform> __sort_zord_previewchars = new List<Transform>();
	public void i_update(GameMain game, GridNavModal gridnav) {
		__active_previewcharacter_keys.Clear();
		
		for (int i = 0; i < gridnav._active_gridnodes.key_itr().Count; i++) {
			GridNode itr_node = gridnav._id_to_gridnode[gridnav._active_gridnodes.key_itr()[i]];
			if (itr_node.get_show_preview_chars_case(game,gridnav)) {
				for (int j = 0; j < itr_node._node_script._previewchars.Count; j++) {
					string char_name = itr_node._node_script._previewchars[j];
					string key = GridCharacterManager.make_preview_char_key(itr_node._node_script._id, char_name);
					__active_previewcharacter_keys[key] = new SPPair<int, string>() {
						_first = itr_node._node_script._id,
						_second = char_name
					};
				}
			}
		}
		
		for (int i = 0; i < __active_previewcharacter_keys.key_itr().Count; i++) {
			string itr_key = __active_previewcharacter_keys.key_itr()[i];
			if (!_key_to_previewcharacter.ContainsKey(itr_key)) {
				SPPair<int,string> nodeid_and_name = __active_previewcharacter_keys[itr_key];
				string name = nodeid_and_name._second;
				PreviewCharacter add_char = game._objpool.generic_depool<PreviewCharacter>();
				GridCharacterAsset add_asset = game._objpool.spbasebehavior_depool<GridCharacterAsset>(false, name);
				if (add_asset == null) {					
					game._objpool.spbasebehavior_repool<GridCharacterAsset>(SPUtil.proto_clone(_name_to_asset_proto[name].gameObject).GetComponent<GridCharacterAsset>(), name);
					add_asset = game._objpool.spbasebehavior_depool<GridCharacterAsset>(false, name);
					if (add_asset == null) {
						SPUtil.errf("Cannot create GridCharacterAsset(%s)",name);
						continue;
					}
				}
				SPUtil.proto_copy_transform(add_asset.gameObject,_name_to_asset_proto[name].gameObject);
				add_char.i_initialize(game,gridnav,this,add_asset,nodeid_and_name);
				_key_to_previewcharacter[itr_key] = add_char;
			}
		}
		
		for (int i = _key_to_previewcharacter.key_itr().Count-1; i >= 0; i--) {
			string itr_key = _key_to_previewcharacter.key_itr()[i];
			PreviewCharacter itr_char = _key_to_previewcharacter[itr_key];
			if (!__active_previewcharacter_keys.ContainsKey(itr_key)) {
				itr_char.set_to_remove();
			}
			itr_char.i_update(game,gridnav,this);
			if (itr_char.should_remove()) {
				itr_char.do_remove(game);
				_key_to_previewcharacter.Remove(itr_key);
			}
		}
		
		__sort_zord_previewchars.Clear();
		for (int i = _key_to_previewcharacter.key_itr().Count-1; i >= 0; i--) {
			string itr_key = _key_to_previewcharacter.key_itr()[i];
			PreviewCharacter itr_char = _key_to_previewcharacter[itr_key];
			__sort_zord_previewchars.Add(itr_char._asset.transform);
		}
		__sort_zord_previewchars.Add(gridnav._selector_character.transform);
		__sort_zord_previewchars.Sort((Transform a, Transform b)=>{
			return (int)(b.localPosition.y-a.localPosition.y);
		});
		for (int i = 0; i < __sort_zord_previewchars.Count; i++) {
			__sort_zord_previewchars[i].transform.SetSiblingIndex(i);
		}
		__sort_zord_previewchars.Clear();
	}
}

public class PreviewCharacter : GenericPooledObject {
	public void depool(){}
	public void repool(){}
	
	public GridCharacterAsset _asset;
	private enum Mode {
		HiddenToShowing,
		Showing,
		ShowingToHidden,
		Hidden
	};
	private Mode _current_mode;
	private float _mode_ct;
	private SPPair<int,string> _nodeid_and_charname;
	
	public void i_initialize(GameMain game, GridNavModal gridnav, GridCharacterManager manager, GridCharacterAsset asset, SPPair<int,string> nodeid_and_charname) {
		_nodeid_and_charname = nodeid_and_charname;
		_asset = asset;
		
		_current_mode = Mode.HiddenToShowing;
		_mode_ct = 0;
		
		_asset.transform.localPosition = this.unshifted_position_get(game,gridnav);
		
		GridNode tar_node = gridnav._id_to_gridnode[_nodeid_and_charname._first];
		if (tar_node._node_script._previewchars.Count > 0 && tar_node._node_script._previewchars[0] == _nodeid_and_charname._second) {
			_asset.gameObject.transform.localScale = new Vector3(-1,1,1);
		} else {
			_asset.gameObject.transform.localScale = new Vector3(1,1,1);
		}
	}
	
	private Vector2 unshifted_position_get(GameMain game, GridNavModal gridnav) {
		GridNode tar_node = gridnav._id_to_gridnode[_nodeid_and_charname._first];
		if (tar_node._node_script._previewchars.Count > 1) {
			if (tar_node._node_script._previewchars[0] == _nodeid_and_charname._second) {
				return tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.DoubleRight);
			} else {
				return tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.DoubleLeft);
			}	
		} else {
			return tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.Solo);
		}
	}
	
	public void i_update(GameMain game, GridNavModal gridnav, GridCharacterManager manager) {
		GridNode tar_node = gridnav._id_to_gridnode[_nodeid_and_charname._first];
		
		Vector2 tar_pos = tar_node.get_center_position();
		if (tar_node.stand_shift_case(game,gridnav)) {
			if (tar_node._node_script._previewchars.Count > 1) {
				if (tar_node._node_script._previewchars[0] == _nodeid_and_charname._second) {
					tar_pos = tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.TripleRight);
				} else {
					tar_pos = tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.TripleLeft);
				}
				
			} else {
				tar_pos = tar_node.get_stand_position_for_anchor(GridNode.StandAnchor.DoubleRight);
			}	
			
		} else {
			tar_pos = this.unshifted_position_get(game,gridnav);
		}
		
		_asset.transform.localPosition = new Vector2(
			SPUtil.drpt(_asset.transform.localPosition.x,tar_pos.x,1/10.0f),
			SPUtil.drpt(_asset.transform.localPosition.y,tar_pos.y,1/10.0f)
		);
	
		switch (_current_mode) {
		case Mode.HiddenToShowing: {
			_mode_ct += SPUtil.sec_to_tick(0.15f) * SPUtil.dt_scale_get();
			
			_asset._canvasgroup.alpha = SPUtil.y_for_point_of_2pt_line(new Vector2(0,0), new Vector2(1,1),_mode_ct);
			
			if (_mode_ct >= 1) {
				_current_mode = Mode.Showing;
			}
			
		} break;
		case Mode.Showing: {
			
		} break;
		case Mode.ShowingToHidden: {
			_mode_ct += SPUtil.sec_to_tick(0.15f) * SPUtil.dt_scale_get();
			
			_asset._canvasgroup.alpha = SPUtil.y_for_point_of_2pt_line(new Vector2(0,1), new Vector2(1,0),_mode_ct);
			
			if (_mode_ct >= 1) {
				_current_mode = Mode.Hidden;
			}
			
		} break;
		default: break;
		}
	}
	public void set_to_remove() {
		if (_current_mode != Mode.ShowingToHidden) {
			_current_mode = Mode.ShowingToHidden;
			_mode_ct = 0;
		}
	}
	public bool should_remove() {
		return _current_mode == Mode.Hidden;
	}
	public void do_remove(GameMain game) {
		game._objpool.generic_repool(this);
		game._objpool.spbasebehavior_repool(_asset,_nodeid_and_charname._second);
		_asset = null;
	}

}
