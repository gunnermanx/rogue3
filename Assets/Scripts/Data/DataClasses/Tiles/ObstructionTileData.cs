using UnityEngine;
using System.Collections;

[System.Serializable]
[CreateAssetMenu]
public class ObstructionTileData : BaseTileData
{
	public const int NEVER_EXPIRES = -1234;

	public int TurnsTilExpired = NEVER_EXPIRES;
}

