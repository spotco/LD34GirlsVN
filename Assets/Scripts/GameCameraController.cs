using UnityEngine;
using System.Collections.Generic;

public class GameCameraController  {
	
	public static GameCameraController cons() {
		return (new GameCameraController()).i_cons();
	}
	
	public GameCameraController i_cons() {
		return this;
	}
	
	private Vector2 _last_shake;
	private void apply_camera_values(GameMain game) {
		Vector2 camera_shake = new Vector2();
		if (_camera_shake_ct > 0) {
			float frame_camera_shake_intensity = SPUtil.bezier_val_for_t(
				new Vector2(0,0),
				new Vector2(0,1),
				new Vector2(0,1),
				new Vector2(1,1),
				_camera_shake_ct/_camera_shake_ct_max
			).y * _camera_shake_intensity;
			camera_shake.x = frame_camera_shake_intensity * Mathf.Cos(_camera_shake_theta * _camera_shake_speed.x);
			camera_shake.y = frame_camera_shake_intensity * Mathf.Sin(_camera_shake_theta * _camera_shake_speed.y);
			_last_shake = camera_shake;
		} else {
			_last_shake.x = SPUtil.drpt(_last_shake.x,0,1/10.0f);
			_last_shake.y = SPUtil.drpt(_last_shake.y,0,1/10.0f);
			camera_shake = _last_shake;
		}
		
		game.transform.localPosition = new Vector3(
			camera_shake.x, camera_shake.y, 0
		);
	}
	
	private float _camera_shake_ct, _camera_shake_theta, _camera_shake_intensity, _camera_shake_ct_max;
	private Vector2 _camera_shake_speed;
	public void camera_shake(Vector2 speed, float intensity, float duration) {
		_camera_shake_speed = speed;
		_camera_shake_intensity = intensity;
		_camera_shake_ct = duration;
		_camera_shake_ct_max = duration;
		_camera_shake_theta = SPUtil.float_random(0,2*Mathf.PI);
	}
	
	public void i_update(GameMain game) {
		_camera_shake_ct = Mathf.Max(0,_camera_shake_ct - SPUtil.dt_scale_get());
		if (_camera_shake_intensity < 0.1f) {
			_camera_shake_theta = 0;
		} else {
			_camera_shake_theta = _camera_shake_theta + SPUtil.dt_scale_get() * 0.1f;
		}
		
		this.apply_camera_values(game);
	}
	
	public void long_shake() {
		this.camera_shake(new Vector2(-2f,2.7f),5,100);
	}
	public void short_shake() {
		this.camera_shake(new Vector2(-4,4.6f),8,10);
	}
	
}
