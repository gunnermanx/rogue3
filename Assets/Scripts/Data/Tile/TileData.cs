using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileData {

	public enum TileType {
		Axe,
		Sword,
		Staff,
		Bow,
		Block
	}

	public string Id;

	public TileType Type;

	public Sprite Sprite;

	public int Damage;

	public int DamagePerLevel;

	public bool Moveable;
}
