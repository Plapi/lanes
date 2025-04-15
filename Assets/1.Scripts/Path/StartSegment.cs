using System.Collections.Generic;
using UnityEngine;

public class StartSegment : Segment {

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
		
		// LeftEnvironment = CreateEnv("LeftEnv");
		// LeftEnvironment.transform.position = transform.position + transform.forward * Length;
		// LeftEnvironment.Generate(LeftEnvironment.transform.position - transform.forward * Length - transform.right * 10f, true);
	}
}
