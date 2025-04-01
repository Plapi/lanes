using UnityEngine;

public abstract class NavHelper : MonoBehaviour {
	
	[SerializeField] protected Transform target;
	[SerializeField] protected Transform edgeLeft0;
	[SerializeField] protected Transform edgeRight0;
	[SerializeField] protected Transform edgeLeft1;
	[SerializeField] protected Transform edgeRight1;

	[Space]
	[SerializeField] protected bool editorUpdate;
	[SerializeField] protected bool drawGizmos;

	protected int gizmosId;
	
	protected void CreateElements() {
		target = new GameObject("Target").transform;
		target.parent = transform;
		edgeLeft0 = new GameObject("EdgeLeft0").transform;
		edgeLeft0.parent = transform;
		edgeRight0 = new GameObject("EdgeRight0").transform;
		edgeRight0.parent = transform;
		edgeLeft1 = new GameObject("EdgeLeft1").transform;
		edgeLeft1.parent = transform;
		edgeRight1 = new GameObject("EdgeRight1").transform;
		edgeRight1.parent = transform;
	}

	protected void DrawElements() {
		GizmosController.Instance.DrawSphere(++gizmosId, target.position, 0.5f, Color.green);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeLeft0.position, 0.5f, Color.blue);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeRight0.position, 0.5f, Color.yellow);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeLeft1.position, 0.5f, Color.cyan);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeRight1.position, 0.5f, Color.magenta);
	}

	protected void DrawLineElements() {
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft0.position, edgeRight0.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft0.position, edgeLeft1.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft1.position, edgeRight1.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeRight0.position, edgeRight1.position, Color.red);
	}
}