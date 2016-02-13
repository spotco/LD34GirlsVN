using UnityEngine;
using System.Collections;

public class SPBaseBehavior : MonoBehaviour, GenericPooledObject {
	public virtual void Start () {}
	public virtual void Update () {}
	public virtual void depool () {}
	public virtual void repool () {}
}
