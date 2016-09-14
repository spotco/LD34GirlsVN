using UnityEngine;
using System.Collections;
using System.Text;

//https://github.com/spotco/pinballhero/blob/master/src/ScrollText.as
public class ScrollText {
	
	public SPText _text;
	public SPTextRenderManager _text_manager;
	
	string _words = "";
	StringBuilder _buf = new StringBuilder(100);
	float _ct = 0;
	int _spd = 0;
	
	public void reset() {
		_words = "";
		_buf.Length = 0;
		_ct = 0;
		_spd = 0;
	}
	
	public void load(string words, int speed = 1) {
		_buf.Length = 0;
		_ct = 0;
		_spd = speed;
		_words = words;
		_text_manager.set_string(_text,_buf.ToString(), _words);
	}
	
	public void i_update() {
		if (_text == null) return;
		_ct += SPUtil.dt_scale_get();
		if (_ct >= _spd) {
			while( _words.Length > _buf.Length && _ct >= _spd) {
				_buf.Append (_words[_buf.Length]);
				_ct -= _spd;
			}
			_text_manager.set_string(_text,_buf.ToString(), _words);
		}
	}
	
	public bool finished() { return _buf.Length == _words.Length; }
	public void finish() { 
		_buf.Length = 0;
		_buf.Append(_words);
		_text_manager.set_string(_text,_buf.ToString(), _words);
	}
	public void clear() {
		_buf.Length = 0;
		_words = "";
		return;
	}
}