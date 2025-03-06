using UnityEngine;

public class RotateObjController : MonoBehaviour {

	[SerializeField] private float rotationSpeed;
	[SerializeField] private Camera cam;
	
	private BoxCollider obj;
	private bool isRotating;
	private Vector2 startPos;

	public void SetObj(BoxCollider obj) {
		this.obj = obj;
	}
    
	private void Update() {
		if (obj == null) {
			return;
		}
		
		if (Input.GetMouseButtonDown(0) && !Utils.IsOverUI()) {
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == obj) {
				isRotating = true;
				startPos = Input.mousePosition;
			}
		}

		if (isRotating) {
			if (Input.GetMouseButton(0)) {
				Vector2 currentPos = Input.mousePosition;
				float deltaX = (currentPos.x - startPos.x) / Screen.width;
                
				if (obj != null) {
					float rotationY = -deltaX * rotationSpeed;
					obj.transform.Rotate(0, rotationY, 0, Space.World);
				}
                
				startPos = currentPos;
			} 
			else if (Input.GetMouseButtonUp(0)) {
				isRotating = false;
			} 
		}
	}
}