using System.Collections.Generic;
using UnityEngine;

public class GizmosController : MonoBehaviourSingleton<GizmosController> {

	private readonly List<ElementData> elements = new();

	public void DrawSphere(int id, Vector3 position, float radius, Color color) {
		DrawSphere(id.ToString(), position, radius, color);
	}

	public void DrawSphere(string id, Vector3 position, float size, Color color) {
		SetElement(new SphereData {
			id = id,
			position = position,
			size = size,
			color = color
		});
	}
	
	public void DrawLine(int id, Vector3 from, Vector3 to, Color color) {
		DrawLine(id.ToString(), from, to, color);
	}

	public void DrawLine(string id, Vector3 from, Vector3 to, Color color) {
		SetElement(new LineData {
			id = id,
			from = from,
			to = to,
			color = color
		});
	}

	private void SetElement(ElementData element) {
		int elementIndex = elements.FindIndex(e => e.id == element.id);
		if (elementIndex == -1) {
			elements.Add(element);
		} else {
			elements[elementIndex] = element;
		}
	}

	private void OnDrawGizmos() {
		for (int i = 0; i < elements.Count; i++) {
			elements[i].Draw();
		}
	}

	private abstract class ElementData {
		public string id;
		public Color color;

		public abstract void Draw();
	}

	private class SphereData : ElementData {
		public Vector3 position;
		public float size;
		public override void Draw() {
			Gizmos.color = color;
			Gizmos.DrawSphere(position, size);
		}
	}

	private class LineData : ElementData {
		public Vector3 from;
		public Vector3 to;
		public override void Draw() {
			Gizmos.color = color;
			Gizmos.DrawLine(from, to);
		}
	}
}
