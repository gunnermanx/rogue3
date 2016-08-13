using System;
using FullSerializer;
using System.Collections.Generic;

public class PlayerBlob {

	public const string SLOT_PREFIX = "charSlot_";

	public static PlayerBlob NewPlayerBlob() {
		PlayerBlob blob = new PlayerBlob() {
			CharacterBlobSlots = new Dictionary<string, CharacterBlob>()
		};

		for ( int i = 0; i < TuningData.Instance.NumSaveSlots; i++ ) {
			blob.CharacterBlobSlots.Add( SLOT_PREFIX + i, null );
		}

		return blob;
	}

	[fsProperty]
	public Dictionary<string, CharacterBlob> CharacterBlobSlots;
}

public class CharacterBlob {

	// these values should come from somewhere else, revisit
	public static CharacterBlob NewCharacterBlob( string name ) {
		CharacterBlob blob = new CharacterBlob() {
			Name = name,
			MaxLives = 3,
			CurrentLives = 3,
			MapBlob = null,
			OwnedTileIds = new List<string>() {
				"WoodenSword",
				"WoodenBow",
				"WoodenTome",
				"WoodenMace"
			},
		};
		return blob;
	}

	[fsProperty]
	public string Name;

	[fsProperty]
	public int MaxLives;

	[fsProperty]
	public int CurrentLives;

	[fsProperty]
	public MapBlob MapBlob;

	[fsProperty]
	public List<string> OwnedTileIds;
}
