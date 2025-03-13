using UnityEngine;

public abstract class Lane<T> : LaneBase where T : LaneData {
	
	public T Data { get; private set; }

	public override void Init(LaneData data) {
		base.Init(data);
		Data = (T)data;
		OnInit();
	}
	
	public override void SetData(LaneData data) {
		Data = (T)data;
	}
	
	public int Length => Data.length;

	protected abstract void OnInit();
}

[ExecuteInEditMode]
public abstract class LaneBase : MonoBehaviour {

	public GameObject meshObj;
	
	public virtual void Init(LaneData data) { }
	
	public abstract void SetData(LaneData data);
	
	public virtual void Clear() {
		if (meshObj != null) {
			Destroy(meshObj);
		}
	}
}

public class LaneData {
	public LaneType type;
	public int length;
}

public enum LaneType {
	SideWalk,
	RoadFirst,
	RoadMiddle,
	RoadLast
}
