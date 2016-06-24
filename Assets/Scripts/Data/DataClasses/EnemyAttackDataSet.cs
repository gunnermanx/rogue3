using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class EnemyAttackDataSet : ScriptableObject
{
	[System.Serializable]
	public class EnemyAttackData {
		public int X;
		public int Y;
		public ObstructionTileData ObstructionTile; 
	}

	public List<EnemyAttackData> AttackData;

}

