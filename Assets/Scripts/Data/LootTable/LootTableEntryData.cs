using System;
using System.Collections.Generic;
using FullSerializer;

public class LootTableEntryData {

	public enum DropStyle {
		ONE_OF,
		ANY_OF
	}

	// Weight of choosing this entry
	[fsProperty]
	public float Weight;

	[fsProperty]
	public DropStyle Style;

	[fsProperty]
	public List<LootTableDrop> Drops;
}


