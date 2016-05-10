using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	[SerializeField]
	private GameObject _boardManagerPrefab;

	[SerializeField]
	private GameObject _battleManagerPrefab;

	[SerializeField]
	private Database _database;

	[SerializeField]
	private PersistenceManager _persistenceManager;

	// Singleton Accessor
	private static GameManager _instance = null;
	public static GameManager Instance {
		get { return _instance; }
	}
	
	private BoardManager _boardManager;
	private BattleManager _battleManager;
	
	private int _currentWindowId = 0;

	private static readonly float WIDTH = 400;
	private static readonly float HEIGHT = 400;
	
	public Rect windowRect = new Rect( (Screen.width - WIDTH)/2, (Screen.height + HEIGHT)/2, 100, 100);

	private void Awake() {
		_instance = this;
		DontDestroyOnLoad( gameObject );
	}

	private void Start() {
		_persistenceManager.LoadPlayerData( OnPlayerBlobLoaded );
	}

	private void Update() {
		if ( Input.GetKeyUp(KeyCode.BackQuote) ) {
			DebugMenu.ToggleDebugMenu();
		}
	}

	private void OnPlayerBlobLoaded( PlayerBlob blob ) {
		Debug.Log( "gameManager: playerblob loaded" );
		SceneManager.LoadScene( "Main" );
		_loaded = true;
	}

	private void StartGame( List<WeaponTileData> tileData, BattleStageData stageData ) {
		// Initialize game board
		GameObject boardManagerGO = GameObject.Instantiate( _boardManagerPrefab );
		_boardManager = boardManagerGO.GetComponent<BoardManager>();
		_boardManager.InitializeBoard( tileData );
		_boardManager.OnTilesSwapped += HandleOnTilesSwapped;
		_boardManager.OnTilesMatched += HandleOnTilesMatched;
		_boardManager.OnTurnEnded += HandleOnTurnEnded;

		// Initialize battle manager
		GameObject battleManagerGO = GameObject.Instantiate( _battleManagerPrefab );
		_battleManager = battleManagerGO.GetComponent<BattleManager>();
		_battleManager.Initialize( stageData );

		// Initialize Game HUD
		GameHud.Instance.gameObject.SetActive( true );
	}

	private void CleanupGame() {
		_boardManager.OnTilesSwapped -= HandleOnTilesSwapped;
		_boardManager.OnTilesMatched -= HandleOnTilesMatched;
		_boardManager.OnTurnEnded -= HandleOnTurnEnded;

		Destroy( _boardManager.gameObject );
		Destroy( _battleManager.gameObject );

		GameHud.Instance.gameObject.SetActive( false );
	
		_gameStarted = false;
	}

#region EventHandlers
	public void HandleOnTilesSwapped() {
	}

	public void HandleOnTilesMatched( List<Tile> matches ) {
		// TODO 
		_battleManager.AttackEnemy( matches );
	}

	public void HandleOnTurnEnded() {
		List<EnemyAttackDataSet.EnemyAttackData> attacks = _battleManager.IncrementTurnAndGetEnemyAttack();

		bool gameComplete = _battleManager.IsBattleComplete();
		if ( gameComplete ) {
			// todo: determine win/loss, show appropriate result then cleanup
			CleanupGame();
		}

		// There is no attack from the enemy, notify the boardmanager to continue to input
		if ( attacks == null ) {
			_boardManager.ContinueToInput();
		}
		// Otherwise we need to attack the board
		else {
			_boardManager.ProcessEnemyAttack( attacks );
		}
	}
#endregion

#region Debug

	bool _gameStarted = false;
	bool _loaded = false;

	private void OnGUI() {
		if ( _loaded && !_gameStarted ) {
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

				// TODO
				List<WeaponTileData> data = GetStartingWeaponTileData();
				BattleStageData stageData = _database.GetRandomTestBattleStageData();

				StartGame( data, stageData );

				_gameStarted = true;
			}
		}
		GUILayout.EndVertical();
	}

	private List<WeaponTileData> GetStartingWeaponTileData() {	
		List<WeaponTileData> data = new List<WeaponTileData>();
		data.Add( _database.GetWeaponTileData( "WoodenAxe" ) );
		data.Add( _database.GetWeaponTileData( "WoodenBow" ) );
		data.Add( _database.GetWeaponTileData( "WoodenSword" ) );
		data.Add( _database.GetWeaponTileData( "WoodenStaff" ) );
		return data;
	}

	public Tile[,] DebugGetBoard() {
		return _boardManager.DebugGetBoard();
	}

	public void ResetBoard() {
		_boardManager.ClearBoard();
		List<WeaponTileData> tileData = GetStartingWeaponTileData();
		_boardManager.InitializeBoard( tileData );
	}
#endregion
}
