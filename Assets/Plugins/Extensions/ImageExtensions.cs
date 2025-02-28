using UnityEngine;
using UnityEngine.UI;

public static class ImageExtensions {

	public static void SetAlpha(this Image image, float alpha) {
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}
	
}
