using UnityEngine;
using System.Collections;

public class RShader {
	public static string DEFAULT = "Custom/SPSpriteDefault";
	public static string SPTEXTCHARACTER = "Custom/SPTextCharacter";
}

public class ShaderResource : Object {

	public static Shader get_shader(string key) {
		return Shader.Find(key);
	}

}
