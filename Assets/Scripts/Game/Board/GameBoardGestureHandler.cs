using UnityEngine;
using System.Collections;

public class GameBoardGestureHandler : MonoBehaviour {

	private enum GestureState {
		None,
		DraggingTile
	}

	public LayerMask TileMask;


	private GestureState _currentState = GestureState.None;



	public delegate void OnSelectTileDelegate( Tile tile );
	public OnSelectTileDelegate onSelectTile;

	public delegate void OnDragTileDelegate( Vector3 dragPosition );
	public OnDragTileDelegate onDragTile;

	public delegate void OnDropTileDelegate( Tile targetTile );
	public OnDropTileDelegate onDropTile;

	private Collider _draggedCollider = null;


	private void Update() {

		if ( _currentState == GestureState.None ) {
			Vector3 position;
#if UNITY_EDITOR
			if ( Input.GetMouseButtonDown(0) ) {
				position = Input.mousePosition;
#else
			if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began ) {
				position = Input.GetTouch(0).position;
#endif
				Ray ray =  Camera.main.ScreenPointToRay( position );
				RaycastHit hit;
				if ( Physics.Raycast( ray, out hit, Mathf.Infinity, TileMask ) ) {
					Tile tile = hit.collider.gameObject.GetComponent<Tile>();
					if ( !tile.IsSelectable() ) {
						return;
					}

					if ( onSelectTile != null ) {
						onSelectTile( tile );
					}

					_draggedCollider = hit.collider;

					_currentState = GestureState.DraggingTile;
				}
			}
		}
		else if ( _currentState == GestureState.DraggingTile ) {
#if UNITY_EDITOR
			if ( Input.GetMouseButtonUp(0) ) {
				Vector3 position = Input.mousePosition;
#else
			if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended ) {
				Vector3 position = Input.GetTouch(0).position;
#endif
				Ray ray =  Camera.main.ScreenPointToRay( position );
				Tile targetTile = null;

				RaycastHit[] hits = Physics.RaycastAll( ray, Mathf.Infinity, TileMask );
				if ( hits != null ) {
					for ( int i = 0, count = hits.Length; i < count; i++ ) {
						if ( hits[ i ].collider != _draggedCollider ) {
							targetTile = hits[ i ].collider.gameObject.GetComponent<Tile>();
							if ( !targetTile.IsSelectable() ) {
								targetTile = null;
							}
							break;
						}
					}
				}
				_currentState = GestureState.None;
				_draggedCollider = null;

				if ( onDropTile != null ) {
					onDropTile( targetTile );
				}
			}
#if UNITY_EDITOR
			if ( Input.GetMouseButton(0) ) {
				Vector3 position = Input.mousePosition;
				position.z = Tile.Z_DEPTH - Camera.main.transform.position.z;
				Vector3 worldPos = Camera.main.ScreenToWorldPoint( position );
#else
			if ( Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved ) {
				Vector3 position = Input.GetTouch(0).position;
				position.z = Tile.Z_DEPTH - Camera.main.transform.position.z;
				Vector3 worldPos = Camera.main.ScreenToWorldPoint( position );
#endif
				
				if ( onDragTile != null ) {
					onDragTile( worldPos );
				}
			} 
		}
	}
}
