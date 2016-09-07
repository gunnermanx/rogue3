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
	private GameObject _playerPrefab;

	[SerializeField]
	private LayerMask _graphNodeLayerMask;
	[SerializeField]
	private float _minDistance;
	[SerializeField]
	private int _mapWidth = 40;
	[SerializeField]
	private int _mapHeight = 60;

	// GameObjects in game map
	private Dictionary<string, MapNode> _mapNodes = new Dictionary<string, MapNode>();
	private List<MapEdge> _mapEdges = new List<MapEdge>();

	private GameObject _player = null;

	private MapNode _currentNode = null;
	private MapNode _selectedNode = null;

	private MapHud _mapHud = null;

	private MapBlob _blob = null;

	public BattleStageData GetBattleStageData() {
		return _currentNode.BattleData;
	}

	public ShopStageData GetShopStageData() {
		return _currentNode.ShopData;
	}

	public void Initialize( MapHud hud ) {
		_mapHud = hud;
		_mapHud.Initialize( MoveToSelectedNode );
		CreatePlayer();
	}

	private void CreatePlayer() {
		_player = GameObject.Instantiate( _playerPrefab );
		_player.transform.position = _currentNode.transform.position;
	}

	public void MapNodeTappedCallback( MapNode node ) {
		Debug.Log( "NodeTapped " + node.NodeId );

		// Toggle edge visibility
		if ( _selectedNode != node || _selectedNode == null) {
			if ( _selectedNode != null && _selectedNode != _currentNode ) {
				_selectedNode.ToggleEdgeVisibility( false );
			}
			node.ToggleEdgeVisibility( true );
			_selectedNode = node;
		}

		if ( _selectedNode == _currentNode ) {
			GameManager.Instance.MapNodeSelected( _currentNode );

			if ( _selectedNode.BattleData != null ) {
				GameManager.Instance.ShowWeaponPicker();
			} else if ( _selectedNode.ShopData != null ) {
				GameManager.Instance.ShowShopDialog();
			}
	
			_mapHud.ToggleTravelButton( false );
		} else {
			bool canTravelToNode = CanTravelToNode( _selectedNode.NodeId );
			_mapHud.ToggleTravelButton( canTravelToNode );
		}
	}

	public bool CanTravelToNode( string nodeId ) {
		// Simple for now

		bool completedSelectedNode = _blob.CompletedNotes.Contains( nodeId );
		bool completedCurrentNode = _blob.CompletedNotes.Contains( _currentNode.NodeId );
		if ( (completedCurrentNode || completedSelectedNode) && _currentNode.HasNeighbour(nodeId) ) {
			return true;
		}
		return false;
	}

	public void MoveToSelectedNode() {
		
		_currentNode.ToggleEdgeVisibility( false );
		_currentNode = _selectedNode;
		_currentNode.ToggleEdgeVisibility( true );


		iTween.MoveTo( _player, 
			iTween.Hash( "position", _currentNode.transform.position, 
				"easetype", iTween.EaseType.easeOutQuart, 
				"time", 1f,
				"oncomplete", "MovedToNode",
				"oncompletetarget", gameObject
			)
		);

		// Might need saving to elsewhere later
		_blob.CurrentNode = _currentNode.NodeId;
		SaveMap();

		GameManager.Instance.MapNodeSelected( _currentNode );


	}

	public void MovedToNode() {
		GameManager.Instance.ShowWeaponPicker();
	}


#region Loading/Generation of Map Data

	public void LoadMap( MapBlob blob ) {

		_blob = blob;

		// Temp
		foreach( KeyValuePair<string, MapNodeData> kvp in blob.MapNodes ) {
			MapNode mapNode = CreateMapNode( kvp.Key );
			mapNode.InitializeFromData( kvp.Value, MapNodeTappedCallback );
		}

		for ( int i = 0, count = blob.MapEdges.Count; i < count; i++ ) {
			MapEdge mapEdge = CreateMapEdge( blob.MapEdges[ i ].P0, blob.MapEdges[ i ].P1 );
			mapEdge.Initialize( blob.MapEdges[ i ] );
		}

		_currentNode = _mapNodes[ blob.CurrentNode ];

		_currentNode.ToggleEdgeVisibility( true );
	}


	public void GenerateNewMap( WorldData worldData ) {
		_currentNode = GenerateGraph( worldData );

		_blob = CreateNewMapBlob();
		SaveMap();

		_currentNode.ToggleEdgeVisibility( true );
	}

	private MapNode GenerateGraph( WorldData worldData ) {		
		List<Vector2> points = new List<Vector2>();
		MapNode startingNode = null;


		// world data says... maybe 8 battle stages, 2 shops, 1 quest
		int numBattleStages = UnityEngine.Random.Range( worldData.MinNumBattleStages, worldData.MaxNumBattleStages+1 );
		int numShopStages = UnityEngine.Random.Range( worldData.MinNumShopStages, worldData.MaxNumShopStages+1 );

		// Create battle stage nodes
		for ( int i = 0; i < numBattleStages; i++ ) {

			Vector3 position;
			if ( i == 0 ) {
				position = FindStartingPosition();
			} else if ( i == numBattleStages ) {
				position = FindEndingPosition();
			} else {
				position = FindOpenPosition();
			}

			BattleStageData stageData = Database.Instance.GetRandomBattleStageData();

			string nodeId = GetNodeIdFromVector3(position);
			MapNode mapNode = CreateMapNode( nodeId );
			mapNode.InitializeAsBattleStage( nodeId, position, stageData, MapNodeTappedCallback );

			points.Add( new Vector2( position.x, position.y ) );

			if ( i == 0 ) {
				// save current node
				startingNode = mapNode;
			}
		}

		// Create shop nodes
		for ( int i = 0; i < numShopStages; i++ ) {

			Vector3 position = FindOpenPosition();
	
			// depending on the world data, we need to pick from a random set

			ShopStageData stageData = Database.Instance.GetRandomShopStageData();

			string nodeId = GetNodeIdFromVector3(position);
			MapNode mapNode = CreateMapNode( nodeId );
			mapNode.InitializeAsShopStage( nodeId, position, stageData, MapNodeTappedCallback );

			points.Add( new Vector2( position.x, position.y ) );
		}

		GenerateEdges( points );

		return startingNode;
	}

	private MapNode CreateMapNode( string nodeId ) {
		GameObject graphNodeGO = GameObject.Instantiate( _graphNodePrefab ) as GameObject;
		graphNodeGO.transform.SetParent( transform );
		MapNode mapNode = graphNodeGO.GetComponent<MapNode>();
		_mapNodes.Add( nodeId, mapNode );

		return mapNode;
	}

	private MapEdge CreateMapEdge( Vector3 p0, Vector3 p1 ) {
		GameObject graphEdgeGO = GameObject.Instantiate( _graphEdgePrefab ) as GameObject;
		graphEdgeGO.transform.SetParent( transform );
		MapEdge mapEdge = graphEdgeGO.GetComponent<MapEdge>();

		string node1Id = GetNodeIdFromVector3( p0 );
		string node2Id = GetNodeIdFromVector3( p1 );

		_mapNodes[ node1Id ].AddNeighbour( node2Id );
		_mapNodes[ node2Id ].AddNeighbour( node1Id );

		_mapNodes[ node1Id ].AddEdge( mapEdge );
		_mapNodes[ node2Id ].AddEdge( mapEdge );

		_mapEdges.Add( mapEdge );

		return mapEdge;
	}

	private void GenerateEdges( List<Vector2> points ) {

		List<LineSegment> minimumSpanningTree;
		List<LineSegment> delaunayTriangulation;
		List<LineSegment> finalEdges = new List<LineSegment>();

		// Create the Voronoi diagram using the AS3 Delaunay library
		List<uint> colors = new List<uint> ();
		for ( int i = 0; i < points.Count; i++ ) { colors.Add (0); }
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
			if ( rand <= 0.35 ) {
				finalEdges.Add( delaunayTriangulation[ i ] );
			}
		}
			
		// Create the edges on the map
		// Also update neighbours properties on the nodes
		for ( int i = 0, count = finalEdges.Count; i < count; i++ ) {
			LineSegment line = finalEdges[ i ];
	
			Vector3 p0 = new Vector3( line.p0.Value.x, line.p0.Value.y, 0 );
			Vector3 p1 = new Vector3( line.p1.Value.x, line.p1.Value.y, 0 );

			MapEdge mapEdge = CreateMapEdge( p0, p1 );
			mapEdge.Initialize( p0, p1 );
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

#endregion

#region SerializeMap
	private MapBlob CreateNewMapBlob() {
		MapBlob blob = new MapBlob();
		Dictionary<string, MapNodeData> mapNodeData = new Dictionary<string, MapNodeData>();
		foreach( KeyValuePair<string, MapNode> kvp in _mapNodes ) {
			mapNodeData.Add( kvp.Key, kvp.Value.GetData() );
		}

		List<MapEdgeData> mapEdgeData = new List<MapEdgeData>();
		for( int i = 0, count = _mapEdges.Count; i < count; i++ ) {
			mapEdgeData.Add( _mapEdges[ i ].GetData() );	
		}

		blob.MapNodes = mapNodeData;
		blob.MapEdges = mapEdgeData;
		blob.CurrentNode = _currentNode.NodeId;
		return blob;
	}

	private void SaveMap() {
		GameManager.Instance.GetPersistenceManager().SaveMapData( _blob );
	}
#endregion

	private static string GetNodeIdFromVector3( Vector3 position ) {
		return "Node@X_" + position.x + "_Y_" + position.y;
	}
}

