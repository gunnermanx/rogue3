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
	[fsProperty]
	private string BattleStageDataId;
}

public class MapNode : MonoBehaviour, IPointerClickHandler {

	[fsProperty]
	private MapNodeData Data;

	public string NodeId { get { return Data.Id; } }

	public delegate void MapNodeTappedCallback( MapNode node );
	private MapNodeTappedCallback _callback;

	private List<MapEdge> _edges = new List<MapEdge>();

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

	public void AddNeighbour( string neighbourId ) {
		if ( !Data.NeighbourIds.Contains( neighbourId ) ) {
			Data.NeighbourIds.Add ( neighbourId );
		}
	}

	public void AddEdge( MapEdge edge ) {
		_edges.Add( edge );
	}

	public bool HasNeighbour( string neighbourId ) {
		return Data.NeighbourIds.Contains( neighbourId );
	}

	public MapNodeData GetData() {
		return Data;
	}

	public void ToggleEdgeVisibility( bool toggle ) {
		for ( int i = 0, count = _edges.Count; i < count; i++ ) {
			_edges[ i ].gameObject.SetActive( toggle );
		}
	}

	#region IPointerClickHandler implementation
	public void OnPointerClick( PointerEventData eventData ) {
		if ( _callback != null ) {
			_callback( this );
		}
	}
	#endregion
}
