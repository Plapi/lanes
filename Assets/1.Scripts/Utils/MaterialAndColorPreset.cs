using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialPreset", menuName = "Custom/MaterialPreset")]
public class MaterialAndColorPreset : ScriptableObject {
    
	public MaterialAndColor[] items;
    
	[NonSerialized]
    private Color[] colors;
    
    public Color[] Colors {
	    get {
		    if (colors == null) {
			    colors = new Color[items.Length];
			    for (int i = 0; i < colors.Length; i++) {
				    colors[i] = items[i].color;
			    }
		    }
		    return colors;
	    }
    }
}

[Serializable]
public class MaterialAndColor {
	public Material material;
	public Color color;
}
