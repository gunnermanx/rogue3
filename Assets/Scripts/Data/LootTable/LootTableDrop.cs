using System;
using FullSerializer;

public class LootTableDrop {

	public enum DropType {
		TABLE,
		CURRENCY,
		WEAPON
	}

	[fsProperty]
	public float Weight;

	[fsProperty]
	public DropType Type;

	[fsProperty]
	public string ItemId;

	[fsProperty]
	public int AmountMin;

	[fsProperty]
	public int AmountMax;

	public int Amount;
}

