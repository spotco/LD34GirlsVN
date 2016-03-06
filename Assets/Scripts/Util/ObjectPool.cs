using UnityEngine;
using System.Collections.Generic;

public interface GenericPooledObject {
	void depool();
	void repool();
}

public class ObjectPool : SPBaseBehavior {
	
	private static ObjectPool __inst;
	public static ObjectPool inst() {
		return __inst;
	}
	
	public static ObjectPool cons() {
		GameObject neu_obj = new GameObject("ObjectPool");
		neu_obj.AddComponent<Canvas>();
		__inst = neu_obj.AddComponent<ObjectPool>().i_cons();	
		return __inst;
	}

	private MultiMap<string,SPBaseBehavior> _spbasebehavior_typekey_to_objlist = new MultiMap<string,SPBaseBehavior>();

	public ObjectPool i_cons() {
		return this;
	}

	public T spbasebehavior_depool<T>(bool create = true) where T : SPBaseBehavior {
		string key = typeof(T).ToString();
		List<SPBaseBehavior> tar_list = _spbasebehavior_typekey_to_objlist.list(key);
		if (tar_list.Count == 0) {
			if (create) {
				GameObject neu_obj = new GameObject(key);
				neu_obj.AddComponent<T>();
				neu_obj.SetActive(true);
				neu_obj.GetComponent<T>().depool();
				return neu_obj.GetComponent<T>();
			} else {
				return null;
			}
			
		} else {
			T rtv = (T)tar_list[0];
			tar_list.RemoveRange(0,1);
			rtv.gameObject.SetActive(true);
			((T)rtv).depool();
			return (T)rtv;
		}
	}

	public void spbasebehavior_repool<T>(T obj) where T : SPBaseBehavior {
		string key = typeof(T).ToString();
		obj.repool();
		obj.gameObject.SetActive(false);
		obj.gameObject.transform.SetParent(this.transform);
		_spbasebehavior_typekey_to_objlist.list(key).Add(obj);
	}


	private MultiMap<string,GenericPooledObject> _generic_typekey_to_objlist = new MultiMap<string,GenericPooledObject>();
	public T generic_depool<T>() where T : GenericPooledObject {
		string key = typeof(T).ToString();
		List<GenericPooledObject> tar_list = _generic_typekey_to_objlist.list(key);
		if (tar_list.Count == 0) {
			T rtv = System.Activator.CreateInstance<T>();
			rtv.depool();
			return rtv;
		} else {
			T rtv = (T)tar_list[0];
			tar_list.RemoveRange(0,1);
			rtv.depool();
			return rtv;
		}
	}

	public void generic_repool<T>(T obj) where T : GenericPooledObject {
		this.type_repool(obj,typeof(T));
	}
	public void type_repool(GenericPooledObject obj, System.Type type) {
		string key = type.ToString();
		obj.repool();
		_generic_typekey_to_objlist.list(key).Add(obj);
	}
}
