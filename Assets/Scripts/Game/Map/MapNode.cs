using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine.EventSystems;

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

public class MapNode : MonoBehaviour, IPointerClickHandler {

	[fsProperty]
	public MapNodeData Data;

	public string NodeId { get { return Data.Id; } }

	public delegate void MapNodeTappedCallback( MapNode node );
	private MapNodeTappedCallback _callback;

	public void Initialize( string id, Vector3 position, MapNodeTappedCallback callback ) {
		Data = new MapNodeData();
		Data.Coordinates = position;
		Data.Id = id;

		transform.position = position;
		_callback = callback;
	}

	public void Initialize( MapNodeData data, MapNodeTappedCallback callback ) {
		Data = data;
		transform.position = Data.Coordinates;
		_callback = callback;
	}

	#region IPointerClickHandler implementation
	public void OnPointerClick( PointerEventData eventData ) {
		if ( _callback != null ) {
			_callback( this );
		}
	}
	#endregion
}
