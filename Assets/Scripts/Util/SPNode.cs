using UnityEngine;
using System.Collections.Generic;

public interface SPNodeHierarchyElement {
	void add_to_parent(SPNode parent);
}

public class SPNode : SPBaseBehavior {

	protected static T generic_cons<T>() where T : SPNode {
		T rtv = GameMain._context._objpool.spbasebehavior_depool<T>();
		rtv.transform.parent = GameMain._context.gameObject.transform;
		rtv.__ACTIVE = true;
		rtv.set_name(typeof(T).ToString());
		rtv.i_spnode_cons();
		return rtv;
	}

	protected static void generic_repool<T>(T obj) where T : SPNode {
		if (!obj.__ACTIVE) {
			Debug.LogError("repool OPERATION ON POOLED SPNODE!");
		}

		if (obj._parent != null) obj._parent.remove_child(obj);
		obj._parent = null;
		obj.remove_all_children(true);
		obj.__ACTIVE = false;
		GameMain._context._objpool.spbasebehavior_repool<T>(obj);
	}

	public static SPNode cons_node() {
		return SPNode.generic_cons<SPNode>();
	}

	[SerializeField] private bool __ACTIVE = false;
	public bool get_obj_active() { return __ACTIVE; }
	public new virtual void repool() {
		SPNode.generic_repool<SPNode>(this);
	}
	
	private SPNode i_spnode_cons() {
		this.transform.localScale = SPUtil.valv(1.0f);
		this.set_u_pos(0,0);
		this.set_u_z(0);
		this.set_rotation(0);
		this.set_scale(1);

		this._has_set_manual_sort_z_order = false;
		this.set_sort_z(0);
		return this;
	}

	public SPNode set_name(string name) {
		this.gameObject.name = name;
		return this;
	}

	[SerializeField] protected float _rotation;
	[SerializeField] protected float _scale_x;
	[SerializeField] protected float _scale_y;
	[SerializeField] protected Vector3 _u_pos = new Vector3();
	
	private void set_u_pos(Vector3 val) {
		if (!__ACTIVE) Debug.LogError("set_u_pos OPERATION ON POOLED SPNODE!");

		_u_pos = val; 
		this.transform.localPosition = new Vector3(_u_pos.x,_u_pos.y,_u_pos.z);
	}
	
	public float _u_x {
		get { 
			return _u_pos.x; 
		} 
		set { 
			_u_pos.x = value; 
			this.set_u_pos(_u_pos);
		}
	}
	public float _u_y {
		get { 
			return _u_pos.y; 
		} 
		set { 
			_u_pos.y = value; 
			this.set_u_pos(_u_pos);
		}
	}
	public float _u_z {
		get { 
			return _u_pos.z; 
		} 
		set { 
			_u_pos.z = value;
			this.set_u_pos(_u_pos);
		}
	}
	public SPNode set_u_pos(float x, float y) { _u_x = x; _u_y = y;  return this; }
	public SPNode set_u_pos(Vector2 u_pos) { return this.set_u_pos(u_pos.x,u_pos.y); }
	public SPNode set_u_z(float z) { 
		_u_z = z;
		return this; 
	}
	public Vector2 get_u_pos() { return new Vector2(_u_x,_u_y); }
	
	
	public SPNode set_rotation(float deg) { this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x,this.transform.localEulerAngles.y, deg); _rotation = deg; return this; }
	public float rotation() { return _rotation; }
	
	public virtual SPNode set_scale_x(float scx) { this.transform.localScale = new Vector3(scx, this.transform.localScale.y, this.transform.localScale.z); _scale_x = scx; return this; }
	public virtual SPNode set_scale_y(float scy) { this.transform.localScale = new Vector3(this.transform.localScale.x, scy, this.transform.localScale.z); _scale_y = scy; return this; }
	public virtual SPNode set_scale(float sc) { this.transform.localScale = new Vector3(sc, sc, this.transform.localScale.z); _scale_x = sc; _scale_y = sc; return this; }
	public virtual float scale_x() { return _scale_x; }
	public virtual float scale_y() { return _scale_y; }

	[SerializeField] protected Vector2 _anchorpoint = new Vector2();
	public virtual Vector2 anchorpoint() { return _anchorpoint; }
	public virtual SPNode set_anchor_point(float x, float y) { _anchorpoint.x = x; _anchorpoint.y = y; return this; }

	public virtual SPNode set_opacity(float val) { return this; }
	public virtual float get_opacity() { return 1.0f; }
	
	[SerializeField] public List<SPNode> _children = new List<SPNode>();
	[SerializeField] public SPNode _parent;
	
	public void add_child(SPNode child) {
		if (child._parent != null) {
			Debug.LogError("Child already has parent");
		}
		child._parent = this;
		child.transform.parent = this.transform;
		child.set_u_pos(child._u_x,child._u_y);
		child.set_scale_x(child._scale_x);
		child.set_scale_y(child._scale_y);
		child.set_rotation(child.rotation());
		_children.Add(child);
		sort_children();
	}
	
	public void remove_child(SPNode child, bool cleanup = false) {
		bool found = false;
		for (int i = 0; i < _children.Count; i++ ){
			SPNode itr = _children[i];
			if (itr == child) {
				child._parent = null;
				child.transform.parent = null;
				_children.RemoveRange(i,1);
				if (cleanup) child.repool();
				found = true;
				break;
			}
		}
		if (!found) {
			Debug.LogError("REMOVE_CHILD NOT FOUND");
		}
		sort_children();
	}

	public void remove_from_parent(bool cleanup = false) {
		if (_parent != null) {
			_parent.remove_child(this,cleanup);
		} else {
			if (cleanup) this.repool();
		}
	}
	
	public void remove_all_children(bool cleanup = false) {
		while (_children.Count > 0) {
			SPNode itr = _children[0];
			itr._parent = null;
			itr.transform.parent = null;
			if (cleanup) itr.repool();
			_children.RemoveAt(0);
		}
	}
	
	private void sort_children() {
		for (int i = 0; i < _children.Count; i++ ){
			SPNode itr = _children[i];
			if (!itr._has_set_manual_sort_z_order) {
				itr.set_sort_z(_sort_z+(i+1));
			}

		}
	}

	[SerializeField] public int _sort_z;
	public virtual void set_sort_z(int zt) {
		_sort_z = zt;
		this.sort_children();
	}

	[SerializeField] protected bool _has_set_manual_sort_z_order = false;
	public virtual void set_manual_sort_z_order(int val) {
		_has_set_manual_sort_z_order = true;
		this.set_sort_z(val);
		this.sort_children();
	}

	public bool is_enabled() {
		return this.gameObject.activeSelf;
	}
	public SPNode set_enabled(bool val) {
		this.gameObject.SetActive(val);
		return this;
	}

}
