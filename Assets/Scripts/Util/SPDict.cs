using UnityEngine;
using System.Collections.Generic;

public class SPDict<TKey,TValue> {
	private Dictionary<TKey,TValue> _dict;
	private List<TKey> _list;

	public SPDict() {
		_dict = new Dictionary<TKey, TValue>();
		_list = new List<TKey>();
	}

	public TValue this[TKey i] {
		get { 
			if (_dict.ContainsKey(i)) {
				return _dict[i];
			} else {
				return default(TValue);
			}
		}
		set { 
			_dict[i] = (TValue)value;
			if (!_list.Contains(i)) {
				_list.Add(i);
			}
		}
	}

	public void Remove(TKey i) {
		_dict.Remove(i);
		_list.Remove(i);
	}

	public void Clear() {
		_dict.Clear();
		_list.Clear();
	}

	public List<TKey> key_itr() {
		return _list;
	}

	public bool ContainsKey(TKey i) {
		return _dict.ContainsKey(i);
	}
}

public class SPSet<TKey> {
	private SPDict<TKey,int> _dict;
	
	public SPSet() {
		_dict = new SPDict<TKey, int>();
	}
	
	public void Add(TKey i) {
		_dict[i] = 1;
	}
	
	public void Remove(TKey i) {
		_dict.Remove(i);
	}
	
	public void Clear() {
		_dict.Clear();
	}
	
	public List<TKey> key_itr() {
		return _dict.key_itr();
	}
	
	public bool ContainsKey(TKey i) {
		return _dict.ContainsKey(i);
	}
	
}
