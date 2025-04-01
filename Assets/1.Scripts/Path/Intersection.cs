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
	
	private Element bottomLeftCorner;
	private Element bottomRightCorner;
	private Element topRightCorner;
	private Element topLeftCorner;
	
	private TrafficLight bottomLeftTrafficLight;
	private TrafficLight bottomRightTrafficLight;
	private TrafficLight topRightTrafficLight;
	private TrafficLight topLeftTrafficLight;

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
		horizontallyRoadLanes.AddRange(this.rightSegment.BackRoadLanes);

		StartCoroutine(SemaphoreSystem());
	}

	private IEnumerator SemaphoreSystem() {

		void allowPassingRoadLanes(List<RoadLane> lanes, bool allowPassing) {
			lanes.ForEach(l => l.AllowPassing = allowPassing);
		}

		void allowPassingVertically() {
			allowPassingRoadLanes(verticallyRoadLanes, true);
			allowPassingRoadLanes(horizontallyRoadLanes, false);
			bottomLeftTrafficLight.SetRed();
			bottomRightTrafficLight.SetGreen();
			topRightTrafficLight.SetRed();
			topLeftTrafficLight.SetGreen();
		}

		void allowPassingHorizontally() {
			allowPassingRoadLanes(verticallyRoadLanes, false);
			allowPassingRoadLanes(horizontallyRoadLanes, true);
			bottomLeftTrafficLight.SetGreen();
			bottomRightTrafficLight.SetRed();
			topRightTrafficLight.SetGreen();
			topLeftTrafficLight.SetRed();
		}

		void disallowPassingAll() {
			allowPassingRoadLanes(verticallyRoadLanes, false);
			allowPassingRoadLanes(horizontallyRoadLanes, false);
			bottomLeftTrafficLight.SetYellow();
			bottomRightTrafficLight.SetYellow();
			topRightTrafficLight.SetYellow();
			topLeftTrafficLight.SetYellow();
		}
		
		while (true) {
			allowPassingVertically();
			yield return new WaitForSeconds(7f);

			disallowPassingAll();
			yield return new WaitForSeconds(4f);
			
			allowPassingHorizontally();
			yield return new WaitForSeconds(7f);

			disallowPassingAll();
			yield return new WaitForSeconds(4f);
		}
	}
	
	private void CreateCorners() {
		bottomLeftCorner = cornerPrefab.Create("BottomLeftCorner", transform, bottomSegment.transform.localEulerAngles.y);
		Utils.GetIntersection(bottomSegment.transform.position, bottomSegment.transform.forward,
			leftSegment.transform.position, leftSegment.transform.forward, out Vector3 p0);
		bottomLeftCorner.transform.position = p0;
		elements.Add(bottomLeftCorner);
		
		bottomRightCorner = cornerPrefab.Create("BottomRightCorner", transform, bottomLeftCorner.transform.localEulerAngles.y - 90f);
		Utils.GetIntersection(bottomSegment.transform.position + bottomSegment.transform.right * bottomSegment.Width, bottomSegment.transform.forward,
			rightSegment.transform.position + rightSegment.transform.right * rightSegment.Width, rightSegment.transform.forward, out Vector3 p1);
		bottomRightCorner.transform.position = p1;
		elements.Add(bottomRightCorner);
		
		topRightCorner = cornerPrefab.Create("TopRightCorner", transform, bottomLeftCorner.transform.localEulerAngles.y - 180f);
		Utils.GetIntersection(rightSegment.transform.position, rightSegment.transform.forward,
			topSegment.transform.position + topSegment.transform.right * topSegment.Width, topSegment.transform.forward, out Vector3 p2);
		topRightCorner.transform.position = p2;
		elements.Add(topRightCorner);

		topLeftCorner = cornerPrefab.Create("TopLeftCorner", transform, bottomLeftCorner.transform.localEulerAngles.y + 90f);
		Utils.GetIntersection(topSegment.transform.position, topSegment.transform.forward,
			leftSegment.transform.position + leftSegment.transform.right * leftSegment.Width, leftSegment.transform.forward, out Vector3 p3);
		topLeftCorner.transform.position = p3;
		elements.Add(topLeftCorner);
	}

	private void CreateTrafficLights() {
		bottomLeftTrafficLight = CreateTrafficLight("BottomLeftTrafficLight", bottomLeftCorner.transform);
		bottomRightTrafficLight = CreateTrafficLight("BottomRightTrafficLight", bottomRightCorner.transform);
		topRightTrafficLight = CreateTrafficLight("TopRightTrafficLight", topRightCorner.transform);
		topLeftTrafficLight = CreateTrafficLight("TopLeftTrafficLight", topLeftCorner.transform);
	}

	private TrafficLight CreateTrafficLight(string name, Transform corner) {
		TrafficLight trafficLight = (TrafficLight)trafficLightPrefab.Create(name, transform, corner.transform.localEulerAngles.y - 90f);
		trafficLight.transform.position = corner.transform.position + corner.transform.forward * 3f;
		elements.Add(trafficLight);
		return trafficLight;
	}

	private void CreateCrossings() {
		CreateCrossing("CurrentCrossing", bottomSegment, bottomSegment.Length + laneSize);
		CreateCrossing("RightCrossing", rightSegment);
		CreateCrossing("TopCrossing", topSegment);
		CreateCrossing("LeftCrossing", leftSegment);
	}

	private void CreateCrossing(string name, Segment segment, int length = 0) {
		GameObject crossing = GeneratorsController.Instance.CrossingGenerator.Generate(name, transform, laneSize, segment.RoadLanes.Count * laneSize);
		crossing.transform.SetLocalAngleY(segment.transform.localEulerAngles.y + 90f);
		crossing.transform.position = segment.transform.position + segment.transform.forward * length + segment.transform.right * laneSize;
	}
	
	private void CreateSideWalks() {
		int bottomLeftSideWalks0 = CreateSideWalk("BottomLeftSideWalk0", 
			leftSegment.transform.position, 
			bottomLeftCorner.transform.position, leftSegment.transform.localEulerAngles.y + 90f, -laneSize);
		int bottomLeftSideWalks1 = CreateSideWalk("BottomLeftSideWalk1", 
			bottomSegment.transform.position + bottomSegment.transform.forward * bottomSegment.Length, 
			bottomLeftCorner.transform.position, bottomSegment.transform.localEulerAngles.y + 90f);
		int bottomRightSideWalk0 = CreateSideWalk("BottomRightSideWalk0",
			bottomSegment.transform.position + bottomSegment.transform.forward * bottomSegment.Length + bottomSegment.transform.right * bottomSegment.Width, 
			bottomRightCorner.transform.position, bottomSegment.transform.localEulerAngles.y - 90f, -laneSize);
		int bottomRightSideWalk1 = CreateSideWalk("BottomRightSideWalk1",
			rightSegment.transform.position + rightSegment.transform.right * rightSegment.Width, 
			bottomRightCorner.transform.position, rightSegment.transform.localEulerAngles.y - 90f);
		int topRightSideWalk0 = CreateSideWalk("TopRightSideWalk0",
			rightSegment.transform.position, 
			topRightCorner.transform.position, rightSegment.transform.localEulerAngles.y + 90f, -laneSize);
		int topRightSideWalk1 = CreateSideWalk("TopRightSideWalk1",
			topSegment.transform.position + topSegment.transform.right * topSegment.Width, 
			topRightCorner.transform.position, topSegment.transform.localEulerAngles.y - 90f);
		int topLeftSideWalk0 = CreateSideWalk("TopLeftSideWalk0",
			topSegment.transform.position, 
			topLeftCorner.transform.position, topSegment.transform.localEulerAngles.y + 90f, -laneSize);
		int topLeftSideWalk1 = CreateSideWalk("TopLeftSideWalk1",
			leftSegment.transform.position + leftSegment.transform.right * leftSegment.Width, 
			topLeftCorner.transform.position, leftSegment.transform.localEulerAngles.y - 90f);
		bottomSegment.SetLengthSides(bottomLeftSideWalks1, bottomRightSideWalk0);
		rightSegment.SetLengthSides(bottomRightSideWalk1, topRightSideWalk0);
		topSegment.SetLengthSides(topRightSideWalk1, topLeftSideWalk0);
		leftSegment.SetLengthSides(topLeftSideWalk1, bottomLeftSideWalks0);
	}
	
	private int CreateSideWalk(string name, Vector3 start, Vector3 end, float angleY, float offset = 0f) {
		int dist = Mathf.RoundToInt(Vector3.Distance(start, end));
		if (dist == 0) {
			return 0;
		}
		int count = dist / laneSize;
		Vector3 dir = (end - start).normalized;
		start += dir * offset;
		for (int i = 0; i < count; i++) {
			Element sideWalk = sideWalkPrefab.Create(name, transform, angleY);
			start += dir * laneSize;
			sideWalk.transform.position = start;
			elements.Add(sideWalk);
		}
		return count;
	}

	private void CreateRoadBares() {
		List<Vector3> polyPoints = new List<Vector3> {
			bottomLeftCorner.transform.position + bottomLeftCorner.transform.forward * laneSize + bottomLeftCorner.transform.right * laneSize,
			bottomSegment.transform.position + bottomSegment.transform.forward * (bottomSegment.Length + laneSize) + bottomSegment.transform.right * laneSize,
			bottomSegment.transform.position + bottomSegment.transform.forward * (bottomSegment.Length + laneSize) + bottomSegment.transform.right * (bottomSegment.Width - laneSize),
			bottomRightCorner.transform.position + bottomRightCorner.transform.forward * laneSize + bottomRightCorner.transform.right * laneSize,
			rightSegment.transform.position - rightSegment.transform.forward * laneSize + rightSegment.transform.right * (rightSegment.Width - laneSize),
			rightSegment.transform.position - rightSegment.transform.forward * laneSize + rightSegment.transform.right * laneSize,
			topRightCorner.transform.position + topRightCorner.transform.forward * laneSize + topRightCorner.transform.right * laneSize,
			topSegment.transform.position - topSegment.transform.forward * laneSize + topSegment.transform.right * (topSegment.Width - laneSize),
			topSegment.transform.position - topSegment.transform.forward * laneSize + topSegment.transform.right * laneSize,
			topLeftCorner.transform.position + topLeftCorner.transform.forward * laneSize + topLeftCorner.transform.right * laneSize,
			leftSegment.transform.position - leftSegment.transform.forward * laneSize + leftSegment.transform.right * (leftSegment.Width - laneSize),
			leftSegment.transform.position - leftSegment.transform.forward * laneSize + leftSegment.transform.right * laneSize
		};

		Vector3[] cornerPoints = new Vector3[4];
		cornerPoints[0] = transform.position;
		Utils.GetIntersection(bottomSegment.transform.position + bottomSegment.transform.forward * (bottomSegment.Length + laneSize), 
			bottomSegment.transform.right,
			rightSegment.transform.position - rightSegment.transform.forward * laneSize,
			-rightSegment.transform.right, out cornerPoints[1]);
		Utils.GetIntersection(rightSegment.transform.position - rightSegment.transform.forward * laneSize, 
			-rightSegment.transform.right,
			topSegment.transform.position - topSegment.transform.forward * laneSize,
			topSegment.transform.right, out cornerPoints[2]);
		Utils.GetIntersection(topSegment.transform.position - topSegment.transform.forward * laneSize, 
			-topSegment.transform.right,
			leftSegment.transform.position - leftSegment.transform.forward * laneSize,
			-leftSegment.transform.right, out cornerPoints[3]);
		
		GeneratorsController.Instance.RoadBareGenerator.Generate("RoadBare", transform, cornerPoints, polyPoints);
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
				RoadLane lane1 = rightSegment.ForwardRoadLanes[Mathf.Min(i, rightSegment.ForwardRoadLanes.Count - 1)];
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
		int maxRightConnections = Mathf.Min(rightSegment.BackRoadLanes.Count, leftSegment.ForwardRoadLanes.Count);
		for (int i = 0; i < maxRightConnections; i++) {
			RoadLane lane0 = rightSegment.BackRoadLanes[^(i + 1)];
			RoadLane lane1 = leftSegment.ForwardRoadLanes[^(i + 1)];
			lane0.AddNextRoadLane(lane1, new List<Vector3> { lane0.EndPos, lane1.StartPos });
		}
		for (int i = 0; i < rightSegment.BackRoadLanes.Count; i++) {
			RoadLane lane0 = rightSegment.BackRoadLanes[i];
			if (i == 0 || !lane0.HasNextRoadLanes()) {
				RoadLane lane1 = topSegment.ForwardRoadLanes[Mathf.Min(i, topSegment.ForwardRoadLanes.Count - 1)];
				List<Vector3> transPoints = GetTransitionPoints(lane1.transform.forward, lane0.EndPos, lane1.StartPos);
				lane0.AddNextRoadLane(lane1, transPoints);
			}
		}

		// leftSegment
		int maxLeftConnections = Mathf.Min(leftSegment.BackRoadLanes.Count, rightSegment.ForwardRoadLanes.Count);
		for (int i = 0; i < maxLeftConnections; i++) {
			RoadLane lane0 = leftSegment.BackRoadLanes[^(i + 1)];
			RoadLane lane1 = rightSegment.ForwardRoadLanes[^(i + 1)];
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
