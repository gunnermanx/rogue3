using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class WeaponTileData : BaseTileData
{
	public int Damage;

	public GameObject AttackVFXPrefab;

	public Texture AttackVFXTexture;

	public List<BaseWeaponSkillData> WeaponSkills;
}

