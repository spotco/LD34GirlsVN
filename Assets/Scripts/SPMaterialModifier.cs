using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SPMaterialModifier : MonoBehaviour, IMaterialModifier {
	
	private Graphic _g;
	private SPDict<string,Vector4> _vector_props;
	
	public void i_initialize() {
		_g = GetComponent<Graphic>();
		_vector_props = new SPDict<string, Vector4>();
	}
	
	public void set_vector(string name, Vector4 val) {
		_vector_props[name] = val;
	}
	
	public void finish_set() {
		_g.SetMaterialDirty();
	}

	public Material GetModifiedMaterial (Material baseMaterial) {
		List<string> vector_key_itr = _vector_props.key_itr();
		for (int i = 0; i < vector_key_itr.Count; i++) {
			baseMaterial.SetVector(vector_key_itr[i], _vector_props[vector_key_itr[i]]);
		}
		return baseMaterial;
	}
}