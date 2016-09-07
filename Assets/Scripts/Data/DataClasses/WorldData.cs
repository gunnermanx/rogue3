using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class WorldData : ScriptableObject {

	public List<BattleStageData> BattleStages;

	public List<ShopStageData> ShopStages;

	public int MinNumBattleStages = 11;

	public int MaxNumBattleStages = 13;

	public int MinNumShopStages = 2;

	public int MaxNumShopStages = 3;
}

