using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;

public class MapNodeData {
	[fsProperty]
	public Vector3 Coordinates;
	[fsProperty]
	public string Id = null;
	[fsProperty]
	public List<string> NeighbourIds = new List<string>();

	public void AddNeighbour( string neighbourId ) {
		if ( !NeighbourIds.Contains( neighbourId ) ) {
			NeighbourIds.Add ( neighbourId );
		}
	}
}

public class MapNode : MonoBehaviour {

	[fsProperty]
	public MapNodeData Data;

	public string NodeId { get { return Data.Id; } }

	public void Initialize( string id, Vector3 position ) {
		Data = new MapNodeData();
		Data.Coordinates = position;
		Data.Id = id;

		transform.position = position;
	}

	public void Initialize( MapNodeData data ) {
		Data = data;
		transform.position = Data.Coordinates;
	}

}
