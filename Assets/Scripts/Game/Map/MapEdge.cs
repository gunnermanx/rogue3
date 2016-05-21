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

	private const float Z_DEPTH = 1;

	[fsProperty]
	private MapEdgeData Data;

	public void Initialize( Vector3 p0, Vector3 p1 ) {
		Data = new MapEdgeData();
		Data.P0 = new Vector3( p0.x, p0.y, Z_DEPTH );
		Data.P1 = new Vector3( p1.x, p1.y, Z_DEPTH );

		Vector3[] lineVerts = new Vector3[] { Data.P0, Data.P1 };
		GetComponent<LineRenderer>().SetPositions( lineVerts );
	}

	public void Initialize( MapEdgeData data ) {
		Data = data;
		Vector3[] lineVerts = new Vector3[] { Data.P0, Data.P1 };
		GetComponent<LineRenderer>().SetPositions( lineVerts );
	}

	public MapEdgeData GetData() {
		return Data;
	}
}


