using System;
#if !SERVER
using UnityEngine;
#endif

public class IOut  {
	
	public static void Log(object o) {
		#if SERVER
		Console.WriteLine(o);
		#else
		Debug.Log(o);
		#endif
		
	}
	
	public static void LogError(object o) {
		#if SERVER
		Console.WriteLine(o);
		#else
		Debug.Log(o);
		#endif
	}
}
