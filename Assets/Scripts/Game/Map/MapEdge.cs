using System;
using UnityEngine;
using FullSerializer;

public class MapEdgeData {
	[fsProperty]
	public Vector3 P0;
	[fsProperty]
	public Vector3 P1;
}

public class MapEdge : MonoBehaviour {

	[fsProperty]
	public MapEdgeData Data;

	public void Initialize( Vector3 p0, Vector3 p1 ) {
		Data = new MapEdgeData();
		Data.P0 = p0;
		Data.P1 = p1;

		Vector3[] lineVerts = new Vector3[] { p0, p1 };
		GetComponent<LineRenderer>().SetPositions( lineVerts );
	}

	public void Initialize( MapEdgeData data ) {
		Data = data;
		Vector3[] lineVerts = new Vector3[] { Data.P0, Data.P1 };
		GetComponent<LineRenderer>().SetPositions( lineVerts );
	}
}


