using System;
using FullSerializer;
using System.Collections.Generic;

public class MapBlob {

	[fsProperty]
	public Dictionary<string, MapNodeData> MapNodes = new Dictionary<string, MapNodeData>();

	[fsProperty]
	public List<MapEdgeData> MapEdges = new List<MapEdgeData>();

	[fsProperty]
	public List<string> CompletedNotes = new List<string>();

	[fsProperty]
	public string CurrentNode;
}

