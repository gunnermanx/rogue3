using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BaseTileData : ScriptableObject {

	public enum TileType {
		Sword,
		Tomes,
		Bow,
		Mace,
		Unbreakable
	}

	public TileType Type;

	public Sprite Sprite;
	
	public bool Moveable;

	public bool Matchable;

	public List<BaseWeaponSkillData> WeaponSkills;
}
