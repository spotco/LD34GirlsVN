using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridCharacterManager : MonoBehaviour {

	[SerializeField] private GridCharacterAsset _mana_normal;
	[SerializeField] private GridCharacterAsset _mana_hero;
	[SerializeField] private GridCharacterAsset _raichi_normal;
	[SerializeField] private GridCharacterAsset _simone_hero;
	
	public void i_initialize(GameMain game, GridNavModal gridnav) {
		this.register_asset(game,gridnav,"mana",_mana_normal);
		this.register_asset(game,gridnav,"mana_hero",_mana_hero);
		this.register_asset(game,gridnav,"raichi",_raichi_normal);
		this.register_asset(game,gridnav,"simone_hero",_simone_hero);
	}
	
	private void register_asset(GameMain game, GridNavModal gridnav, string name, GridCharacterAsset asset) {
		asset.gameObject.SetActive(false);
		game._objpool.spbasebehavior_repool<GridCharacterAsset>(SPUtil.proto_clone(asset.gameObject).GetComponent<GridCharacterAsset>(), name);
	}
	
	private static string make_preview_char_key(int node_id, string index) { return string.Format("{0}-{1}",node_id,index); }
	private SPDict<string,PreviewCharacter> _key_to_previewcharacter = new SPDict<string, PreviewCharacter>();
	
	private SPDict<string,SPPair<int,string>> __active_previewcharacter_keys = new SPDict<string,SPPair<int,string>>();
	public void i_update(GameMain game, GridNavModal gridnav) {
		__active_previewcharacter_keys.Clear();
		
		for (int i = 0; i < gridnav._active_gridnodes.key_itr().Count; i++) {
			GridNode itr_node = gridnav._id_to_gridnode[gridnav._active_gridnodes.key_itr()[i]];
			if (gridnav.is_selected_node(game,itr_node)) {
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
				
				SPUtil.logf("create(%s)",itr_key);
				
				PreviewCharacter add_char = game._objpool.generic_depool<PreviewCharacter>();
				GridCharacterAsset add_asset = game._objpool.spbasebehavior_depool<GridCharacterAsset>(false, itr_key);
				if (add_asset == null) {
					continue;
				}
				add_char.i_initialize(game,gridnav,this,add_asset,__active_previewcharacter_keys[itr_key]);
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
			
				SPUtil.logf("remove(%s)",itr_key);
			
				itr_char.do_remove(game);
				_key_to_previewcharacter.Remove(itr_key);
			}
		}
	}
}

public class PreviewCharacter : GenericPooledObject {
	public void depool(){}
	public void repool(){}
	
	private GridCharacterAsset _asset;
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
		_asset = asset;
		_nodeid_and_charname = nodeid_and_charname;
		
		_current_mode = Mode.HiddenToShowing;
		_mode_ct = 0;
	}
	public void i_update(GameMain game, GridNavModal gridnav, GridCharacterManager manager) {
	
		GridNode tar_node = gridnav._id_to_gridnode[_nodeid_and_charname._first];
		_asset.transform.localPosition = tar_node.get_selector_stand_position();
	
		switch (_current_mode) {
		case Mode.HiddenToShowing: {
			_mode_ct += SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get();
			
			_asset._image.color = new Color(1,1,1,SPUtil.y_for_point_of_2pt_line(new Vector2(0,0), new Vector2(1,1),_mode_ct));
			
			if (_mode_ct >= 1) {
				_current_mode = Mode.Showing;
			}
			
		} break;
		case Mode.Showing: {
			
		} break;
		case Mode.ShowingToHidden: {
			_mode_ct += SPUtil.sec_to_tick(0.5f) * SPUtil.dt_scale_get();
			
			_asset._image.color = new Color(1,1,1,SPUtil.y_for_point_of_2pt_line(new Vector2(0,1), new Vector2(1,0),_mode_ct));
			
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
