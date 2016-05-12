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
	private List<GameObject> _mapNodes = new List<GameObject>();
	private List<GameObject> _mapEdges = new List<GameObject>();

	// Data representation of game map nodes/edges
	private Delaunay.Voronoi _voronoi;
	private List<Vector2> _points = new List<Vector2>();
	private List<LineSegment> _edges = new List<LineSegment>();

	private void Start() {
		GenerateGraphNodes();
		GenerateGraphEdges();
	}

	private void GenerateGraphNodes() {		
		for ( int i = 0; i < _nodeCount; i++ ) {
			Vector3 position = FindOpenPosition();
			GameObject graphNodeGO = GameObject.Instantiate( _graphNodePrefab, position, Quaternion.identity ) as GameObject;
			graphNodeGO.transform.SetParent( transform );
			graphNodeGO.name = "Node" + i + " @X_" + position.x + "_Y_" + position.y;

			_mapNodes.Add( graphNodeGO );
			_points.Add( new Vector2( position.x, position.y ) );
		}
	}

	private void GenerateGraphEdges() {

		List<LineSegment> minimumSpanningTree;
		List<LineSegment> delaunayTriangulation;

		// Create the Voronoi diagram using the AS3 Delaunay library
		List<uint> colors = new List<uint> ();
		for ( int i = 0; i < _nodeCount; i++ ) { colors.Add (0); }
		_voronoi = new Delaunay.Voronoi( _points, colors, new Rect( 0, 0, _mapWidth, _mapHeight ) );
		minimumSpanningTree = _voronoi.SpanningTree( KruskalType.MINIMUM );
		delaunayTriangulation = _voronoi.DelaunayTriangulation ();

		// First add any line segment in the minimum spanning tree to the list _edges
		for ( int i = delaunayTriangulation.Count-1; i >= 0; i-- ) {
			for ( int j = 0; j < minimumSpanningTree.Count; j++ ) {
				if ( LineSegmentEquals(  minimumSpanningTree[j], delaunayTriangulation[i] ) ) {

					_edges.Add( delaunayTriangulation[ i ] );

					delaunayTriangulation.RemoveAt( i );

					break;
				}
			}
		}

		// Add a random amount of line segments in the delaunay triangulation but NOT in the minimum spanning tree to the list _edges
		for ( int i = 0, count = delaunayTriangulation.Count; i < count; i++ ) {
			float rand = UnityEngine.Random.value;
			if ( rand <= 0.25 ) {
				_edges.Add( delaunayTriangulation[ i ] );
			}
		}
			
		// Create the edges on the map
		for ( int i = 0, count = _edges.Count; i < count; i++ ) {
			LineSegment line = _edges[ i ];

			GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
			graphEdgeGO.transform.SetParent( transform );

			Vector2 p0 = line.p0.Value;
			Vector2 p1 = line.p1.Value;
			Vector3[] lineVerts = new Vector3[] { new Vector3( p0.x, p0.y, 0 ), new Vector3( p1.x, p1.y, 0 ) };

			//graphEdgeGO.name = "Edge_" + p0.x + "_" + line.p1;

			graphEdgeGO.GetComponent<LineRenderer>().SetPositions( lineVerts );

			_mapEdges.Add( graphEdgeGO );
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

	private bool LineSegmentEquals( LineSegment a, LineSegment b ) {
		return ( a.p0.HasValue && b.p0.HasValue && a.p0.Value == b.p0.Value ) && 
			( a.p1.HasValue && b.p1.HasValue && a.p1.Value == b.p1.Value );
	}
}
