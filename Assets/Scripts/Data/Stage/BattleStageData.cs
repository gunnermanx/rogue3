using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BattleStageData : ScriptableObject {

	public enum AttackPattern {
		RandomSet,
		All
	}

	public enum AttackType {
		ReplaceBlock,
		Encase
	}

	[System.Serializable]
	public class EnemyAttackData {
		public int X;
		public int Y;
		public AttackType Type; 
	}

	[System.Serializable]
	public class EnemyAttackDataSet {
		public List<EnemyAttackData> Attacks;
	}

	public int HPMin;
	public int HPMax;
	public int CooldownMin;
	public int CooldownMax;
	public int TurnsMin;
	public int TurnsMax;
	public Sprite EnemySprite;

	public AttackPattern Pattern;
	public List<EnemyAttackDataSet> AttackSets;
}

