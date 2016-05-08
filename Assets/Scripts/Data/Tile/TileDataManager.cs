using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileDataManager : MonoBehaviour {

	[SerializeField]
	private List<TileData> TileData = new List<TileData>();

	[SerializeField]
	private List<TileData> BlockTileData = new List<TileData>();

	public List<TileData> GetAllTileData() {
		return TileData;
	}

	public TileData GetRandomBlockTileData() {
		TileData data = null;
		if ( BlockTileData.Count > 0 ) {
			data = BlockTileData[ UnityEngine.Random.Range( 0, BlockTileData.Count ) ];
		}
		return data;
	}

}
