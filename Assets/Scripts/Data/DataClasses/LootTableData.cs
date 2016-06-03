using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class LootTableData : ScriptableObject {

	public List<LootTableEntryData> rows;

	public static LootTableDrop RollForDrop( LootTableData data ) {

		float totalWeight = 0f;
		for ( int i = 0, count = data.rows.Count; i < count; i ++ ) {
			totalWeight += data.rows[ i ].weight;
		}

		float runningWeight = 0f;
		float roll = UnityEngine.Random.Range( 0, totalWeight );
		LootTableEntryData pickedData = null;

		for ( int i = 0, count = data.rows.Count; i < count; i ++ ) {
			runningWeight += data.rows[ i ].weight;
			if ( runningWeight >= roll ) {
				pickedData = data.rows[ i ];
				break;
			}
		}

		if ( pickedData.table != null ) {
			return RollForDrop( pickedData.table );
		} else {
			return pickedData.drop;
		}
	}
}

[System.Serializable]
public class LootTableEntryData {

	public float weight;

	public LootTableData table;

	public LootTableDrop drop;
}

[System.Serializable]
public class LootTableDrop {
	
	public WeaponTileData tile;

	public GoldDrop gold;
}

[System.Serializable]
public class GoldDrop {
	public int min;
	public int max;
}


	