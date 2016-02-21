using UnityEngine;
using System.Collections;

public class SPBaseBehavior : MonoBehaviour, GenericPooledObject {
	public virtual void Start () {}
	public virtual void Update () {}
	
	private bool _active = false;
	public virtual bool is_active() {
		return _active;
	}
	public virtual void depool () {
		_active = true;
	}
	public virtual void repool () {
		_active = false;
	}
}
