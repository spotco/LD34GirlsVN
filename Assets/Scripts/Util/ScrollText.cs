using UnityEngine;
using System.Collections;

//https://github.com/spotco/pinballhero/blob/master/src/ScrollText.as
public class ScrollText : MonoBehaviour {
	[SerializeField] public MultiText _text;
	string _words = "";
	string _buf = "";
	int _ct = 0;
	int _spd = 0;
	
	public void load(string words, int speed = 5) {
		_buf = "";
		_ct = 0;
		_spd = speed;
		_text.set_string(_buf);
		_words = words;
	}
	
	int _chars_per_tick = 3;
	public void i_update() {
		if (_text == null) return;
		_ct++;
		if (_ct >= _spd) {
			_ct = 0;
			int tmp = 0;
			while( _words.Length > _buf.Length) {
				_buf += _words[_buf.Length];
				tmp++;
				if (tmp >= _chars_per_tick) break;
			}
			_text.set_string(_buf);
		}
	}
	
	public bool finished() { return _buf.Length == _words.Length; }
	public void finish() { 
		_buf = _words;
		_text.set_string(_buf);
	}
	public void clear() {
		_buf = "";
		_words = "";
		return;
	}
}