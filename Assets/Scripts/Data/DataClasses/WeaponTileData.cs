using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu]
public class WeaponTileData : BaseTileData
{
	public int Damage;

	public GameObject AttackVFXPrefab;

	public Texture AttackVFXTexture;
}

