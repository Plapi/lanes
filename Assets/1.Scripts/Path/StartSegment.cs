using System.Collections.Generic;
using UnityEngine;

public class StartSegment : Segment {

	public Building garage;
	
	public override void Init(SegmentData segmentData) {
		SegmentData = segmentData;
		RoadLanes = new List<RoadLane>();
		ForwardRoadLanes = new List<RoadLane>();
		BackRoadLanes = new List<RoadLane>();
		Width = segmentData.lanes.Length * Settings.Instance.laneSize;
		for (int i = 0; i < lanes.Count; i++) {
			lanes[i].SetData(segmentData.lanes[i]);
			if (lanes[i] is RoadLane roadLane) {
				RoadLanes.Add(roadLane);
				if (roadLane.Data.hasFrontDirection) {
					ForwardRoadLanes.Insert(0, roadLane);
				} else {
					BackRoadLanes.Add(roadLane);
				}
			}
		}
	}

	public void CreateRightEnvironment(Segment rightSegment) {
		SegmentEnvironment segmentEnvironment = CreateEnvironment();
		segmentEnvironment.transform.SetLocalXZ(Width, garage.transform.localPosition.z + garage.Length + Settings.Instance.laneSize);
		segmentEnvironment.Create((int)segmentEnvironment.transform.InverseTransformPoint(rightSegment.transform.position).z, false);
	}
}
