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
	public string BattleStageDataId;
	[fsProperty]
	public string ShopStageDataId;

}

public class MapNode : MonoBehaviour, IPointerClickHandler {

	[fsProperty]
	private MapNodeData Data;

	[SerializeField]
	private GameObject BattleStageSprite;

	[SerializeField]
	private GameObject ShopStageSprite;

	public string NodeId { get { return Data.Id; } }

	public delegate void MapNodeTappedCallback( MapNode node );
	private MapNodeTappedCallback _callback;

	private List<MapEdge> _edges = new List<MapEdge>();

	private BattleStageData _battleStageData = null;
	public BattleStageData BattleData { get { return _battleStageData; } }

	private ShopStageData _shopStageData = null;
	public ShopStageData ShopData { get { return _shopStageData; } }

	private void Start() {
		BattleStageSprite.SetActive( _battleStageData != null );
		ShopStageSprite.SetActive( _shopStageData != null );
	}

	public void InitializeAsBattleStage( string id, Vector3 position, BattleStageData stageData, MapNodeTappedCallback callback ) {
		BaseInitialize( id, position );

		Data.BattleStageDataId = stageData.name;
		_battleStageData = stageData;
		_callback = callback;
	}

	public void InitializeAsShopStage( string id, Vector3 position, ShopStageData stageData, MapNodeTappedCallback callback ) {
		BaseInitialize( id, position );

		Data.ShopStageDataId = stageData.name;
		_shopStageData = stageData;
		_callback = callback;
	}

	private void BaseInitialize( string id, Vector3 position ) {
		Data = new MapNodeData();
		Data.Coordinates = position;
		Data.Id = id;

		transform.position = position;
	}

	public void InitializeFromData( MapNodeData data, MapNodeTappedCallback callback ) {
		Data = data;

		if ( data.BattleStageDataId != null ) _battleStageData = Database.Instance.GetBattleStageData( data.BattleStageDataId );
		else if ( data.ShopStageDataId != null ) _shopStageData = Database.Instance.GetShopStageData( data.ShopStageDataId );

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
