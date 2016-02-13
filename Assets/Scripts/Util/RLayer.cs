using UnityEngine;
using System.Collections;

public class RLayer {
	public static string DEFAULT = "Default";
	
	public static string REFLECTION_VILLAGE_BACK_HILLS = "ReflectionObjects1";
	public static string REFLECTION_SURFACE_CHARACTER = "ReflectionObjects2";
	public static string REFLECTION_SURFACE_DOCK = "ReflectionObjects3";
	public static string REFLECTION_BUILDINGS = "ReflectionObjects4";
	
	public static string REFLECTIONS = "Reflections";
	public static string SPRITER_NODE = "SpriterNode";
	public static string UNDERWATER_ELEMENTS = "UnderwaterElements";
	public static string SURFACEREFLECTION_ONLY = "SurfaceReflectionOnly";
	public static string UI = "UI";
	public static string OUTPUT = "Output";

	public static int get_layer(string key) {
		return LayerMask.NameToLayer(key);
	}

}
