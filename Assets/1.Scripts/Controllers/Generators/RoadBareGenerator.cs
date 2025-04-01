using System.Collections.Generic;
using UnityEngine;

public class RoadBareGenerator : MeshGenerator {

	[SerializeField] private bool drawGizmos;
	
	private List<Vector3> polyPointsDebug = new();
	private readonly List<Vector3> offsets = new();
	
	public void Generate(string name, Transform parent, Vector3[] cornerPoints, List<Vector3> polyPoints) {

		cornerPointsDebug = cornerPoints;

		if (drawGizmos) {
			polyPointsDebug = new List<Vector3>(polyPoints);
			if (debugObj == null) {
				debugObj = new GameObject("DebugObj").transform;
			}	
		}
		
		Vector3 dir0 = (cornerPoints[1] - cornerPoints[0]).normalized;
		Vector3 dir1 = (cornerPoints[3] - cornerPoints[0]).normalized;
		
		int size = Settings.Instance.laneSize;
		int repetitions0 = Mathf.RoundToInt(Vector3.Distance(cornerPoints[0], cornerPoints[1])) / size;
		int repetitions1 = Mathf.RoundToInt(Vector3.Distance(cornerPoints[0], cornerPoints[3])) / size;
		
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		int vertexOffset = 0;
		offsets.Clear();
		for (int i = 0; i < repetitions0; i++) {
			Vector3 offset = cornerPoints[0] - dir1 * size + dir0 * (size * i) - parent.position;
			offset += dir0 * size / 2f + dir1 * size / 2f;
			for (int j = 0; j < repetitions1; j++) {
				offset += dir1 * size;
				offsets.Add(offset + parent.position);
				if (!GeometryUtils.PointInPolygon(offset + parent.position, polyPoints)) {
					continue;
				}
				Mesh sourceMesh = Utils.SelectRandomItem(meshFilters, probabilities, out int randomIndex).sharedMesh;
				foreach (Vector3 vertex in meshVertices[randomIndex]) {
					vertices.Add(vertex + offset);
				}
				foreach (Vector2 uv in sourceMesh.uv) {
					uvs.Add(uv);
				}
				foreach (int triangle in sourceMesh.triangles) {
					triangles.Add(triangle + vertexOffset);
				}
				vertexOffset += sourceMesh.vertexCount;
			}
		}
		Mesh combinedMesh = new Mesh {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			uv = uvs.ToArray()
		};
		combinedMesh.RecalculateNormals();

		GameObject go = new GameObject(name) {
			transform = {
				parent = parent,
				localPosition = Vector3.zero
			}
		};
		MeshFilter goMeshFilter = go.AddComponent<MeshFilter>();
		goMeshFilter.mesh = combinedMesh;
		MeshRenderer goMeshRenderer = go.AddComponent<MeshRenderer>();
		goMeshRenderer.materials = materials;
	}
	
	private Vector3[] cornerPointsDebug;
	private Transform debugObj;
	private void OnDrawGizmos() {
		if (!Application.isPlaying || !drawGizmos || polyPointsDebug == null || polyPointsDebug.Count == 0) {
			return;
		}
		
		Gizmos.color = Color.green;

		if (debugObj != null && GeometryUtils.PointInPolygon(debugObj.transform.position, polyPointsDebug)) {
			Gizmos.color = Color.red;
		}
		
		for (int i = 0; i < polyPointsDebug.Count; i++) {
			Gizmos.DrawSphere(polyPointsDebug[i], 0.2f);
		}
		for (int i = 0; i < polyPointsDebug.Count - 1; i++) {
			Gizmos.DrawLine(polyPointsDebug[i], polyPointsDebug[i + 1]);
		}
		Gizmos.DrawLine(polyPointsDebug[^1], polyPointsDebug[0]);

		Gizmos.color = Color.red;
		for (int i = 0; i < offsets.Count; i++) {
			Gizmos.DrawSphere(offsets[i], 0.4f);
		}
		
		Gizmos.color = Color.yellow;
		for (int i = 0; i < cornerPointsDebug.Length - 1; i++) {
			Gizmos.DrawLine(cornerPointsDebug[i], cornerPointsDebug[i + 1]);
		}
		Gizmos.DrawLine(cornerPointsDebug[^1], cornerPointsDebug[0]);
	}
}
