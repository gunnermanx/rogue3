using System;
using FullSerializer;
using System.Collections.Generic;

public class PlayerBlob {

	[fsProperty]
	public List<string> OwnedTileIds;

	[fsProperty]
	public MapBlob MapBlob;

	[fsProperty]
	public int MaxLives;

	[fsProperty]
	public int CurrentLives;
}

