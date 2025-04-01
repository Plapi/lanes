using UnityEngine;

public class GeneratorsController : MonoBehaviourSingleton<GeneratorsController> {

	public EnvironmentGenerator EnvironmentGenerator;
	public LaneGenerator LaneGenerator;
	public MeshGenerator CrossingGenerator;
	public MeshGenerator SideWalkGenerator;
	public RoadBareGenerator RoadBareGenerator;
	
}
