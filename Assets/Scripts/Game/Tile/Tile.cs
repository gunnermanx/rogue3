using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {

	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	private BaseTileData _tileData = null;

	public const float Z_DEPTH = 0f;

	private int _xCoord;
	public int X { get { return _xCoord; } }

	private int _yCoord;
	public int Y { get { return _yCoord; } }

	private bool _isSwapping = false;
	public bool IsSwapping { get { return _isSwapping; } }

	private bool _isMatching = false;
	public bool IsMatching { get { return _isMatching; } set { _isMatching = value; } }

	private int _turnsTilExpired = -1;
	public int TurnsTilExpired { get { return _turnsTilExpired; } set { _turnsTilExpired = value; } }

	public void Initialize( BaseTileData data, int xCoord, int yCoord ) {
		_tileData = data;

		_spriteRenderer.sprite = data.Sprite;
		_spriteRenderer.color = GetDebugColor();

		_xCoord = xCoord;
		_yCoord = yCoord;

		if ( data is ObstructionTileData ) {
			_turnsTilExpired = ( data as ObstructionTileData ).TurnsTilExpired;
		}
	}

	public List<BaseWeaponSkillData> GetWeaponSkills() {
		return _tileData.WeaponSkills;
	}

	public Color GetDebugColor() {
		Color color = Color.white;
		switch( _tileData.Type ) {
		case BaseTileData.TileType.Tomes:
			color = Color.blue;
			break;
		case BaseTileData.TileType.Sword:
			color = Color.green;
			break;
		case BaseTileData.TileType.Bow:
			color = Color.yellow;
			break;
		case BaseTileData.TileType.Mace:
			color = Color.red;
			break;
			
		}
		return color;
	}

	public bool Matches( Tile other ) {
		return _tileData.name == other._tileData.name;
	}
	public bool Matches( BaseTileData other ) {
		return _tileData.name == other.name;
	}

	public bool IsSelectable() {
		return _tileData.Moveable;
	}	           

	public bool IsMatchable() {
		return _tileData.Matchable;
	}

	public BaseTileData.TileType TileType {
		get { return _tileData.Type; }
	}

	public int GetDamage() {
		WeaponTileData weaponTileData = _tileData as WeaponTileData;
		if ( weaponTileData ) {
			return weaponTileData.Damage;
		}
		return 0;
	}

	public GameObject GetAttackVFXPrefab() {
		WeaponTileData weaponTileData = _tileData as WeaponTileData;
		if ( weaponTileData ) {
			return weaponTileData.AttackVFXPrefab;
		}
		return null;
	}

	public Texture GetAttackVFXTexture() {
		WeaponTileData weaponTileData = _tileData as WeaponTileData;
		if ( weaponTileData ) {
			return weaponTileData.AttackVFXTexture;
		}
		return null;
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
