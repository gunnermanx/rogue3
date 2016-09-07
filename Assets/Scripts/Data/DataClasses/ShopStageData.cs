using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class ShopStageData : ScriptableObject {

	public List<WeaponTileData> Weapons;

	public List<ConsumableItemData> Consumables;
}

