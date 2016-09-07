using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

/// <summary>
/// BoardManager
/// 
/// Handles board/tile state
/// </summary>
public partial class GameBoard : MonoBehaviour {
	
	// Data class that represents a swap
	public class Swap {
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

	// Tiles that are pending a tween to drop down to its correct location ( visually )
	private List<Tile> _droppingTiles = new List<Tile>();

	private List<Tile> _dirtyTiles = new List<Tile>();

	// The matches that have been made, currently
	private List<List<Tile>> _matches = new List<List<Tile>>();

	private List<Tile> _expiringObstructions = new List<Tile>();

	private bool _isGameOver = false;

	private Battle _battle = null;

	private TileRecipe _recipe = null;

	private GameHud _gameHud = null;

	private GameStateMachine _gameStateMachine = null;

#region PsuedoRandom Tile Picking
	private int _randomIndex = 0;
	private int[] _psuedoRandomIndices = new int[] { 
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
		2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }; 

	private void ShufflePsuedoRandomIndices() {
		_randomIndex = 0;

		for ( int i = _psuedoRandomIndices.Length - 1; i > 0; i-- ) {
			int randomIndex = UnityEngine.Random.Range( 0, i );
			int tmp = _psuedoRandomIndices[ i ];
			_psuedoRandomIndices[ i ] = _psuedoRandomIndices[ randomIndex ];
			_psuedoRandomIndices[ randomIndex ] = tmp;
		}
	}

	private BaseTileData PickRandomTileData() {
		if ( _randomIndex >= _psuedoRandomIndices.Length ) {
			ShufflePsuedoRandomIndices();
		}
		return _equippedTileData[ _psuedoRandomIndices[_randomIndex++] ];
	}
#endregion

	private void Awake() {
		GameBoardGestureHandler.onSelectTile += HandleOnSelectTile;
		GameBoardGestureHandler.onDragTile += HandleOnDragTile;
		GameBoardGestureHandler.onDropTile += HandleOnDropTile;

		_gameStateMachine = new GameStateMachine();
		_gameStateMachine.Initialize( this );
	}

	private void OnDestroy() {
		GameBoardGestureHandler.onSelectTile -= HandleOnSelectTile;
		GameBoardGestureHandler.onDragTile -= HandleOnDragTile;
		GameBoardGestureHandler.onDropTile -= HandleOnDropTile;
	}

	public void Initialize( List<WeaponTileData> tileData, Battle battle, GameHud gameHud, TileRecipe recipe ) {

		_equippedTileData = tileData;
		_battle = battle;
		_gameHud = gameHud;
		_recipe = recipe;

		_gameHud.SetupRecipeChargeHud( this, _recipe, RecipeSkillButtonTapped );

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


	private void RecipeSkillButtonTapped() {
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.ActivateRecipeSkill );
	}


	public void ActivateRecipeSkill() {
		// TODO ayuen any other stuff
		_recipe.ActivateRecipeSkill( this );
		PerformDirtyTileCleanup();
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.CreatedAdditionalMatches );
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
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.ProceedToInput );
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

		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.ProceedToInput );
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
		while ( --wSearch >= 0 && _board[ wSearch, h ] != null && _board[ wSearch, h ].Matches( movedTile ) ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}
		wSearch = w;
		// Check right
		while ( ++wSearch < BOARD_WIDTH && _board[ wSearch, h ] != null && _board[ wSearch, h ].Matches( movedTile ) ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}

		// Check Vertical matches
		verticalMatches = new List<Tile>();
		verticalMatches.Add( movedTile );
		// Check bottom
		while ( --hSearch >= 0 && _board[ w, hSearch ] != null && _board[ w, hSearch ].Matches( movedTile ) ) {
			verticalMatches.Add( _board[ w, hSearch ] );
		}
		hSearch = h;
		// Check top
		while ( ++hSearch < BOARD_HEIGHT && _board[ w, hSearch ] != null && _board[ w, hSearch ].Matches( movedTile ) ) {
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
				"time", TuningData.Instance.SwapAnimationTime,
				"oncompletetarget", gameObject,
				"oncomplete", "UndoSwapAnimationComplete"
	        )
		);
	}

	public void UndoSwapAnimationComplete() {
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.InvalidTileDrop );
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
				"time", TuningData.Instance.SwapAnimationTime,
				"oncompletetarget", gameObject,
				"oncomplete", "SwapAnimationComplete"
			)
		);
		
		// Tween the selected tile
		_swap.SelectedTile.StartSwapping();
		Vector3 selectedTilePos = CoordsToWorldPosition( _swap.SelectedTile.X, _swap.SelectedTile.Y );
		iTween.MoveTo( _swap.SelectedTile.gameObject, 
			iTween.Hash( "position", selectedTilePos, 
				"easetype", iTween.EaseType.easeOutQuart, 
				"time", TuningData.Instance.SwapAnimationTime
			)
		);
	}


	public void SwapAnimationComplete() {
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.ValidTileDrop );
	}


	private void PerformObstructionExpiration() {
		StartCoroutine( PerformObstructionExpirationCoroutine() );
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

		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.ProcessedAllMatches );
	}

	private void PerformMatch() {		
		StartCoroutine( PerformMatchCoroutine() );
	}


	//private void 

	private IEnumerator PerformMatchCoroutine() {		
		while ( _matches.Count > 0 ) {
			for ( int i = 0, count = _matches.Count; i < count; i++ ) {
				
				// Examine each match, they should trigger some sort of skill

				List<Tile> match = _matches[ i ];

				if ( _initialMatch ) {
					Tile firstTile = match[ 0 ];
					List<BaseWeaponSkillData> weaponSkills = firstTile.GetWeaponSkills();
					for ( int j = 0, countj = weaponSkills.Count; j < countj; j++ ) {
						yield return weaponSkills[ j ].PerformWeaponSkill( this, _battle, match, _swap );
					}
				}
					
				RaiseOnTilesMatched( match );

				for ( int j = match.Count - 1; j >= 0; j-- ) {
					Tile tile = match[ j ];
					ClearTile( tile );
				}

				yield return new WaitForSeconds( TuningData.Instance.MatchTimeDelay );
			}
			_matches.Clear();

			// TODO
			_initialMatch = false;

			// TODO: check this in the weapon skill state
			PerformDirtyTileCleanup();
		}

		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.CheckObstructions );
	}

	private bool PerformDirtyTileCleanup() {

		// sanity check the dirtyTiles list
		for ( int i = _dirtyTiles.Count-1; i >= 0; i-- ) {
			if ( _dirtyTiles[ i ].IsMatching ) {
				_dirtyTiles.RemoveAt( i );
			}
		}


		bool matchesFound = CheckForCascadingMatches( _dirtyTiles );
		_dirtyTiles.Clear();
		if ( matchesFound ) {
			return true;
		} else {
			return false;
		}
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
										"easetype", TuningData.Instance.TileDropEaseType, 
										"speed", TuningData.Instance.TileDropSpeed,
							            "oncomplete", "FinishedDroppingAnimation",
							            "oncompletetarget", gameObject
			            )
			);
		}

		while ( isWaitingForDroppingTweenToComplete > 0 ) {
			yield return new WaitForEndOfFrame();
		}

		if ( isWaitingForDroppingTweenToComplete == 0 ) {
		_gameStateMachine.TriggerTransition( GameStateMachine.Transition.NewTilesDropped );
		}
	}

	private void PerformCleanup() {

		if ( CheckForCascadingMatches( _droppingTiles ) ) {
			_gameStateMachine.TriggerTransition( GameStateMachine.Transition.CascadingMatchesFound );
		} else {
			_gameStateMachine.TriggerTransition( GameStateMachine.Transition.EndTurn );
		}

	}

	private bool CheckForCascadingMatches( List<Tile> tilesToCheck ) {
	
		for ( int i = 0, count = tilesToCheck.Count; i < count; i++ ) {
			List<Tile> horizontalTiles = new List<Tile>();
			List<Tile> verticalTiles = new List<Tile>();

			Tile tileToCheck = tilesToCheck[ i ];

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
												"easetype", TuningData.Instance.TileClearEaseType,
												"time", TuningData.Instance.TileClearTime );
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

	public bool IsObstructionsOnGameBoard() {
		return _expiringObstructions.Count > 0;
	}

	public void DestroyRandomObstruction() {
		int randomIndex = UnityEngine.Random.Range( 0, _expiringObstructions.Count );
		ClearTile( _expiringObstructions[ randomIndex ] );
		_expiringObstructions.RemoveAt( randomIndex );
	}

	public void ExtendMatch( List<Tile> match ) {

		// find out if we need to extend vertically or horizontally
		bool isHorizontal = match[ 0 ].Y == match[ 1 ].Y;

		if ( isHorizontal ) {
			int y =  match[ 0 ].Y;
			for ( int x = 0; x < BOARD_WIDTH; x++ ) {
				Tile tile = _board[ x, y ];
				if ( tile != null ) {
					if ( !match.Contains( tile ) ) {
						ClearTile( tile );
					}
				}
			}
		}
		else {
			int x =  match[ 0 ].X;
			for ( int y = 0; y < BOARD_WIDTH; y++ ) {
				Tile tile = _board[ x, y ];
				if ( tile != null ) {
					if ( !match.Contains( tile ) ) {
						ClearTile( tile );
					}
				}
			}
		}
	}

	public void ClearRow( int y ) {
		List<Tile> match = new List<Tile>();
		for ( int x = 0, count = BOARD_WIDTH; x < count; x++ ) {
			match.Add( _board[ x, y ] );
		}
		_matches.Add( match );
	}

	private bool TileIsMatched( Tile tile ) {
		bool found = false;
		for ( int i = 0, count = _matches.Count; i < count; i++ ) { 
			for ( int j = 0, countj = _matches[ i ].Count; j < countj; j++ ) {
				if ( _matches[ i ][ j ] == tile ) {
					found = true;
					break;
				}
			}
		}
		return found;
	}

	public void ClearAdjacentTiles( Tile tile, TileSearchCondition condition ) {
		int x = tile.X;
		int y = tile.Y;

		if ( x < BOARD_WIDTH-1 && condition( _board[ x+1, y ] ) ) {
			if ( !TileIsMatched( _board[ x+1, y ] ) ) {
				ClearTile( _board[ x+1, y ] );
			}
		}
		if ( x > 0 && condition( _board[ x-1, y ] ) ) {
			if ( !TileIsMatched( _board[ x-1, y ] ) ) {
				ClearTile( _board[ x-1, y ] );
			}
		}
		if ( y < BOARD_HEIGHT-1 && condition( _board[ x, y+1 ] ) ) {
			if ( !TileIsMatched( _board[ x, y+1 ] ) ) {
				ClearTile( _board[ x, y+1 ] );
			}
		}
		if ( y > 0 && condition( _board[ x, y-1 ] ) ) {
			if ( !TileIsMatched( _board[ x, y-1 ] ) ) {
				ClearTile( _board[ x, y-1 ] );
			}
		}

	}

	public void ClearNRandomTiles( int n, TileSearchCondition condition ) {

		// first find a set of tiles that do not match the tiledata passed in
		List<Tile> eligibleTiles = FindNRandomTiles( n, condition );


		// pick n tiles out of the list to replace
		int numReplaced = 0;
		while ( eligibleTiles.Count > 0 && numReplaced < n ) {
			int randIndex = UnityEngine.Random.Range( 0, eligibleTiles.Count );
			Tile tile = eligibleTiles[ randIndex ];

			if ( !TileIsMatched( tile ) ) {
				ClearTile( tile );
			}

			numReplaced++;
			eligibleTiles.RemoveAt( randIndex );
		}
	}

	public void ReplaceNRandomTiles( int n, BaseTileData data, TileSearchCondition condition ) {		

		// first find a set of tiles that do not match the tiledata passed in
		List<Tile> eligibleTiles = FindNRandomTiles( n, condition );

		// pick n tiles out of the list to replace
		int numReplaced = 0;
		while ( eligibleTiles.Count > 0 && numReplaced < n ) {
			int randIndex = UnityEngine.Random.Range( 0, eligibleTiles.Count );
			Tile tile = eligibleTiles[ randIndex ];
			tile.Initialize( data, tile.X, tile.Y );

			numReplaced++;
			eligibleTiles.RemoveAt( randIndex );

			if ( !_dirtyTiles.Contains( tile ) ) {
				_dirtyTiles.Add( tile );
			}
		}
	}

	public delegate bool TileSearchCondition( Tile tile );

	public List<Tile> FindNRandomTiles( int n, TileSearchCondition condition ) {
		
		List<Tile> eligibleTiles = new List<Tile>();
		for ( int x = 0; x < BOARD_WIDTH; x++ ) {
			for ( int y = 0; y < BOARD_HEIGHT; y++ ) {

				if ( _board[ x, y ] == null ) {
					continue;
				}

				if ( condition == null || ( condition != null && condition( _board[ x, y ] ) ) ) {
					eligibleTiles.Add( _board[ x, y ] );
				}
			}
		}

		return eligibleTiles;
	}

#region BoardGestureManager event handlers
	void HandleOnSelectTile( Tile tile ) {
		if ( _isGameOver || !_gameStateMachine.CheckCurrentState( GameStateMachine.State.Input ) ) return;

		_swap = new Swap {
			SelectedTile = tile
		};
	}

	void HandleOnDragTile( Vector3 dragPosition ) {
		if ( _isGameOver || !_gameStateMachine.CheckCurrentState( GameStateMachine.State.Input ) ) return;

		if ( _swap != null && _swap.SelectedTile != null ) {
			_swap.SelectedTile.transform.position = dragPosition;
		}
	}

	bool _initialMatch = false;

	void HandleOnDropTile( Tile targetTile ) {
		if ( _isGameOver || !_gameStateMachine.CheckCurrentState( GameStateMachine.State.Input ) ) return;

		if ( targetTile != null ) {
			_swap.TargetTile = targetTile;
			_gameStateMachine.TriggerTransition( GameStateMachine.Transition.TileDropped );

			// TODO
			_initialMatch = true;
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
