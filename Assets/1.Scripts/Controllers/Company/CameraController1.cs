using UnityEngine;

public class CameraController1 : MonoBehaviour {
	
	[SerializeField] private new Camera camera;
	
	private Plane plane;
	private GTouch firstTouch;
	private bool move = true;
	private bool overUI;
	
#if UNITY_EDITOR || UNITY_STANDALONE
	private Vector3? prevMousePosition;
#endif

	public void SetMoveEnable(bool enable) {
		move = enable;
	}

	public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}
	
	public void Update() {
		if (!move) {
			return;
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		bool fixCameraPos = false;

		if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
			if (prevMousePosition != null) {
				Vector3 rotate = (Vector3)prevMousePosition - Input.mousePosition;
				Vector3 pos1 = PlanePosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
				camera.transform.RotateAround(pos1, Vector3.up, -rotate.x / 5f);
			}
			prevMousePosition = Input.mousePosition;
			fixCameraPos = true;
		}

		if (Input.GetMouseButtonUp(1) || Input.GetMouseButton(2)) {
			prevMousePosition = null;
		}

		float scrollDelta = Input.mouseScrollDelta.y;
		if (Mathf.Abs(scrollDelta) > Mathf.Epsilon) {
			Vector3 pos1 = PlanePosition(Input.mousePosition);
			camera.transform.position += (pos1 - camera.transform.position).normalized * scrollDelta;
			fixCameraPos = true;
		}
		if (fixCameraPos) {
			FixCameraPos();
		}
#endif

		GTouch[] touches = GTouch.GetTouches();
		if (touches[0] == null) {
			return;
		}

		plane.SetNormalAndPosition(transform.up, transform.position);

		if (touches[0].phase == TouchPhase.Began || touches[0].phase == TouchPhase.Moved) {
			overUI = Utils.IsOverUI();
			if (touches[0].phase == TouchPhase.Began) {
				firstTouch = touches[0];
			} else if (!overUI) {
				camera.transform.Translate(PlanePositionDelta(touches[0]), Space.World);
			}
		} else if (touches[0].phase == TouchPhase.Ended && touches[1] == null) {
			// if (ConstructionController.instance.hasConstruction) {
			// 	ConstructionController.instance.enabled = true;
			// 	SetMoveEnable(false);
			// } else if (!m_overUI && IsTap(touches[0]) && TryGetConstruction(out Construction construction)) {
			// 	if (UIController.instance.GetCurrentView() is UITileView) {
			// 		UIController.instance.PopCurrentView();
			// 	} else {
			// 		UIController.instance.PushView<UITileView>(construction);
			// 	}
			// }
		}

		if (overUI) {
			return;
		}

		if (touches[1] != null) {
			Vector3 pos1 = PlanePosition(touches[0].position);
			Vector3 pos2 = PlanePosition(touches[1].position);
			Vector3 pos1b = PlanePosition(touches[0].position - touches[0].deltaPosition);
			Vector3 pos2b = PlanePosition(touches[1].position - touches[1].deltaPosition);

			float zoom = Vector3.Distance(pos1, pos2) / Vector3.Distance(pos1b, pos2b);

			if (zoom == 0 || zoom > 10) {
				return;
			}

			camera.transform.position = Vector3.LerpUnclamped(pos1, camera.transform.position, 1 / zoom);

			// if (pos2b != pos2) {
			// 	camera.transform.RotateAround(pos1, plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, plane.normal));
			// }
		}

		FixCameraPos();
	}
	
	private void FixCameraPos() {
		camera.transform.SetY(Mathf.Clamp(camera.transform.position.y, 5f, 200f));
		camera.transform.SetX(Mathf.Clamp(camera.transform.position.x, -300f, 300f));
		camera.transform.SetZ(Mathf.Clamp(camera.transform.position.z, -300f, 300f));

		// if (Utils.TryGetPerpHitPoint(out Vector3 hitPoint) && TilesManager.instance.TryGetTile(TilesManager.instance.Snap(hitPoint), out Tile tile)
		//                                                    && tile.construction != null && tile.construction.constructionCollider != null) {
		// 	if (tile.construction.constructionCollider.Contains(m_camera.transform.position, out Bounds bounds)) {
		// 		m_camera.transform.SetY(bounds.max.y);
		// 	}
		// }
	}
	
	private bool IsTap(GTouch lastTouch) {
		if (firstTouch != null) {
			return lastTouch.time - firstTouch.time < 0.2f && Vector2.Distance(lastTouch.position, firstTouch.position) < 10;
		}
		return false;
	}
	
	private Vector3 PlanePositionDelta(GTouch touch) {
		if (touch.phase != TouchPhase.Moved) {
			return Vector3.zero;
		}

		var rayBefore = camera.ScreenPointToRay(touch.position - touch.deltaPosition);
		var rayNow = camera.ScreenPointToRay(touch.position);
		if (plane.Raycast(rayBefore, out var enterBefore) && plane.Raycast(rayNow, out var enterNow)) {
			return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);
		}

		return Vector3.zero;
	}

	private Vector3 PlanePosition(Vector2 screenPos) {
		var rayNow = camera.ScreenPointToRay(screenPos);
		return plane.Raycast(rayNow, out var enterNow) ? rayNow.GetPoint(enterNow) : Vector3.zero;
	}
}
