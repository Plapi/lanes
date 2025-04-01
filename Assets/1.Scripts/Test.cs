using UnityEngine;

[ExecuteInEditMode]
public class Test : MonoBehaviour {
    
	[SerializeField] protected Transform target;
	[SerializeField] protected Transform edgeLeft0;
	[SerializeField] protected Transform edgeRight0;
	[SerializeField] protected Transform edgeLeft1;
	[SerializeField] protected Transform edgeRight1;
	
	private void Update() {
		int gizmosId = 0;
		
		GizmosController.Instance.DrawSphere(++gizmosId, target.position, 0.5f, Color.green);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeLeft0.position, 0.5f, Color.blue);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeRight0.position, 0.5f, Color.yellow);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeLeft1.position, 0.5f, Color.cyan);
		GizmosController.Instance.DrawSphere(++gizmosId, edgeRight1.position, 0.5f, Color.magenta);
		
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft0.position, edgeRight0.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft0.position, edgeLeft1.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeLeft1.position, edgeRight1.position, Color.red);
		GizmosController.Instance.DrawLine(++gizmosId, edgeRight0.position, edgeRight1.position, Color.red);
		
		float progress = Utils.ComputeProgress(target.position, edgeLeft0.position, edgeRight0.position, edgeLeft1.position, edgeRight1.position);
		Debug.LogError("Progress: " + progress);
	}
	
	private static float ComputeProgress(Vector3 targetPos, Vector3 left0, Vector3 right0, Vector3 left1, Vector3 right1) {
		Vector3 mid0 = (left0 + right0) * 0.5f;
		Vector3 mid1 = (left1 + right1) * 0.5f;

		Vector3 dir = mid1 - mid0;
		float length = dir.magnitude;
		dir.Normalize();

		Vector3 toTarget = targetPos - mid0;
		float projection = Vector3.Dot(toTarget, dir);

		return Mathf.Clamp01(projection / length);
	}

}
