using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BoardManager
/// 
/// Handles board/tile state
/// </summary>
using System.Text;


public class GameBoard : MonoBehaviour {

	// Data class that represents a swap
	private class Swap {
		public Tile SelectedTile = null;
		public Tile TargetTile = null;

		public List<Tile> SelectedHorizontalMatches = new List<Tile>();
		public List<Tile> SelectedVerticalMatches = new List<Tile>();

		public List<Tile> TargetHorizontalMatches = new List<Tile>();
		public List<Tile> TargetVerticalMatches = new List<Tile>();
	}

	public const int BOARD_WIDTH = 6;
	public const int BOARD_HEIGHT = 6;

	[SerializeField]
	private GameObject TilePrefab;

	[SerializeField]
	private GameBoardGestureHandler GameBoardGestureHandler;

	[SerializeField]
	private Transform TilesContainer;

	// Game states
	private enum State {
		Input, // User input state, may also show hints during this state in the future
		Swap, // User input has occured, checking for a valid swap
		SwapAnimating, // Idle state where we wait for swap tweens to complete
		UndoSwapAnimating, // Idle state where we wait for UNDO swap tweens to complete
		Match, // State to clear out all matches and wait for match tweens
		ExpireObstructions,
		DropAndFill, // Fix data so that we have a complete board once more
		DropAnimating, // Idle state to wait for tiles to visually drop into place
		Cleanup, // Check for additional matches, either brings us back to Match or to the TurnEnd
		TurnEnd
	}

	// Delegates
	public delegate void OnTilesMatchedDelegate( List<Tile> matches );
	public OnTilesMatchedDelegate OnTilesMatched;
	private void RaiseOnTilesMatched( List<Tile> matches ) { if ( OnTilesMatched != null ) OnTilesMatched( matches ); }

	public delegate void OnTurnEndedDelegate();
	public OnTurnEndedDelegate OnTurnEnded;
	private void RaiseOnTurnEnded() { if ( OnTurnEnded != null ) OnTurnEnded(); }


	// Data representation of the board
	private Tile[,] _board = new Tile[ BOARD_WIDTH, BOARD_HEIGHT ];

	// The list of tiles equipped for this stage
	private List<WeaponTileData> _equippedTileData;

	// Any current swaps ( created by user input )
	private Swap _swap = null;

	// The game state
	private State _state = State.Input;

	// Tiles that are pending a tween to drop down to its correct location ( visually )
	private List<Tile> _droppingTiles = new List<Tile>();

	// The matches that have been made, currently
	private List<List<Tile>> _matches = new List<List<Tile>>();

	private List<Tile> _expiringObstructions = new List<Tile>();

	private bool _isGameOver = false;

	private Battle _battle = null;

	private void Awake() {
		GameBoardGestureHandler.onSelectTile += HandleOnSelectTile;
		GameBoardGestureHandler.onDragTile += HandleOnDragTile;
		GameBoardGestureHandler.onDropTile += HandleOnDropTile;
	}

	private void OnDestroy() {
		GameBoardGestureHandler.onSelectTile -= HandleOnSelectTile;
		GameBoardGestureHandler.onDragTile -= HandleOnDragTile;
		GameBoardGestureHandler.onDropTile -= HandleOnDropTile;
	}

	private bool _holdStateChanges = false;

	/// <summary>
	/// Basic Game Loop
	/// </summary>
	private void Update() {

		// If the game is over or we dont want state changes or logic to be done, we should return
		if ( _isGameOver || _holdStateChanges == true ) {
			return;
		}

		switch( _state ) {
			// On the input state, we are waiting for the player to perform a swap
			// Only state where user input is valid
			case State.Input:
				// TODO: show hints after some time?
				// Wait until we recieve a drop tile event
				break;
			// A selection has been made, the player has dropped the tile, try to swap
			case State.Swap:
				PerformSwap();
				
				bool selectedTileMatched = CheckForMatchAtCoords( _swap.SelectedTile, _swap.SelectedTile.X, _swap.SelectedTile.Y, out _swap.SelectedHorizontalMatches, out _swap.SelectedVerticalMatches );
				bool targetTileMatched = CheckForMatchAtCoords( _swap.TargetTile, _swap.TargetTile.X, _swap.TargetTile.Y, out _swap.TargetHorizontalMatches, out _swap.TargetVerticalMatches );
				
				// If there are matches at either the target or selected tile locations, we need to swap
				if ( selectedTileMatched || targetTileMatched ) {
					
					_matches.Clear();

					if ( _swap.SelectedHorizontalMatches.Count > 0 ) _matches.Add( _swap.SelectedHorizontalMatches );
					if ( _swap.SelectedVerticalMatches.Count > 0 ) _matches.Add( _swap.SelectedVerticalMatches );
					if ( _swap.TargetHorizontalMatches.Count > 0 ) _matches.Add( _swap.TargetHorizontalMatches );
					if ( _swap.TargetVerticalMatches.Count > 0 ) _matches.Add( _swap.TargetVerticalMatches );

					PerformSwapAnimation();
					_state = State.SwapAnimating;
				}
				// Otherwise undo the swap
				else {
					PerformUndoSwap();
					PerformUndoSwapAnimation();
					_state = State.UndoSwapAnimating;
				}
				break;
			// A successful swap has occured, we need to wait until the swap tweens are complete
			case State.SwapAnimating:
				if ( !_swap.SelectedTile.IsSwapping && !_swap.TargetTile.IsSwapping ) {
					_state = State.Match;
				}
				break;
			// An unsuccessful swap has occured, the dropped tile is tweening back to its original position
			// return to user input once its complete
			case State.UndoSwapAnimating:
				if ( !_swap.SelectedTile.IsSwapping ) {
					_state = State.Input;
				}
				break;
			// Successful swap has occured, the tweens are done, we need to clear out the matches
			case State.Match:
				PerformMatch();
				_state = State.ExpireObstructions;
				break;
			case State.ExpireObstructions:
				PerformObstructionExpiration();
				_state = State.DropAndFill;
				break;
			// The matching tiles have been removed from data and visually
			// We need to drop existing tiles down ( in data ) and also fill up the missing spots ( in data )
			case State.DropAndFill:
				PerformDrop();
				PerformFill();	
				_state = State.DropAnimating;
				break;
			// The board state is correct, but visually, we need to drop the tiles down to the correct spots
			case State.DropAnimating:
				PerformDroppingAnimation();
				_state = State.Cleanup;
				break;
			// This step will check every tile that has been dropped for matches
			// If there are any matches, we want to return to the match state, otherwise we can continue back to the user input state
			case State.Cleanup:
				if ( CheckForCascadingMatches() ) {
					_state = State.Match;
				} else {
					_state = State.TurnEnd;
				}
				break;
			// The board manager idles in this state until the gamemanager tells us we can move
			case State.TurnEnd:
				RaiseOnTurnEnded();
				break;
		}
	}

	public void Initialize( List<WeaponTileData> tileData, Battle battle ) {

		_equippedTileData = tileData;
		_battle = battle;

		// 0, 0 is the bottom left corner tile
		for ( int w = 0; w < BOARD_WIDTH; w++ ) {
			for ( int h = 0; h < BOARD_HEIGHT; h++ ) {

				BaseTileData data = PickRandomTileData();
				// This can be done in a less dumb way later
				while ( CheckForMatchAtCoordsOnInitializing( data, w, h ) ) {
					data = PickRandomTileData();
				}
				CreateTileAtCoords( data, w, h );
			}
		}
	}

	public void ClearBoard() {
		for ( int w = 0; w < BOARD_WIDTH; w++ ) {
			for ( int h = 0; h < BOARD_HEIGHT; h++ ) {
				if ( _board[ w, h ] != null ) {
					GameObject.Destroy( _board[ w, h ].gameObject );
				}
			}
		}
	}

	public void ContinueToInput() {
		_state = State.Input;
	}

	public void GameComplete() {
		_isGameOver = true;
	}

	public void ProcessEnemyAttack( List<EnemyAttackDataSet.EnemyAttackData> attacks ) {

		for ( int i = 0, count = attacks.Count; i < count; i++ ) {
			EnemyAttackDataSet.EnemyAttackData attack = attacks[ i ];

			ObstructionTileData obstructionData = attack.ObstructionTile;
			Tile tile = _board[ attack.X, attack.Y ];
			ClearTile( tile, false /* dont tween out */ );
			Tile obstructionTile = CreateTileAtCoords( obstructionData, attack.X, attack.Y, true /* tweens in */ );

			if ( obstructionTile.TurnsTilExpired != ObstructionTileData.NEVER_EXPIRES ) {
				_expiringObstructions.Add( obstructionTile );
			}
		}

		_state = State.Input;
	}


	private BaseTileData PickRandomTileData() {
		// simple 
		int index = UnityEngine.Random.Range( 0, _equippedTileData.Count );
		return _equippedTileData[ index ];
	}
	
	private Tile CreateTileAtCoords( BaseTileData data, int xCoords, int yCoords, bool tweensIn = false ) {		
		GameObject tileGO = GameObject.Instantiate( TilePrefab );
		Tile tile = tileGO.GetComponent<Tile>();
		tile.Initialize( data, xCoords, yCoords );

		tileGO.transform.SetParent( TilesContainer );
		tileGO.transform.position =  CoordsToWorldPosition( xCoords, yCoords );

		if ( tweensIn ) {
			tileGO.transform.localScale = Vector3.zero;
			iTween.ScaleTo ( tile.gameObject,
				iTween.Hash( "scale", Vector3.one,
					"easetype", iTween.EaseType.easeOutBack,
					"time", 0.5f
				)
			);
		}
		
		_board[ xCoords, yCoords ] = tile;

		return tile;
	}

	private Tile CreateTileAtCoordsforDropping( BaseTileData data, int xCoords, int yCoords, int yCoordsFake ) {
		GameObject tileGO = GameObject.Instantiate( TilePrefab );
		Tile tile = tileGO.GetComponent<Tile>();
		tile.Initialize( data, xCoords, yCoords );

		tileGO.transform.SetParent( TilesContainer );
		tileGO.transform.position =  CoordsToWorldPosition( xCoords, yCoordsFake );
		
		_board[ xCoords, yCoords ] = tile;

		return tile;
	}

	private bool CheckForMatchAtCoordsOnInitializing( BaseTileData data, int w, int h ) {
		// The board is filled from bottom left to top right ( column first, then row )
		// So we only need to search to left and bottom
		// We also only care that a match happened, so that we try different data

		// Horizontal checks only need to be done if the column is past 1 
		if ( w > 1 ) {
			int wSearch = w;
			int found = 0;
			while( --wSearch >= 0 && _board[ wSearch, h ].Matches( data ) ) {
				found++;
			}
			// 2 because there are two others that match the tile
			if ( found >= 2 ) {
				return true;
			}
		}

		// Vertical checks only need to be done if the row is past 1 
		if ( h > 1 ) {
			int hSearch = h;
			int found = 0;
			while( --hSearch >= 0 && _board[ w, hSearch ].Matches( data ) ) {
				found++;
			}

			// 2 because there are two others that match the tile
			if ( found >= 2 ) {
				return true;
			}
		}

		return false;
	}

	private bool CheckForMatchAtCoords( Tile movedTile, int w, int h, out List<Tile> horizontalMatches, out List<Tile> verticalMatches ) {
	
		int wSearch = w;
		int hSearch = h;

		// Check Horizontal matches
		horizontalMatches = new List<Tile>();
		horizontalMatches.Add( movedTile );
		// Check left
		while ( --wSearch >= 0 && _board[ wSearch, h ].Matches( movedTile ) ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}
		wSearch = w;
		// Check right
		while ( ++wSearch < BOARD_WIDTH && _board[ wSearch, h ].Matches( movedTile ) ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}

		// Check Vertical matches
		verticalMatches = new List<Tile>();
		verticalMatches.Add( movedTile );
		// Check bottom
		while ( --hSearch >= 0 && _board[ w, hSearch ].Matches( movedTile ) ) {
			verticalMatches.Add( _board[ w, hSearch ] );
		}
		hSearch = h;
		// Check top
		while ( ++hSearch < BOARD_HEIGHT && _board[ w, hSearch ].Matches( movedTile ) ) {
			verticalMatches.Add( _board[ w, hSearch ] );
		}

		if ( horizontalMatches.Count < 3 ) horizontalMatches.Clear();
		if ( verticalMatches.Count < 3 ) verticalMatches.Clear();

		return ( horizontalMatches.Count >= 3 || verticalMatches.Count >= 3 );
	}

	private void PerformUndoSwap() {

		if ( _swap.TargetTile != null ) {
			// Swap the coords
			int selectedX =  _swap.TargetTile.X;
			int selectedY =  _swap.TargetTile.Y;
			
			int targetX = _swap.SelectedTile.X;
			int targetY = _swap.SelectedTile.Y;
			
			// Update the tiles
			_swap.TargetTile.UpdateCoords( targetX, targetY );
			_swap.SelectedTile.UpdateCoords( selectedX, selectedY );
			
			// Update the board
			_board[ _swap.TargetTile.X, _swap.TargetTile.Y ] = _swap.TargetTile;
			_board[ _swap.SelectedTile.X, _swap.SelectedTile.Y ] = _swap.SelectedTile;
		}
	}

	private void PerformUndoSwapAnimation() {
		// Tween the selected tile
		_swap.SelectedTile.StartSwapping();
		Vector3 selectedTilePos = CoordsToWorldPosition( _swap.SelectedTile.X, _swap.SelectedTile.Y );
		iTween.MoveTo( _swap.SelectedTile.gameObject, 
		              iTween.Hash( "position", selectedTilePos, 
		            "easetype", iTween.EaseType.easeOutQuart, 
		            "time", 0.25f,
		            "oncomplete", "StopSwapping"
		            )
		              );
	}

	private void PerformSwap() {
		// Swap the coords
		int selectedX =  _swap.TargetTile.X;
		int selectedY =  _swap.TargetTile.Y;

		int targetX = _swap.SelectedTile.X;
		int targetY = _swap.SelectedTile.Y;

		// Update the tiles
		_swap.TargetTile.UpdateCoords( targetX, targetY );
		_swap.SelectedTile.UpdateCoords( selectedX, selectedY );

		// Update the board
		_board[ _swap.TargetTile.X, _swap.TargetTile.Y ] = _swap.TargetTile;
		_board[ _swap.SelectedTile.X, _swap.SelectedTile.Y ] = _swap.SelectedTile;
	}

	private void PerformSwapAnimation() {
		// Tween the target tile
		_swap.TargetTile.StartSwapping();
		Vector3 targetTilePos = CoordsToWorldPosition( _swap.TargetTile.X, _swap.TargetTile.Y );
		iTween.MoveTo( _swap.TargetTile.gameObject, 
		              iTween.Hash( "position", targetTilePos, 
		            "easetype", iTween.EaseType.easeOutQuart, 
		            "time", 0.25f,
		            "oncomplete", "StopSwapping"
		            )
		              );
		
		// Tween the selected tile
		_swap.SelectedTile.StartSwapping();
		Vector3 selectedTilePos = CoordsToWorldPosition( _swap.SelectedTile.X, _swap.SelectedTile.Y );
		iTween.MoveTo( _swap.SelectedTile.gameObject, 
		              iTween.Hash( "position", selectedTilePos, 
		            "easetype", iTween.EaseType.easeOutQuart, 
		            "time", 0.25f,
		            "oncomplete", "StopSwapping"
		            )
		              );
	}

	private void PerformObstructionExpiration() {
		if ( _expiringObstructions.Count > 0 ) {
			_holdStateChanges = true;
			StartCoroutine( PerformObstructionExpirationCoroutine() );
		}
	}

	private IEnumerator PerformObstructionExpirationCoroutine() {		
		bool doDelay = false;
		for ( int i = _expiringObstructions.Count-1; i >= 0; i-- ) {
			if ( --_expiringObstructions[ i ].TurnsTilExpired <= 0 ) {
				ClearTile( _expiringObstructions[ i ] );
				doDelay = true;
			}
		}
		if ( doDelay ) {
			yield return new WaitForSeconds( 0.35f );
		}

		_holdStateChanges = false;
	}

	bool isPerformingMatch = false;
	private void PerformMatch() {
		_holdStateChanges = true;
		if ( !isPerformingMatch ) {
			StartCoroutine( PerformMatchCoroutine() );
		}
	}

	private IEnumerator PerformMatchCoroutine() {
		isPerformingMatch = true;
		for ( int i = 0, count = _matches.Count; i < count; i++ ) {


			// Examine each match, they should trigger some sort of skill
			TriggerMatchSkill( _matches[ i ] );

			RaiseOnTilesMatched( _matches[ i ] );

			for ( int j = _matches[ i ].Count - 1; j >= 0; j-- ) {
				Tile tile = _matches[ i ][ j ];
				ClearTile( tile );
			}

			yield return new WaitForSeconds( 0.35f );
		}
		_matches.Clear();

		_holdStateChanges = false;
		isPerformingMatch = false;
	}

	private void PerformFill() {
		// start from the top
		int h = BOARD_HEIGHT-1;
		for ( int w = 0; w < BOARD_WIDTH; w++ ) {
			if ( _board[ w, h ] == null ) {

				// search downwards and count the number of tiles we need to fill
				int hSearch = h;
				while( hSearch >= 0  ) {
					Tile tile = _board[ w, hSearch ];
					if ( tile != null ) {
						break;
					}
					hSearch--;
				}

				// once the bottom location is found, we need to iterate up to create the tiles	
				int offset = 1;
				hSearch += 1; // we ended at a non null h, we need to move up one
				while( hSearch < BOARD_HEIGHT ) {
					BaseTileData tileData = PickRandomTileData();
					Tile tileToDrop = CreateTileAtCoordsforDropping( tileData, w, hSearch, h+offset );
					_droppingTiles.Add( tileToDrop );
					offset++;
					hSearch++;
				}
			}
		}
	}

	private void PerformDrop() {
		_droppingTiles.Clear();

		for ( int h = 0; h < BOARD_HEIGHT; h++ ) {
			for ( int w = 0; w < BOARD_WIDTH; w++ ) {
				if ( _board[ w, h ] == null  ) {

					int hSearch = h;
					Tile droppingTile = null;

					// Search up from the empty spot to look for a tile that can drop
					while( hSearch < BOARD_HEIGHT  ) {
						droppingTile = _board[ w, hSearch ];
						// If we found a tile to drop
						if ( droppingTile != null ) {
							// clear the board data where the tile currently lies
							_board[ w, hSearch ] = null;
							break;
						}
						hSearch++;
					}	

					// Update the data, since we dropped the tile down
					if ( droppingTile != null ) {
						droppingTile.UpdateCoords( w, h );
						_board[ w, h ] = droppingTile;

						_droppingTiles.Add( droppingTile );
					}
				}
			}
		}
	}


	private void PerformDroppingAnimation() {
		_holdStateChanges = true;
		StartCoroutine( PerformDroppingAnimationCoroutine() );	
	}

	int isWaitingForDroppingTweenToComplete = 0;
	public void FinishedDroppingAnimation() {
		isWaitingForDroppingTweenToComplete--;
	}

	private IEnumerator PerformDroppingAnimationCoroutine() {

		isWaitingForDroppingTweenToComplete = _droppingTiles.Count;

		for ( int i = 0, count = _droppingTiles.Count; i < count; i++ ) {
			Tile tile = _droppingTiles[ i ];
			Vector3 position = CoordsToWorldPosition( tile.X, tile.Y );
			iTween.MoveTo( tile.gameObject, 
			              iTween.Hash( "position", position, 
			            "easetype", iTween.EaseType.linear, 
			            "speed", 6f,
			            "oncomplete", "FinishedDroppingAnimation",
			            "oncompletetarget", gameObject
			            )
			);
		}

		while ( isWaitingForDroppingTweenToComplete > 0 ) {
			yield return new WaitForEndOfFrame();
		}

		_holdStateChanges = false;
	}

	private bool CheckForCascadingMatches() {
	
		for ( int i = 0, count = _droppingTiles.Count; i < count; i++ ) {
			List<Tile> horizontalTiles = new List<Tile>();
			List<Tile> verticalTiles = new List<Tile>();

			Tile tileToCheck = _droppingTiles[ i ];

			// Ignore things that are not matchable
			if ( !tileToCheck.IsMatchable() ) {
				continue;
			}

			bool isMatched = CheckForMatchAtCoords( tileToCheck, tileToCheck.X, tileToCheck.Y, out horizontalTiles, out verticalTiles );

			if ( isMatched ) {
				List<Tile> totalMatches = new List<Tile>();
				if ( horizontalTiles.Count > 0 ) totalMatches.AddRange( horizontalTiles );
				if ( verticalTiles.Count > 0 ) totalMatches.AddRange( verticalTiles );
				if ( totalMatches.Count > 0 ) {

					totalMatches.Sort( delegate(Tile a, Tile b) {
						int compareX = a.X.CompareTo( b.X );
						if ( compareX == 0 ) {
							return a.Y.CompareTo( b.Y );
						}
						return compareX;
					} );


					// Assume there is a duplicate match, if there are any matches
					bool sameMatch = _matches.Count > 0;
					// Go through the current matchess
					for ( int n = 0; n < _matches.Count; n++ ) {

						sameMatch = true;

						List<Tile> match = _matches[ n ];
						for ( int m = 0; m < match.Count; m++ ) {
							if ( match[ m ] != totalMatches[ m ] ) {
								sameMatch = false;
								break;
							}
						}

						if ( sameMatch ) break;

					}
					if ( !sameMatch ) {
						_matches.Add( totalMatches );
					}
				}
			}
		}

		if ( _matches.Count > 0 ) {
			return true;
		}
		return false;
	}

	private void ClearTile( Tile tile, bool tweensOut = true, bool destroyOnComplete = false ) {
		if ( !tile.IsMatching ) {
			tile.IsMatching = true;
			if ( tweensOut ) {

				Hashtable hash = iTween.Hash( "scale", Vector3.zero,
				            "easetype", iTween.EaseType.easeInBack,
				            "time", 0.35f );
				if ( destroyOnComplete ) {
					hash.Add( "oncomplete", "MatchedComplete" );
				}

				iTween.ScaleTo ( tile.gameObject, hash );			
			}
			else {
				tile.MatchedComplete();
			}
			_board[ tile.X, tile.Y ] = null;
		}
	}
		
	public static Vector3 CoordsToWorldPosition( int xCoord, int yCoord ) {
		//TODO
		return new Vector3( xCoord, yCoord, Tile.Z_DEPTH );
	}

	private void TriggerMatchSkill( List<Tile> match ) {
		// Check out the first tile to see what tile skill to trigger
		Tile tile = match[ 0 ];
		List<BaseWeaponSkillData> weaponSkills = tile.GetWeaponSkills();
		for ( int i = 0, count = weaponSkills.Count; i < count; i++ ) {
			weaponSkills[ i ].PerformWeaponSkill( this, _battle, match );
		}
	}

	public bool IsObstructionsOnGameBoard() {
		return _expiringObstructions.Count > 0;
	}

	public void DestroyRandomObstruction() {
		int randomIndex = UnityEngine.Random.Range( 0, _expiringObstructions.Count );
		ClearTile( _expiringObstructions[ randomIndex ] );
		_expiringObstructions.RemoveAt( randomIndex );
	}

#region BoardGestureManager event handlers
	void HandleOnSelectTile( Tile tile ) {
		if ( _isGameOver || _state != State.Input ) return;

		_swap = new Swap {
			SelectedTile = tile
		};
	}

	void HandleOnDragTile( Vector3 dragPosition ) {
		if ( _isGameOver || _state != State.Input ) return;

		if ( _swap != null && _swap.SelectedTile != null ) {
			_swap.SelectedTile.transform.position = dragPosition;
		}
	}

	void HandleOnDropTile( Tile targetTile ) {
		if ( _isGameOver || _state != State.Input ) return;

		if ( targetTile != null ) {
			_swap.TargetTile = targetTile;
			_state = State.Swap;
		} 
		else {
			PerformUndoSwapAnimation();
		}
	}
#endregion

#region Debug
	public Tile[,] DebugGetBoard() {
		return _board;
	}
#endregion

}
