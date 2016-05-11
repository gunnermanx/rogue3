﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class BaseTileData : ScriptableObject {

	public enum TileType {
		Axe,
		Sword,
		Staff,
		Bow,
		Mace,
		Unbreakable
	}

	public TileType Type;

	public Sprite Sprite;
	
	public bool Moveable;

	public bool Matchable;
}
