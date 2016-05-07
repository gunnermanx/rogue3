using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	private TileData _tileData = null;

	public const float Z_DEPTH = 0f;

	private int _xCoord;
	public int X { get { return _xCoord; } }

	private int _yCoord;
	public int Y { get { return _yCoord; } }

	private bool _isSwapping = false;
	public bool IsSwapping { get { return _isSwapping; } }

	private bool _isMatching = false;
	public bool IsMatching { get { return _isMatching; } set { _isMatching = value; } }

	public void Initialize( TileData data, int xCoord, int yCoord ) {
		_tileData = data;

		_spriteRenderer.sprite = data.Sprite;
		_spriteRenderer.color = GetDebugColor();

		_xCoord = xCoord;
		_yCoord = yCoord;
	}

	public Color GetDebugColor() {
		Color color = Color.white;
		switch( _tileData.Type ) {
		case TileData.TileType.Axe:
			color = Color.red;
			break;
		case TileData.TileType.Staff:
			color = Color.blue;
			break;
		case TileData.TileType.Sword:
			color = Color.green;
			break;
		case TileData.TileType.Bow:
			color = Color.yellow;
			break;
		}
		return color;
	}

	public TileData.TileType TileType {
		get { return _tileData.Type; }
	}

	public void UpdateCoords( int xCoord, int yCoord ) {
		_xCoord = xCoord;
		_yCoord = yCoord;
	}

	public void StartSwapping() {
		_isSwapping = true;
	}

	public void StopSwapping() {
		_isSwapping = false;
	}

	public void MatchedComplete() {
		GameObject.Destroy( gameObject );
	}

	public override string ToString() {
		return "Tile: " + _tileData.Type.ToString() + " (" + _xCoord.ToString() + "," + _yCoord.ToString() + ")"; 		
	}
}
