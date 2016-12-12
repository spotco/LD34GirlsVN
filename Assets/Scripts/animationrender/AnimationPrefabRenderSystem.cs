using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnimationPrefabRenderSystem {

	public static AnimationPrefabRenderSystem cons(GameMain game) {
		return (new AnimationPrefabRenderSystem()).i_cons(game);
	}

	private GameObject _anim_prefab_render_system_root;


	private RenderTexture __tmp_rtv;

	public AnimationPrefabRenderSystem i_cons(GameMain game) {

		_anim_prefab_render_system_root = new GameObject("AnimationPrefabRenderSystem");

		GameObject load_resc_obj = Object.Instantiate(Resources.Load<GameObject>("AnimationPrefabs/animprefab_prologue_frame1_mana"));
		load_resc_obj.transform.parent = _anim_prefab_render_system_root.transform;
		load_resc_obj.transform.localPosition = new Vector3(-500,0,0);

		Camera load_resc_render_camera = load_resc_obj.GetComponentInChildren<Camera>(true);
		AnimationPrefabParams load_resc_prefab_params = load_resc_obj.GetComponentInChildren<AnimationPrefabParams>(true);

		RenderTexture load_resc_render_tex = new RenderTexture(
			(int)load_resc_prefab_params._rendertex_size.x, 
			(int)load_resc_prefab_params._rendertex_size.y, 
			16
		);

		load_resc_render_camera.targetTexture = load_resc_render_tex;
		load_resc_render_camera.gameObject.SetActive(true);

		__tmp_rtv = load_resc_render_tex;

		return this;
	}

	public Texture get_texture_for_animprefab(string animprefab) {
		return __tmp_rtv;
	}


}
