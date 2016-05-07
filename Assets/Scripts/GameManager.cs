using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private BoardManager 	_boardManager;
	[SerializeField]
	private TileDataManager _tileDataManager;

	// Singleton Accessor
	private static GameManager _instance = null;
	public static GameManager Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
	}

	private void Start() {
		List<TileData> tileData = _tileDataManager.GetAllTileData();
		_boardManager.InitializeBoard( tileData );
	}

	private void Update() {
		if ( Input.GetKeyUp(KeyCode.BackQuote) ) {
			DebugMenu.ToggleDebugMenu();
		}
	}

	#region Debug
	public Tile[,] DebugGetBoard() {
		return _boardManager.DebugGetBoard();
	}

	public void ResetBoard() {
		_boardManager.ClearBoard();
		List<TileData> tileData = _tileDataManager.GetAllTileData();
		_boardManager.InitializeBoard( tileData );
	}
	#endregion
}
