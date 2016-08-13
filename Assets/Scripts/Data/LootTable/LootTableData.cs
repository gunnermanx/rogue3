using UnityEngine;
using System.Collections.Generic;
using FullSerializer;

public class LootTableData {

	// These are stored inside the loot table manager for reuse within the tables
	// Only LootTableData can get reused, entries are specific
	[fsProperty]
	public string Id;

	[fsProperty]
	public List<LootTableEntryData> Entries;

}

public class LootTableSetData {

	[fsProperty]
	public List<LootTableData> Set;
}
