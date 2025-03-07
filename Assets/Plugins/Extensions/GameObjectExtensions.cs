using UnityEngine;

public static class GameObjectExtensions {
    
	public static void SetX(this GameObject obj, float x) {
		obj.transform.SetX(x);
	}
	
	public static void SetY(this GameObject obj, float y) {
		obj.transform.SetY(y);
	}
	
	public static void SetZ(this GameObject obj, float z) {
		obj.transform.SetZ(z);
	}
	
	public static void SetLocalX(this GameObject obj, float x) {
		obj.transform.SetLocalX(x);
	}
	
	public static void SetLocalY(this GameObject obj, float y) {
		obj.transform.SetLocalY(y);
	}
	
	public static void SetLocalZ(this GameObject obj, float z) {
		obj.transform.SetLocalZ(z);
	}
}
