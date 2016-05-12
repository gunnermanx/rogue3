using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Delaunay.Geo;
using Delaunay;
using System.Diagnostics;

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

	private List<GameObject> _nodes = new List<GameObject>();

	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private Delaunay.Voronoi v;

	private void Start() {
		GenerateGraphNodes();
	}

	private void GenerateGraphNodes() {

		System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		List<Vector2> points = new List<Vector2>();
		for ( int i = 0; i < _nodeCount; i++ ) {
			Vector3 position = FindOpenPosition();
			GameObject graphNodeGO = GameObject.Instantiate( _graphNodePrefab, position, Quaternion.identity ) as GameObject;
			graphNodeGO.transform.SetParent( transform );
			graphNodeGO.name = "Node_X_" + position.x + "_Y_" + position.y;

			_nodes.Add( graphNodeGO );

			points.Add( new Vector2( position.x, position.y ) );
		}

		stopwatch.Stop();
		UnityEngine.Debug.Log( "Generate Graph Done... Took " + stopwatch.ElapsedMilliseconds.ToString() );

		CreateDelaunayTriangulation( points );
	}

	private bool LineSegEquals( LineSegment a, LineSegment b ) {
		return ( a.p0.HasValue && b.p0.HasValue && a.p0.Value == b.p0.Value ) && 
				( a.p1.HasValue && b.p1.HasValue && a.p1.Value == b.p1.Value );
	}

	private void CreateDelaunayTriangulation( List<Vector2> points ) {

		System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		//List<uint> colors = new List<uint> ();

		List<uint> colors = new List<uint> ();

		for (int i = 0; i < _nodeCount; i++) {
			colors.Add (0);
		}

		v = new Delaunay.Voronoi( points, colors, new Rect (0, 0, _mapWidth, _mapHeight) );
		//m_edges = v.VoronoiDiagram ();

		m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
		m_delaunayTriangulation = v.DelaunayTriangulation ();

		stopwatch.Stop();
		UnityEngine.Debug.Log( "Delaunay Done... Took " + stopwatch.ElapsedMilliseconds.ToString() );
	
		// loop through delaunay triangulation, remove spanning tree item

		// test for equivilence
		for ( int i = m_delaunayTriangulation.Count-1; i >= 0; i-- ) {
			for ( int j = 0; j < m_spanningTree.Count; j++ ) {

				if ( LineSegEquals(  m_spanningTree[j], m_delaunayTriangulation[i] ) ) {
					UnityEngine.Debug.Log( "function" );
					m_delaunayTriangulation.RemoveAt( i );
					break;
				}
			}
		}


		for ( int i = 0, count = m_spanningTree.Count; i < count; i++ ) {
			LineSegment line = m_spanningTree[ i ];
			GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
			graphEdgeGO.transform.SetParent( transform );

			Vector2 p0 = line.p0.Value;
			Vector2 p1 = line.p1.Value;
			Vector3[] lineVerts = new Vector3[] { new Vector3( p0.x, p0.y, 0 ), new Vector3( p1.x, p1.y, 0 ) };

			//graphEdgeGO.name = "Edge_" + p0.x + "_" + line.p1;

			graphEdgeGO.GetComponent<LineRenderer>().SetPositions( lineVerts );
		}
//

		// only create a percentage of this list
		for ( int i = 0, count = m_delaunayTriangulation.Count; i < count; i++ ) {

			float rand = UnityEngine.Random.value;
			if ( rand > 0.25 ) continue;

			LineSegment line = m_delaunayTriangulation[ i ];
			GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
			graphEdgeGO.transform.SetParent( transform );

			Vector2 p0 = line.p0.Value;
			Vector2 p1 = line.p1.Value;
			Vector3[] lineVerts = new Vector3[] { new Vector3( p0.x, p0.y, 0 ), new Vector3( p1.x, p1.y, 0 ) };

			//graphEdgeGO.name = "Edge_" + p0.x + "_" + line.p1;

			graphEdgeGO.GetComponent<LineRenderer>().SetPositions( lineVerts );
		}

	}

	private Vector3 FindOpenPosition() {

		Collider[] neighbours;
		Vector3 candidatePosition;
		do {
			// draw a new position
			candidatePosition = new Vector3( Random.Range(0, _mapWidth), Random.Range(0, _mapHeight), 0 );
			// get neighbours inside minDistance:
			neighbours = Physics.OverlapSphere( candidatePosition, _minDistance);
			// if there's any neighbour inside range, repeat the loop:
		} while (neighbours.Length > 0);

		return candidatePosition; // otherwise return the new position
	}
}
