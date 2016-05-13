using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay;

public class GameMap : MonoBehaviour {

	[SerializeField]
	private GameObject _graphNodePrefab;
	[SerializeField]
	private GameObject _graphEdgePrefab;
	[SerializeField]
	private LayerMask _graphNodeLayerMask;
	[SerializeField]
	private int _nodeCount;
	[SerializeField]
	private float _minDistance;
	[SerializeField]
	private int _mapWidth = 40;
	[SerializeField]
	private int _mapHeight = 60;

	// GameObjects in game map
	private Dictionary<string, MapNodeData> _mapNodes = new Dictionary<string, MapNodeData>();
	private List<MapEdgeData> _mapEdges = new List<MapEdgeData>();

	// Singleton Accessor
	private static GameMap _instance = null;
	public static GameMap Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
	}

	public void LoadMap( MapBlob blob ) {
		_mapNodes = blob.MapNodes;
		_mapEdges = blob.MapEdges;

		// Temp
		foreach( KeyValuePair<string, MapNodeData> kvp in _mapNodes ) {
			GameObject graphNodeGO = GameObject.Instantiate( _graphNodePrefab ) as GameObject;
			graphNodeGO.transform.SetParent( transform );
			MapNode mapNode = graphNodeGO.GetComponent<MapNode>();
			mapNode.Initialize( kvp.Value, MapNodeTappedCallback );
		}

		for ( int i = 0, count = _mapEdges.Count; i < count; i++ ) {
			GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
			graphEdgeGO.transform.SetParent( transform );
			MapEdge edge = graphEdgeGO.GetComponent<MapEdge>();
			edge.Initialize( _mapEdges[ i ] );
		}
	}


	public MapBlob GenerateNewMap() {

		MapBlob blob = new MapBlob();
		GenerateGraph();

		blob.MapNodes = _mapNodes;
		blob.MapEdges = _mapEdges;

		return blob;
	}

	public void MapNodeTappedCallback( MapNode node ) {
		Debug.Log( "NodeTapped " + node.Data.Id );
		GameManager.Instance.ShowWeaponPicker();
	}

	private void GenerateGraph() {		
		List<Vector2> points = new List<Vector2>();

		for ( int i = 0; i < _nodeCount; i++ ) {

			Vector3 position;
			if ( i == 0 ) {
				position = FindStartingPosition();
			} else if ( i == _nodeCount ) {
				position = FindEndingPosition();
			} else {
				position = FindOpenPosition();
			}

			GameObject graphNodeGO = GameObject.Instantiate( _graphNodePrefab ) as GameObject;
			graphNodeGO.transform.SetParent( transform );

			MapNode mapNode = graphNodeGO.GetComponent<MapNode>();
			mapNode.Initialize( GetNodeIdFromVector3(position), position, MapNodeTappedCallback );

			_mapNodes.Add( mapNode.NodeId, mapNode.Data );
			points.Add( new Vector2( position.x, position.y ) );

			if ( i == 0 ) {
				// save current node		
			}
		}

		GenerateEdges( points );
	}

	private void GenerateEdges( List<Vector2> points ) {

		List<LineSegment> minimumSpanningTree;
		List<LineSegment> delaunayTriangulation;
		List<LineSegment> finalEdges = new List<LineSegment>();

		// Create the Voronoi diagram using the AS3 Delaunay library
		List<uint> colors = new List<uint> ();
		for ( int i = 0; i < _nodeCount; i++ ) { colors.Add (0); }
		Delaunay.Voronoi voronoi = new Delaunay.Voronoi( points, colors, new Rect( 0, 0, _mapWidth, _mapHeight ) );
		minimumSpanningTree = voronoi.SpanningTree( KruskalType.MINIMUM );
		delaunayTriangulation = voronoi.DelaunayTriangulation ();

		// First add any line segment in the minimum spanning tree to the list _edges
		for ( int i = delaunayTriangulation.Count-1; i >= 0; i-- ) {
			for ( int j = 0; j < minimumSpanningTree.Count; j++ ) {
				if ( LineSegmentEquals(  minimumSpanningTree[j], delaunayTriangulation[i] ) ) {

					finalEdges.Add( delaunayTriangulation[ i ] );

					delaunayTriangulation.RemoveAt( i );

					break;
				}
			}
		}

		// Add a random amount of line segments in the delaunay triangulation but NOT in the minimum spanning tree to the list _edges
		for ( int i = 0, count = delaunayTriangulation.Count; i < count; i++ ) {
			float rand = UnityEngine.Random.value;
			if ( rand <= 0.25 ) {
				finalEdges.Add( delaunayTriangulation[ i ] );
			}
		}
			
		// Create the edges on the map
		// Also update neighbours properties on the nodes
		for ( int i = 0, count = finalEdges.Count; i < count; i++ ) {
			LineSegment line = finalEdges[ i ];

			GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
			graphEdgeGO.transform.SetParent( transform );
			MapEdge edge = graphEdgeGO.GetComponent<MapEdge>();

			Vector3 p0 = new Vector3( line.p0.Value.x, line.p0.Value.y, 0 );
			Vector3 p1 = new Vector3( line.p1.Value.x, line.p1.Value.y, 0 );

			string node1Id = GetNodeIdFromVector3( p0 );
			string node2Id = GetNodeIdFromVector3( p1 );

			_mapNodes[ node1Id ].AddNeighbour( node2Id );
			_mapNodes[ node2Id ].AddNeighbour( node1Id );

			edge.Initialize( p0, p1 );
			_mapEdges.Add( edge.Data );
		}
	}

	private Vector3 FindOpenPosition() {
		Collider[] overlappingNodes;
		Vector3 candidatePosition;
		// Find a random spot, check if there are any nodes, within a distance, at that spot
		do {
			// Pick a random position within our map boundaries and check if it is an open spot
			candidatePosition = new Vector3( Random.Range(0, _mapWidth), Random.Range(0, _mapHeight), 0 );
			overlappingNodes = Physics.OverlapSphere( candidatePosition, _minDistance );

		} 
		// If there are any overlapping nodes, we need to choose a new random spot
		while ( overlappingNodes.Length > 0 );

		return candidatePosition; 
	}

	private Vector3 FindStartingPosition() {
		return new Vector3( Random.Range(0, _mapWidth), 0, 0 );
	}

	private Vector3 FindEndingPosition() {
		return new Vector3( Random.Range(0, _mapWidth), _mapHeight-1, 0 );
	}

	private bool LineSegmentEquals( LineSegment a, LineSegment b ) {
		return ( a.p0.HasValue && b.p0.HasValue && a.p0.Value == b.p0.Value ) && 
			( a.p1.HasValue && b.p1.HasValue && a.p1.Value == b.p1.Value );
	}

	private static string GetNodeIdFromVector3( Vector3 position ) {
		return "Node@X_" + position.x + "_Y_" + position.y;
	}
}

