using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField] private Transform pivot;
    [SerializeField] private new Camera camera;
    [SerializeField] private BoxCollider bounds;
    [SerializeField] private LayerMask tapableLayerMask;

    private new bool enabled = true;
    private Plane plane;
    private GTouch firstTouch;
    private Vector3 futurePos;
    private Vector3 futureZoom;
    private Vector3 prevPosZoom;
    private bool overUI;
    private GTouch[] touches;

    public Camera Camera => camera;

    private void Awake() {
	    futurePos = pivot.position;
	    futureZoom = camera.transform.localPosition;
	    UpdateBoundsScale();
	    ClampFuturePos();
    }
    
    public void SetEnabled(bool enabled) {
	    this.enabled = enabled;
    }

    public void Zoom(Vector3 pos) {
	    prevPosZoom = new Vector3(futurePos.x, futureZoom.y, futurePos.z);
	    futurePos = transform.TransformPoint(new Vector3(pos.x, 0f, pos.y));
	    UpdateFutureZoom(pos.z);
	    UpdateBoundsScale();
    }

    public void ZoomBack() {
	    futurePos = new Vector3(prevPosZoom.x, 0f, prevPosZoom.z);
	    UpdateFutureZoom(prevPosZoom.y);
    }

    private void Update() {
	    if (enabled) {
		    touches = GTouch.GetTouches();
		    float scrollDelta = GetScrollDelta();
		    if (Mathf.Abs(scrollDelta) > Mathf.Epsilon) {
			    UpdateFutureZoom(Mathf.Clamp(futureZoom.y - scrollDelta, 10f, 100f));
			    ClampFuturePos();
			    pivot.position = futurePos;
		    } else if (touches[0] != null) {
			    if (touches[0].phase == TouchPhase.Began) {
				    firstTouch = Utils.IsOverUI() ? null : touches[0];
			    } else if (firstTouch != null) {
				    if (touches[0].phase == TouchPhase.Moved) {
					    plane.SetNormalAndPosition(transform.up, transform.position);
					    futurePos += PlanePositionDelta(touches[0]);
					    ClampFuturePos();
				    } else if (touches[0].phase == TouchPhase.Ended) {
					    if (IsTap(touches[0]) && Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, tapableLayerMask) &&
					        hit.collider.TryGetComponent(out TapableObj tapableObj)) {
						    tapableObj.OnTap();
					    }
					    firstTouch = null;
				    }
			    }  
		    }
		    UpdateBoundsScale();
	    }
	    
	    float time = Time.deltaTime * 20f;
	    pivot.position = new Vector3(pivot.position.x + (futurePos.x - pivot.position.x) * time, 0f, 
		    pivot.position.z + (futurePos.z - pivot.position.z) * time);
	    camera.transform.localPosition = new Vector3(0f,
		    camera.transform.localPosition.y + (futureZoom.y - camera.transform.localPosition.y) * time,
		    camera.transform.localPosition.z + (futureZoom.z - camera.transform.localPosition.z) * time);
    }

    private float GetScrollDelta() {
	    if (touches[1] != null) {
		    Vector3 pos1 = PlanePosition(touches[0].position);
		    Vector3 pos2 = PlanePosition(touches[1].position);
		    Vector3 pos1b = PlanePosition(touches[0].position - touches[0].deltaPosition);
		    Vector3 pos2b = PlanePosition(touches[1].position - touches[1].deltaPosition);
		    return (Vector3.Distance(pos1, pos2) - Vector3.Distance(pos1b, pos2b)) * 10f;
	    }
	    return Input.mouseScrollDelta.y;
    }

    private void UpdateFutureZoom(float value) {
	    futureZoom.y = value;
	    futureZoom.z = -futureZoom.y + 10f;
    }

    private void UpdateBoundsScale() {
	    float boundsScale = Mathf.Lerp(1.8f, 0.15f, Mathf.InverseLerp(10f, 100f, camera.transform.localPosition.y));
	    bounds.transform.localScale = new Vector3(boundsScale, 1f, boundsScale);
    }

    private void ClampFuturePos() {
	    Vector3 min = bounds.bounds.min;
	    Vector3 max = bounds.bounds.max;
	    futurePos = new Vector3(Mathf.Clamp(futurePos.x, min.x, max.x), 0f, Mathf.Clamp(futurePos.z, min.z, max.z));
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
    
	private bool IsTap(GTouch lastTouch) {
		if (firstTouch != null) {
			return lastTouch.time - firstTouch.time < 0.2f && Vector2.Distance(lastTouch.position, firstTouch.position) < 10;
		}
		return false;
	}
}

public class GTouch {

		public Vector2 position { get; private set; }
		public Vector2 deltaPosition { get; private set; }
		public TouchPhase phase { get; private set; }
		public float time { get; private set; }

		private static readonly GTouch[] sTouches = new GTouch[2];

#if UNITY_EDITOR || UNITY_STANDALONE
		private static Vector3 s_prevMousePos;
#endif

		public static GTouch[] GetTouches() {
#if UNITY_EDITOR || UNITY_STANDALONE
			return GetTouchesInStandalone();
#else
			return GetTouchesOnMobile();
#endif
		}

#if UNITY_EDITOR || UNITY_STANDALONE
		private static GTouch[] GetTouchesInStandalone() {
			if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) {
				sTouches[0] = new GTouch {
					position = Input.mousePosition,
					time = Time.time
				};

				if (Input.GetMouseButtonDown(0)) {
					s_prevMousePos = Input.mousePosition;
					sTouches[0].phase = TouchPhase.Began;
				} else {
					sTouches[0].deltaPosition = Input.mousePosition - s_prevMousePos;
					s_prevMousePos = Input.mousePosition;
					if (Input.GetMouseButtonUp(0)) {
						sTouches[0].phase = TouchPhase.Ended;
					} else {
						sTouches[0].phase = sTouches[0].deltaPosition.magnitude > 0f ? TouchPhase.Moved : TouchPhase.Stationary;
					}
				}

				if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) {
					sTouches[1] = new GTouch {
						position = sTouches[0].position - Vector2.one * 150f,
						deltaPosition = sTouches[0].deltaPosition,
						phase = sTouches[0].phase
					};
				} else {
					sTouches[1] = null;
				}
			} else {
				sTouches[0] = null;
				sTouches[1] = null;
			}
			return sTouches;
		}
#endif

		private static GTouch[] GetTouchesOnMobile() {
			for (int i = 0; i < sTouches.Length; i++) {
				if (Input.touchCount > i) {
					Touch touch = Input.GetTouch(i);
					sTouches[i] = new GTouch {
						position = touch.position,
						deltaPosition = touch.deltaPosition,
						phase = touch.phase,
						time = Time.time
					};
				} else {
					sTouches[i] = null;
				}
			}
			return sTouches;
		}
	}
