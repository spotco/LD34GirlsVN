using UnityEngine;
using UnityEngine.UI;

public class PooledRawImageTarget : SPBaseBehavior {

	private RawImage _image = null;
	private SPSpriteAnimator.RawImageTargetAdapter _target = null;
	
	public RawImage get_image() {
		if (_image == null) {
			_image = this.gameObject.AddComponent<RawImage>();
		}
		return _image;
	}
	public SPSpriteAnimator.Target get_target() {
		if (_target == null) {
			_target = new SPSpriteAnimator.RawImageTargetAdapter() {
				_image = this.get_image()
			};
		}
		return _target;
	}
	
}
