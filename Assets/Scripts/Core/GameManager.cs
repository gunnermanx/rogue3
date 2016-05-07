using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public enum GameType {
		TurnLimit,
		TimeLimit
	}

	public class GameSession {
		public GameType Type;
		public long Time = -1;
		public int TurnsRemaining = -1;
	}

	[SerializeField]
	private GameObject _boardManagerPrefab;

	[SerializeField]
	private GameObject _battleManagerPrefab;
	
	[SerializeField]
	private TileDataManager _tileDataManager;

	[SerializeField]
	private GameHud _gameHud;

	// Singleton Accessor
	private static GameManager _instance = null;
	public static GameManager Instance {
		get { return _instance; }
	}
	
	private BoardManager _boardManager;
	private BattleManager _battleManager;

	private GameSession _gameSession = null;
	private int _currentWindowId = 0;

	private static readonly float WIDTH = 400;
	private static readonly float HEIGHT = 400;
	
	public Rect windowRect = new Rect( (Screen.width - WIDTH)/2, (Screen.height + HEIGHT)/2, 100, 100);

	private void Awake() {
		_instance = this;
	}

	private void Update() {
		if ( Input.GetKeyUp(KeyCode.BackQuote) ) {
			DebugMenu.ToggleDebugMenu();
		}
	}

	private void StartGame( List<TileData> tileData ) {
		_gameSession = new GameSession() {
			Type = GameType.TurnLimit,
			TurnsRemaining = 5
		};

		// Initialize game board
		GameObject boardManagerGO = GameObject.Instantiate( _boardManagerPrefab );
		_boardManager = boardManagerGO.GetComponent<BoardManager>();
		_boardManager.InitializeBoard( tileData );
		_boardManager.OnTilesSwapped += HandleOnTilesSwapped;
		_boardManager.OnTilesMatched += HandleOnTilesMatched;

		// Initialize battle manager
		GameObject battleManagerGO = GameObject.Instantiate( _battleManagerPrefab );
		_battleManager = battleManagerGO.GetComponent<BattleManager>();


		// Initialize Game HUD
		_gameHud.gameObject.SetActive( true );
		_gameHud.UpdateHUD();
	}

	private void CleanupGame() {
		_boardManager.OnTilesSwapped -= HandleOnTilesSwapped;
		_boardManager.OnTilesMatched -= HandleOnTilesMatched;

		Destroy( _boardManager.gameObject );
		Destroy( _battleManager.gameObject );

		_gameSession = null;

		_gameHud.gameObject.SetActive( false );
	}

	private bool CheckGameComplete() {
		// only lose condition right now

		bool gameComplete = false;

		switch( _gameSession.Type ) {
		case GameType.TurnLimit:
			if ( _gameSession.TurnsRemaining <= 0 ) {
				gameComplete = true;
			}
			break;
		}

		return gameComplete;
	}

	public GameSession GetCurrentGameSession() {
		return _gameSession;
	}

#region EventHandlers
	public void HandleOnTilesSwapped() {
		_gameSession.TurnsRemaining--;

		_gameHud.UpdateHUD();

		bool gameComplete = CheckGameComplete();
		if ( gameComplete ) {
			CleanupGame();
		}
	}

	public void HandleOnTilesMatched( List<Tile> matches ) {

	}
#endregion

#region Debug
	private void OnGUI() {
		if ( _gameSession == null ) {
			windowRect = GUILayout.Window( _currentWindowId, windowRect, DrawMenu, "Rogue3" );
			windowRect.x = (int) ( Screen.width * 0.5f - windowRect.width * 0.5f );
			windowRect.y = (int) ( Screen.height * 0.5f - windowRect.height * 0.5f );
			GUILayout.Window( _currentWindowId, windowRect, DrawMenu, "Rogue3" );
		}
	}
	
	private void DrawMenu( int windowId ) {
		GUILayout.BeginVertical();
		{
			if ( GUILayout.Button( "START", GUILayout.Height(100f) ) ) {
				List<TileData> tileData = _tileDataManager.GetAllTileData();
				StartGame( tileData );
			}
		}
		GUILayout.EndVertical();
	}

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
