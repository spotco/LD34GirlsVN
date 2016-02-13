using UnityEngine;
using System.Collections;

public class MeshGen : Object {

	static Mesh _cached_unit_quad_mesh;
	public static Mesh get_cached_unit_quad_mesh() {
		if (_cached_unit_quad_mesh == null) {
			_cached_unit_quad_mesh = MeshGen.get_unit_quad_mesh();
		}
		return _cached_unit_quad_mesh;
	}

	public static Mesh get_unit_quad_mesh() {
		var verts = new Vector3[4] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0) };
		var uvs = new Vector2[4] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
		var tris = new int[6] { 2, 1, 0,  0, 3, 2 };

		var rtv = new Mesh();
		rtv.vertices = verts;
		rtv.uv = uvs;
		rtv.triangles = tris;

		return rtv;
	}

}
