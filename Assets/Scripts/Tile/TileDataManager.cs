using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileDataManager : MonoBehaviour {

	[SerializeField]
	private List<TileData> TileData = new List<TileData>();

	public List<TileData> GetAllTileData() {
		return TileData;
	}

}
