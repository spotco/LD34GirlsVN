using UnityEngine;
using UnityEngine.UI;

public class PooledSpriteImageTarget : SPBaseBehavior {
	
	private Image _image = null;
	private SPSpriteAnimator.SpriteImageTargetAdapter _target = null;
	
	public Image get_image() {
		if (_image == null) {
			_image = this.gameObject.AddComponent<Image>();
		}
		return _image;
	}
	public SPSpriteAnimator.Target get_target(Texture tex) {
		if (_target == null) {
			_target = new SPSpriteAnimator.SpriteImageTargetAdapter() {
				_image = this.get_image(),
				_texture = tex
			};
		}
		return _target;
	}
	
}
