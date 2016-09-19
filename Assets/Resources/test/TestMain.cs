using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestMain : MonoBehaviour {

	[SerializeField] private Image _test1;
	[SerializeField] private Image _test2;
	[SerializeField] private Image _test3;


	public void Start() {
		Texture2D tex = Resources.Load<Texture2D>("test/osaka_test");
		
		SPUtil.logf("%s",tex);
		
		//char(T) rect((x:879.00, y:79.00, width:51.00, height:61.00))
		Sprite sp1 = Sprite.Create(tex, new Rect(879,512-79-61, 51, 61), new Vector2(0, 0));
		Sprite sp2 = Sprite.Create(tex, new Rect(100,0, 512, 512), new Vector2(0f, 1f));
		Sprite sp3 = Sprite.Create(tex, new Rect(200,0, 512, 512), new Vector2(0f, 1f));
		
		Material mat1 = new Material(Shader.Find("Custom/testchar"));
		mat1.SetVector("_fill_color", new Vector4(1,1,1,1));
		mat1.SetVector("_stroke_color", new Vector4(94/255.0f,94/255.0f,94/255.0f,1));
		mat1.SetVector("_shadow_color", new Vector4(0,0,1,0));
		mat1.SetFloat("_opacity", 0.5f);
		
		Material mat2 = new Material(Shader.Find("Custom/testchar"));
		mat2.SetVector("_fill_color", new Vector4(1,1,1,1));
		mat2.SetVector("_stroke_color", new Vector4(94/255.0f,94/255.0f,94/255.0f,1));
		mat2.SetVector("_shadow_color", new Vector4(0,0,1,0));
		mat2.SetFloat("_opacity", 0.1f);
		
		_test1.material = mat1;
		_test3.material = mat2;
		
		_test1.sprite = sp1;
		_test2.sprite = sp2;
		_test3.sprite = sp3;
	}
}
