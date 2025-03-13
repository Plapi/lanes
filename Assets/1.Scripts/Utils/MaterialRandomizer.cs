using UnityEngine;

[CreateAssetMenu(fileName = "MaterialRandomizer", menuName = "Custom/MaterialRandomizer")]
public class MaterialRandomizer : ScriptableObject {
	
	[SerializeField] private MaterialProbability[] materials;

	public Material GetRandomMaterial() {
		float[] probabilities = new float[materials.Length];
		for (int i = 0; i < probabilities.Length; i++) {
			probabilities[i] = materials[i].probability;
		}
		return Utils.SelectRandomItem(materials, probabilities, out _).material;
	}
}

[System.Serializable]
public class MeshMaterialRandomizer {
	[SerializeField] private MaterialRandomizer preset;
	[SerializeField] private Renderer[] renderers;

	public void SetRandomMaterial() {
		if (preset == null) {
			return;
		}
		Material[] materials = { preset.GetRandomMaterial() };
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].materials = materials;
		}
	}
}

[System.Serializable]
public class MaterialProbability {
	public Material material;
	[Range(0f, 1f)] public float probability = 0.5f;
}

