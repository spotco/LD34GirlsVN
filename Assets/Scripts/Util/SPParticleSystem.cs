using UnityEngine;
using System.Collections.Generic;

public interface SPParticle {
	void i_update(GameMain game, System.Object context);
	bool should_remove(GameMain game, System.Object context);
	void do_remove(GameMain game, System.Object context);
	void add_to_parent(Transform parent);
}

public class SPParticleSystem<T> where T : SPParticle {
	public static SPParticleSystem<T> cons(Transform parent) {
		return (new SPParticleSystem<T>()).i_cons(parent);
	}
	
	private List<T> _particles = new List<T>(), _to_remove = new List<T>(), _to_add = new List<T>();
	private Transform _parent;
	public Transform get_parent() { return _parent; }
	
	private SPParticleSystem<T> i_cons(Transform parent) {
		_parent = parent;
		return this;
	}
	
	public virtual void add_particle(T p) { _to_add.Add(p); }
	public virtual void i_update(GameMain game, System.Object context) {
		for (int i = 0; i < _to_add.Count; i++) {
			T itr = _to_add[i];
			_particles.Add(itr);
			itr.add_to_parent(_parent);
		}
		_to_add.Clear();
		
		for (int i = 0; i < _particles.Count; i++) {
			T itr = _particles[i];
			itr.i_update(game,context);
			if (itr.should_remove(game,context)) {
				itr.do_remove(game,context);
				_to_remove.Add(itr);
			}
		}
		
		for (int i = 0; i < _to_remove.Count; i++) {
			T itr = _to_remove[i];
			_particles.Remove(itr);
		}
		_to_remove.Clear();
	}
	public virtual void clear(GameMain game, System.Object context) {
		for (int i = 0; i < _particles.Count; i++) {
			T itr = _particles[i];
			itr.do_remove(game,context);
		}
		_particles.Clear();
	}
	public List<T> list() { return _particles; }
}