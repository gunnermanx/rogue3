using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BoardManager
/// 
/// Handles board/tile state
/// </summary>
public class BoardManager : MonoBehaviour {

	// Data class that represents a swap
	private class Swap {
		public Tile SelectedTile = null;
		public Tile TargetTile = null;

		public List<Tile> SelectedHorizontalMatches = new List<Tile>();
		public List<Tile> SelectedVerticalMatches = new List<Tile>();

		public List<Tile> TargetHorizontalMatches = new List<Tile>();
		public List<Tile> TargetVerticalMatches = new List<Tile>();
	}

	[SerializeField]
	private GameObject TilePrefab;

	[SerializeField]
	private BoardGestureManager BoardGestureManager;

	[SerializeField]
	private Transform TilesContainer;

	// Game states
	private enum State {
		Input, // User input state, may also show hints during this state in the future
		Swap, // User input has occured, checking for a valid swap
		SwapAnimating, // Idle state where we wait for swap tweens to complete
		UndoSwapAnimating, // Idle state where we wait for UNDO swap tweens to complete
		Match, // State to clear out all matches and wait for match tweens
		DropAndFill, // Fix data so that we have a complete board once more
		DropAnimating, // Idle state to wait for tiles to visually drop into place
		Cleanup // Check for additional matches, either brings us back to Match or to Input
	}

	// Data representation of the board
	private Tile[,] _board = new Tile[ TileConsts.BOARD_WIDTH, TileConsts.BOARD_HEIGHT ];

	// The list of tiles equipped for this stage
	private List<TileData> _equippedTileData;

	// Any current swaps ( created by user input )
	private Swap _swap = null;

	// The game state
	private State _state = State.Input;

	// Tiles that are pending a tween to drop down to its correct location ( visually )
	private List<Tile> _droppingTiles = new List<Tile>();

	// The matches that have been made, currently
	private List<List<Tile>> _matches = new List<List<Tile>>();

	private void Awake() {
		BoardGestureManager.onSelectTile += HandleOnSelectTile;
		BoardGestureManager.onDragTile += HandleOnDragTile;
		BoardGestureManager.onDropTile += HandleOnDropTile;
	}

	private void OnDestroy() {
		BoardGestureManager.onSelectTile -= HandleOnSelectTile;
		BoardGestureManager.onDragTile -= HandleOnDragTile;
		BoardGestureManager.onDropTile -= HandleOnDropTile;
	}

	/// <summary>
	/// Basic Game Loop
	/// </summary>
	private void Update() {

		switch( _state ) {
			// On the input state, we are waiting for the player to perform a swap
			// Only state where user input is valid
			case State.Input:
				// TODO: show hints after some time?
				break;
			// A selection has been made, the player has dropped the tile, try to swap
			case State.Swap:
				PerformSwap();
				
				bool selectedTileMatched = CheckForMatchAtCoords( _swap.SelectedTile, _swap.SelectedTile.X, _swap.SelectedTile.Y, out _swap.SelectedHorizontalMatches, out _swap.SelectedVerticalMatches );
				bool targetTileMatched = CheckForMatchAtCoords( _swap.TargetTile, _swap.TargetTile.X, _swap.TargetTile.Y, out _swap.TargetHorizontalMatches, out _swap.TargetVerticalMatches );
				
				// If there are matches at either the target or selected tile locations, we need to swap
				if ( selectedTileMatched || targetTileMatched ) {
					
					_matches.Clear();
					List<Tile> selectedMatches = new List<Tile>();
					List<Tile> targetMatches = new List<Tile>();
					if ( _swap.SelectedHorizontalMatches.Count > 0 ) selectedMatches.AddRange( _swap.SelectedHorizontalMatches );
					if ( _swap.SelectedVerticalMatches.Count > 0 ) selectedMatches.AddRange( _swap.SelectedVerticalMatches );
					if ( _swap.TargetHorizontalMatches.Count > 0 ) targetMatches.AddRange( _swap.TargetHorizontalMatches );
					if ( _swap.TargetVerticalMatches.Count > 0 ) targetMatches.AddRange( _swap.TargetVerticalMatches );
					
					if ( selectedMatches.Count > 0 ) _matches.Add( selectedMatches );
					if ( targetMatches.Count > 0 ) _matches.Add( targetMatches );
					
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
				break;
			// This step will check every tile that has been dropped for matches
			// If there are any matches, we want to return to the match state, otherwise we can continue back to the user input state
			case State.Cleanup:
				PerformCleanup();
				break;
		}
	}

	public void InitializeBoard( List<TileData> tileData ) {

		_equippedTileData = tileData;

		// 0, 0 is the bottom left corner tile
		for ( int w = 0; w < TileConsts.BOARD_WIDTH; w++ ) {
			for ( int h = 0; h < TileConsts.BOARD_HEIGHT; h++ ) {

				TileData data = PickRandomTileData();
				// This can be done in a less dumb way later
				while ( CheckForMatchAtCoordsOnInitializing( data, w, h ) ) {
					data = PickRandomTileData();
				}
				CreateTileAtCoords( data, w, h );
			}
		}
	}

	public void ClearBoard() {
		for ( int w = 0; w < TileConsts.BOARD_WIDTH; w++ ) {
			for ( int h = 0; h < TileConsts.BOARD_HEIGHT; h++ ) {
				if ( _board[ w, h ] != null ) {
					GameObject.Destroy( _board[ w, h ].gameObject );
				}
			}
		}
	}

	private TileData PickRandomTileData() {
		// simple 
		int index = UnityEngine.Random.Range( 0, _equippedTileData.Count );
		return _equippedTileData[ index ];
	}

	private Tile CreateTileAtCoords( TileData data, int xCoords, int yCoords ) {		
		GameObject tileGO = GameObject.Instantiate( TilePrefab );
		Tile tile = tileGO.GetComponent<Tile>();
		tile.Initialize( data, xCoords, yCoords );

		tileGO.transform.SetParent( TilesContainer );
		tileGO.transform.position =  CoordsToWorldPosition( xCoords, yCoords );
		
		_board[ xCoords, yCoords ] = tile;

		return tile;
	}

	private Tile CreateTileAtCoordsforDropping( TileData data, int xCoords, int yCoords, int yCoordsFake ) {
		GameObject tileGO = GameObject.Instantiate( TilePrefab );
		Tile tile = tileGO.GetComponent<Tile>();
		tile.Initialize( data, xCoords, yCoords );

		tileGO.transform.SetParent( TilesContainer );
		tileGO.transform.position =  CoordsToWorldPosition( xCoords, yCoordsFake );
		
		_board[ xCoords, yCoords ] = tile;

		return tile;
	}

	private bool CheckForMatchAtCoordsOnInitializing( TileData data, int w, int h ) {
		// The board is filled from bottom left to top right ( column first, then row )
		// So we only need to search to left and bottom
		// We also only care that a match happened, so that we try different data

		// Horizontal checks only need to be done if the column is past 1 
		if ( w > 1 ) {
			int wSearch = w;
			int found = 0;
			while( --wSearch >= 0 && data.Type == _board[ wSearch, h ].TileType ) {
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
			while( --hSearch >= 0 && data.Type == _board[ w, hSearch ].TileType ) {
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
		while ( --wSearch >= 0 && _board[ wSearch, h ].TileType == movedTile.TileType ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}
		wSearch = w;
		// Check right
		while ( ++wSearch < TileConsts.BOARD_WIDTH && _board[ wSearch, h ].TileType == movedTile.TileType ) {
			horizontalMatches.Add( _board[ wSearch, h ] );
		}

		// Check Vertical matches
		verticalMatches = new List<Tile>();
		verticalMatches.Add( movedTile );
		// Check bottom
		while ( --hSearch >= 0 && _board[ w, hSearch ].TileType == movedTile.TileType ) {
			verticalMatches.Add( _board[ w, hSearch ] );
		}
		hSearch = h;
		// Check top
		while ( ++hSearch < TileConsts.BOARD_HEIGHT && _board[ w, hSearch ].TileType == movedTile.TileType ) {
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
		// Tween Swap
		//Debug.Log( "TARGET: " + _swap.TargetTile.ToString() );

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



	bool isPerformingMatch = false;
	private void PerformMatch() {
		if ( !isPerformingMatch ) {
			StartCoroutine( PerformMatchCoroutine() );
		}
	}

	private IEnumerator PerformMatchCoroutine() {
		isPerformingMatch = true;

		for ( int i = 0, count = _matches.Count; i < count; i++ ) {

			for ( int j = _matches[ i ].Count - 1; j >= 0; j-- ) {
				Tile tile = _matches[ i ][ j ];
				ClearTileAtCoords( tile );
			}

			yield return new WaitForSeconds( 0.35f );
		}
		_matches.Clear();

		isPerformingMatch = false;

		_state = State.DropAndFill;
	}

	private void PerformFill() {
		// start from the top
		int h = TileConsts.BOARD_HEIGHT-1;
		for ( int w = 0; w < TileConsts.BOARD_WIDTH; w++ ) {
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
				while( hSearch < TileConsts.BOARD_HEIGHT ) {
					TileData tileData = PickRandomTileData();
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

		for ( int h = 0; h < TileConsts.BOARD_HEIGHT; h++ ) {
			for ( int w = 0; w < TileConsts.BOARD_WIDTH; w++ ) {
				if ( _board[ w, h ] == null  ) {

					int hSearch = h;
					Tile droppingTile = null;

					// Search up from the empty spot to look for a tile that can drop
					while( hSearch < TileConsts.BOARD_HEIGHT  ) {
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

	bool isPerformingDroppingAnimation = false;
	private void PerformDroppingAnimation() {
		if ( !isPerformingDroppingAnimation ) {
			StartCoroutine( PerformDroppingAnimationCoroutine() );
		}
	}

	int isWaitingForDroppingTweenToComplete = 0;
	public void FinishedDroppingAnimation() {
		isWaitingForDroppingTweenToComplete--;
	}

	private IEnumerator PerformDroppingAnimationCoroutine() {

		isPerformingDroppingAnimation = true;

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

		isPerformingDroppingAnimation = false;

		_state = State.Cleanup;
	}

	private void PerformCleanup() {
	
		for ( int i = 0, count = _droppingTiles.Count; i < count; i++ ) {
			List<Tile> horizontalTiles = new List<Tile>();
			List<Tile> verticalTiles = new List<Tile>();

			Tile tileToCheck = _droppingTiles[ i ];
			bool isMatched = CheckForMatchAtCoords( tileToCheck, tileToCheck.X, tileToCheck.Y, out horizontalTiles, out verticalTiles );
			if ( isMatched ) {
				List<Tile> totalMatches = new List<Tile>();
				if ( horizontalTiles.Count > 0 ) totalMatches.AddRange( horizontalTiles );
				if ( verticalTiles.Count > 0 ) totalMatches.AddRange( verticalTiles );
				if ( totalMatches.Count > 0 ) _matches.Add( totalMatches );
			}
		}

		if ( _matches.Count > 0 ) {
			_state = State.Match;
		} else {
			_state = State.Input;
		}
	}

	private void ClearTileAtCoords( Tile tile ) {
		if ( !tile.IsMatching ) {
			tile.IsMatching = true;
			iTween.ScaleTo ( tile.gameObject,
			                iTween.Hash( "scale", Vector3.zero,
			            "easetype", iTween.EaseType.easeInBack,
			            "time", 0.5f,
			            "oncomplete", "MatchedComplete"
			            )
			                );
			//Debug.Log( "####Deleting " + tile.ToString() );
			_board[ tile.X, tile.Y ] = null;
		}
	}
		
	public static Vector3 CoordsToWorldPosition( int xCoord, int yCoord ) {
		//TODO
		return new Vector3( xCoord, yCoord, Tile.Z_DEPTH );
	}


	void HandleOnSelectTile( Tile tile ) {
		if ( _state != State.Input ) return;

		_swap = new Swap {
			SelectedTile = tile
		};
	}

	void HandleOnDragTile( Vector3 dragPosition ) {
		if ( _state != State.Input ) return;

		if ( _swap != null && _swap.SelectedTile != null ) {
			_swap.SelectedTile.transform.position = dragPosition;
		}
	}

	void HandleOnDropTile( Tile targetTile ) {
		if ( _state != State.Input ) return;

		if ( targetTile != null ) {
			_swap.TargetTile = targetTile;
			_state = State.Swap;
		} 
		else {
			PerformUndoSwapAnimation();
		}
	}

	#region Debug

	public Tile[,] DebugGetBoard() {
		return _board;
	}

	#endregion

}
