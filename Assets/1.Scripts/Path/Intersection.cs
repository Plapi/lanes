using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour {

	[SerializeField] private Element cornerPrefab;
	[SerializeField] private TrafficLight trafficLightPrefab;
	[SerializeField] private Element crossingPrefab;
	[SerializeField] private Element sideWalkPrefab;
	[SerializeField] private Element roadBarePrefab;

	private readonly List<Element> elements = new();

	private Segment bottomSegment;
	private Segment leftSegment;
	private Segment rightSegment;
	private Segment topSegment;
	
	private TrafficLight bottomTrafficLight;
	private TrafficLight leftTrafficLight;
	private TrafficLight rightTrafficLight;
	private TrafficLight topTrafficLight;
	
	private Element bottomLeftCorner;
	private Element bottomRightCorner;
	private Element topRightCorner;
	private Element topLeftCorner;

	private int laneSize;

	private readonly List<RoadLane> verticallyRoadLanes = new();
	private readonly List<RoadLane> horizontallyRoadLanes = new();
	
	public void Init(Segment bottomSegment, Segment leftSegment, Segment rightSegment, Segment topSegment) {
		this.bottomSegment = bottomSegment;
		this.leftSegment = leftSegment;
		this.rightSegment = rightSegment;
		this.topSegment = topSegment;
		laneSize = Settings.Instance.laneSize;
		
		CreateCorners();
		CreateTrafficLights();
		CreateCrossings();
		CreateSideWalks(); 
		CreateRoadBares();
		
		verticallyRoadLanes.AddRange(this.bottomSegment.ForwardRoadLanes);
		verticallyRoadLanes.AddRange(this.topSegment.BackRoadLanes);
		
		horizontallyRoadLanes.AddRange(this.leftSegment.BackRoadLanes);
		horizontallyRoadLanes.AddRange(this.rightSegment.ForwardRoadLanes);

		StartCoroutine(SemaphoreSystem());
	}

	private IEnumerator SemaphoreSystem() {

		void allowPassingRoadLanes(List<RoadLane> lanes, bool allowPassing) {
			lanes.ForEach(l => l.AllowPassing = allowPassing);
		}

		void allowPassingVertically() {
			allowPassingRoadLanes(verticallyRoadLanes, true);
			allowPassingRoadLanes(horizontallyRoadLanes, false);
			bottomTrafficLight.SetGreen();
			topTrafficLight.SetGreen();
			leftTrafficLight.SetRed();
			rightTrafficLight.SetRed();
		}

		void allowPassingHorizontally() {
			allowPassingRoadLanes(verticallyRoadLanes, false);
			allowPassingRoadLanes(horizontallyRoadLanes, true);
			bottomTrafficLight.SetRed();
			topTrafficLight.SetRed();
			leftTrafficLight.SetGreen();
			rightTrafficLight.SetGreen();
		}

		void disallowPassingAll() {
			allowPassingRoadLanes(verticallyRoadLanes, false);
			allowPassingRoadLanes(horizontallyRoadLanes, false);
			bottomTrafficLight.SetYellow();
			topTrafficLight.SetYellow();
			leftTrafficLight.SetYellow();
			rightTrafficLight.SetYellow();
		}
		
		while (true) {
			allowPassingVertically();
			yield return new WaitForSeconds(5f);

			disallowPassingAll();
			yield return new WaitForSeconds(2f);
			
			allowPassingHorizontally();
			yield return new WaitForSeconds(5f);

			disallowPassingAll();
			yield return new WaitForSeconds(2f);
		}
	}
	
	private void CreateCorners() {
		bottomLeftCorner = cornerPrefab.Create("BottomLeftCorner", transform, 0f, bottomSegment.transform.position.x, leftSegment.transform.position.z);
		bottomRightCorner = cornerPrefab.Create("BottomRightCorner", transform, -90f, bottomSegment.transform.position.x + bottomSegment.Width, rightSegment.transform.position.z);
		topRightCorner = cornerPrefab.Create("TopRightCorner", transform, 180f, topSegment.transform.position.x + topSegment.Width, rightSegment.transform.position.z + rightSegment.Width);
		topLeftCorner = cornerPrefab.Create("TopLeftCorner", transform, 90f, topSegment.transform.position.x, leftSegment.transform.position.z + leftSegment.Width);
		elements.Add(bottomLeftCorner);
		elements.Add(bottomRightCorner);
		elements.Add(topRightCorner);
		elements.Add(topLeftCorner);
	}

	private void CreateTrafficLights() {
		bottomTrafficLight = (TrafficLight)trafficLightPrefab.Create("BottomTrafficLight", transform, 180f, bottomRightCorner.transform.position.x - 3f, bottomRightCorner.transform.position.z);
		leftTrafficLight = (TrafficLight)trafficLightPrefab.Create("LeftTrafficLight", transform, 270f, bottomLeftCorner.transform.position.x, bottomLeftCorner.transform.position.z + 3f);
		rightTrafficLight = (TrafficLight)trafficLightPrefab.Create("RightTrafficLight", transform, 90f, topRightCorner.transform.position.x, topRightCorner.transform.position.z - 3f);
		topTrafficLight = (TrafficLight)trafficLightPrefab.Create("TopTrafficLight", transform, 0f, topLeftCorner.transform.position.x + 3f, topLeftCorner.transform.position.z);
		elements.Add(bottomTrafficLight);
		elements.Add(leftTrafficLight);
		elements.Add(rightTrafficLight);
		elements.Add(topTrafficLight);
	}

	private void CreateCrossings() {
		int start = Mathf.RoundToInt(bottomLeftCorner.transform.position.z) + laneSize;
		int end = Mathf.RoundToInt(topLeftCorner.transform.position.z) - laneSize;
		for (int z = start; z < end; z += laneSize) {
			elements.Add(crossingPrefab.Create("LeftCrossing", transform, 0f, Mathf.Min(bottomLeftCorner.transform.position.x, topLeftCorner.transform.position.x), z));
		}
		start = Mathf.RoundToInt(bottomLeftCorner.transform.position.x) + laneSize;
		end = Mathf.RoundToInt(bottomRightCorner.transform.position.x) - laneSize;
		for (int x = start; x < end; x += laneSize) {
			elements.Add(crossingPrefab.Create("BottomCrossing", transform, 90f, x, Mathf.Min(bottomLeftCorner.transform.position.z, bottomRightCorner.transform.position.z) + laneSize));
		}
		start = Mathf.RoundToInt(bottomRightCorner.transform.position.z) + laneSize;
		end = Mathf.RoundToInt(topRightCorner.transform.position.z) - laneSize;
		for (int z = start; z < end; z += laneSize) {
			elements.Add(crossingPrefab.Create("RightCrossing", transform, 0f, Mathf.Max(bottomRightCorner.transform.position.x, topRightCorner.transform.position.x) - laneSize, z));
		}
		start = Mathf.RoundToInt(topLeftCorner.transform.position.x) + laneSize;
		end = Mathf.RoundToInt(topRightCorner.transform.position.x) - laneSize;
		for (int x = start; x < end; x += laneSize) {
			elements.Add(crossingPrefab.Create("TopCrossing", transform, 90f, x, Mathf.Max(topLeftCorner.transform.position.z, topRightCorner.transform.position.z)));
		}
	}

	private void CreateSideWalks() {
		int start = Mathf.RoundToInt(leftSegment.transform.position.x);
		int end = Mathf.RoundToInt(bottomLeftCorner.transform.position.x);
		for (int x = start; x < end; x += laneSize) {
			elements.Add(sideWalkPrefab.Create("LeftBottomSideWalk", transform, 0f, x, leftSegment.transform.position.z));
		}
		start = Mathf.RoundToInt(bottomSegment.transform.position.z + bottomSegment.Length);
		end = Mathf.RoundToInt(bottomLeftCorner.transform.position.z);
		for (int z = start; z < end; z += laneSize) {
			elements.Add(sideWalkPrefab.Create("BottomLeftSideWalk", transform, 90f, bottomSegment.transform.position.x, z + laneSize));
		}
		start = Mathf.RoundToInt(bottomSegment.transform.position.z + bottomSegment.Length);
		end = Mathf.RoundToInt(bottomRightCorner.transform.position.z);
		for (int z = start; z < end; z += laneSize) {
			elements.Add(sideWalkPrefab.Create("BottomRightSideWalk", transform, -90f, bottomSegment.transform.position.x + bottomSegment.Width, z));
		}
		start = Mathf.RoundToInt(bottomRightCorner.transform.position.x);
		end = Mathf.RoundToInt(rightSegment.transform.position.x - rightSegment.Length);
		for (int x = start; x < end; x += laneSize) {
			elements.Add(sideWalkPrefab.Create("RightBottomSideWalk", transform, 0f, x, bottomRightCorner.transform.position.z));
		}
		start = Mathf.RoundToInt(topRightCorner.transform.position.x);
		end = Mathf.RoundToInt(rightSegment.transform.position.x - rightSegment.Length);
		for (int x = start; x < end; x += laneSize) {
			elements.Add(sideWalkPrefab.Create("RightTopSideWalk", transform, 180f, x + laneSize, topRightCorner.transform.position.z));
		}
		start = Mathf.RoundToInt(topRightCorner.transform.position.z);
		end = Mathf.RoundToInt(topSegment.transform.position.z);
		for (int z = start; z < end; z += laneSize) {
			elements.Add(sideWalkPrefab.Create("TopRightSideWalk", transform, -90f, topSegment.transform.position.x + topSegment.Width, z));
		}
		start = Mathf.RoundToInt(topLeftCorner.transform.position.z);
		end = Mathf.RoundToInt(topSegment.transform.position.z);
		for (int z = start; z < end; z += Settings.Instance.laneSize) {
			elements.Add(sideWalkPrefab.Create("TopLeftSideWalk", transform, 90f, topSegment.transform.position.x, z + laneSize));
		}
		start = Mathf.RoundToInt(leftSegment.transform.position.x);
		end = Mathf.RoundToInt(topLeftCorner.transform.position.x);
		for (int x = start; x < end; x += Settings.Instance.laneSize) {
			elements.Add(sideWalkPrefab.Create("LeftTopSideWalk", transform, 180f, x + laneSize, topLeftCorner.transform.position.z));
		}
	}

	private void CreateRoadBares() {
		Vector2[] points = {
			new (bottomLeftCorner.transform.position.x + laneSize, bottomLeftCorner.transform.position.z + laneSize),
			new (bottomRightCorner.transform.position.x - laneSize, bottomRightCorner.transform.position.z + laneSize),
			new (topRightCorner.transform.position.x - laneSize, topRightCorner.transform.position.z - laneSize),
			new (topLeftCorner.transform.position.x + laneSize, topLeftCorner.transform.position.z - laneSize)
		};

		int minX = Mathf.RoundToInt(Mathf.Min(points[0].x, points[1].x, points[2].x, points[3].x));
		int maxX = Mathf.RoundToInt(Mathf.Max(points[0].x, points[1].x, points[2].x, points[3].x));
		int minZ = Mathf.RoundToInt(Mathf.Min(points[0].y, points[1].y, points[2].y, points[3].y));
		int maxZ = Mathf.RoundToInt(Mathf.Max(points[0].y, points[1].y, points[2].y, points[3].y));
		
		List<Vector3> polyPoints = new() {
			new Vector3(points[0].x, 0f, points[0].y),
			points[0].y > points[1].y ? new Vector3(points[0].x, 0f, points[1].y) : new Vector3(points[1].x, 0f, points[0].y),
			new Vector3(points[1].x, 0f, points[1].y),
			points[1].x > points[2].x ? new Vector3(points[1].x, 0f, points[2].y) : new Vector3(points[2].x, 0f, points[1].y),
			new Vector3(points[2].x, 0f, points[2].y),
			points[2].y > points[3].y ? new Vector3(points[3].x, 0f, points[2].y) : new Vector3(points[2].x, 0f, points[3].y),
			new Vector3(points[3].x, 0f, points[3].y),
			points[3].x > points[0].x ? new Vector3(points[0].x, 0f, points[3].y) : new Vector3(points[3].x, 0f, points[0].y)
		};
		for (int x = minX; x < maxX; x += laneSize) {
			for (int z = minZ; z < maxZ; z += laneSize) {
				Vector3 point = new Vector3(x + laneSize / 2f, 0f, z + laneSize / 2f);
				if (GeometryUtils.PointInPolygon(point, polyPoints)) {
					elements.Add(roadBarePrefab.Create("RoadBare", transform, 0f, point.x, point.z));
				}
			}
		}
	}
	
	public void CreateRoadConnections() {
		// bottomSegment
		int maxFrontConnections = Mathf.Min(bottomSegment.ForwardRoadLanes.Count, topSegment.ForwardRoadLanes.Count);
		for (int i = 0; i < maxFrontConnections; i++) {
			RoadLane lane0 = bottomSegment.ForwardRoadLanes[^(i + 1)];
			RoadLane lane1 = topSegment.ForwardRoadLanes[^(i + 1)];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
		for (int i = 0; i < bottomSegment.ForwardRoadLanes.Count; i++) {
			RoadLane lane0 = bottomSegment.ForwardRoadLanes[i];
			if (i == 0 || !lane0.HasNextRoadLanes()) {
				RoadLane lane1 = rightSegment.BackRoadLanes[Mathf.Min(i, rightSegment.BackRoadLanes.Count - 1)];
				List<Vector3> transPoints = GetTransitionPoints(lane1.transform.forward, lane0.EndPos, lane1.StartPos);
				lane0.AddNextRoadLane(lane1, transPoints);
			}
		}
		
		// topSegment
		int maxBackConnections = Mathf.Min(topSegment.BackRoadLanes.Count, bottomSegment.BackRoadLanes.Count);
		for (int i = 0; i < maxBackConnections; i++) {
			RoadLane lane0 = topSegment.BackRoadLanes[^(i + 1)];
			RoadLane lane1 = bottomSegment.BackRoadLanes[^(i + 1)];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
		for (int i = 0; i < topSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = topSegment.BackRoadLanes[i];
			if (i == 0 || !lane0.HasNextRoadLanes()) {
				RoadLane lane1 = leftSegment.ForwardRoadLanes[Mathf.Min(i, leftSegment.ForwardRoadLanes.Count - 1)];
				List<Vector3> transPoints = GetTransitionPoints(lane1.transform.forward, lane0.EndPos, lane1.StartPos);
				lane0.AddNextRoadLane(lane1, transPoints);
			}
		}
		
		// rightSegment
		int maxRightConnections = Mathf.Min(rightSegment.ForwardRoadLanes.Count, leftSegment.ForwardRoadLanes.Count);
		for (int i = 0; i < maxRightConnections; i++) {
			RoadLane lane0 = rightSegment.ForwardRoadLanes[^(i + 1)];
			RoadLane lane1 = leftSegment.ForwardRoadLanes[^(i + 1)];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
		for (int i = 0; i < rightSegment.ForwardRoadLanes.Count; i++) {
			RoadLane lane0 = rightSegment.ForwardRoadLanes[i];
			if (i == 0 || !lane0.HasNextRoadLanes()) {
				RoadLane lane1 = topSegment.ForwardRoadLanes[Mathf.Min(i, topSegment.ForwardRoadLanes.Count - 1)];
				List<Vector3> transPoints = GetTransitionPoints(lane1.transform.forward, lane0.EndPos, lane1.StartPos);
				lane0.AddNextRoadLane(lane1, transPoints);
			}
		}
		
		// leftSegment
		int maxLeftConnections = Mathf.Min(rightSegment.BackRoadLanes.Count, leftSegment.BackRoadLanes.Count);
		for (int i = 0; i < maxLeftConnections; i++) {
			RoadLane lane0 = leftSegment.BackRoadLanes[^(i + 1)];
			RoadLane lane1 = rightSegment.BackRoadLanes[^(i + 1)];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
		for (int i = 0; i < leftSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = leftSegment.BackRoadLanes[i];
			if (i == 0 || !lane0.HasNextRoadLanes()) {
				RoadLane lane1 = bottomSegment.BackRoadLanes[Mathf.Min(i, bottomSegment.BackRoadLanes.Count - 1)];
				List<Vector3> transPoints = GetTransitionPoints(lane1.transform.forward, lane0.EndPos, lane1.StartPos);
				lane0.AddNextRoadLane(lane1, transPoints);
			}
		}
	}
	
	private static List<Vector3> GetTransitionPoints(Vector3 dir, Vector3 point0, Vector3 point1) {
		dir = dir.normalized;
		Vector3 vectorToPoint1 = point1 - point0;
		Vector3 projection = Vector3.Dot(vectorToPoint1, dir) * dir;
		Vector3 perpendicularVector = vectorToPoint1 - projection;
		Vector3 perpendicularPoint = point0 + perpendicularVector;
		return Chaikin.SmoothPath(new List<Vector3> { point0, perpendicularPoint, point1 }, 3);
	}

	public void Clear() {
		for (int i = 0; i < elements.Count; i++) {
			ObjectPoolManager.Release(elements[i]);
		}
		elements.Clear();
		Destroy(gameObject);
	}
}
