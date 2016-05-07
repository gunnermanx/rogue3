using UnityEngine;
using System.Collections;

public class DebugMenu : MonoBehaviour {

	private enum WindowIds {
		Options = 0,
		Board = 1,
		Tiles = 2
	}

	private static bool _isShown = false;
	public Rect windowRect = new Rect(20, 20, 120, 50);

	private int _currentWindowId = (int) WindowIds.Options;

	public static void ToggleDebugMenu() {
		_isShown = !_isShown;
	}

	void OnGUI() {
		if ( _isShown ) {
			windowRect = GUILayout.Window( _currentWindowId, windowRect, DrawMenu, "Debug", GUILayout.Width(400));
		}
	}

	void DrawMenu( int windowID ) {
		GUILayout.BeginHorizontal();
		if ( GUILayout.Button( "Main Menu" ) ) {
			_currentWindowId = (int)WindowIds.Options;
		}
		GUILayout.EndHorizontal();

		switch( windowID ) {
		case (int)WindowIds.Options:
			DrawOptions();
			break;
		case (int)WindowIds.Board:
			DrawBoard();
			break;
		case (int)WindowIds.Tiles:
			DrawTiles();
			break;
		}
	}

	void DrawOptions() {
		if ( GUILayout.Button( "Board" ) ) {
			_currentWindowId = (int)WindowIds.Board;
		}
		if ( GUILayout.Button( "Tiles" ) ) {
			_currentWindowId =(int) WindowIds.Tiles;
		}
	}

	void DrawBoard() {
		Tile[,] board = GameManager.Instance.DebugGetBoard();

		GUILayout.BeginVertical();
		{
			if ( GUILayout.Button( "Reset Board" ) ) {
				GameManager.Instance.ResetBoard();
			}

			GUILayout.BeginVertical("box");
			{
				for ( int h = TileConsts.BOARD_HEIGHT-1; h >= 0; h-- ) {
					GUILayout.BeginHorizontal();
					{
						GUILayout.Box( h.ToString(), GUILayout.Height(50f), GUILayout.Width(25f) );

						for ( int w = 0, count = TileConsts.BOARD_WIDTH; w < count; w++ ) {
							Tile tile = board[ w, h ];

							if ( tile != null ) {
								Color oldColor = GUI.color;
								GUI.color = tile.GetDebugColor();
								GUILayout.Box( tile.TileType.ToString(), GUILayout.Height(50f), GUILayout.Width(50f) );
								GUI.color = oldColor;
							}
							else {
								GUILayout.Box("NULL", GUILayout.Height(50f), GUILayout.Width(50f));
							}
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				{
					GUILayout.Box( string.Empty, GUILayout.Height(25f), GUILayout.Width(25f) );
					for ( int w = 0, count = TileConsts.BOARD_WIDTH; w < count; w++ ) {
						GUILayout.Box( w.ToString(), GUILayout.Height(25f), GUILayout.Width(50f) );
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndVertical();
	}

	void DrawTiles() {
	}
}
