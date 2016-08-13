using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private GameObject _gameBoardPrefab;

	[SerializeField]
	private GameObject _battlePrefab;

	[SerializeField]
	private GameObject _gameMapPrefab;

	[SerializeField]
	private Database _database;

	[SerializeField]
	private PersistenceManager _persistenceManager;
	public PersistenceManager GetPersistenceManager() {
		return _persistenceManager;
	}

	// Singleton Accessor
	private static GameManager _instance = null;
	public static GameManager Instance {
		get { return _instance; }
	}

	private GameBoard _gameBoard;
	private Battle _battle;
	private GameMap _gameMap;

	private GameHud _gameHud = null;

	private MapHud _mapHud = null;

	private string _currentNodeId = null;


	private void Awake() {
		_instance = this;
		DontDestroyOnLoad( gameObject );
	}

	private void Start() {
		_persistenceManager.LoadPlayerData( OnPlayerBlobLoaded );
	}

	private void Update() {

#if UNITY_EDITOR
		if ( Input.GetKeyUp(KeyCode.BackQuote) ) {
#else
		if ( Input.touchCount >= 3 && 
			Input.GetTouch(0).phase == TouchPhase.Began && 
			Input.GetTouch(1).phase == TouchPhase.Began &&
			Input.GetTouch(2).phase == TouchPhase.Began ) {
#endif
			DebugMenu.ToggleDebugMenu();
		}
	}

	private void OnPlayerBlobLoaded( PlayerBlob blob ) {
		Debug.Log( "gameManager: playerblob loaded" );
		SceneManager.LoadScene( "Main" );
	}

	List<WeaponTileData> gameTileData = null;
	BattleStageData gameStageData = null;

	public void StartGame( List<WeaponTileData> tileData, BattleStageData stageData ) {

		gameTileData = tileData;
		gameStageData = stageData;

		SceneManager.LoadScene( "Game" );

	}

	public void MapNodeSelected( MapNode node ) {
		_currentNodeId = node.NodeId;
	}

	public void ShowWeaponPicker() {
		List<string> ownedTileIds = _persistenceManager.PlayerBlob.OwnedTileIds;
		TilePickerDialog weaponPicker = UIManager.Instance.OpenDialog( TilePickerDialog.DIALOG_ID ) as TilePickerDialog;
		weaponPicker.Initialize( ownedTileIds, delegate(List<WeaponTileData> tileData ) {
			StartGame( tileData, _gameMap.GetCurrentStageData() );
		});
	}

	private void OnLevelWasLoaded( int level ) {
		if ( level == 1 ) {

			InitializeWorldMap();
		}
		else if ( level == 2 ) {

			if ( _mapHud != null ) {
				UIManager.Instance.CloseDialog( MapHud.DIALOG_ID );
			}

			StartGame();
		}
	}


	private void InitializeWorldMap() {
		
		// TODO: load world data properly
		Database.Instance.LoadWorldStageData( 1 );

		// Open map hud
		_mapHud = UIManager.Instance.OpenDialog( MapHud.DIALOG_ID ) as MapHud;
		_mapHud.UpdateLife( _persistenceManager.PlayerBlob.CurrentLives, _persistenceManager.PlayerBlob.MaxLives );

		// Create map
		GameObject gameMapGO = GameObject.Instantiate( _gameMapPrefab ) as GameObject;
		_gameMap = gameMapGO.GetComponent<GameMap>();

		if ( _persistenceManager.PlayerBlob.MapBlob == null ) {
			_gameMap.GenerateNewMap();
		} else {
			_gameMap.LoadMap( _persistenceManager.PlayerBlob.MapBlob );
		}
		_gameMap.Initialize( _mapHud );
	}

	private void StartGame() {

		// Initialize Game HUD
		_gameHud = UIManager.Instance.OpenDialog( GameHud.DIALOG_ID ) as GameHud;

		// Create the gameboard and battle
		GameObject gameBoard = GameObject.Instantiate( _gameBoardPrefab );
		_gameBoard = gameBoard.GetComponent<GameBoard>();

		GameObject battle = GameObject.Instantiate( _battlePrefab );
		_battle = battle.GetComponent<Battle>();

		// Initialize battle manager
		_battle.Initialize( gameStageData, _gameHud, _gameBoard );

		// Initialize game board
		_gameBoard.Initialize( gameTileData, _battle );
	}

	public void CleanupGame() {

		Destroy( _gameBoard.gameObject );
		Destroy( _battle.gameObject );

		UIManager.Instance.CloseDialog( GameHud.DIALOG_ID );
		_gameHud = null;

		_currentNodeId = null;
	
		SceneManager.LoadScene( "Main" );
	}

	public void GameComplete( bool isVictory ) {

		// this should check failure or success
		if ( isVictory ) {
			// we need to save which node was completed
			_persistenceManager.SaveCompletedNode( _currentNodeId );
		} else {
			// TODO lose life
			_persistenceManager.UpdateCurrentLives( _persistenceManager.PlayerBlob.CurrentLives - 1 );
		}
	}

#region Debug
	public Tile[,] DebugGetBoard() {
		return _gameBoard.DebugGetBoard();
	}

	public void ResetBoard() {
//		_boardManager.ClearBoard();
//		List<WeaponTileData> tileData = GetStartingWeaponTileData();
//		_boardManager.Initialize( tileData );
	}

	public void ResetPlayerBlob() {
		_persistenceManager.DeletePlayerData();
		Destroy( gameObject );
		Destroy( UIManager.Instance.gameObject );
		SceneManager.LoadScene( "Loader" );
	}

	public void SetEnemyHP( int hp ) {
		_battle.SetEnemyRemainingHP( hp );
	}
#endregion
}
