using UnityEngine;

[ExecuteInEditMode]
public class SegmentNavHelper : NavHelper {
	
	[SerializeField] private int length;
	[SerializeField] private int width;
	
	[SerializeField] [Range(0f, 1f)] private float curveProgress;

	public static SegmentNavHelper Create(Segment segment) {
		SegmentNavHelper helper = new GameObject("SegmentNavHelper").AddComponent<SegmentNavHelper>();
		helper.CreateElements();
		helper.target.position = Vector3.zero;
		helper.edgeLeft0.position = segment.transform.position;
		helper.edgeLeft0.position += segment.transform.right * (Settings.Instance.laneSize + 1f);
		helper.edgeRight0.position = helper.edgeLeft0.position + segment.transform.right * (segment.Width - Settings.Instance.laneSize * 2f - 2f);
		helper.edgeLeft1.position = helper.edgeLeft0.position + segment.transform.forward * segment.Length;
		helper.edgeRight1.position =  helper.edgeLeft1.position + segment.transform.right * (segment.Width - Settings.Instance.laneSize * 2f - 2f);
		helper.drawGizmos = true;
		return helper;
	}

	public Vector3 CalculateTarget(Vector3 targetPosition) {
		target.position = new Vector3(targetPosition.x, 0f, targetPosition.z);
		
		Vector3 dir = (edgeLeft1.position - edgeLeft0.position).normalized;
		target.position = Utils.GetNearestPoints(edgeLeft0.position, edgeRight0.position, target.position, dir, out Vector3 leftPoint, out Vector3 rightPoint);
		
		if (drawGizmos) {
			gizmosId = 200;
			DrawElements();
			DrawLineElements();
			GizmosController.Instance.DrawSphere(++gizmosId, leftPoint, 0.5f, Color.black);
			GizmosController.Instance.DrawSphere(++gizmosId, rightPoint, 0.5f, Color.black);
		}
		
		return new Vector3(target.position.x, targetPosition.y, target.position.z);
	}
	
	private void Update() {
		if (!editorUpdate) {
			return;
		}
		edgeRight0.position = edgeLeft0.position + Vector3.right * width; 
		edgeLeft1.position = edgeLeft0.position + Vector3.forward * length; 
		edgeRight1.position = edgeLeft1.position + Vector3.right * width;
		CalculateTarget(target.position);
	}

}