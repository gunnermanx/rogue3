using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LootTableManager : MonoBehaviour {

	private const string LOOT_TABLE_DATA_PATH = "LootJSON";

	private Dictionary<string, LootTableData> _cachedLootTableData = new Dictionary<string, LootTableData>();

	private LootTableManager _instance = null;
	public LootTableManager Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
		LoadLootTableData();
	}

	public IEnumerator LoadLootTableData() {

		TextAsset[] data = Resources.LoadAll<TextAsset>( LOOT_TABLE_DATA_PATH );

		// Look at each file, each file contains a LootTableSetData which can contain one or more LootTableData
		for ( int i = 0, count = data.Length; i < count; i++ ) {
			LootTableSetData lootTableSet = (LootTableSetData) Serializer.Deserialize( typeof(LootTableSetData), data[ i ].text );
			if ( lootTableSet != null ) {
				// Look at each LootTableData in the set
				for ( int j = 0, count2 = lootTableSet.Set.Count; j < count2; j++ ) {
					LootTableData lootTableData = lootTableSet.Set[ j ];
					if ( !_cachedLootTableData.ContainsKey( lootTableData.Id ) ) {
						_cachedLootTableData.Add( lootTableData.Id, lootTableData );
					} else {
						Debug.LogError( "LootTableManager already has a loot table with id: " + lootTableData.Id + " cached" );
					}
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public List<LootTableDrop> RollFromTable( string lootTableId ) {

		LootTableData lootTable = null;
		List<LootTableDrop> drops = null;

		if ( _cachedLootTableData.TryGetValue( lootTableId, out lootTable ) ) {
			drops = RollFromTableInternal( lootTable );
		} else {
			Debug.LogError( "No loot table with id: " + lootTableId + " found" );
		}

		return drops;
	}

	private List<LootTableDrop> RollFromTableInternal( LootTableData table ) {
		LootTableEntryData entry = RollForEntry( table );

		List<LootTableDrop> drops = RollForDrop( entry );
		List<LootTableDrop> unprocessedDrops = new List<LootTableDrop>();
		List<LootTableDrop> finalDrops = new List<LootTableDrop>();

		while ( drops.Count > 0 ) {
			for ( int i = drops.Count-1; i >= 0; i-- ) {
				// Check if the drop is a table
				LootTableDrop drop = drops[ i ];
				if ( IsDropATable( drop ) ) {
					// first remove the drop, add results list to tableDrops
					drops.RemoveAt( i );
					// Add new drops to the unprocessed drops list
					unprocessedDrops.AddRange( RollFromTable( drop.ItemId ) );
				} else {
					finalDrops.Add( drop );
				}
			}
			// Things that we've found need to be processed
			drops = unprocessedDrops;
			unprocessedDrops.Clear();
		}

		return finalDrops;
	}

	private LootTableEntryData RollForEntry( LootTableData table ) {
		List<LootTableEntryData> entries = table.Entries;
		LootTableEntryData entry = null;

		// sum up the weights
		float totalWeight = 0f;
		for ( int i = 0, count = entries.Count; i < count; i++ ) {
			totalWeight += entries[ i ].Weight;
		}
			
		// Roll a random value from 0 to totalWeight and pick the first item that brings rollingTotalWeight above totalweight
		float roll = UnityEngine.Random.Range( 0, totalWeight );
		float rollingTotalWeight = 0f;
		for ( int i = 0, count = entries.Count; i < count; i++ ) {
			rollingTotalWeight += entries[ i ].Weight;
			if ( roll <= rollingTotalWeight ) {
				entry = entries[ i ];
				break;
			}
		}

		return entry;
	}

	private List<LootTableDrop> RollForDrop( LootTableEntryData entry ) {
		List<LootTableDrop> possibleDrops = entry.Drops;
		List<LootTableDrop> drops = new List<LootTableDrop>();

		// Check the style of the entry
		// If the drop style is to pick one of the drops, do a simple weighted roll
		if ( entry.Style == LootTableEntryData.DropStyle.ONE_OF ) {
			// sum up the weights
			float totalWeight = 0f;
			for ( int i = 0, count = possibleDrops.Count; i < count; i++ ) {
				totalWeight += possibleDrops[ i ].Weight;
			}

			// Roll a random value from 0 to totalWeight and pick the first item that brings rollingTotalWeight above totalweight
			float roll = UnityEngine.Random.Range( 0, totalWeight );
			float rollingTotalWeight = 0f;
			for ( int i = 0, count = possibleDrops.Count; i < count; i++ ) {
				rollingTotalWeight += possibleDrops[ i ].Weight;
				if ( roll <= rollingTotalWeight ) {
					drops.Add( possibleDrops[ i ] );
					break;
				}
			}
		}
		else if ( entry.Style == LootTableEntryData.DropStyle.ANY_OF ) {
			// We want to roll a dice for each drop, the weights of an ANY_OF are treated as a percentage
			for ( int i = 0, count = possibleDrops.Count; i < count; i++ ) {
				float roll = UnityEngine.Random.Range( 0f, 1f );
				if ( roll <= possibleDrops[ i ].Weight ) {
					drops.Add( possibleDrops[ i ] );
				}
			}
		}

		return drops;
	}

	private bool IsDropATable( LootTableDrop drop ) {
		// kind of temp for now
		if ( _cachedLootTableData.ContainsKey( drop.ItemId ) ) {
			return true;
		}
		return false;
	}
}

