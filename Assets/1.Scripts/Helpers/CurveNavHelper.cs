using UnityEngine;

[ExecuteInEditMode]
public class CurveNavHelper : NavHelper {
	
	[SerializeField] [Range(0f, 1f)] private float curveProgress;

	[Space]
	[SerializeField] private Vector3 direction0 = Vector3.forward;
	[SerializeField] private Vector3 direction1 = Vector3.right;
	
	private readonly Vector3[] curvePoints0 = new Vector3[100];
	private readonly Vector3[] curvePoints1 = new Vector3[100];
	
	public static CurveNavHelper Create(Segment segment0, Segment segment1) {
		CurveNavHelper helper = new GameObject("CurveNavHelper").AddComponent<CurveNavHelper>();
		helper.CreateElements();
		helper.target.position = Vector3.zero;
		helper.edgeLeft0.position = segment0.transform.position + segment0.transform.forward * segment0.Length;
		helper.edgeLeft0.position += segment0.transform.right * (Settings.Instance.laneSize + 1f);
		helper.edgeRight0.position = helper.edgeLeft0.position + segment0.transform.right * (segment0.Width - Settings.Instance.laneSize * 2f - 2f);
		helper.edgeLeft1.position = segment1.transform.position;
		helper.edgeLeft1.position += segment1.transform.right * (Settings.Instance.laneSize + 1f);
		helper.edgeRight1.position = helper.edgeLeft1.position + segment1.transform.right * (segment1.Width - Settings.Instance.laneSize * 2f - 2f);

		helper.direction0 = segment0.transform.forward;
		helper.direction1 = -segment1.transform.forward;
		
		helper.Init();
		
		helper.drawGizmos = true;
		
		return helper;
	}

	private void Init() {
		SetCurvePoints(edgeLeft0.position, direction0, edgeLeft1.position, direction1, curvePoints0);
		SetCurvePoints(edgeRight0.position, direction0, edgeRight1.position, direction1, curvePoints1);
	}
	
	private static void SetCurvePoints(Vector3 p0, Vector3 dir0, Vector3 p1, Vector3 dir1, Vector3[] curvePoints) {
		Utils.GetIntersection(p0, dir0, p1, dir1, out Vector3 intersection0);
		for (int i = 0; i < curvePoints.Length; i++) {
			curvePoints[i] = Bezier.GetPoint(p0, intersection0, p1, (float)i / curvePoints.Length);
		}
	}

	public float CalculateProgress(Vector3 targetPosition) {
		target.position = new Vector3(targetPosition.x, 0f, targetPosition.z);
		curveProgress = GetProgress();
		return curveProgress;
	}

	public Vector3 CalculateTarget(Vector3 targetPosition) {
		target.position = new Vector3(targetPosition.x, 0f, targetPosition.z);
		
		curveProgress = GetProgress();
		CalculateEdges(out Vector3 edgeLeft, out Vector3 edgeRight, out Vector3 edgeLeftNext, out Vector3 edgeRightNext);

		Vector3 dir0 = (edgeLeftNext - edgeLeft).normalized;
		Vector3 dir1 = (edgeRightNext - edgeRight).normalized;
		
		target.position = Utils.GetNearestPoints(edgeLeft, edgeRight, target.position, dir0, out Vector3 leftPoint, out Vector3 rightPoint);
		target.position = Utils.KeepOnSide(edgeLeft0.position, (edgeRight0.position - edgeLeft0.position).normalized * 20f, target.position);
		
		if (drawGizmos) {
			gizmosId = 100;
			DrawElements();
			
			DrawCurve(curvePoints0);
			DrawCurve(curvePoints1);
			
			GizmosController.Instance.DrawSphere(++gizmosId, edgeLeft, 0.3f, Color.gray);
			GizmosController.Instance.DrawSphere(++gizmosId, edgeRight, 0.3f, Color.gray);
			GizmosController.Instance.DrawLine(++gizmosId, edgeLeft, edgeRight, Color.red);
			
			GizmosController.Instance.DrawLine(++gizmosId, edgeRight, edgeRight + dir1 * 20f, Color.black);
			GizmosController.Instance.DrawLine(++gizmosId, edgeRight, edgeRight - dir1 * 20f, Color.black);
			
			GizmosController.Instance.DrawLine(++gizmosId, edgeLeft, edgeLeft + dir0 * 20f, Color.black);
			GizmosController.Instance.DrawLine(++gizmosId, edgeLeft, edgeLeft - dir0 * 20f, Color.black);
			
			GizmosController.Instance.DrawSphere(++gizmosId, leftPoint, 0.5f, Color.white);
        	GizmosController.Instance.DrawSphere(++gizmosId, rightPoint, 0.5f, Color.white);
	        
	        GizmosController.Instance.DrawLine(++gizmosId, edgeRight0.position, edgeLeft0.position, Color.yellow);
		}
		
		return new Vector3(target.position.x, targetPosition.y, target.position.z);
	}
	
	private void Update() {
		if (!editorUpdate) {
			return;
		}
		Init();
		CalculateTarget(target.position);
	}
	
	private void CalculateEdges(out Vector3 edgeLeft, out Vector3 edgeRight, out Vector3 edgeLeftNext, out Vector3 edgeRightNext) {
		Utils.GetIntersection(edgeLeft0.position, direction0, edgeLeft1.position, direction1,
			out Vector3 intersection0);
		edgeLeft = Bezier.GetPoint(edgeLeft0.position, intersection0, edgeLeft1.position, curveProgress);
		Utils.GetIntersection(edgeRight0.position, direction0, edgeRight1.position, direction1,
			out Vector3 intersection1);
		edgeRight = Bezier.GetPoint(edgeRight0.position, intersection1, edgeRight1.position, curveProgress);
		edgeLeftNext = Bezier.GetPoint(edgeLeft0.position, intersection0, edgeLeft1.position, curveProgress + 0.1f);
		edgeRightNext = Bezier.GetPoint(edgeRight0.position, intersection1, edgeRight1.position, curveProgress + 0.1f);
	}

	private float GetProgress() {
		float minDistance = float.MaxValue;
		int closestIndex = 0;
		for (int i = 0; i < curvePoints0.Length; i++) {
			float distance0 = Vector3.Distance(curvePoints0[i], target.position);
			float distance1 = Vector3.Distance(curvePoints1[i], target.position);
			if (distance0 < minDistance) {
				minDistance = distance0;
				closestIndex = i;
			}
			if (distance1 < minDistance) {
				minDistance = distance1;
				closestIndex = i;
			}
		}
		return closestIndex / (float)(curvePoints0.Length - 1);
	}

	private void DrawCurve(Vector3[] curvePoints) {
		for (int i = 0; i < curvePoints.Length - 1; i++) {
			GizmosController.Instance.DrawLine(++gizmosId, curvePoints[i], curvePoints[i + 1], Color.red);
		}
	}

}