using UnityEngine;

[ExecuteInEditMode]
public class ForwardNavHelper : NavHelper {

	public static ForwardNavHelper Create(Segment segment0, Segment segment1) {
		ForwardNavHelper helper = new GameObject("ForwardNavHelper").AddComponent<ForwardNavHelper>();
		helper.CreateElements();
		
		helper.edgeLeft0.position = segment0.transform.position + segment0.transform.forward * segment0.Length;
		helper.edgeLeft0.position += segment0.transform.right * (Settings.Instance.laneSize + 1f);
		helper.edgeRight0.position = helper.edgeLeft0.position + segment0.transform.right * (segment0.Width - Settings.Instance.laneSize * 2f - 2f);
		helper.edgeLeft1.position = segment1.transform.position;
		helper.edgeLeft1.position += segment1.transform.right * (Settings.Instance.laneSize + 1f);
		helper.edgeRight1.position = helper.edgeLeft1.position + segment1.transform.right * (segment1.Width - Settings.Instance.laneSize * 2f - 2f);
		
		helper.drawGizmos = true;
		return helper;
	}
	
	public Vector3 CalculateTarget(Vector3 targetPosition) {
		target.position = new Vector3(targetPosition.x, 0f, targetPosition.z);
		
		target.position = Utils.KeepOnSide(edgeLeft0.position, (edgeLeft0.position - edgeLeft1.position).normalized, target.position);
		target.position = Utils.KeepOnSide(edgeRight0.position, (edgeRight1.position - edgeRight0.position).normalized, target.position);
		
		if (drawGizmos) {
			gizmosId = 300;
			DrawElements();
			DrawLineElements();
		}
		
		return new Vector3(target.position.x, targetPosition.y, target.position.z);
	}
	
	private void Update() {
		if (!editorUpdate) {
			return;
		}
		CalculateTarget(target.position);
	}

}